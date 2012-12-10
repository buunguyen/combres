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

using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace Combres
{
    internal static class ConfigReader
    {
        internal static string ReadCombresUrl(string filePath)
        {
            var root = XDocument.Load(filePath).Root;
            var rs = root.Element(XName.Get(SchemaConstants.Setting.ResourceSets, SchemaConstants.Namespace));
            return rs.Attr<string>(SchemaConstants.Setting.Url);
        }

        internal static Settings Read(string filePath)
        {
            XDocument xdoc = LoadAndValidate(filePath);
            return CreateSettings(xdoc);
        }

        private static XDocument LoadAndValidate(string filePath)
        {
            var xdoc = XDocument.Load(filePath);
            var assembly = Assembly.GetExecutingAssembly();
            var xsdStream = assembly.GetManifestResourceStream(
                assembly.GetManifestResourceNames()[0]);
            xdoc.Validate(SchemaConstants.Namespace, XmlReader.Create(xsdStream));
            return xdoc;
        }

        private static Settings CreateSettings(XDocument xdoc)
        {
            return new Settings(xdoc.Root);
        }
    }
}
