using System;
using System.Web;
using Combres;

namespace SampleCommon
{
    /// <summary>
    /// A sample implementation of ICacheVaryProvider which varies
    /// the Combres cache key for a resource set per language preference
    /// stored in the session state.  
    /// </summary>
    public class LanguageCacheVaryProvider : ICacheVaryProvider
    {
        public string SessionKey { get; set; }

        public CacheVaryState Build(HttpContext ctx, ResourceSet resourceSet)
        {
            var userLanguage = (string)ctx.Session[SessionKey];
            userLanguage = userLanguage ?? "English";
            return new CacheVaryState(this, userLanguage, new { language = userLanguage });
        }

        public bool AppendKeyToUrl
        {
            get { return true; }
        }
    }
}
