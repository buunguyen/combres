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

using dotless.Core;
using dotless.Core.configuration;

namespace Combres.Filters
{
    /// <summary>
    /// Filter which applies .less CSS rules (see http://www.dotlesscss.com/).
    /// </summary>
    public sealed class DotLessCssFilter : ISingleContentFilter
    {
        private static readonly DotlessConfiguration Config = new DotlessConfiguration
        {
            CacheEnabled = false,
            MinifyOutput = false
        };

        private static readonly EngineFactory EngineFactory = new EngineFactory(Config);

        /// <inheritdoc cref="IContentFilter.CanApplyTo" />
        public bool CanApplyTo(ResourceType resourceType)
        {
            return resourceType == ResourceType.CSS;
        }

        /// <inheritdoc cref="ISingleContentFilter.TransformContent" />
        public string TransformContent(ResourceSet resourceSet, Resource resource, string content)
        {
            return EngineFactory.GetEngine().TransformToCss(content, null);
        }
    }
}
