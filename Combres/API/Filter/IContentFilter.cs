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
    /// Base interface for content filter.
    /// </summary>
    public interface IContentFilter
    {
        /// <summary>
        /// Returns true if the filter can be applied to the specified <paramref name="resourceType"/>.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <returns>True if the filter can be applied to the specified <paramref name="resourceType"/>.</returns>
        bool CanApplyTo(ResourceType resourceType);
    }
}
