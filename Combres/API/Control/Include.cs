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
using System.ComponentModel;
using System.Web.UI;

namespace Combres
{
    /// <summary>
    /// Server control which can be used in place of inline code to include
    /// Combres when inline code isn't allowed (e.g. CMS environment).
    /// </summary>
    [DefaultPropertyAttribute("SetName")]
    [ToolboxData("<{0}:Include runat=\"server\" SetName=\"\" />")]
    public class Include : Control
    {
        /// <summary>
        /// The resource set name.
        /// </summary>
        [BrowsableAttribute(true)]
        [DescriptionAttribute("Get or set the resource set name")]
        [Category("Data")]
        [DefaultValue("")]
        public string SetName { get; set; }

        /// <summary>
        /// The HTML attributes to be appended within the generated tag.
        /// </summary>
        [BrowsableAttribute(true)]
        [DescriptionAttribute("Get or set the HTML attributes to be appended within the generated tag")]
        [Category("Data")]
        [DefaultValue("")]
        public string HtmlAttributes { get; set; }

        protected override void Render(HtmlTextWriter writer)
        {
            if (string.IsNullOrEmpty(HtmlAttributes))
            {
                writer.Write(WebExtensions.CombresLink(SetName));
            }
            else
            {
                var attributes = new Dictionary<string, string>();
                var array = HtmlAttributes.Split('|');
                for (int i = 0; i < array.Length; i += 2)
                {
                    attributes[array[i]] = array[i + 1];
                }
                writer.Write(WebExtensions.CombresLink(SetName, attributes));
            }
        }
    }
}
