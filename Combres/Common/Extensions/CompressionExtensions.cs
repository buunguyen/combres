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

using System.IO;
using System.IO.Compression;

namespace Combres
{
    internal static class CompressionExtensions
    {
        enum CompressionType
        {
            GZip,
            Delfate
        }

        /// <summary>
        /// Returns the ungzipped byte array. Returns empty byte array if <paramref name="bytes"/> is null or empty.
        /// </summary>
        /// <param name="bytes">The byte array to be ungzipped.</param>
        /// <returns>The ungzipped byte array.  Empty byte array if <paramref name="bytes"/> is null or empty.</returns>
        public static byte[] UnGzip(this byte[] bytes)
        {
            return UnCompress(bytes, CompressionType.GZip);
        }

        /// <summary>
        /// Returns the undeflated byte array. Returns empty byte array if <paramref name="bytes"/> is null or empty.
        /// </summary>
        /// <param name="bytes">The byte array to be undeflated.</param>
        /// <returns>The undeflated byte array.  Empty byte array if <paramref name="bytes"/> is null or empty.</returns>
        public static byte[] UnDeflate(this byte[] bytes)
        {
            return UnCompress(bytes, CompressionType.Delfate);
        }

        private static byte[] UnCompress(this byte[] bytes, CompressionType type)
        {
            if (bytes == null || bytes.Length == 0)
                return new byte[0];
            using (var stream = type == CompressionType.GZip
                ? (Stream)new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress)
                : new DeflateStream(new MemoryStream(bytes), CompressionMode.Decompress))
            {
                var buffer = new byte[4096];
                using (var memory = new MemoryStream())
                {
                    int count;
                    do
                    {
                        count = stream.Read(buffer, 0, buffer.Length);
                        if (count > 0)
                        {
                            memory.Write(buffer, 0, count);
                        }
                    }
                    while (count > 0);
                    bytes = memory.ToArray();
                }
            }
            return bytes;
        }
    }
}
