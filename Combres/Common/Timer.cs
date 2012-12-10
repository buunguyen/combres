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
using System.Diagnostics;
using System.Globalization;

namespace Combres
{
    internal sealed class Timer : IDisposable
    {
        private string ActionName { get; set; }
        private Stopwatch Stopwatch { get; set; }
        private bool Enabled { get; set; }
        private Action<string> Action { get; set; }

        public Timer(String actionName, bool enabled, Action<string> action)
        {
            Enabled = enabled;
            if (Enabled)
            {
                ActionName = actionName;
                Action = action;
                Stopwatch = new Stopwatch();
                Stopwatch.Start();
            }
        }

        public void Dispose()
        {
            if (Enabled)
            {
                Stopwatch.Stop();
                Action(string.Format(CultureInfo.InvariantCulture, 
                    "Elapsed time for '{0}': {1} ms", 
                    ActionName,
                    Stopwatch.ElapsedMilliseconds));
            }
        }
    }
}
