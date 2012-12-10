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

using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace Combres
{
    internal static class XLinqExtensions
    {
        /// <summary>
        /// Validates <paramref name="xdoc"/> against one or more schemas.
        /// </summary>
        /// <param name="xdoc">The XML document to be validated</param>
        /// <param name="schemaReaders">Readers representing XSD schemas</param>
        public static void Validate(this XDocument xdoc, params XmlReader[] schemaReaders)
        {
            xdoc.Validate(null, schemaReaders);
        }

        /// <summary>
        /// Validates <paramref name="xdoc"/> against one or more schemas.
        /// </summary>
        /// <param name="xdoc">The XML document to be validated</param>
        /// <param name="nameSpace">Namespace of the document</param>
        /// <param name="schemaReaders">Readers representing XSD schemas</param>
        public static void Validate(this XDocument xdoc, string nameSpace,
                                    params XmlReader[] schemaReaders)
        {
            var schemas = new XmlSchemaSet();
            
            foreach (var sr in schemaReaders)
                schemas.Add(nameSpace, sr);
            xdoc.Validate(schemas, (sender, e) => { throw e.Exception; });
        }

        /// <summary>
        /// Retrieves the list of child elements for a child whose name is <paramref name="name"/>
        /// of the element <paramref name="xe"/>.
        /// </summary>
        /// <param name="xe">The element whose grand-children are to be retrieved</param>
        /// <param name="name">The name of the child element whose children are to be retrieved</param>
        /// <param name="nameSpace">The namespace of the XML document</param>
        /// <returns>The list of child elements for a child whose name is <paramref name="name"/>
        /// of the element <paramref name="xe"/></returns>
        public static IEnumerable<XElement> ChildrenOfChild(this XElement xe, string name, string nameSpace)
        {
            var childXe = xe.Element(XName.Get(name, nameSpace));
            return childXe == null
                       ? new XElement[0]
                       : childXe.Elements();
        }

        /// <summary>
        /// Determines whether to lists of XElement are equal.
        /// </summary>
        public static bool IsEqualTo(this IList<XElement> me, IList<XElement> other)
        {
            if (me == null && other == null)
                return true;
            if (me == null || other == null)
                return false;
            if (me.Count != other.Count)
                return false;
            var equalityComparer = new XNodeEqualityComparer();
            for (var i = 0; i < me.Count; i++)
            {
                if (!equalityComparer.Equals(me[i], other[i]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Retrieves the value of the attribute <paramref name="attName"/> of
        /// <paramref name="xe"/>.
        /// </summary>
        /// <typeparam name="T">The data type of this value</typeparam>
        /// <param name="xe">The element whose attribute value is to be retrieved</param>
        /// <param name="attName">The name of the attribute to be retrieve</param>
        /// <returns>The value of <paramref name="attName"/>.  Returns null if
        /// the attribute doesn't exist or if it contains empty value.</returns>
        public static T Attr<T>(this XElement xe, string attName)
        {
            return Attr(xe, attName, default(T));
        }

        /// <summary>
        /// Retrieves the value of the attribute <paramref name="attName"/> of
        /// <paramref name="xe"/>.
        /// </summary>
        /// <typeparam name="T">The data type of this value.  
        /// Support string, enum, nullable and primitive types.</typeparam>
        /// <param name="xe">The element whose attribute value is to be retrieved</param>
        /// <param name="attName">The name of the attribute to be retrieve</param>
        /// <param name="defaultValue">The default value to be used if the attribute
        /// doesn't exist or if it contains empty value</param>
        /// <returns>The value of <paramref name="attName"/>.  Returns <paramref name="defaultValue"/> 
        /// if the attribute doesn't exist or if it contains empty value.</returns>
        public static T Attr<T>(this XElement xe, string attName, T defaultValue)
        {
            var att = xe.Attribute(attName);
            var value = att == null ? null : att.Value;
            if (string.IsNullOrEmpty(value))
                return defaultValue;
            return (T)value.ConvertToType(typeof(T));
        }
    }
}