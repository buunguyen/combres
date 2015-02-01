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

using System.Collections.Generic;
using System.Linq;

namespace Combres.VersionGenerators
{
    /// <summary>
    /// Use the inherent <c>GetHashCode()</c> on all contributing change factors (resources' contents/mode/cookie/minifier and 
    /// resource set's applied filters/debug-enabled) to generate version string.
    /// </summary>
    /// <remarks>Resulted version string has the form of an integer.  It doesn't
    /// guarantee no collision for 2 different resource sets.
    /// </remarks>
    public sealed class HashCodeVersionGenerator : IVersionGenerator
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
                       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <inheritdoc cref="IVersionGenerator.Generate" />
        public string Generate(ResourceSet rs)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Computing hash for set " + rs.Name + ".  Current hash: " + rs.Hash);

            var contributingFactors = new List<object> {rs.DebugEnabled};
            rs.Filters.ToList().ForEach(contributingFactors.Add);
            rs.CacheVaryProviders.ToList().ForEach(contributingFactors.Add);
            rs.Resources.ToList().ForEach(r =>
                                  {
                                      contributingFactors.Add(r.ReadFromCache(true));
                                      contributingFactors.Add(r.ForwardCookie);
                                      contributingFactors.Add(r.Mode);
                                      contributingFactors.Add(r.Minifier);
                                  });
            var hash = contributingFactors.Select(f => f.GetHashCode())
                                          .Aggregate(17, (accum, element) => 31 * accum + element)
                                          .ToString();

            if (Log.IsDebugEnabled)
                Log.Debug("New hash: " + hash);
            return hash;
        }
    }
}
