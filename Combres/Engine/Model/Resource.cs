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
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Combres
{
    /// <summary>
    /// Represents a client-side resource, i.e. JavaScript or CSS file.
    /// </summary>
    public sealed class Resource
    {
        /// <summary>
        /// The path/URL to this resource. 
        /// </summary>
        /// <seealso cref="Mode"/>
        public string Path { get; private set; }

        /// <summary>
        /// The type of this resource.
        /// </summary>
        /// <seealso cref="ResourceMode"/>
        public ResourceMode Mode { get; private set; }

        /// <summary>
        /// Whether cookie should be fowarded or not.
        /// </summary>
        public bool ForwardCookie { get; private set; }

        /// <summary>
        /// The parent resource set.
        /// </summary>
        public ResourceSet ParentSet { get; private set; }

        /// <summary>
        /// The minifier which is to be used for this resource.
        /// </summary>
        public MinifierInfo Minifier { get; private set; }

        internal bool IsInSameApplication
        {
            get { return Path.StartsWith("~", StringComparison.Ordinal); }
        }
        
        internal Resource(ResourceSet parent, Resource copy)
        {
            ParentSet = parent;
            Path = copy.Path;
            Mode = copy.Mode;
            ForwardCookie = copy.ForwardCookie;
            Minifier = copy.Minifier;
        }

        internal Resource(ResourceSet parent, XElement xe)
        {
            ParentSet = parent;
            Path = xe.Attr<string>(SchemaConstants.Resource.Path);
            Mode = xe.Attr(SchemaConstants.Resource.Mode, Default.Resource.Mode);
            ForwardCookie = xe.Attr(SchemaConstants.Resource.ForwardCookie,
                Default.Resource.ForwardCookie);
            Minifier = ParentSet.Type == ResourceType.JS
                ? ModelUtils.LoadMinifier(xe, SchemaConstants.Resource.MinifierRef, ParentSet.Minifier.Name, ParentSet.Settings.JSMinifierMap)
                : ModelUtils.LoadMinifier(xe, SchemaConstants.Resource.MinifierRef, ParentSet.Minifier.Name, ParentSet.Settings.CssMinifierMap);
            if (Mode == ResourceMode.Static && ForwardCookie)
                throw new XmlSchemaException("ForwardCookie must not be True when Mode is Static");
        }

        /// <summary>
        /// Returns true if both resources share the same value for <see cref="Path"/> (case insensitive), <see cref="Mode"/>, and
        /// <see cref="ForwardCookie"/>.
        /// </summary>
        /// <param name="obj">The resource to be compared.</param>
        /// <returns>Returns true if both resources share the same value for <see cref="Path"/> (case insensitive), <see cref="Mode"/>, and
        /// <see cref="ForwardCookie"/>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            var other = obj as Resource;
            if (other == null)
                return false;
            return Path.Equals(other.Path, StringComparison.OrdinalIgnoreCase) &&
                   Mode == other.Mode &&
                   ForwardCookie.Equals(other.ForwardCookie);
        }

        /// <remark>
        /// Guarantee to return same hashcode for identical resource across multiple runs.
        /// </remark>
        public override int GetHashCode()
        {
            return new object[] { Path.ToLowerInvariant(), Mode.ToString(), ForwardCookie }
                .Select(o => o.GetHashCode())
                .Aggregate(17, (accum, element) => 31 * accum + element);
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "Resource '{0}' (Mode: {1})", Path, Mode);
        }
    }
}