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
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using System.Xml.Linq;

namespace Combres
{
    internal sealed class ClosureCodeRequest
    {
        public enum CompilationLevel
        {
// ReSharper disable InconsistentNaming
            SIMPLE_OPTIMIZATIONS,
// ReSharper restore InconsistentNaming
// ReSharper disable InconsistentNaming
            ADVANCED_OPTIMIZATIONS,
// ReSharper restore InconsistentNaming
// ReSharper disable InconsistentNaming
            WHITESPACE_ONLY
// ReSharper restore InconsistentNaming
        }

        private const string DefaultApiUrl = "http://closure-compiler.appspot.com/compile";

        private readonly Request request;

        public ClosureCodeRequest(string apiUrl, CompilationLevel level, string jsContent)
        {
            apiUrl = string.IsNullOrEmpty(apiUrl) ? DefaultApiUrl : apiUrl;
            request = new Request(apiUrl,
                new List<KeyValuePair<string, string>>
                    {
                             new KeyValuePair<string, string>("compilation_level", level.ToString()),
                             new KeyValuePair<string, string>("js_code", jsContent),
                             new KeyValuePair<string, string>("output_format", "xml"),
                             new KeyValuePair<string, string>("output_info", "compiled_code"),
                        });
        }

        /// <summary>
        /// Sends the request and returns the optimized code.
        /// </summary>
        /// <returns>The optimized code</returns>
        public string GetCode()
        {
            var responseText = request.Send();
            var xe = XElement.Parse(responseText);
            var code = xe.Element("compiledCode");
            if (code != null)
                return code.Value;
            var error = xe.Element("serverErrors");
            throw new CombresException(error == null
                ? "Unknown error occurred when requesting code from Closure"
                : error.Value);
        }

        private sealed class Request
        {
            private readonly string url;
            private readonly IList<KeyValuePair<string, string>> data;

            public Request(string url, IList<KeyValuePair<string, string>> data)
            {
                if (url == null)
                    throw new ArgumentNullException("url");
                if (data == null)
                    data = new List<KeyValuePair<string, string>>();
                this.url = url;
                this.data = data;
            }

            /// <summary>
            /// Sends the POST request and returns the response from the server.
            /// </summary>
            /// <returns>The response from the server.</returns>
            public string Send()
            {
                var dataString = BuildPostData();
                using (var webClient = new WebClient())
                {
                    webClient.Headers.Add("content-type", "application/x-www-form-urlencoded");
                    return webClient.UploadString(url, dataString);
                }
            }

            private string BuildPostData()
            {
                var parameters = new StringBuilder();
                foreach (var item in data)
                {
                    if (parameters.Length != 0) parameters.Append("&");
                    parameters.Append(item.Key)
                              .Append("=")
                              .Append(HttpUtility.UrlEncode(item.Value));
                }
                return parameters.ToString();
            }
        }
    }
}
