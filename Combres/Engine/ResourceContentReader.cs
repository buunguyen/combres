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

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Hosting;

namespace Combres
{
    /// <summary>
    /// Class providing methods to read static/dynamic resource contents from cache and/or
    /// from their original sources.
    /// </summary>
    internal static class ResourceContentReader
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
                       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly ResourceContentCache Cache = new ResourceContentCache();

        /// <summary>
        /// Loads the content of the resource from the cache.  Otherwise,
        /// reload the content from its source (e.g. file system or HTTP download)
        /// and store it in the cache for subsequent accesses.
        /// </summary>
        /// <param name="resource">The resource whose cached content is to be retrieved.</param>
        /// <param name="readNewIfEmptyCache">If true, attempt to read the content from the orginal source (file system or HTTP)
        /// when the cached content isn't found.  If false, not attempting to do so.</param>
        /// <returns>The cached content of the resource.  If not found and <paramref name="readNewIfEmptyCache"/> is true, return
        /// the content from source.  Otherwise, return null.</returns>
        public static string ReadFromCache(this Resource resource, bool readNewIfEmptyCache)
        {
            var content = Cache.Get(resource);
            if (content != null)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Use content from content's cache for " + resource);
                return content;
            }
            return readNewIfEmptyCache 
                ? resource.ReadNewContent()
                : null;
        }

        /// <summary>
        /// Reloads the content from its source (e.g. file system or HTTP download) and stores into the cache.
        /// </summary>
        /// <param name="resource">The resource whose content is to be read & stored in the cache.</param>
        /// <returns>The latest content of the resource read from source.</returns>
        public static string ReadNewContent(this Resource resource)
        {
            string content;
            if (Log.IsDebugEnabled)
                Log.Debug("Retrieving new content for " + resource);
            switch (resource.Mode)
            {
                case ResourceMode.Dynamic:
                    var absoluteUrl = resource.Path.ToAbsoluteUrl();
                    if (absoluteUrl == null)
                        throw new ResourceNotFoundException(resource.Path);
                    try
                    {
                        using (var webClient = new WebClient())
                        {
                            if (resource.ForwardCookie)
                            {
                                var context = HttpContext.Current;
                                if (context == null)
                                    throw new CombresException("HttpContext must present to forward cookie");
                                webClient.Headers[HttpRequestHeader.Cookie] = context.Request.Headers["Cookie"];
                            }
                            webClient.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
                            var bytes = webClient.DownloadData(absoluteUrl);
                            var acceptEncoding = webClient.ResponseHeaders[HttpResponseHeader.ContentEncoding];
                            if (acceptEncoding != null)
                            {
                                if (acceptEncoding.Contains("gzip"))
                                    bytes = bytes.UnGzip();
                                else if (acceptEncoding.Contains("deflate"))
                                    bytes = bytes.UnDeflate();
                            }
                            content = Encoding.UTF8.GetString(bytes);
                        }
                    }
                    catch (WebException ex)
                    {
                        throw new ResourceNotFoundException(resource.Path, ex);
                    }
                    break;
                default:
                    var fullPath = HostingEnvironment.MapPath(resource.Path);
                    if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
                        throw new ResourceNotFoundException(resource.Path);
                    content = File.ReadAllText(fullPath);
                    break;
            }
            Cache.Set(resource, content);
            return content;
        }

        /// <summary>
        /// Removes the cached content, if exist.
        /// </summary>
        /// <param name="resource">The resource whos content is to be removed from the cache.</param>
        public static void RemoveInCache(this Resource resource)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Attempt to remove cached content for " + resource);
            Cache.Remove(resource);
        }

        /// <summary>
        /// Clears the whole cache.
        /// </summary>
        public static void ClearCache()
        {
            Cache.Clear();
        }

        private sealed class ResourceContentCache
        {
            private readonly ILock synchLock = LockFactory.CreateLock(LockPolicy.SimilarReadAndWrite);
            private readonly IDictionary<Resource, string> cache = new Dictionary<Resource, string>();

            public string Get(Resource resource)
            {
                using (synchLock.Read())
                {
                    return cache.ContainsKey(resource) ? cache[resource] : null;
                }
            }

            public void Set(Resource resource, string content)
            {
                using (synchLock.Write())
                {
                    cache[resource] = content;
                }
            }

            public void Remove(Resource resource)
            {
                using (synchLock.Write())
                {
                    cache.Remove(resource);
                }
            }

            public void Clear()
            {
                using (synchLock.Write())
                {
                    cache.Clear();
                }
            }
        }
    }
}
