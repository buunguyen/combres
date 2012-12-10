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

using System.Web.Mvc;

namespace Combres.Mvc
{
    /// <summary>
    /// Utility class providing extension methods for ASP.NET MVC applications to
    /// work with the library.
    /// </summary>
    public static class MvcExtensions
    {
        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.CombresUrl"/>.
        /// </summary>
        public static MvcHtmlString CombresUrl(this HtmlHelper html, string setName)
        {
            return MvcHtmlString.Create(WebExtensions.CombresUrl(setName));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.CombresUrl"/>.
        /// </summary>
        public static MvcHtmlString CombresUrl(this UrlHelper url, string setName)
        {
            return MvcHtmlString.Create(WebExtensions.CombresUrl(setName));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.CombresLink(string)"/>.
        /// </summary>
        public static MvcHtmlString CombresLink(this HtmlHelper html, string setName)
        {
            return MvcHtmlString.Create(WebExtensions.CombresLink(setName));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.CombresLink(string)"/>.
        /// </summary>
        public static MvcHtmlString CombresLink(this UrlHelper url, string setName)
        {
            return MvcHtmlString.Create(WebExtensions.CombresLink(setName));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.CombresLink(string, object)"/>.
        /// </summary>
        public static MvcHtmlString CombresLink(this HtmlHelper html, string setName, object htmlAttributes)
        {
            return MvcHtmlString.Create(WebExtensions.CombresLink(setName, htmlAttributes));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.CombresLink(string, object)"/>.
        /// </summary>
        public static MvcHtmlString CombresLink(this UrlHelper url, string setName, object htmlAttributes)
        {
            return MvcHtmlString.Create(WebExtensions.CombresLink(setName, htmlAttributes));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.EnableClientUrls"/>.
        /// </summary>
        public static MvcHtmlString EnableClientUrls(this HtmlHelper html, params string[] setNames)
        {
            return MvcHtmlString.Create(WebExtensions.EnableClientUrls(setNames));
        }

        /// <summary>
        /// Forwards the call to <see cref="WebExtensions.EnableClientUrls"/>.
        /// </summary>
        public static MvcHtmlString EnableClientUrls(this UrlHelper url, params string[] setNames)
        {
            return MvcHtmlString.Create(WebExtensions.EnableClientUrls(setNames));
        }
    }
}
