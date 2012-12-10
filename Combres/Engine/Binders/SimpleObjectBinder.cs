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
using System.Xml.Linq;
using Combres.Minifiers;
using Fasterflect;

namespace Combres.Binders
{
    /// <summary>
    /// This binder supports setting values to properties of an object.
    /// Only properties are supported, fields are *not* supported.
    /// 
    /// Nested properties are supposed although complex types need to 
    /// have no-argumemt constructor.
    /// 
    /// See code of <see cref="MSAjaxJSMinifier"/>, <see cref="YuiCssMinifier"/>, 
    /// and <see cref="YuiJSMinifier"/> for example usages of this binder.
    /// </summary>
    public sealed class SimpleObjectBinder : IObjectBinder
    {
        private static readonly Flags DefaultFlags = Flags.InstanceAnyVisibility | Flags.IgnoreCase;

        /// <inheritdoc cref="IObjectBinder.Bind" />
        public void Bind(IEnumerable<XElement> parameters, object instance)
        {
            BindInternal(parameters, ref instance);
        }

        private static void BindInternal(IEnumerable<XElement> parameters, ref object instance)
        {
            foreach (var parameter in parameters)
            {
                var name = parameter.Attr<string>("name");
                var typeName = parameter.Attr<string>("type");
                var value = parameter.Attr<string>("value");
                if (typeName != null && value != null) // simple type
                {
                    var type = GetType(typeName);
                    instance.SetPropertyValue(name, value.ConvertToType(type), DefaultFlags);
                }
                else // complex type
                {
                    var prop = instance.GetPropertyValue(name, DefaultFlags);
                    if (prop == null) // current property is null, initialize it by invoking ctor
                    {
                        var propertyType = GetPropertyType(typeName, instance, name);
                        prop = propertyType.CreateInstance(); // Assume no-arg constructor
                    }
                    BindInternal(parameter.Elements(), ref prop);

                    // Set back, in case of newly created instance or struct type
                    instance.SetPropertyValue(name, prop, DefaultFlags);
                }
            }
        }

        private static Type GetPropertyType(string typeName, object instance, string name)
        {
            return typeName == null
                   ? instance.GetType().GetProperty(name, DefaultFlags).PropertyType
                   : GetType(typeName);
        }

        private static Type GetType(string name)
        {
            return PrimitiveMap.ContainsKey(name) 
                ? PrimitiveMap[name] 
                : Type.GetType(name);
        }

        private static readonly IDictionary<string, Type> PrimitiveMap = new Dictionary<string, Type>
                                                                        {
                                                                            {"byte", typeof(byte)},
                                                                            {"short", typeof(short)},
                                                                            {"ushort", typeof(ushort)},
                                                                            {"int", typeof(int)},
                                                                            {"uint", typeof(uint)},
                                                                            {"long", typeof(long)},
                                                                            {"ulong", typeof(ulong)},
                                                                            {"bool", typeof(bool)},
                                                                            {"float", typeof(float)},
                                                                            {"double", typeof(double)},
                                                                            {"decimal", typeof(decimal)},
                                                                            {"char", typeof(char)},
                                                                            {"string", typeof(string)},
                                                                        };
    }
}
