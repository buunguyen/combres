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
using System.Collections.Generic;
using System.Text;

namespace Combres
{
    internal delegate void ChangeDetected(ChangeDetectedEventArg arg);

    internal enum ChangeType
    {
        Config,
        Resource
    }

    internal sealed class ChangeDetectedEventArg
    {
        public ChangeType ChangeType { get; private set; }
        public List<string> ModifiedResourcePaths { get; private set; }

        public ChangeDetectedEventArg(ChangeType changeType,
            List<string> affectedResources)
        {
            ChangeType = changeType;
            ModifiedResourcePaths = affectedResources ?? new List<string>();
        }

        public ChangeDetectedEventArg(ChangeDetectedEventArg other)
        {
            ChangeType = other.ChangeType;
            ModifiedResourcePaths = new List<string>(other.ModifiedResourcePaths);
        }

        public override string ToString()
        {
            var sb = new StringBuilder(Environment.NewLine);
            foreach (var path in ModifiedResourcePaths)
            {
                sb.Append(" - ")
                  .Append(path)
                  .Append(Environment.NewLine);
            }
            return "Change Type: " + ChangeType + " - Affected resource list: " + sb;
        }
    }
}
