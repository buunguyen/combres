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
    /// Filter to transform the compressed version of all files in a specific resource set.
    /// </summary>
    /// <seealso cref="DefaultProcessingWorkflow"/>
    /// <seealso cref="DebugProcessingWorkflow"/>
    public interface ICompressedContentFilter : IContentFilter
    {
        /// <summary>
        /// Transforms the compressed version of all files in a specific resource set.
        /// </summary>
        /// <param name="resourceSet">The resource set whose compressed combined content is being transformed.</param>
        /// <param name="content">The compressed combined content of all files in the resource set.</param>
        /// <returns>The transformed compressed combined content of all files in the resource set.</returns>
        byte[] TransformContent(ResourceSet resourceSet, byte[] content);
    }
}
