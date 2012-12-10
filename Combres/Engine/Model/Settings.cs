#region License
// Copyright 2011 Buu Nguyen (http://www.buunguyen.net/blog)
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
// The latest version of this file can be found at http://combres.codeplex.com
#endregion

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Combres
{
    /// <summary>
    /// Represents the whole Combres configuration settings.
    /// </summary>
    public sealed class Settings
    {
        /// <summary>
        /// Relative URL of the combiner.   
        /// </summary>
        /// <example>
        /// combres.axd
        /// </example>
        public string Url { get; private set; }

        /// <summary>
        /// Host URL, used as prefix to generated URLs.
        /// </summary>
        public string Host { get; private set; }
        
        /// <summary>
        /// SSL Host URL, used as prefix to generated URLs when the request is made via SSL.
        /// </summary>
        public string SslHost { get; private set; }

        /// <summary>
        /// Default version which will be used if <see cref="ResourceSet.DurationInDays"/> is 
        /// not specified.
        /// </summary>
        public int? DefaultDuration { get; private set; }

        /// <summary>
        /// Default version which will be used if <see cref="ResourceSet.Version"/> is 
        /// not specified.
        /// </summary>
        public string DefaultVersion { get; private set; }

        /// <summary>
        /// Default version generator which will be used if <see cref="ResourceSet.VersionGeneratorType"/> is 
        /// not specified.
        /// </summary>
        public Type DefaultVersionGeneratorType { get; private set; }

        /// <summary>
        /// Default debug enabled setting which will be used if <see cref="ResourceSet.DebugEnabled"/> is 
        /// not specified.
        /// </summary>
        public bool DefaultDebugEnabled { get; private set; }

        /// <summary>
        /// Default setting which will be used if <see cref="ResourceSet.IgnorePipelineWhenDebug"/> is 
        /// not specified.
        /// </summary>
        public bool DefaultIgnorePipelineWhenDebug { get; private set; }

        /// <summary>
        /// Default CSS minifier which will be used if a CSS resource set doesn't specify a specific minifier.
        /// </summary>
        public string DefaultCssMinifierRef { get; private set; }

        /// <summary>
        /// Default JavaScript minifier which will be used if a JS resource set doesn't specify a specific minifier.
        /// </summary>
        public string DefaultJSMinifierRef { get; private set; }

        /// <summary>
        /// Default compression enabling setting which will be used if <see cref="ResourceSet.CompressionEnabled"/> is 
        /// not specified.
        /// </summary>
        public bool DefaultCompressionEnabled { get; private set; }

        /// <summary>
        /// Interval in seconds that the ChangeMonitor will use to monitor changes to
        /// dynamic resources belonging to the same web application.
        /// </summary>
        public int? LocalChangeMonitorInterval { get; private set; }

        /// <summary>
        /// Interval in seconds that the ChangeMonitor will use to monitor changes to
        /// dynamic resources belonging to another web application or server.
        /// </summary>
        public int? RemoteChangeMonitorInterval { get; private set; }

        /// <summary>
        /// CSS minifier types.
        /// </summary>
        public IDictionary<string, MinifierInfo> CssMinifierMap { get; private set; }

        /// <summary>
        /// JavaScript minifier types.
        /// </summary>
        public IDictionary<string, MinifierInfo> JSMinifierMap { get; private set; }

        /// <summary>
        /// Lists of filter info.
        /// </summary>
        public IList<FilterInfo> FilterInfoList { get; private set; }

        /// <summary>
        /// Lists of cache vary provider info.
        /// </summary>
        public IList<CacheVaryProviderInfo> CacheVaryProviderInfoList { get; private set; }

        /// <summary>
        /// Lists of child resource sets.
        /// </summary>
        public IList<ResourceSet> ResourceSets { get; private set; }

        internal Settings(XElement xe)
        {
            LoadFilters(xe);
            LoadCacheVaryProvider(xe);
            LoadJsMinifiers(xe);
            LoadCssMinifiers(xe);
            LoadResourceSets(xe);
        }

        private void LoadFilters(XElement xe)
        {
            var filterInfoList = new List<FilterInfo>();
            foreach (var child in xe.ChildrenOfChild(SchemaConstants.Setting.Filters, SchemaConstants.Namespace))
            {
                var typeName = child.Attr<string>(SchemaConstants.Filter.Type);
                var type = ModelUtils.LoadType("Filter", typeName, SchemaConstants.Filter.ValidTypes);
                var acceptedResourceSets = child.Attr<string>(SchemaConstants.Filter.AcceptedResourceSets);
                var setNameList = acceptedResourceSets == null
                                           ? null
                                           : acceptedResourceSets.Trim(';', ' ').Split(';').ToList();
                var filter = new FilterInfo
                {
                    Type = type,
                    Parameters = child.Elements(XName.Get(SchemaConstants.Param.Root, SchemaConstants.Namespace)).ToList(),
                    BinderType = GetBinderType(child),
                    ResourceSetNames = setNameList
                };
                filterInfoList.Add(filter);
            }
            FilterInfoList = filterInfoList.AsReadOnly();
        }

        private void LoadCacheVaryProvider(XElement xe)
        {
            var providerInfoList = new List<CacheVaryProviderInfo>();
            foreach (var child in xe.ChildrenOfChild(SchemaConstants.Setting.CacheVaryProviders, SchemaConstants.Namespace))
            {
                var typeName = child.Attr<string>(SchemaConstants.CacheVaryProvider.Type);
                var type = ModelUtils.LoadType("CacheVaryProvider", typeName, SchemaConstants.CacheVaryProvider.ValidTypes);
                var acceptedResourceSets = child.Attr<string>(SchemaConstants.CacheVaryProvider.AcceptedResourceSets);
                var setNameList = acceptedResourceSets == null
                                           ? null
                                           : acceptedResourceSets.Trim(';', ' ').Split(';').ToList();
                var provider = new CacheVaryProviderInfo
                {
                    Type = type,
                    Parameters = child.Elements(XName.Get(SchemaConstants.Param.Root, SchemaConstants.Namespace)).ToList(),
                    BinderType = GetBinderType(child),
                    ResourceSetNames = setNameList
                };
                providerInfoList.Add(provider);
            }
            CacheVaryProviderInfoList = providerInfoList.AsReadOnly();
        }

        private static Type GetBinderType(XElement child)
        {
            var binderTypeName = child.Attr<string>(SchemaConstants.Binder.Type);
            return binderTypeName == null
                       ? Default.Binder.Type
                       : ModelUtils.LoadType("Binder", binderTypeName, SchemaConstants.Binder.ValidTypes);
        }

        private void LoadJsMinifiers(XElement xe)
        {
            var jsMinifierMap = new Dictionary<string, MinifierInfo>();
            jsMinifierMap[SchemaConstants.Minifier.Off] = Default.Minifier.Off;
            jsMinifierMap[SchemaConstants.Minifier.Default] = Default.Minifier.JS;
            LoadMinifiers(xe, SchemaConstants.Setting.JSMinifiers, jsMinifierMap);
            JSMinifierMap = jsMinifierMap.AsReadonly();
        }

        private void LoadCssMinifiers(XElement xe)
        {
            var cssMinifierMap = new Dictionary<string, MinifierInfo>();
            cssMinifierMap[SchemaConstants.Minifier.Off] = Default.Minifier.Off;
            cssMinifierMap[SchemaConstants.Minifier.Default] = Default.Minifier.Css;
            LoadMinifiers(xe, SchemaConstants.Setting.CssMinifiers, cssMinifierMap);
            CssMinifierMap = cssMinifierMap.AsReadonly();
        }

        private static void LoadMinifiers(XElement xe, string elementName, IDictionary<string, MinifierInfo> map)
        {
            foreach (var childXe in xe.ChildrenOfChild(elementName, SchemaConstants.Namespace))
            {
                var name = childXe.Attr<string>(SchemaConstants.Minifier.Name);
                if (map.ContainsKey(name))
                    throw new XmlSchemaException(string.Format(CultureInfo.InvariantCulture, 
                                                     "Minifier {0} already exists", name));
                var typeName = childXe.Attr<string>(SchemaConstants.Minifier.Type);
                var type = ModelUtils.LoadType("Minifier", typeName, SchemaConstants.Minifier.ValidTypes);
                var minifier = new MinifierInfo
                                   {
                                       Name = name,
                                       Type = type,
                                       Parameters = childXe.Elements(XName.Get(SchemaConstants.Param.Root, SchemaConstants.Namespace)).ToList(),
                                       BinderType = GetBinderType(childXe),
                                   };
                map.Add(name, minifier);
            }
        }

        private void LoadResourceSets(XContainer xe)
        {
            var rsXe = xe.Element(XName.Get(SchemaConstants.Setting.ResourceSets, SchemaConstants.Namespace));

            Url = rsXe.Attr<string>(SchemaConstants.Setting.Url);
            Host = rsXe.Attr<string>(SchemaConstants.Setting.Host);
            SslHost = rsXe.Attr<string>(SchemaConstants.Setting.SslHost);
            DefaultDuration = rsXe.Attr<int?>(SchemaConstants.Setting.DefaultDuration);
            DefaultVersion = rsXe.Attr<string>(SchemaConstants.Setting.DefaultVersion);
            var generatorType = rsXe.Attr<string>(SchemaConstants.Setting.DefaultVersionGenerator);
            DefaultVersionGeneratorType = string.IsNullOrEmpty(generatorType)
                                              ? Default.ResourceSet.VersionGeneratorType
                                              : ModelUtils.LoadType("Generator",
                                                    generatorType,
                                                    SchemaConstants.VersionGenerator.ValidTypes);

            LocalChangeMonitorInterval = rsXe.Attr<int?>(SchemaConstants.Setting.LocalChangeMonitorInterval);
            RemoteChangeMonitorInterval = rsXe.Attr<int?>(SchemaConstants.Setting.RemoteChangeMonitorInterval);

            var debugEnabled = rsXe.Attr<string>(SchemaConstants.Setting.DefaultDebugEnabled);
            DefaultDebugEnabled = string.IsNullOrEmpty(debugEnabled) 
                ? Default.ResourceSet.DebugEnabled
                : debugEnabled.Equals(SchemaConstants.Set.Auto, StringComparison.OrdinalIgnoreCase) 
                    ? HttpContext.Current.IsDebuggingEnabled 
                    : bool.Parse(debugEnabled);

            DefaultIgnorePipelineWhenDebug = (bool)rsXe.Attr<string>(SchemaConstants.Setting.DefaultIgnorePipelineWhenDebug)
                .ConvertToType(typeof(bool), Default.ResourceSet.IgnorePipelineWhenDebug);

            DefaultCompressionEnabled = (bool)rsXe.Attr<string>(SchemaConstants.Setting.DefaultCompressionEnabled)
                .ConvertToType(typeof(bool), Default.ResourceSet.CompressionEnabled);

            DefaultJSMinifierRef = GetMinifier(rsXe, SchemaConstants.Setting.DefaultJSMinifierRef, JSMinifierMap);
            DefaultCssMinifierRef = GetMinifier(rsXe, SchemaConstants.Setting.DefaultCssMinifierRef, CssMinifierMap);

            ResourceSets = new List<ResourceSet>();
            foreach (var node in rsXe.Elements())
            {
                var rs = new ResourceSet(this, node);
                if (ResourceSets.Contains(rs))
                    throw new XmlSchemaException("Duplicated resource set");
                ResourceSets.Add(rs);
            }
            ResourceSets = ResourceSets.ToList().AsReadOnly();
        }

        private static string GetMinifier(XElement xe, string elementName, IDictionary<string, MinifierInfo> map)
        {
            var minifierRef = xe.Attr(elementName, SchemaConstants.Minifier.Default);
            if (!map.ContainsKey(minifierRef))
                throw new XmlSchemaException(string.Format(CultureInfo.InvariantCulture, "Minifier {0} is not declared", minifierRef));
            return minifierRef;
        }

        ///<summary>
        /// Returns the resource set with the specified <paramref name="setName"/>
        ///</summary>
        ///<param name="setName">The name of the resource set to be retrieved.</param>
        public ResourceSet this[string setName]
        {
            get
            {
                return ResourceSets.FirstOrDefault(resource => resource.Name == setName);
            }
        }
    }
}