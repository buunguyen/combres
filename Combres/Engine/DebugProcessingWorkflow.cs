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

using System.IO;

namespace Combres
{
    /// <summary>
    /// <para>Workflow which is executed if debugging mode is enabled.  Unlike the 
    /// <see cref="DebugProcessingWorkflow"/>, this workflow is optimized for debugging purpose.  
    /// Specifically, client-side and server-side caching and minification won't be employed at all.  
    /// The steps include:</para>
    /// <list type="bullet">
    /// <item>Read content of every resource in the set, invoking <see cref="ISingleContentFilter"/> for each read.</item>
    /// <item>Combine the contents of all resources into one, invoking <see cref="ICombinedContentFilter"/>.</item>
    /// <item>Compres the combined content, invoking <see cref="ICompressedContentFilter"/>.</item>
    /// </list> 
    /// </summary>
    internal sealed class DebugProcessingWorkflow : IProcessingWorkflow
    {
        private readonly RequestProcessor processor;

        public DebugProcessingWorkflow(RequestProcessor processor)
        {
            this.processor = processor;
        }

        public void Execute()
        {
            using (var memoryStream = new MemoryStream(4096))
            {
                var singleContents = processor.GetSingleContents(processor.ResourceSet, false);
                var combinedContent = processor.GetCombinedContents(processor.ResourceSet, singleContents, true);
                var compressedContent = processor.TryZipContent(combinedContent, memoryStream);
                processor.SendOutputToClient(compressedContent, false, null);
            }
        }
    }
}
