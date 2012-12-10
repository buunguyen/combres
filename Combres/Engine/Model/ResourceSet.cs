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
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.Schema;
using Fasterflect;

namespace Combres
{
    /// <summary>
    /// Represents a group of resource files.
    /// </summary>
    public sealed class ResourceSet : IEnumerable<Resource>
    {
        /// <summary>
        /// The name of the resource group.  This is to be referred to when generating
        /// links or URLs to the group.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Version of this resource group.
        /// </summary>
        public string Version { get; private set; }

        /// <summary>
        /// The algorithm used to generate auto-version.
        /// </summary>
        public Type VersionGeneratorType { get; private set; }

        private string hash;
        /// <summary>
        /// The content hash to be used if Version is 'auto'.
        /// 
        /// This is only thing in this class that is publicly mutable
        /// (via RecomputeHash()).
        /// 
        /// Accesses to this property is thread-safe (protected by <c>Configuration.ConfigLock</c>).
        /// </summary>
        public string Hash { 
            get
            {
                using (Configuration.ConfigLock.Read())
                {
                    return hash;
                }
            }
            set
            {
                using (Configuration.ConfigLock.Write())
                {
                    hash = value;
                } 
            } 
        }

        /// <summary>
        /// Duration of this resource group.  Specify how long the generated result
        /// should be cached at both client and server.
        /// </summary>
        public TimeSpan DurationInDays { get; private set; }

        /// <summary>
        /// The type of resource (JavaScript or CSS).
        /// </summary>
        public ResourceType Type { get; private set; }

        /// <summary>
        /// If true, items in this resource set won't be cached, compressed and minimized 
        /// to facilitate debugging.
        /// </summary>
        public bool DebugEnabled { get; private set; }

        /// <summary>
        /// <para>This has no effect if <see cref="DebugEnabled"/> is <c>false</c> or <see cref="WebExtensions.CombresUrl"/> is used.</para>
        /// <para>If this is set to <c>true</c>, Combres will generate script and link tags corresponding to each 
        /// resource in the set.  The URL for each of these script and link tags are generated 
        /// from <see cref="Resource.Path"/>.  The whole Combres pipeline (together with any filter) 
        /// is ignored completely.</para>
        /// <para>If this is set to <c>false</c> , the inherent debug workflow (including combining, filtering and compressing) of 
        /// Combres is executed</para>
        /// </summary>
        public bool IgnorePipelineWhenDebug { get; private set; }

        /// <summary>
        /// If 'true' compression is enabled.  If false compression is not enabled.
        /// </summary>
        public bool CompressionEnabled { get; private set; }

        /// <summary>
        /// The default minifier which is to be used for this resource set.
        /// </summary>
        public MinifierInfo Minifier { get; private set; }

        /// <summary>
        /// The providers which are to be used for this resource set.
        /// </summary>
        public IList<CacheVaryProviderInfo> CacheVaryProviders { get; private set; }

        /// <summary>
        /// The filters which are to be used for this resource set.
        /// </summary>
        public IList<FilterInfo> Filters { get; private set; }

        /// <summary>
        /// The application setting object.
        /// </summary>
        public Settings Settings { get; private set; }

        /// <summary>
        /// List of child resources.
        /// </summary>
        public IList<Resource> Resources { get; private set; }

        internal ResourceSet(Settings settings, XElement xe)
        {
            Settings = settings;
            InitializeSettings(xe);
            InitializeMinifier(xe);
            InitializeFilters(Settings);
            InitializeCacheVaryProviders(Settings);
            InitializeResources(xe);
            InitializeVersion(xe, Settings);
        }

        private void InitializeMinifier(XElement xe)
        {
            Minifier = Type == ResourceType.JS
                           ? ModelUtils.LoadMinifier(xe, SchemaConstants.Set.MinifierRef, Settings.DefaultJSMinifierRef, Settings.JSMinifierMap)
                           : ModelUtils.LoadMinifier(xe, SchemaConstants.Set.MinifierRef, Settings.DefaultCssMinifierRef, Settings.CssMinifierMap);
        }

        private void InitializeSettings(XElement xe)
        {
            Name = xe.Attr<string>(SchemaConstants.Set.Name);

            var duration = ModelUtils.GetString(xe, SchemaConstants.Set.Duration, Settings.DefaultDuration == null
                                                                                ? null
                                                                                : Settings.DefaultDuration.ToString());
            DurationInDays = TimeSpan.FromDays(int.Parse(duration, CultureInfo.InvariantCulture));

            Type = xe.Attr<ResourceType>(SchemaConstants.Set.Type);

            var debugEnabled = xe.Attr<string>(SchemaConstants.Set.DebugEnabled);
            DebugEnabled = string.IsNullOrEmpty(debugEnabled)
                               ? Settings.DefaultDebugEnabled // use parent if not specified
                               : debugEnabled.Equals(SchemaConstants.Set.Auto, StringComparison.OrdinalIgnoreCase)
                                     ? HttpContext.Current.IsDebuggingEnabled // use web.config if autor
                                     : bool.Parse(debugEnabled);

            IgnorePipelineWhenDebug = (bool)xe.Attr<string>(SchemaConstants.Set.IgnorePipelineWhenDebug)
                .ConvertToType(typeof(bool), Settings.DefaultIgnorePipelineWhenDebug);

            CompressionEnabled = (bool)xe.Attr<string>(SchemaConstants.Set.CompressionEnabled)
                .ConvertToType(typeof(bool), Settings.DefaultCompressionEnabled);
        }

        private void InitializeResources(XContainer xe)
        {
            var resources = new List<Resource>();
            foreach (var child in xe.Elements())
            {
                var referenceName = child.Attr<string>(SchemaConstants.Resource.Reference);
                if (referenceName != null) // referencing an existing resource set
                {
                    var referencedSet = Settings[referenceName];
                    if (referencedSet == null)
                        throw new XmlSchemaException(string.Format(CultureInfo.InvariantCulture, 
                            "Referenced resource set {0} must be declared before this resource set", referenceName));
                    resources.AddRange(referencedSet.Resources.Select(r => new Resource(this, r)));
                }
                else
                {
                    resources.Add(new Resource(this, child));    
                }
            }
            Resources = resources.AsReadOnly();
        }

        private void InitializeVersion(XElement xe, Settings parent)
        {
            Version = ModelUtils.GetString(xe, SchemaConstants.Set.Version, parent.DefaultVersion);

            var generatorType = xe.Attr<string>(SchemaConstants.Set.VersionGenerator);
            if (string.IsNullOrEmpty(generatorType))
            {
                VersionGeneratorType = parent.DefaultVersionGeneratorType;
            }
            else
            {
                VersionGeneratorType = ModelUtils.LoadType("Generator",
                    generatorType,
                    SchemaConstants.VersionGenerator.ValidTypes);
            }

            if (IsAutoVersion)
            {
                Hash = ComputeHash();
            }
        }

        /// <summary>
        /// Returns the hash if <code>Version</code> is 'auto'.  Otherwise returns the
        /// version itself.  
        /// </summary>
        /// <returns>Returns the hash if <code>Version</code> is 'auto'.  Otherwise returns the
        /// version itself.</returns>
        internal string GetVersionString()
        {
            return IsAutoVersion ? Hash : Version;
        }

        internal bool IsAutoVersion
        {
            get { return SchemaConstants.Set.Auto.Equals(Version, StringComparison.OrdinalIgnoreCase); }
        }

        internal string ComputeHash()
        {
            var generator = (IVersionGenerator)VersionGeneratorType.CreateInstance();
            return generator.Generate(this);
        }

        private void InitializeFilters(Settings settings)
        {
            Filters = settings.FilterInfoList.Where(
                filterInfo =>   
                {
                    if (filterInfo.ResourceSetNames != null &&
                            !filterInfo.ResourceSetNames.Contains(Name))
                    {
                        return false;
                    }
                    var filter = (IContentFilter)filterInfo.Type.CreateInstance();
                    return filter.CanApplyTo(Type);
                }).ToList().AsReadOnly();
        }

        private void InitializeCacheVaryProviders(Settings settings)
        {
            CacheVaryProviders = settings.CacheVaryProviderInfoList.Where(
                providerInfo => providerInfo.ResourceSetNames == null ||
                                providerInfo.ResourceSetNames.Contains(Name))
                                .ToList().AsReadOnly();
        }

        IEnumerator<Resource> IEnumerable<Resource>.GetEnumerator()
        {
            return Resources.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Resource>)this).GetEnumerator();
        }

        /// <summary>
        /// Returns true if both resource sets have the same <see cref="Name"/>.
        /// </summary>
        /// <param name="obj">The object to be compared.</param>
        /// <returns>Returns true if both resource sets have the same <see cref="Name"/>.  False otherwise.</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            var rs = obj as ResourceSet;
            if (rs == null)
                return false;
            return Name.Equals(rs.Name);
        }

        internal IList<CacheVaryState> GetCacheVaryStates(HttpContext ctx, 
            Predicate<ICacheVaryProvider> predicate)
        {
            var states = new List<CacheVaryState>();
            foreach (var providerInfo in CacheVaryProviders)
            {
                var provider = (ICacheVaryProvider)providerInfo.Type.CreateInstance();
                if (predicate == null || predicate(provider))
                {
                    var binder = (IObjectBinder) providerInfo.BinderType.CreateInstance();
                    binder.Bind(providerInfo.Parameters, provider);
                    states.Add(provider.Build(ctx, this));
                }
            }
            return states.AsReadOnly();
        }

        /// <remark>
        /// Guarantee to return same hashcode for identical resource set across multiple runs.
        /// </remark>
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Resource Set '{0}' (Type: {1})", Name, Type);
        }
    }
}