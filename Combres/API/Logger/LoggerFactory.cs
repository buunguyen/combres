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
using Fasterflect;

namespace Combres
{
    /// <summary>
    /// Represents a factory to create instances of a specific logger type implementing <see cref="ILogger"/>.
    /// Each call to <see cref="CreateLogger(Type)"/> or <see cref="CreateLogger(string)"/> will invoke 
    /// a specific constructor or static factory method implemented by the corresponding implementation;
    /// see <see cref="DetermineLoggerCreator"/> for more details on the selection of creation mechanism.  
    /// It's up to the specific implementation of <see cref="ILogger"/> to properly optimize resource consumption
    /// and ensure thread-safety.
    /// </summary>
    internal static class LoggerFactory
    {
        private static readonly Type LoggerType;
        internal static readonly ConfigSectionSetting Config = ConfigSectionSetting.Get();
        private static readonly ILoggerCreator Creator;

        static LoggerFactory()
        {
            LoggerType = Type.GetType(Config.LogProvider, true);
            InitializeLoggerType();
            Creator = DetermineLoggerCreator();
        }

        private static void InitializeLoggerType()
        {
            var methodInfo = LoggerType.Method("InitializeLoggerType", Flags.StaticAnyVisibility);
            if (methodInfo != null)
                methodInfo.Call();
        }

        /// <summary>
        /// <para>Priority from top to bottom.</para>
        /// <list type="bullet">
        ///     <listheader>
        ///         <term>Method/Constructor</term>
        ///         <description>Description</description>
        ///     </listheader>
        ///     <item>
        ///         <term><c>static Create(string name)</c></term>
        ///         <description>Factory method accepting string argument</description>
        ///     </item>
        ///     <item>
        ///         <term><c>static Create()</c></term>
        ///         <description>No-argument factory method</description>
        ///     </item>
        ///     <item>
        ///         <term><c>ctor(string name)</c></term>
        ///         <description>Constructor accepting string argument</description>
        ///     </item>
        ///     <item>
        ///         <term><c>ctor()</c></term>
        ///         <description>No-argument constructor</description>
        ///     </item>
        /// </list>
        /// </summary>
        /// <returns></returns>
        private static ILoggerCreator DetermineLoggerCreator()
        {
            if (LoggerType.Method("Create", new[] { typeof(string) }, Flags.StaticAnyVisibility) != null)
            {
                return new StringArgCreationMethodCreator();
            }
            if (LoggerType.Method("Create", Flags.StaticAnyVisibility) != null)
            {
                return new NoArgCreationMethodCreator();
            }
            if (LoggerType.Constructor(Flags.InstanceAnyVisibility, typeof(string)) != null)
            {
                return new StringArgCtorCreator();
            }
            return new NoArgCtorCreator();
        }

        /// <summary>
        /// Creates a logger based on the name of the specified <paramref name="type"/>.
        /// </summary>
        public static ILogger CreateLogger(Type type)
        {
            return CreateLogger(type.FullName);
        }

        /// <summary>
        /// Creates a logger based on the the specified <paramref name="name"/>.
        /// </summary>
        public static ILogger CreateLogger(string name)
        {
            return Creator.Create(LoggerType, name);
        }

        #region Logger Creators
        private interface ILoggerCreator
        {
            ILogger Create(Type loggerType, string name);
        }

        private sealed class NoArgCtorCreator : ILoggerCreator
        {
            public ILogger Create(Type loggerType, string name)
            {
                return (ILogger)loggerType.CreateInstance();
            }
        }

        private sealed class StringArgCtorCreator : ILoggerCreator
        {
            public ILogger Create(Type loggerType, string name)
            {
                return (ILogger)loggerType.CreateInstance(name);
            }
        }

        private sealed class NoArgCreationMethodCreator : ILoggerCreator
        {
            public ILogger Create(Type loggerType, string name)
            {
                return (ILogger)loggerType.CallMethod("Create");
            }
        }

        private sealed class StringArgCreationMethodCreator : ILoggerCreator
        {
            public ILogger Create(Type loggerType, string name)
            {
                return (ILogger)loggerType.CallMethod("Create", name);
            }
        }
        #endregion
    }
}
