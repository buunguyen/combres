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
using System.Xml.Linq;

namespace Combres
{
    /// <summary>
    /// Represents a binder which can bind values specified in a list of <code>XElement</code> to an object.
    /// </summary>
    public interface IObjectBinder
    {
        /// <summary>
        /// Bind the values in <paramref name="parameters"/> into an existing object.
        /// </summary>
        /// <param name="instance">The object whose properties are to be set.</param>
        /// <param name="parameters">The parameter list.</param>
        void Bind(IEnumerable<XElement> parameters, object instance);
    }
}
