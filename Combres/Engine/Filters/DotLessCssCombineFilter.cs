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

using System.Collections.Generic;
using dotless.Core;
using dotless.Core.configuration;

namespace Combres.Filters
{
    /// <summary>
    /// Filter which applies .less CSS rules (see http://www.dotlesscss.com/) to a combined group of files.
    /// </summary>
    public sealed class DotLessCssCombineFilter : ICombinedContentFilter
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

        /// <inheritdoc cref="ICombinedContentFilter.TransformContent" />
        public string TransformContent(ResourceSet resourceSet, IEnumerable<Resource> resources, string content)
        {
            return EngineFactory.GetEngine().TransformToCss(content, null);
        }
    }
}
