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

namespace Combres
{
    /// <summary>
    /// Implementations of <see cref="IContentFilter"/> and/or <see cref="IResourceMinifier"/>, 
    /// if also implement this interface, will be injected with a list of <see cref="CacheVaryState"/>
    /// built by a resource set's registered <see cref="ICacheVaryProvider"/>.
    /// </summary>
    public interface ICacheVaryStateReceiver
    {
        /// <summary>
        /// The list of <see cref="CacheVaryState"/> built by a resource set's 
        /// registered <see cref="ICacheVaryProvider"/>.
        /// </summary>
        IList<CacheVaryState> CacheVaryStates { get; set; }
    }
}
