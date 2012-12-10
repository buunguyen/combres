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
using log4net;
using log4net.Config;

namespace Combres.Loggers
{
    /// <summary>
    /// Log4Net-based logger implementation.
    /// </summary>
    public sealed class Log4NetLogger : ILogger
    {
        private readonly ILog log;

        /// <summary>
        /// Callback method that is invoked once for all instances for this logger type.
        /// </summary>
        private static void InitializeLoggerType()
        {
            XmlConfigurator.Configure();       
        }

        /// <summary>
        /// Callback constructor that is used by <see cref="LoggerFactory"/>.
        /// </summary>
        private Log4NetLogger(string name)
        {
            log = LogManager.GetLogger(name);
        }

        public bool IsDebugEnabled
        {
            get { return log.IsDebugEnabled; }
        }
        public void Debug(string message)
        {
            if (log.IsDebugEnabled) log.Debug(message);
        }
        public void Debug(string message, Exception exception)
        {
            if (log.IsDebugEnabled) log.Debug(message, exception);
        }

        public bool IsInfoEnabled
        {
            get { return log.IsInfoEnabled; }
        }
        public void Info(string message)
        {
            if (log.IsInfoEnabled) log.Info(message);
        }
        public void Info(string message, Exception exception)
        {
            if (log.IsInfoEnabled) log.Info(message, exception);
        }

        public bool IsWarnEnabled
        {
            get { return log.IsWarnEnabled; }
        }
        public void Warn(string message)
        {
            if (log.IsWarnEnabled) log.Warn(message);
        }
        public void Warn(string message, Exception exception)
        {
            if (log.IsWarnEnabled) log.Warn(message, exception);
        }

        public bool IsErrorEnabled
        {
            get { return log.IsErrorEnabled; }
        }
        public void Error(string message)
        {
            if (log.IsErrorEnabled) log.Error(message);
        }
        public void Error(string message, Exception exception)
        {
            if (log.IsErrorEnabled) log.Error(message, exception);
        }

        public bool IsFatalEnabled
        {
            get { return log.IsFatalEnabled; }
        }
        public void Fatal(string message)
        {
            if (log.IsFatalEnabled) log.Fatal(message);
        }
        public void Fatal(string message, Exception exception)
        {
            if (log.IsFatalEnabled) log.Fatal(message, exception);
        }
    }
}
