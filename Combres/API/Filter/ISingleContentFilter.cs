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

namespace Combres
{
    /// <summary>
    /// Filter to transform the content of a single resource file.
    /// </summary>
    /// <seealso cref="DefaultProcessingWorkflow"/>
    /// <seealso cref="DebugProcessingWorkflow"/>
    public interface ISingleContentFilter : IContentFilter
    {
        /// <summary>
        /// Transforms the content of a single resource file.
        /// </summary>
        /// <param name="resourceSet">The resource set being worked on.</param>
        /// <param name="resource">The resource whose content is being transformed.</param>
        /// <param name="content">The content of the resource.</param>
        /// <returns>The transformed content of the resource.</returns>
        string TransformContent(ResourceSet resourceSet, Resource resource, string content);
    }
}
