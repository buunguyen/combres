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
using System.Xml.Linq;
using System.Xml.Schema;

namespace Combres
{
    internal static class ModelUtils
    {
        public static Type LoadType(string itemName, string typeName, IEnumerable<Type> expectedTypes)
        {
            var type = Type.GetType(typeName);
            if (type == null)
                throw new XmlSchemaException(string.Format(CultureInfo.InvariantCulture,
                                                           "{0} type {1} cannot be found", itemName, typeName));
            if (!expectedTypes.Any(validType => validType.IsAssignableFrom(type)))
                throw new XmlSchemaException(string.Format(CultureInfo.InvariantCulture,
                                                           "{0} {1} doesn't implement/extend the correct interface/class",
                                                           itemName, typeName));
            return type;
        }

        public static MinifierInfo LoadMinifier(XElement xe, string attName, string defaultMinifierRef, IDictionary<string, MinifierInfo> map)
        {
            var minifierRef = GetString(xe, attName, defaultMinifierRef);
            if (!map.ContainsKey(minifierRef))
                throw new XmlSchemaException(string.Format(CultureInfo.InvariantCulture,
                    "Minifier {0} is not declared", minifierRef));
            return map[minifierRef];
        }


        public static string GetString(XElement xe, string attrName, string parentDefault)
        {
            var value = xe.Attr<string>(attrName);
            if (string.IsNullOrEmpty(value))
            {
                if (parentDefault == null)
                    throw new XmlSchemaException(attrName + " must be specified if parent's default is not specified");
                return parentDefault;
            }
            return value;
        }

        public static IDictionary<TKey, TValue> AsReadonly<TKey, TValue>(this IDictionary<TKey, TValue> dictionaryToWrap)
        {
            return new ReadOnlyDictionary<TKey, TValue>(dictionaryToWrap);
        }
    }
}
