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

namespace Combres.Loggers
{
    /// <summary>
    /// Logger implementation which basically does no logging at all (lol).
    /// </summary>
    public sealed class IgnoreLogger : ILogger
    {
        public bool IsDebugEnabled { get { return false; } }
        public void Debug(string message) { }
        public void Debug(string message, Exception exception) { }

        public bool IsInfoEnabled { get { return false; } }
        public void Info(string message) { }
        public void Info(string message, Exception exception) { }

        public bool IsWarnEnabled { get { return false; } }
        public void Warn(string message) { }
        public void Warn(string message, Exception exception) { }

        public bool IsErrorEnabled { get { return false; } }
        public void Error(string message) { }
        public void Error(string message, Exception exception) { }

        public bool IsFatalEnabled { get { return false; } }
        public void Fatal(string message) { }
        public void Fatal(string message, Exception exception) { }
    }
}
