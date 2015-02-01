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

namespace Combres
{
    internal static class SchemaConstants
    {
        public const string RootName = "combres";
        public const string Namespace = "urn:combres";

        public static class Setting
        {
            public const string Filters = "filters";
            public const string CacheVaryProviders = "cacheVaryProviders";
            public const string JSMinifiers = "jsMinifiers";
            public const string CssMinifiers = "cssMinifiers";
            public const string ResourceSets = "resourceSets";
            public const string Url = "url";
            public const string Host = "host";
            public const string SslHost = "sslHost";
            public const string DefaultDuration = "defaultDuration";
            public const string DefaultVersion = "defaultVersion";
            public const string DefaultVersionGenerator = "defaultVersionGenerator";
            public const string DefaultDebugEnabled = "defaultDebugEnabled";
            public const string DefaultIgnorePipelineWhenDebug = "defaultIgnorePipelineWhenDebug";
            public const string DefaultCompressionEnabled = "defaultCompressionEnabled";
            public const string DefaultJSMinifierRef = "defaultJSMinifierRef";
            public const string DefaultCssMinifierRef = "defaultCssMinifierRef";
            public const string LocalChangeMonitorInterval = "localChangeMonitorInterval";
            public const string RemoteChangeMonitorInterval = "remoteChangeMonitorInterval";
        }

        public static class Set
        {
            public const string Name = "name";
            public const string Duration = "duration";
            public const string Version = "version";
            public const string VersionGenerator = "versionGenerator";
            public const string Type = "type";
            public const string DebugEnabled = "debugEnabled";
            public const string IgnorePipelineWhenDebug = "ignorePipelineWhenDebug";
            public const string CompressionEnabled = "compressionEnabled";
            public const string MinifierRef = "minifierRef";
            public const string Auto = "auto";
        }

        public static class Resource
        {
            public const string Path = "path";
            public const string Reference = "reference";
            public const string Mode = "mode";
            public const string ForwardCookie = "forwardCookie";
            public const string MinifierRef = "minifierRef";
        }

        public static class Binder
        {
            public const string Type = "binderType";
            public static readonly IList<Type> ValidTypes = new List<Type>
                {
                    typeof (IObjectBinder)
                }.AsReadOnly();
        }

        public static class Filter
        {
            public const string Type = "type";
            public const string AcceptedResourceSets = "acceptedResourceSets";

            public static readonly IList<Type> ValidTypes = new List<Type>
             {
                 typeof (ISingleContentFilter),
                 typeof (ICombinedContentFilter),
                 typeof (IMinifiedContentFilter),
                 typeof (ICompressedContentFilter)
             }.AsReadOnly();
        }

        public static class CacheVaryProvider
        {
            public const string Type = "type";
            public const string AcceptedResourceSets = "acceptedResourceSets";

            public static readonly IList<Type> ValidTypes = new List<Type>
             {
                 typeof (ICacheVaryProvider)
             }.AsReadOnly();
        }

        public static class VersionGenerator
        {
            public static readonly IList<Type> ValidTypes = new List<Type>
             {
                 typeof (IVersionGenerator)
             }.AsReadOnly();
        }

        public static class Minifier
        {
            public const string Default = "default";
            public const string Off = "off";
            public const string Name = "name";
            public const string Type = "type";

            public static readonly IList<Type> ValidTypes = new List<Type>
            {
                typeof (IResourceMinifier)
            }.AsReadOnly();
        }

        public static class Param
        {
            public const string Root = "param";
        }
    }
}
