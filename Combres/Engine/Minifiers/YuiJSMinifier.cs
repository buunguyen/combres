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
using System.Globalization;
using System.Text;
using System.Xml.Linq;
using Yahoo.Yui.Compressor;

namespace Combres.Minifiers
{
    /// <summary>
    /// JavaScript minifier which delegates the minification process to the YUI Combression library (http://yuicompressor.codeplex.com/).
    /// </summary>
    public sealed class YuiJSMinifier : IResourceMinifier
    {
        /// <summary>
        /// Log informational messages and warnings.
        /// Default is false.
        /// </summary>   
        public bool? IsVerboseLogging { get; set; }

        /// <summary>
        /// If true, obfuscate JavaScript in addition to minification.  No obfuscation if false.
        /// Default is true.
        /// </summary>
        public bool? ObfuscateJavascript { get; set; }

        /// <summary>
        /// If true, ignores `eval`.
        /// Default is true.
        /// </summary>
        public bool? IgnoreEval { get; set; }

        /// <summary>
        /// Preserve unnecessary semicolons (such as right before a '}').  
        /// Default is false.
        /// </summary>
        public bool? PreserveAllSemicolons { get; set; }

        /// <summary>
        /// Disable all the built-in micro optimizations.
        /// Default is false.
        /// </summary>
        public bool? DisableOptimizations { get; set; }

        /// <summary>
        /// <p>Some source control tools don't like files containing lines longer than,
        /// say 8000 characters. The linebreak option is used in that case to split
        /// long lines after a specific column. It can also be used to make the code
        /// more readable, easier to debug.  Specify 0 to get a line break after 
        /// each semi-colon in JavaScript.</p>
        /// 
        /// <p>Default is no line break.</p>
        /// </summary>
        public int? LineBreakPosition { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            var compressor = new JavaScriptCompressor();
            compressor.Encoding = Encoding.UTF8;
            compressor.ThreadCulture = CultureInfo.InvariantCulture;
            compressor.LoggingType = IsVerboseLogging == true ? LoggingType.Debug : LoggingType.None;
            compressor.PreserveAllSemicolons = PreserveAllSemicolons == null ? false : PreserveAllSemicolons.Value;
            compressor.DisableOptimizations = DisableOptimizations == null ? false : DisableOptimizations.Value;
            compressor.LineBreakPosition = LineBreakPosition == null ? -1 : LineBreakPosition.Value;
            compressor.IgnoreEval = IgnoreEval == null ? true : IgnoreEval.Value;
            compressor.ObfuscateJavascript = ObfuscateJavascript == null ? true : ObfuscateJavascript.Value;
            return compressor.Compress(combinedContent);
        }
    }
}
