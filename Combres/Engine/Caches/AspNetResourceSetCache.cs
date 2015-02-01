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

using System;
using System.Web;
using System.Web.Caching;

namespace Combres.Caches
{
    /// <summary>
    /// Implementation of <see cref="IResourceSetCache"/> which uses the ASP.NET Cache.
    /// </summary>
    public sealed class AspNetResourceSetCache : IResourceSetCache
    {
        /// <inheritdoc cref="IResourceSetCache.Add" />
        public void Add(string key, object value, TimeSpan slidingExpiration)
        {
            HttpContext.Current.Cache.Insert(key, value,
                                             null /* cache dependencies */,
                                             Cache.NoAbsoluteExpiration,
                                             slidingExpiration);
        }

        /// <inheritdoc cref="IResourceSetCache.Get" />
        public object Get(string key)
        {
            return HttpContext.Current.Cache[key];
        }

        /// <inheritdoc cref="IResourceSetCache.this" />
        public object this[string key]
        {
            get { return Get(key); }
        }
    }
}