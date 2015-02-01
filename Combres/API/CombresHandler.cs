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
using System.Net;
using System.Web;
using System.Web.SessionState;

namespace Combres
{
    /// <summary>
    /// Represents the resource combine handler.
    /// </summary>
    internal sealed class CombresHandler : IHttpHandler, IReadOnlySessionState
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
                       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly string setName;
        private readonly string version;

        public CombresHandler(string setName, string version)
        {
            this.setName = setName;
            this.version = version;
        }

        public void ProcessRequest(HttpContext context)
        {
            if (context == null)
                return;
            var settings = Configuration.GetSettings();
            try
            {
                new RequestProcessor(context, settings, setName, version).Execute();
            } 
            catch (ResourceSetNotFoundException ex)
            {
                NotFound(context, ex);
            }
            catch (ResourceNotFoundException ex)
            {
                NotFound(context, ex);
            }
        }

        private static void NotFound(HttpContext context, Exception ex)
        {
            if (Log.IsWarnEnabled) Log.Warn(ex.Message, ex);
            context.Response.AppendHeader("Content-Length", "0");
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            context.Response.End();
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}