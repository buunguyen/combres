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
using System.Diagnostics;
using System.IO;
using System.Web.Hosting;

namespace Combres.Minifiers
{
    /// <summary>
    /// JavaScript minifier which uses the Closure compiler library to minify JavaScript.
    /// JDK must be installed in the web server in order to use this minifier.
    /// </summary>
    public sealed class LocalClosureJSMinifier : IResourceMinifier
    {
        /// <summary>
        /// Compilation level, either ADVANCED_OPTIMIZATIONS, SIMPLE_OPTIMIZATIONS or WHITESPACE_ONLY.
        /// Check out this webpage for more information: http://code.google.com/closure/compiler/docs/api-tutorial3.html.
        /// Default is SIMPLE_OPTIMIZATIONS.
        /// </summary>
        public string CompilationLevel { get; set; }

        /// <summary>
        /// The absolute path to the java.exe program.  This can be left empty if 
        /// the program's folder is already included in the PATH user/system variable.
        /// </summary>
        public string JavaExePath { get; set; }

        /// <inheritdoc cref="IResourceMinifier.Minify" />
        public string Minify(Settings settings, ResourceSet resourceSet, string combinedContent)
        {
            return Utils.UseTempFile(combinedContent, new Func<string, string>(Compile));
        }

        private string Compile(string inputFile)
        {
            return Utils.UseTempFile(outputFile =>
                {
                    const string argsTemplate = @"-jar ""{0}"" --compilation_level {1} --js ""{2}"" --js_output_file ""{3}""";
                    string javaPath = string.IsNullOrEmpty(JavaExePath)
                        ? "java.exe"
                        : JavaExePath;
                    string compilerPath = HostingEnvironment.MapPath("~/bin") + "\\compiler.jar";
                    string compilationLevel = string.IsNullOrEmpty(CompilationLevel)
                        ? "SIMPLE_OPTIMIZATIONS"
                        : CompilationLevel;
                    var args = string.Format(argsTemplate, compilerPath     /* {0} */,
                                                           compilationLevel /* {1} */,
                                                           inputFile        /* {2} */,
                                                           outputFile       /* {3} */);
                    var startInfo = new ProcessStartInfo
                    {
                        FileName = javaPath,
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Hidden
                    };
                    Process.Start(startInfo).WaitForExit();
                    return File.ReadAllText(outputFile);
                });
        }
    }
}