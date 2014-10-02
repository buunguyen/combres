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

using com.google.javascript.jscomp;
using java.util.logging;
using System;

namespace Combres.Minifiers
{
    /// <summary>
    /// JavaScript minifier which uses the Closure compiler library to minify JavaScript.
    /// </summary>
    public sealed class LocalClosureJSMinifier : IResourceMinifier
    {
        /// <summary>
        /// Compilation level, either ADVANCED_OPTIMIZATIONS, SIMPLE_OPTIMIZATIONS or WHITESPACE_ONLY.
        /// Check out this webpage for more information: http://code.google.com/closure/compiler/docs/api-tutorial3.html.
        /// Default is SIMPLE_OPTIMIZATIONS.
        /// </summary>
        public string CompilationLevel { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            string compilationLevel = string.IsNullOrEmpty(CompilationLevel) ? "SIMPLE_OPTIMIZATIONS" : CompilationLevel;
            Compiler compiler = null;
            CompilerOptions options = null;

            MinifyInit(ref compiler, ref options, compilationLevel);
            return OutputMinifiedContent(compiler, options, combinedContent);
        }

        /// <summary>
        /// Init Google Closure Compiler (JavaScript)
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="compilerOptions"></param>
        /// <param name="compilationLevel">"WHITESPACE_ONLY" | "SIMPLE_OPTIMIZATIONS" | "ADVANCED_OPTIMIZATIONS"</param>
        private void MinifyInit(ref Compiler compiler, ref CompilerOptions compilerOptions, string compilationLevel)
        {
            try
            {
                compiler = new Compiler();
                compiler.disableThreads();
                Compiler.setLoggingLevel(Level.OFF);

                compilerOptions = new CompilerOptions();
                compilerOptions.setLineBreak(false);
                compilerOptions.setLineLengthThreshold(524288);
                switch (compilationLevel)
                {
                    case "WHITESPACE_ONLY":
                        com.google.javascript.jscomp.CompilationLevel.WHITESPACE_ONLY.setOptionsForCompilationLevel(compilerOptions);
                        break;
                    case "SIMPLE_OPTIMIZATIONS":
                        com.google.javascript.jscomp.CompilationLevel.SIMPLE_OPTIMIZATIONS.setOptionsForCompilationLevel(compilerOptions);
                        break;
                    case "ADVANCED_OPTIMIZATIONS":
                        com.google.javascript.jscomp.CompilationLevel.ADVANCED_OPTIMIZATIONS.setOptionsForCompilationLevel(compilerOptions);
                        break;
                    default:
                        com.google.javascript.jscomp.CompilationLevel.WHITESPACE_ONLY.setOptionsForCompilationLevel(compilerOptions);
                        break;
                }
                WarningLevel.DEFAULT.setOptionsForWarningLevel(compilerOptions);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Return minifiedContent
        /// </summary>
        /// <param name="compiler"></param>
        /// <param name="compilerOptions"></param>
        /// <param name="inputFile"></param>
        /// <returns></returns>
        private string OutputMinifiedContent(Compiler compiler, CompilerOptions compilerOptions, string combinedContent)
        {
            try
            {
                SourceFile @extern = SourceFile.fromCode("extern", string.Empty);
                SourceFile input = SourceFile.fromCode("input", combinedContent);
                Result result = compiler.compile(@extern, input, compilerOptions);
                input.clearCachedSource();
                input = null;
                return result.success ? compiler.toSource() : string.Empty;
            }
            catch (Exception)
            {
                compiler = null;
                compilerOptions = null;
                return string.Empty;
            }
        }
    }
}