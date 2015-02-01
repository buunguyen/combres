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
using System.Globalization;

namespace Combres
{
    internal static class TypeExtensions
    {
        /// <summary>
        /// Converts <paramref name="value"/> to the corresponding value of 
        /// type <paramref name="targetType"/>.  <paramref name="targetType"/>
        /// must be one of the following types: string, value type, and nullable type.
        /// </summary>
        /// <param name="value">The string to be converted.</param>
        /// <param name="targetType">The target type to convert <paramref name="value"/> to.</param>
        /// <returns>The corresponding value, in <paramref name="targetType"/>, of <paramref name="value"/>.</returns>
        /// <seealso cref="Convert.ChangeType(object,System.Type)"/>
        public static object ConvertToType(this string value, Type targetType)
        {
            if (value == null)
                return null;
            if (targetType == typeof(string))
                return value;
            if (targetType.IsEnum)
                return Enum.Parse(targetType, value, true);
            if (targetType.IsNullable())
                targetType = Nullable.GetUnderlyingType(targetType);
            return Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Forwards the call to <see cref="ConvertToType(string, Type)"/>.  If there's any exception
        /// or if <c>null</c> is return, <paramref name="defaultValue"/> is returned by this method instead.
        /// </summary>
        public static object ConvertToType(this string value, Type targetType, object defaultValue)
        {
            try
            {
                var result = ConvertToType(value, targetType);    
                return result ?? defaultValue;
            } 
            catch
            {
                return defaultValue;
            }
        }

        private static bool IsNullable(this Type type)
        {
            if (!type.IsValueType) 
                return false;
            return Nullable.GetUnderlyingType(type) != null;
        }
    }
}