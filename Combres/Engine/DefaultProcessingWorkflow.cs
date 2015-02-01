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
using System.IO;

namespace Combres
{
    /// <summary>
    /// <para>Workflow which is executed if debugging mode is <b>disabled</b>.  The workflow starts by partitioning 
    /// the resources into various minification groups, each associated with the same minifier.  
    /// If there's only one minifier configured for all resources, then there's only one 
    /// minification group.  For each minification group:</para>
    /// <list type="bullet">
    /// <item>Read content of every resource in the set, invoking <see cref="ISingleContentFilter"/> for each read.</item>
    /// <item>Combine the contents of all resources into one, invoking <see cref="ICombinedContentFilter"/>.</item>
    /// <item>Minify the combined content, invoking <see cref="IMinifiedContentFilter"/>.</item>
    /// </list> 
    /// <para>
    /// Once having the minified contents of all minification groups, the workflow merges them together, compressing 
    /// the merged content and invoking <see cref="ICompressedContentFilter"/>.
    /// </para>
    /// </summary>
    internal sealed class DefaultProcessingWorkflow : IProcessingWorkflow
    {
        private readonly RequestProcessor processor;

        public DefaultProcessingWorkflow(RequestProcessor processor)
        {
            this.processor = processor;
        }

        public void Execute()
        {
            if (processor.IsInBrowserCache())
                return;

            if (processor.WriteFromServerCache())
                return;

            using (var memoryStream = new MemoryStream(4096))
            {
                /*
                 * Each resource in a set may have a unique minifier.  At the same time,
                 * the order of resource as configured in the XML data file must be honored
                 * when merging them together.
                 * 
                 * Combres will group resources into merge-groups each includes resources sitting
                 * next to each other in the XML data file having the same minifier.
                 */
                var minifiedContents = new List<string>();
                var mergeGroup = new List<Resource>();
                MinifierInfo currentMinifier = null;
                foreach (var resource in processor.ResourceSet)
                {
                    // not the first time AND hit a different minifier, finish up the current merge-group
                    if (currentMinifier != null && currentMinifier != resource.Minifier) 
                    {
                        ProcessMergeGroup(minifiedContents, mergeGroup, currentMinifier);
                        mergeGroup.Clear();
                    }
                    currentMinifier = resource.Minifier;
                    mergeGroup.Add(resource);
                }
                if (mergeGroup.Count > 0) // there's some left-over
                {
                    ProcessMergeGroup(minifiedContents, mergeGroup, currentMinifier);
                }

                var mergedContent = processor.MergeContents(minifiedContents.ToArray());
                var compressedContent = processor.TryZipContent(mergedContent, memoryStream);
                string etag = processor.GenerateETag(compressedContent);
                processor.CacheNewResponse(compressedContent, etag);
                processor.SendOutputToClient(compressedContent, true, etag);
            }
        }

        private void ProcessMergeGroup(ICollection<string> minifiedContents, 
            IEnumerable<Resource> mergeGroup, MinifierInfo currentMinifier)
        {
            var singleContents = processor.GetSingleContents(mergeGroup, true);
            var combinedContent = processor.GetCombinedContents(mergeGroup, singleContents, false);
            var minifiedContent = processor.MinifyContent(currentMinifier, mergeGroup, combinedContent);
            minifiedContents.Add(minifiedContent);
        }
    }
}
