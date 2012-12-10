using System.Collections.Generic;
using Combres;

namespace SampleCommon
{
    /// <summary>
    /// A sample implementation of IMinifiedContentFilter which also implements
    /// ICacheVaryStateReceiver.
    /// </summary>
    class CopyrightFilter : IMinifiedContentFilter, ICacheVaryStateReceiver
    {
        public bool CanApplyTo(ResourceType resourceType)
        {
            return true;
        }

        public string TransformContent(ResourceSet resourceSet, IEnumerable<Resource> resources, string content)
        {
            var str = GetCopyrightString();
            return "/* " + str + " */\n" + content;
        }

        private string GetCopyrightString()
        {
            var language = (string)CacheVaryStates[0].Values["language"];
            switch (language.ToLower())
            {
                case "vietnamese":
                    return "Kiểm tra CopyrightFilter: Bản Quyền 2010 Combres";
                default:
                    return "Test CopyrightFilter: Copyright 2010 Combres";
            }
        }

        public IList<CacheVaryState> CacheVaryStates { get; set; }
    }
}
