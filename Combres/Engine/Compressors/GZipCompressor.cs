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

using System.Web;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Combres.Compressors
{
    internal sealed class GZipCompressor : ICompressor
    {
        public string EncodingName
        {
            get { return "gzip"; }
        }

        public bool CanHandle(string acceptEncoding)
        {
            return !string.IsNullOrEmpty(acceptEncoding) &&
                   acceptEncoding.Contains(EncodingName);
        }

        public void AppendHeader(HttpResponse response)
        {
            response.AppendHeader("Content-Encoding", EncodingName);
        }

        public void Compress(string content, Stream targetStream)
        {
            using (var writer = new GZipStream(targetStream, CompressionMode.Compress))
            {
                var bytes = Encoding.UTF8.GetBytes(content);
                writer.Write(bytes, 0, bytes.Length);
            }
        }
    }
}


