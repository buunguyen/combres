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
    /// <p>JavaScript minifier which delegates the minification process to the MS Ajax Minifier library (http://aspnet.codeplex.com/Release/ProjectReleases.aspx?ReleaseId=34488).
    /// Configuration settings are internally bound to <c>Microsoft.Ajax.Utilities.CodeSettings</c>.
    /// Check out MS Ajax Minifier documentation for details about these configuration settings.</p>
    /// <see cref="Microsoft.Ajax.Utilities.CodeSettings"/>.
    /// Note that this class only exposes some common properties from <c>Microsoft.Ajax.Utilities.CodeSettings</c>,
    /// if you want more, create your own <see cref="IResourceMinifier"/>.
    /// </summary>
    public sealed class MSAjaxJSMinifier : IResourceMinifier
    {
        /// <summary>
        /// Convert <c>new Object()</c> to <c>{}</c>, <c>new Array()</c> to <c>[]</c>, 
        /// <c>new Array(1,2,3,4,5)</c> to <c>[1,2,3,4,5]</c>, and <c>new Array("foo")</c> becomes <c>["foo"]</c>. 
        /// 
        /// Default is <c>true</c>.
        /// </summary>
        public bool? CollapseToLiteral { get; set; }

        /// <summary>
        /// One of: Ignore, MakeImmediateSafe or MakeAllSafe.
        /// </summary>
        public string EvalTreatment { get; set; }

        /// <summary>
        /// There was one quirk that Safari on the Mac (not the PC) needed that we were crunching out: 
        /// throw statements always seem to require a terminating semicolon. 
        /// Another Safari-specific quirk is that an if-statement only contains a function declaration, 
        /// Safari will throw a syntax error if the declaration isn’t surrounded with curly-braces. 
        /// Basically, if you want your code to always work in Safari, set this to true. 
        /// If you don’t care about Safari, it might save a few bytes. 
        /// 
        /// Default is <c>true</c>.
        /// </summary>
        public bool? MacSafariQuirks { get; set; }

        /// <summary>
        /// Renaming of locals. There are a couple settings: 
        /// - KeepAll is the default and doesn’t rename variables or functions at all. 
        /// - CrunchAll renames everything it can. 
        /// - KeepLocalizationVars renames everything it can except for variables starting with L_. 
        ///   Those are left as-is so localization efforts can continue on the crunched code. 
        /// </summary>
        public string LocalRenaming { get; set; }

        /// <summary>
        /// <c>SingleLine</c> crunches everything to a single line. 
        /// <c>MultipleLines</c> breaks the crunched code into multiple lines for easier reading.
        /// 
        /// Default is <c>SingleLine</c>.
        /// </summary>
        public string OutputMode { get; set; }

        /// <summary>
        /// Removes unreferenced local functions (not global functions, though), 
        /// unreferenced function parameters, quotes around object literal field names 
        /// that won’t be confused with reserved words, and it does some interesting things 
        /// with switch statements.
        /// 
        /// Default is <c>true</c>.
        /// </summary>
        public bool? RemoveUnneededCode { get; set; }

        /// <summary>
        /// Removes "debugger" statements, any calls into certain namespaces like 
        /// $Debug, Debug, Web.Debug or Msn.Debug. also strips calls to the WAssert function.
        /// 
        /// Default is <c>true</c>.
        /// </summary>
        public bool? StripDebugStatements { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            var localRenaming = (LocalRenaming)LocalRenaming.ConvertToType(
                typeof(LocalRenaming), Microsoft.Ajax.Utilities.LocalRenaming.CrunchAll);

            var outputMode = (OutputMode)OutputMode.ConvertToType(
                typeof(OutputMode), Microsoft.Ajax.Utilities.OutputMode.SingleLine);

            var evalTreatment = (EvalTreatment)EvalTreatment.ConvertToType(
                typeof(EvalTreatment), Microsoft.Ajax.Utilities.EvalTreatment.MakeAllSafe);

            var codeSettings = new CodeSettings
                                   {
                                       EvalTreatment = evalTreatment,
                                       MacSafariQuirks = MacSafariQuirks == null ? true : MacSafariQuirks.Value,
                                       CollapseToLiteral = CollapseToLiteral == null ? true : CollapseToLiteral.Value,
                                       LocalRenaming = localRenaming,
                                       OutputMode = outputMode,
                                       RemoveUnneededCode = RemoveUnneededCode == null ? true : RemoveUnneededCode.Value,
                                       StripDebugStatements = StripDebugStatements == null ? true : StripDebugStatements.Value
                                   };
            return new Minifier().MinifyJavaScript(combinedContent, codeSettings);
        }
    }
}
