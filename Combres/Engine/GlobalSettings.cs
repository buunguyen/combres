using System.Net;
using System.Net.Security;

namespace Combres.Engine
{
    /// <summary>
    /// Puts methods which setup global settings necessary for Combres to operate here.
    /// If a particular configuration setting should be customizable by users,
    /// change the Combres.xml schema and write code for it instead.
    /// </summary>
    internal static class GlobalSettings
    {
        /// <summary>
        /// Makes the current process trusts all SSL certificates.
        /// </summary>
        public static void AllowAllSslCertificates()
        {
            ServicePointManager.ServerCertificateValidationCallback =
                new RemoteCertificateValidationCallback(delegate { return true; });
        }
    }
}
