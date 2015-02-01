#region License
// Copyright 2009-2015 Buu Nguyen
// 
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at https://github.com/buunguyen/combres
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Xml.Linq;
using Combres.Compressors;
using Fasterflect;

namespace Combres
{
    internal sealed class RequestProcessor 
    {
        public static readonly string CachePrefix = Assembly.GetExecutingAssembly().GetName().Name;

        private static readonly IResourceSetCache ContentCache = (IResourceSetCache)
            Type.GetType(ConfigSectionSetting.Get().CacheProvider, true).CreateInstance();

        private static readonly ILogger Log = LoggerFactory.CreateLogger(
            MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 1. According to Steve Souders, browsers supporting gzip also supporting 
        /// deflate and gzip.
        /// 
        /// 2. http://developer.yahoo.com/performance/rules.html:
        ///         Gzip is the most popular and effective compression method at this time. 
        ///         It was developed by the GNU project and standardized by RFC 1952. 
        ///         The only other compression format you're likely to see is deflate, 
        ///         but it's less effective and less popular.
        /// 
        /// Despites #1, for the sake of robustness and the fact that I never took chance to
        /// verfify what Mr. Sounders says, Combres will support both GZip and Deflate although
        /// GZip is the default choice if a browser supports both.
        /// </summary>
        private static readonly ICompressor[] Compressors = {
                                                                new GZipCompressor(),
                                                                new DeflateCompressor(),
                                                                new NullCompressor()
                                                            };

        internal ResourceSet ResourceSet { get; set; }
        private HttpContext Context { get; set; }
        private Settings Settings { get; set; }
        private String CacheKey { get; set; }
        private String ETagCacheKey { get; set; }
        private IProcessingWorkflow Workflow { get; set; }
        private ICompressor Compressor { get; set; }
        private IList<CacheVaryState> CacheVaryStates { get; set; }

        public RequestProcessor(HttpContext context, Settings settings,
                                string setName, string version)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("***** Processing request for resource set " + setName);

            Context = context;
            Settings = settings;
            ResourceSet = Settings[setName];
            if (ResourceSet == null)
            {
                throw new ResourceSetNotFoundException(setName);
            }

            // Picks compressor & handling workflow
            var acceptEncoding = Context.Request.Headers["Accept-Encoding"];
            Compressor = Compressors.First(compressor =>
                                           ResourceSet.CompressionEnabled
                                               ? compressor.CanHandle(acceptEncoding)
                                               : compressor is NullCompressor);

            // Builds cache key and etag key
            CacheVaryStates = ResourceSet.GetCacheVaryStates(context, null);
            CacheKey = string.Join("/",
                                  new[]
                                       {
                                           CachePrefix,
                                           typeof (RequestProcessor).ToString(),
                                           setName,
                                           version,
                                           Compressor.EncodingName
                                       }.Concat(CacheVaryStates.Select(s => s.Key)).ToArray());
            ETagCacheKey = string.Concat(CacheKey, "/@etag");

            // Initilizes response
            Context.Response.ContentType = ResourceSet.Type == ResourceType.JS ? "application/x-javascript" : "text/css";
            Context.Response.Charset = "utf-8";
            Compressor.AppendHeader(Context.Response);

            // Initilizes handling workflow
            Workflow = ResourceSet.DebugEnabled
                           ? (IProcessingWorkflow)new DebugProcessingWorkflow(this)
                           : new DefaultProcessingWorkflow(this);
        }

        public void Execute()
        {
            using (new Timer("Processing " + ResourceSet.Name, Log.IsDebugEnabled, Log.Debug))
            {
                Workflow.Execute();
            }
        }

        internal bool IsInBrowserCache()
        {
            var etag = ContentCache[ETagCacheKey] as string;
            if (etag == null)
                return false;

            string incomingEtag = Context.Request.Headers["If-None-Match"];
            if (String.Equals(incomingEtag, etag, StringComparison.Ordinal))
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("ETag matches, ending request...");

                Context.Response.Cache.SetETag(etag);
                Context.Response.AppendHeader("Content-Length", "0");
                Context.Response.StatusCode = (int) HttpStatusCode.NotModified;
                Context.Response.End();
                return true;
            }
            return false;
        }

        internal void SendOutputToClient(byte[] bytes, bool insertCacheHeaders, string etag)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Writing content to browser...");

            var response = Context.Response;

            if (insertCacheHeaders)
            {
                var cache = Context.Response.Cache;
                cache.SetETag(etag);
                cache.SetOmitVaryStar(true);
                cache.SetMaxAge(ResourceSet.DurationInDays);
                cache.SetLastModified(DateTime.Now);
                cache.SetExpires(DateTime.Now.Add(ResourceSet.DurationInDays)); // For HTTP 1.0 browsers
                cache.SetValidUntilExpires(true);
                cache.SetCacheability(HttpCacheability.Public); 
                cache.SetRevalidation(HttpCacheRevalidation.AllCaches); 
                cache.VaryByHeaders["Accept-Encoding"] = true; // Tell proxy to cache different versions depending on Accept-Encoding
            }

            var length = bytes.Length;
            response.AppendHeader("Content-Length", length.ToString(CultureInfo.InvariantCulture));
            if (length > 0)
                response.OutputStream.Write(bytes, 0, length);
        }

        internal void CacheNewResponse(byte[] responseBytes, string etag)
        {
            ContentCache.Add(CacheKey,
                             responseBytes,
                             ResourceSet.DurationInDays);
            ContentCache.Add(ETagCacheKey,
                             etag,
                             ResourceSet.DurationInDays);
        }

        /// <summary>
        /// ETag for a resource set is basically the checksum of its content.
        /// If the resource set is auto-version, the Hash property is used because
        /// it's already a strong checksum.  Otherwise, a new checksum is calculated
        /// out of the resource set's content.
        /// </summary>
        internal string GenerateETag(byte[] content)
        {
            string etag = ResourceSet.IsAutoVersion 
                ? ResourceSet.Hash 
                : content.GetHash();
            return string.Concat("\"", etag, "\"");
        }

        internal bool WriteFromServerCache()
        {
            var responseBytes = ContentCache[CacheKey] as byte[];
            var etag = ContentCache[ETagCacheKey] as string;

            /*
             * If data for either CacheKey or ETagCacheKey is already invalidated, we'll
             * serve new content.  This might not be very efficient when data for
             * CacheKey still exists while data for ETagCacheKey doesn't.  We could have
             * re-generated etag out of the data for CacheKey, but doing so would be 
             * problematic:
             * 1. If we store re-generated etag into the cache, we don't know how 
             *    much time left to store.
             * 2. If we don't store re-generated etag into the cache, client won't
             *    be able to receive Non-Modified response anymore.
             */
            if (responseBytes == null || etag == null)
                return false;

            if (Log.IsDebugEnabled)
                Log.Debug("Writing to client from server's cache...");

            SendOutputToClient(responseBytes, true, etag);
            return true;
        }

        internal string[] GetSingleContents(IEnumerable<Resource> resources, bool useCache)
        {
            using (new Timer("Reading & filtering " + ResourceSet.Name, Log.IsDebugEnabled, Log.Debug))
            {
                return resources.Select(resource =>
                                        FilterContent<string, ISingleContentFilter, Resource>(
                                            resource, ReadResourceContent(resource, useCache))).ToArray();
            }
        }

        internal string GetCombinedContents(IEnumerable<Resource> resources, string[] singleContents,
                                            bool addFilePathComment)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Combining resources' contents...");
            using (new Timer("Combining & filtering " + ResourceSet.Name, Log.IsDebugEnabled, Log.Debug))
            {
                if (addFilePathComment)
                {
                    var resourceArray = resources.ToArray();
                    for (int i = 0; i < resourceArray.Length; i++)
                    {
                        var newLines = i == 0 ? "{0}" : "{0}{0}{0}";
                        singleContents[i] = string.Format(
                            "{0}/* Comment Generated by Combres - {2} */{1}{3}",
                            string.Format(newLines, Environment.NewLine),
                            Environment.NewLine,
                            resourceArray[i],
                            singleContents[i]);
                    }
                }
                var combinedContent = MergeContents(singleContents);
                return FilterContent<string, ICombinedContentFilter, IEnumerable<Resource>>(
                    resources, combinedContent);
            }
        }

        internal string MergeContents(string[] singleContents)
        {
            var separator = ResourceSet.Type == ResourceType.JS
                                ? ";\n" // safe-guard when merging JavaScript files
                                : string.Empty;
            return string.Join(separator, singleContents);
        }

        internal string MinifyContent(MinifierInfo minifierInfo, IEnumerable<Resource> resources, string combinedContent)
        {
            if (Log.IsDebugEnabled)
                Log.Debug(string.Format(CultureInfo.InvariantCulture,
                                        "Minifying combined content using minifier {0} and binder {1}...",
                                        minifierInfo.Name,
                                        minifierInfo.BinderType));

            using (new Timer("Minifying & filtering " + ResourceSet.Name, Log.IsDebugEnabled, Log.Debug))
            {
                var minifier = (IResourceMinifier) minifierInfo.Type.CreateInstance();
                BindParameters(minifier, minifierInfo.BinderType, minifierInfo.Parameters);
                var minifiedContent = minifier.Minify(Settings, ResourceSet, combinedContent);
                return FilterContent<string, IMinifiedContentFilter, IEnumerable<Resource>>(
                    resources, minifiedContent);
            }
        }

        internal byte[] TryZipContent(string content, MemoryStream stream)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Compressing combined content...");
            using (new Timer("Compressing & filtering " + ResourceSet.Name, Log.IsDebugEnabled, Log.Debug))
            {
                Compressor.Compress(content, stream);
                return FilterContent<byte[], ICompressedContentFilter, ResourceSet>(
                    ResourceSet, stream.ToArray());
            }
        }

        private static string ReadResourceContent(Resource resource, bool useCache)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Reading content of " + resource.Path + "...");
            return useCache
                       ? resource.ReadFromCache(true)
                       : resource.ReadNewContent();
        }

        private TContent FilterContent<TContent, TFilter, TContext>(TContext context, TContent content)
        {
            var filterInfoList = ResourceSet.Filters.Where(t => typeof (TFilter).IsAssignableFrom(t.Type));

            foreach (var filterInfo in filterInfoList)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Performing filtering with " + filterInfo.Type + " for " + context + "...");

                var filter = (IContentFilter) filterInfo.Type.CreateInstance();
                BindParameters(filter, filterInfo.BinderType, filterInfo.Parameters);

                /* 
                 * It's possible to get rid of reflection for the "invoke" part, but requiring 
                 * more logic while the time gain is insignificant as Fasterflect
                 * performs nearly as fast as native call from the 2nd invocations
                 */
                if (typeof (TContext) == typeof (ResourceSet)) // The API won't pass resource set twice
                {
                    content = (TContent) filter.CallMethod("TransformContent",
                                                           new[] {typeof (ResourceSet), typeof (TContent)},
                                                           new object[] {ResourceSet, content});
                }
                else
                {
                    content = (TContent)filter.CallMethod("TransformContent",
                                                          new[] { typeof(ResourceSet), typeof(TContext), typeof(TContent) },
                                                          new object[] { ResourceSet, context, content });
                }
            }
            return content;
        }

        private void BindParameters(object target, Type binderType, IEnumerable<XElement> parameters)
        {
            var binder = (IObjectBinder) binderType.CreateInstance();
            binder.Bind(parameters, target);
            if (target is ICacheVaryStateReceiver)
            {
                var receiver = target as ICacheVaryStateReceiver;
                receiver.CacheVaryStates = CacheVaryStates;
            }
        }
    }
}