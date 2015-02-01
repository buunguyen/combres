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
using System.Xml.Linq;
using Combres.Binders;
using Combres.Minifiers;
using Combres.VersionGenerators;

namespace Combres
{
    /// <summary>
    /// Class containing default values for various configuration settings.
    /// </summary>
    internal static class Default
    {
        public static class ResourceSet
        {
            public static readonly bool DebugEnabled = false;
            public static readonly bool IgnorePipelineWhenDebug = false;
            public static readonly Type VersionGeneratorType = typeof (HashCodeVersionGenerator);
            public static readonly bool CompressionEnabled = true;
        }

        public static class Resource
        {
            public static readonly bool ForwardCookie = false;
            public static readonly ResourceMode Mode = ResourceMode.Static;
        }

        public static class Binder
        {
            public static readonly Type Type = typeof(SimpleObjectBinder);
        }

        public static class Minifier
        {
            private static readonly XElement[] EmptyParamList = new XElement[0];

            public static readonly MinifierInfo Off = new MinifierInfo
                                                    {
                                                        Name = "off",
                                                        Type = typeof(NullMinifier),
                                                        BinderType = Binder.Type,
                                                        Parameters = EmptyParamList
                                                    };

            public static readonly MinifierInfo JS = new MinifierInfo
                                                    {
                                                        Name = "default",
                                                        Type = typeof(YuiJSMinifier),
                                                        BinderType = Binder.Type,
                                                        Parameters = EmptyParamList
                                                    };

            public static readonly MinifierInfo Css = new MinifierInfo
                                                    {
                                                        Name = "default",
                                                        Type = typeof(YuiCssMinifier),
                                                        BinderType = Binder.Type,
                                                        Parameters = EmptyParamList
                                                    };
        }
    }
}
