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

namespace Combres
{
    /// <summary>
    /// Represents a logger.
    /// </summary>
    public interface ILogger
    {
        bool IsDebugEnabled { get;}
        void Debug(string message);
        void Debug(string message, Exception exception);

        bool IsInfoEnabled { get; }
        void Info(string message);
        void Info(string message, Exception exception);

        bool IsWarnEnabled { get; }
        void Warn(string message);
        void Warn(string message, Exception exception);

        bool IsErrorEnabled { get; }
        void Error(string message);
        void Error(string message, Exception exception);

        bool IsFatalEnabled { get; }
        void Fatal(string message);
        void Fatal(string message, Exception exception);
    }
}
