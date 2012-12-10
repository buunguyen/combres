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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Hosting;

namespace Combres
{
    internal sealed class ChangeMonitor
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private readonly HttpContext fakeContext;
        private readonly object syncLock = new object();
        public event ChangeDetected ChangeDetected;
        private readonly List<FileSystemWatcher> watchers = new List<FileSystemWatcher>();
        private readonly DynamicChangesMonitorInfo currentMonitorInfo = new DynamicChangesMonitorInfo();

        private sealed class DynamicChangesMonitorInfo
        {
            public DynamicChangesMonitorInfo()
            {
                DynamicResourcesToWatch = new List<Resource>();
                LocalChangeMonitorInterval = null;
                RemoteChangeMonitorInterval = null;
            }

            public DynamicChangesMonitorInfo(DynamicChangesMonitorInfo other)
            {
                DynamicResourcesToWatch = other.DynamicResourcesToWatch;
                LocalChangeMonitorInterval = other.LocalChangeMonitorInterval;
                RemoteChangeMonitorInterval = other.RemoteChangeMonitorInterval;
            }

            public List<Resource> DynamicResourcesToWatch { get; set; }
            public long? LocalChangeMonitorInterval { get; set; }
            public long? RemoteChangeMonitorInterval { get; set; }
        }

        public ChangeMonitor()
        {
            fakeContext = HttpContext.Current.FakeHttpContext();
            ThreadPool.QueueUserWorkItem(MonitorDynamicChanges);
        }

        /// <summary>
        /// Invoked whenever there's a new settings object.
        /// </summary>
        public void Watch(Settings settings)
        {
            lock (syncLock)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Watch new settings");
                MonitorLocalChanges(settings);
                MonitorRemoteChanges(settings);
            }
        }

        private static bool IsSamePath(string pathFromFileSystemWatcher, string path)
        {
            /*
             * In some machines (mine, particularly), pathFromFileSystemWatcher comes with ~... appended at the end.
             * So remove that "tail" before the comparison is necessary.
             * See http://combres.codeplex.com/Thread/View.aspx?ThreadId=79884
             */
            var index = pathFromFileSystemWatcher.LastIndexOf('~');
            if (index != -1)
            {
                pathFromFileSystemWatcher = pathFromFileSystemWatcher.Substring(0, index);
            }
            return pathFromFileSystemWatcher.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        private void MonitorRemoteChanges(Settings settings)
        {
            var dynamicResourcesToWatch = (from rs in settings.ResourceSets
                                           where rs.IsAutoVersion
                                           from resource in rs.Resources
                                           where resource.Mode == ResourceMode.Dynamic
                                           select resource).Distinct().ToList();
            lock (currentMonitorInfo)
            {
                currentMonitorInfo.DynamicResourcesToWatch = dynamicResourcesToWatch;
                var localInterval = (settings.LocalChangeMonitorInterval == null ||
                                     settings.LocalChangeMonitorInterval <= 0)
                                        ? null
                                        : settings.LocalChangeMonitorInterval*1000L;
                currentMonitorInfo.LocalChangeMonitorInterval = localInterval;

                var remoteInterval = (settings.RemoteChangeMonitorInterval == null ||
                                      settings.RemoteChangeMonitorInterval <= 0)
                                         ? null
                                         : settings.RemoteChangeMonitorInterval*1000L;
                currentMonitorInfo.RemoteChangeMonitorInterval = remoteInterval;
            }
        }

        private void MonitorDynamicChanges(object state)
        {
            if (Log.IsDebugEnabled)
                Log.Debug("Dynamic resource change monitor thread starts...");
            long localIntervalElapsed = 0;
            long remoteIntervalElapsed = 0;
            var watch = new Stopwatch();
            while (true)
            {
                try
                {
                    watch.Start();
                    DynamicChangesMonitorInfo tmpMonitorInfo;
                    lock (currentMonitorInfo)
                    {
                        /*
                         * local.DynamicResourcesToWatch may be referring to an old Setting object here.
                         * The code should deal with that accordingly.
                         */
                        tmpMonitorInfo = new DynamicChangesMonitorInfo(currentMonitorInfo);
                    }

                    bool nothingToWatch = false;
                    if (tmpMonitorInfo.DynamicResourcesToWatch.Count == 0 ||
                        (tmpMonitorInfo.LocalChangeMonitorInterval == null &&
                         tmpMonitorInfo.RemoteChangeMonitorInterval == null))
                    {
                        nothingToWatch = true;
                    }
                    else
                    {
                        var modifiedResourcePaths = new List<string>();
                        var localCheckShouldRun = tmpMonitorInfo.LocalChangeMonitorInterval != null &&
                                                  localIntervalElapsed >= tmpMonitorInfo.LocalChangeMonitorInterval;
                        var remoteCheckShouldRun = tmpMonitorInfo.RemoteChangeMonitorInterval != null &&
                                                   remoteIntervalElapsed >= tmpMonitorInfo.RemoteChangeMonitorInterval;
                        if (localCheckShouldRun || remoteCheckShouldRun)
                        {
                            if (localCheckShouldRun)
                            {
                                if (Log.IsDebugEnabled)
                                    Log.Debug("Checking changes to local dynamic resources...");
                                CheckChange(tmpMonitorInfo, modifiedResourcePaths, r => r.IsInSameApplication);
                                localIntervalElapsed = 0;
                            }
                            if (remoteCheckShouldRun)
                            {
                                if (Log.IsDebugEnabled)
                                    Log.Debug("Checking changes to remote dynamic resources...");
                                CheckChange(tmpMonitorInfo, modifiedResourcePaths, r => !r.IsInSameApplication);
                                remoteIntervalElapsed = 0;
                            }
                            if (modifiedResourcePaths.Count > 0)
                            {
                                if (Log.IsDebugEnabled)
                                    Log.Debug("Dynamic resources change count: " + modifiedResourcePaths.Count);
                                OnChange(ChangeType.Resource, modifiedResourcePaths);
                            }
                        }
                    }

                    // nothingToWatch is a status that won't change unless there's a configuration change
                    // so a reasonably long sleep till the next check shouldn't hurt.
                    // Otherwise, sleep for 1 second, which is the smallest value that can be specified for an interval.
                    Thread.Sleep(nothingToWatch ? 5000 : 1000);

                    // Accummulate elapsed periods
                    watch.Stop();
                    var elapsed = watch.ElapsedMilliseconds;
                    localIntervalElapsed += elapsed;
                    remoteIntervalElapsed += elapsed;
                    watch.Reset();
                }
                catch (Exception ex)
                {
                    if (Log.IsWarnEnabled)
                        Log.Warn("Error in dynamic resource monitor thread", ex);
                }
            }
// ReSharper disable FunctionNeverReturns
        }
// ReSharper restore FunctionNeverReturns

        private void CheckChange(DynamicChangesMonitorInfo monitorInfo,
                                 ICollection<string> modifiedResourcePaths,
                                 Func<Resource, bool> resourceTypeFunc)
        {
            var resourcesToCheck = monitorInfo.DynamicResourcesToWatch.Where(resourceTypeFunc);

            foreach (var resource in resourcesToCheck)
            {
                var cachedContent = resource.ReadFromCache(false);

                /*
                 * If content is null, means two things:
                 * - No request ever made to that dynamic resource by Combres 
                 *   Combres will make such request anyway, so there's no need to post a change here.
                 * - The resource doesn't exist anymore in the latest Setting object,
                 *   thus a change should not be posted here.
                 */
                if (cachedContent == null)
                    continue;

                /*
                 * URL of local dynamic resources (i.e. starting with ~) must be resolved
                 * with a reference to HttpContext.Current, which does not really exist
                 * because we're in a custom thread.  This injects a fake context which 
                 * has enough information for the URL resolution to work.
                 */
                HttpContext.Current = fakeContext;
                var newContent = resource.ReadNewContent();
                if (!cachedContent.Equals(newContent))
                {
                    modifiedResourcePaths.Add(resource.Path);
                }
            }
        }

        private void MonitorLocalChanges(Settings settings)
        {
            watchers.ForEach(w => w.Dispose());
            watchers.Clear();

            var combresConfigPath = HostingEnvironment.MapPath(Configuration.Config.DefinitionUrl);
            var staticFilesToWatch = (from rs in settings.ResourceSets
                                      where rs.IsAutoVersion
                                      from resource in rs.Resources
                                      where resource.Mode == ResourceMode.Static
                                      select HostingEnvironment.MapPath(resource.Path))
                .Concat(new[] {combresConfigPath}).ToList();

            var directoriesToWatch = staticFilesToWatch
                .Select(Path.GetDirectoryName).Distinct();

            foreach (var directory in directoriesToWatch)
            {
                var watcher = new FileSystemWatcher
                                  {
                                      Path = directory,
                                      Filter = "*.*",
                                      NotifyFilter = NotifyFilters.LastWrite,
                                      IncludeSubdirectories = false
                                  };
                watcher.Changed += (sender, arg) =>
                                       {
                                           if (IsSamePath(arg.FullPath, combresConfigPath))
                                           {
                                               if (Log.IsDebugEnabled)
                                                   Log.Debug("OnConfigChanged");
                                               OnChange(ChangeType.Config, null);
                                           }
                                           else
                                           {
                                               var path =
                                                   staticFilesToWatch.FirstOrDefault(f => IsSamePath(arg.FullPath, f));
                                               if (path != null)
                                               {
                                                   var modifiedResourcePaths = (from rs in settings.ResourceSets
                                                                                where rs.IsAutoVersion
                                                                                from resource in rs.Resources
                                                                                where
                                                                                    resource.Mode == ResourceMode.Static &&
                                                                                    path.Equals(
                                                                                        HostingEnvironment.MapPath(
                                                                                            resource.Path),
                                                                                        StringComparison.
                                                                                            OrdinalIgnoreCase)
                                                                                select resource.Path).Distinct().ToList();
                                                   if (modifiedResourcePaths.Count > 0)
                                                   {
                                                       if (Log.IsDebugEnabled)
                                                           Log.Debug("Static resources change count: " +
                                                                     modifiedResourcePaths.Count);
                                                       OnChange(ChangeType.Resource, modifiedResourcePaths);
                                                   }
                                               }
                                           }
                                       };
                watcher.EnableRaisingEvents = true;
                watchers.Add(watcher);
            }
        }

        private void OnChange(ChangeType changeType, List<string> affectedResources)
        {
            if (ChangeDetected == null)
                return;
            var arg = new ChangeDetectedEventArg(changeType, affectedResources);
            ChangeDetected(arg);
        }
    }
}