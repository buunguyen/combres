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

using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

namespace Combres.Filters
{
    /// <summary>
    /// Enables variable support in CSS. 
    /// Credit to Rory Neopoleon (http://neopoleon.com/home/blogs/neo/archive/2004/03/06/8705.aspx)
    /// for the original idea/code.
    /// </summary>
    /// <example>
    /// Specifically, this filter allows you to have a CSS file like this:
    /// <code>
    /// @define
    /// {
    ///     boxColor: #345131;
    ///     boxWidth: 150px;
    /// }
    /// p
    /// {
    ///     color: @boxColor;
    ///     width: @boxWidth;
    /// }
    /// </code>
    /// 
    /// The filter will make it becomes the following:
    /// <code>
    /// p
    /// {
    ///     color: #345131;
    ///     width: 150px;
    /// }
    /// </code>
    /// </example>
    public sealed class HandleCssVariablesFilter : ISingleContentFilter
    {
        /// <inheritdoc cref="IContentFilter.CanApplyTo" />
        public bool CanApplyTo(ResourceType resourceType)
        {
            return resourceType == ResourceType.CSS;
        }

        /// <inheritdoc cref="ISingleContentFilter.TransformContent" />
        public string TransformContent(ResourceSet resourceSet, Resource resource, string content)
        {
            // Remove comments because they may mess up the result
            content = Regex.Replace(content, @"/\*.*?\*/", string.Empty, RegexOptions.Singleline);
            var regex = new Regex(@"@define\s*{(?<define>.*?)}", RegexOptions.Singleline);
            var match = regex.Match(content);
            if (!match.Success)
                return content;

            var value = match.Groups["define"].Value;
            var variables = value.Split(';');
            var sb = new StringBuilder(content);
            variables.ToList().ForEach(variable =>
                          {
                             if (string.IsNullOrEmpty(variable.Trim()))
                                 return;
                             var pair = variable.Split(':');
                             sb.Replace("@" + pair[0].Trim(), pair[1].Trim());
                          });

            // Remove the variables declaration, it's not needed in the final output
            sb.Replace(match.ToString(), string.Empty);
            return sb.ToString();
        }
    }
}
