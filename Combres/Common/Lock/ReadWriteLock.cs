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
using System.Threading;

namespace Combres
{
    internal sealed class ReadWriteLock : ILock
    {
        private readonly ReaderWriterLockSlim synchronizer;

        public ReadWriteLock() : this (false) {}

        public ReadWriteLock(bool recursion)
        {
            synchronizer = new ReaderWriterLockSlim(recursion
                ? LockRecursionPolicy.SupportsRecursion
                : LockRecursionPolicy.NoRecursion);
        }

        public IDisposable Read()
        {
            return new ReaderLock(synchronizer);
        }

        public IDisposable Write()
        {
            return new WriterLock(synchronizer);
        }

        public void Dispose()
        {
            synchronizer.Dispose();
        }

        private sealed class ReaderLock : IDisposable
        {
            private readonly ReaderWriterLockSlim synchronizer;

            public ReaderLock(ReaderWriterLockSlim synchronizer)
            {
                this.synchronizer = synchronizer;
                synchronizer.EnterReadLock();
            }

            public void Dispose()
            {
                synchronizer.ExitReadLock();
            }
        }

        private sealed class WriterLock : IDisposable
        {
            private readonly ReaderWriterLockSlim synchronizer;

            public WriterLock(ReaderWriterLockSlim synchronizer)
            {
                this.synchronizer = synchronizer;
                this.synchronizer.EnterWriteLock();
            }

            public void Dispose()
            {
                synchronizer.ExitWriteLock();
            }
        }
    }
}
