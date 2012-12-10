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

namespace Combres.Minifiers
{
    /// <summary>
    /// JavaScript minifier which delegates the minification process to the Google Closure Compiler (http://closure-compiler.appspot.com/).
    /// </summary>
    public sealed class ClosureJSMinifier : IResourceMinifier
    {
        /// <summary>
        /// The URL of the Closure REST API.
        /// </summary>
        public string ApiUrl { get; set; }

        /// <summary>
        /// Compilation level, either ADVANCED_OPTIMIZATIONS, SIMPLE_OPTIMIZATIONS or WHITESPACE_ONLY.
        /// Check out this webpage for more information: http://code.google.com/closure/compiler/docs/api-tutorial3.html.
        /// Default is SIMPLE_OPTIMIZATIONS.
        /// </summary>
        public string CompilationLevel { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            var level = (ClosureCodeRequest.CompilationLevel)CompilationLevel.ConvertToType(
                typeof(ClosureCodeRequest.CompilationLevel), 
                ClosureCodeRequest.CompilationLevel.SIMPLE_OPTIMIZATIONS);
            var request = new ClosureCodeRequest(ApiUrl, level, combinedContent);
            return request.GetCode();
        }
    }
}
