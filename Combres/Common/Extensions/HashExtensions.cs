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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Combres
{
    internal static class HashExtensions
    {
        public static string GetHash(this byte[] input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            using (var sha = SHA512.Create())
            {
                return sha.ComputeHash(input).ToHexa();
            }
        }

        public static string GetHash(this string input)
        {
            if (input == null)
                throw new ArgumentNullException("input");
            return Encoding.UTF8.GetBytes(input).GetHash();
        }

        private static string ToHexa(this IEnumerable<byte> data)
        {
            var sb = new StringBuilder();
            foreach (var b in data)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }
}
