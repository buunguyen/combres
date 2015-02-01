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

using Microsoft.Ajax.Utilities;

namespace Combres.Minifiers
{
    /// <summary>
    /// <p>CSS minifier which delegates the minification process to the MS Ajax Minifier library (http://aspnet.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=34488).
    /// Configuration settings are internally bound to <c>Microsoft.Ajax.Utilities.CssSettings</c>.
    /// Check out MS Ajax Minifier documentation for details about these configuration settings.
    /// Note that this class only exposes some common properties from <c>Microsoft.Ajax.Utilities.CssSettings</c>,
    /// if you want more, create your own <see cref="IResourceMinifier"/>.</p>
    /// <see cref="Microsoft.Ajax.Utilities.CodeSettings"/>
    /// </summary>
    public sealed class MSAjaxCssMinifier : IResourceMinifier
    {
        /// <summary>
        /// <c>SingleLine</c> crunches everything to a single line. 
        /// <c>MultipleLines</c> breaks the crunched code into multiple lines for easier reading.
        /// 
        /// Default is <c>SingleLine</c>.
        /// </summary>
        public string OutputMode { get; set; }

        /// <summary>
        /// Whether to minify CSS expressions or not.
        /// Default is true.
        /// </summary>
        public bool? MinifyExpressions { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            var outputMode = (OutputMode)OutputMode.ConvertToType(
                typeof(OutputMode), Microsoft.Ajax.Utilities.OutputMode.SingleLine);
            var codeSettings = new CssSettings()
            {
                OutputMode = outputMode,
                MinifyExpressions = MinifyExpressions == null ? true : MinifyExpressions.Value,
            };
            return new Minifier().MinifyStyleSheet(combinedContent, codeSettings);
        }
    }
}
