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

namespace Combres
{
    /// <summary>
    /// Represents the actual type of the resource.
    /// </summary>
    public enum ResourceType
    {
        /// <summary>
        /// The resource is a JavaScript resource.
        /// </summary>
        JS, 

        /// <summary>
        /// The resource is a CSS resource.
        /// </summary>
        CSS
    }
}