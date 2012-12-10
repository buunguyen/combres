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

using System.Linq;
using System.Text;

namespace Combres.VersionGenerators
{
    /// <summary>
    /// Applies SHA512 algorithm on all contributing change factors (resources' contents/mode/cookie/minifier and 
    /// resource set's applied filters/debug-enabled) to generate version string.
    /// </summary>
    /// <remarks>
    /// <p>SHA512 generates 64-bit hash thus minimizes collision.  However, it doesn't
    /// guarantee no collision for 2 different resource sets.</p>
    /// </remarks>
    public sealed class Sha512VersionGenerator : IVersionGenerator
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
                       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc cref="IVersionGenerator.Generate" />
        public string Generate(ResourceSet rs)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Computing hash for set " + rs.Name + ".  Current hash: " + rs.Hash);

            var contributingFactors = new StringBuilder(rs.DebugEnabled.ToString());
            rs.Filters.ToList().ForEach(f => contributingFactors.Append(f.GetHashCode().ToString()));
            rs.CacheVaryProviders.ToList().ForEach(f => contributingFactors.Append(f.GetHashCode().ToString()));
            rs.Resources.ToList().ForEach(r =>
            {
                contributingFactors.Append(r.ReadFromCache(true));
                contributingFactors.Append(r.ForwardCookie.ToString());
                contributingFactors.Append(r.Mode.GetHashCode().ToString());
                contributingFactors.Append(r.Minifier.GetHashCode().ToString());
            });
            var hash = contributingFactors.ToString().GetHash();

            if (Log.IsDebugEnabled)
                Log.Debug("New hash: " + hash);
            return hash;
        }
    }
}
