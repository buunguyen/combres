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
using System.Globalization;
using System.Text.RegularExpressions;

namespace Combres.Filters
{
    /// <summary>
    /// Filter which modifies relative urls to absolute urls (works for both virtual directory 
    /// as well as website).
    /// 
    /// Assume the path of the CSS file is ~/content/site.css, then the following transformations
    /// are done to each url reference in the CSS file:
    /// <list type="table">
    /// <listheader>
    ///     <term>Source</term>
    ///     <description>Effect</description>
    /// </listheader>
    /// <item>
    ///     <term>Absolute path (e.g. /path/to/image.gif)</term>
    ///     <description>Keep as-is (e.g. /path/to/image.gif)</description>
    /// </item>
    /// <item>
    ///     <term>Relative from CSS location (e.g. path/to/image.gif, ../path/to/style1.css)</term>
    ///     <description>Make relative to application root (e.g. /content/path/to/imag2.gif, /path/to/style1.css)</description>
    /// </item>
    /// <item>
    ///     <term>Starting from application root (e.g. ~/path/to/style2.css)</term>
    ///     <description>Replace ~ with application root (e.g. /content/path/to/style2.css)</description>
    /// </item>
    /// </list>
    /// </summary>
    /// <remarks>
    /// See http://combres.codeplex.com/Thread/View.aspx?ThreadId=64366 for error description that leads
    /// to the implementation of this filter.
    /// </remarks>
    public sealed class FixUrlsInCssFilter : ISingleContentFilter
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
                       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc cref="IContentFilter.CanApplyTo" />
        public bool CanApplyTo(ResourceType resourceType)
        {
            return resourceType == ResourceType.CSS;
        }

        /// <inheritdoc cref="ISingleContentFilter.TransformContent" />
        public string TransformContent(ResourceSet resourceSet, Resource resource, string content)
        {
            const RegexOptions regexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.ExplicitCapture;
            string importsFixed = Regex.Replace(content, @"@import\s+(?<url>.*?)\s*;", FixImport, regexOptions);
            return Regex.Replace(importsFixed, @"url\((?<url>.*?)\)", match => FixUrl(resource, match), regexOptions);
        }

        private string FixImport(Match match)
        {
            const string template = "@import {0};";
            var url = match.Groups["url"].Value;
            if (!url.StartsWith("url", StringComparison.InvariantCultureIgnoreCase))
                url = string.Format("url({0})", url);
            return string.Format(template, url);
        }

        private static string FixUrl(Resource resource, Match match)
        {
            try
            {
                const string template = "url(\"{0}\")";
                bool isInSameApp = resource.IsInSameApplication;
                var url = match.Groups["url"].Value.Trim('\"', '\'');

                // Return as-is if
                // - Absolute URL
                // - Data http://combres.codeplex.com/workitem/7654
                if (url.StartsWith("http", true, CultureInfo.InvariantCulture) ||
                    url.StartsWith("data:", true, CultureInfo.InvariantCulture))
                    return string.Format(CultureInfo.InvariantCulture, template, url);

                if (url.StartsWith("~", StringComparison.Ordinal))
                {
                    // The CSS is in the same application 
                    // resolve partial URLs found in the CSS to full relative paths
                    if (isInSameApp)
                        return string.Format(CultureInfo.InvariantCulture, template, url.ResolveUrl());
                    
                    /* Otherwise, attempt to treat ~ as /
                     * 
                     * @NOTE: This won't work if the remote app is in a virtual directory
                     * See my comment dated 11:00PM Monday Nov 23 in this discussion
                     * http://combres.codeplex.com/Thread/View.aspx?ThreadId=64366
                     */
                    url = "/" + url.Substring(2); // 2 for the "~/"
                }

                var cssPath = resource.Path;
                if (url.StartsWith("/", StringComparison.Ordinal))
                {
                    // The CSS is in the same application, keep root-based URLs as-is
                    if (isInSameApp)
                        return string.Format(CultureInfo.InvariantCulture, template, url);

                    // Otherwise, append root URL of the remote server/app to this url object
                    var uri = new Uri(cssPath);
                    return string.Format(CultureInfo.InvariantCulture, template, uri.GetBase() + url);
                }

                // Relative URL in CSS mean relative to the CSS location
                // Because CSS location must either be ~/ or absolute, the ResolveUrl() 
                // at the end of this code block will do 
                var cssFolder = cssPath.Substring(0, cssPath.LastIndexOf("/", StringComparison.Ordinal)); // e.g. ~/content/css
                while (url.StartsWith("../", StringComparison.Ordinal))
                {
                    url = url.Substring(3); // skip one '../'
                    cssFolder = cssFolder.Substring(0, cssFolder.LastIndexOf("/", StringComparison.Ordinal)); // move back one folder
                }

                return string.Format(CultureInfo.InvariantCulture, template, (cssFolder + "/" + url).ResolveUrl());
            }
            catch (Exception ex)
            {
                // Be lenient here, only log.  After all, this is just an image in the CSS file
                // and it should't be the reason to stop loading that CSS file.
                if (Log.IsWarnEnabled) 
                    Log.Warn("Cannot fix url " + match.Value, ex);
                return match.Value;
            }
        }
    }
}
