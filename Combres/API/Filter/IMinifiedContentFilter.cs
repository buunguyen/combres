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

namespace Combres
{
    /// <summary>
    /// Filter to transform the minified version of the combined content of either one of the followings.
    /// In default mode (<see cref="DefaultProcessingWorkflow"/>, all files in a minification group of a particular resource set.
    /// In debug mode (<see cref="DebugProcessingWorkflow"/>, all files in the resource set.
    /// </summary>
    /// <seealso cref="DefaultProcessingWorkflow"/>
    /// <seealso cref="DebugProcessingWorkflow"/>
    public interface IMinifiedContentFilter : IContentFilter
    {
        /// <summary>
        /// Transforms the minified content.
        /// </summary>
        /// <param name="resourceSet">The resource set being worked on.</param>
        /// <param name="resources">The resources whose minified combined content is to be transformed.</param>
        /// <param name="content">The minified content of <paramref name="resources"/>.</param>
        /// <returns>The transformed minified content.</returns>
        string TransformContent(ResourceSet resourceSet, IEnumerable<Resource> resources, string content);
    }
}
