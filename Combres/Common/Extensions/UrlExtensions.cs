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
using System.Web;

namespace Combres
{
    /// <summary>
    /// Utility class providing methods to convert from relative ASP.NET URLs to absolute URLs.
    /// Refactored from original code at http://www.west-wind.com/Weblog/posts/154812.aspx.
    /// </summary>
    internal static class UrlExtensions
    {
        /// <summary>
        /// Returns an absolute URL from a relative server path starting with ~ character.
        /// If the path is already an absolute path, it is returned to the caller.
        /// </summary>
        public static string ToAbsoluteUrl(this string serverUrl)
        {
            return ToAbsoluteUrl(serverUrl, false);
        }

        /// <summary>
        /// Returns an absolute URL from a relative server path starting with ~ character.
        /// If <paramref name="forceHttps"/> is true, return HTTPS URL regardless of the
        /// HTTPS status of the current web request.
        /// </summary>
        public static string ToAbsoluteUrl(this string serverUrl, bool forceHttps)
        {
            if (string.IsNullOrEmpty(serverUrl))
                return serverUrl;
            if (IsAbsoluteUrl(serverUrl))
                return serverUrl;

            var adjustedServerUrl = ResolveUrl(serverUrl);
            Uri baseUrl = HttpContext.Current.GetPublicFacingBaseUrl();
            var absoluteUrl = new Uri(baseUrl, adjustedServerUrl);
            return forceHttps
                ? absoluteUrl.ToHttps().ToString()
                : absoluteUrl.ToString();
        }

        /// <summary>
        /// In Azure, <c>HttpRequest.Url</c> returns the internal URL, so
        /// we'll attempt the appropriate host headers first, which are supposed
        /// to not to be modified by the Azure's load balancer/proxy.
        /// </summary>
        public static Uri GetPublicFacingBaseUrl(this HttpContext context)
        {
            var request = context.Request;
            var headers = request.ServerVariables;
            if (headers["HTTP_HOST"] != null)
            {
                var scheme = headers["HTTP_X_FORWARDED_PROTO"] ?? request.Url.Scheme;
                return new Uri(scheme + Uri.SchemeDelimiter + headers["HTTP_HOST"]);
            }
            return request.Url;
        }

        /// <summary>
        /// Mock an <see cref="HttpContext"/> object with the base URL, together with appropriate
        /// host headers so that code that doesn't operate under an <see cref="HttpContext"/>
        /// can still use one in replacement.
        /// </summary>
        public static HttpContext FakeHttpContext(this HttpContext realContext)
        {
            var baseUrl = realContext.GetPublicFacingBaseUrl();
            var request = new HttpRequest(string.Empty, baseUrl.ToString(), string.Empty);
            var response = new HttpResponse(null);
            return new HttpContext(request, response);
        }

        /// <summary>
        /// Returns the relative HTTP path from a partial path starting out with a ~ character
        /// or the original URL if it's an absolute or relative URL that doesn't start with ~.
        /// </summary>
        public static string ResolveUrl(this string originalUrl)
        {
            if (string.IsNullOrEmpty(originalUrl) ||
                    IsAbsoluteUrl(originalUrl) ||
                    !originalUrl.StartsWith("~", StringComparison.Ordinal))
                return originalUrl;

            /* 
             * Fix up path for ~ root app dir directory
             * VirtualPathUtility blows up if there is a 
             * query string, so we have to account for this.
             */
            var queryStringStartIndex = originalUrl.IndexOf('?');
            string result;
            if (queryStringStartIndex != -1)
            {
                var baseUrl = originalUrl.Substring(0, queryStringStartIndex);
                var queryString = originalUrl.Substring(queryStringStartIndex);
                result = string.Concat(VirtualPathUtility.ToAbsolute(baseUrl), queryString);
            }
            else
            {
                result = VirtualPathUtility.ToAbsolute(originalUrl);
            }

            return result.StartsWith("/", StringComparison.Ordinal)
                       ? result
                       : "/" + result;
        }

        private static Uri ToHttps(this Uri uri)
        {
            var builder = new UriBuilder(uri)
            {
                Scheme = Uri.UriSchemeHttps,
                Port = 443
            };
            return builder.Uri;
        }

        private static bool IsAbsoluteUrl(string url)
        {
            int indexOfSlashes = url.IndexOf("://", StringComparison.Ordinal);
            int indexOfQuestionMarks = url.IndexOf("?", StringComparison.Ordinal);

            /*
             * This has :// but still NOT an absolute path:
             * ~/path/to/page.aspx?returnurl=http://www.my.page
             */
            return indexOfSlashes > -1
                && (indexOfQuestionMarks < 0 || indexOfQuestionMarks > indexOfSlashes);
        }

        /// <summary>
        /// Returns the base URL of the specified <paramref name="uri"/> object.
        /// </summary>
        /// <param name="uri">The object whose base URL is to be retrieved.</param>
        /// <returns>The base URL of the specified <paramref name="uri"/> object.</returns>
        public static string GetBase(this Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Authority); 
        }
    }
}
