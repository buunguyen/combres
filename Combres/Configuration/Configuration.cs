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
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Xml.Schema;

namespace Combres
{
    internal static class Configuration
    {
        private static readonly ILogger Log = LoggerFactory.CreateLogger(
                       System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static readonly ConfigSectionSetting Config = ConfigSectionSetting.Get();
        private static readonly object InitializationSyncLock = new object();
        internal static readonly ILock ConfigLock = LockFactory.CreateLock(LockPolicy.MostlyRead, true);
        private static readonly ILock ChangeLock = LockFactory.CreateLock(LockPolicy.MostlyRead);
        private static readonly string AppKey = string.Concat(typeof(Configuration).FullName,
            ".", DateTime.Now);

        private static volatile bool IsInitialized = false;
        private static readonly List<ChangeDetectedEventArg> Changes = new List<ChangeDetectedEventArg>();
        private static ChangeMonitor Monitor;

        private static void ReloadSettings()
        {
            Log.Debug("Reload Combres settings");

            using (new Timer("ReloadSettings", Log.IsDebugEnabled, Log.Debug))
            {  
                using (ConfigLock.Write())
                {
                    /*
                     * Must clear the content cache.  For auto-versioned sets, it's not necessary since Combres
                     * already reads the latest content and updates the cache during the change detection proces.
                     * 
                     * For manual-versioned sets, currently Combres doesn't attempt to detect change and invalidate
                     * the cache.  Therefore, when config file is changed, we should clear the cache to allow
                     * new contents, if any, to be fetched.
                     * 
                     * Alternative approaches include:
                     * - Detect change for all resource sets -> too much overhead of change monitoring, esp. dynamic resources
                     * - Detect config change type, only clear the cache if the version value of manual-versioned sets is updated
                     */
                    ResourceContentReader.ClearCache();
                    var filePath = HostingEnvironment.MapPath(Config.DefinitionUrl);
                    var settings = ConfigReader.Read(filePath);
                    HttpContext.Current.Application[AppKey] = settings;
                    Monitor.Watch(settings);
                }
            }
        }

        private static void ChangeDetectedHandler(ChangeDetectedEventArg arg)
        {
            using (ChangeLock.Write())
            {
                /*
                 * At any point of time there can't be no more than 2 change, corresponding
                 * to the Config and Resource change type.
                 * 
                 * New change of the Resource change type will have its affected-resources
                 * merged into existing change.
                 */
                var change = Changes.FirstOrDefault(c => c.ChangeType == arg.ChangeType);
                if (change == null)
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug("Change posted. " + arg);
                    Changes.Add(arg);
                }
                else if (arg.ChangeType == ChangeType.Resource)
                {
                    foreach (var path in arg.ModifiedResourcePaths)
                    {
                        if (!change.ModifiedResourcePaths.Contains(path))
                        {
                            change.ModifiedResourcePaths.Add(path);
                        }
                    }
                    if (Log.IsDebugEnabled)
                        Log.Debug("Change merged. " + change);
                }
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="Settings"/> based on the XML definition file.
        /// </summary>
        /// <returns>An instance of <see cref="Settings"/> based on the XML definition file.</returns>
        /// <exception cref="XmlSchemaException">If there is any validation error in the 
        /// XML definition file.</exception>
        internal static Settings GetSettings()
        {
            InitializeConfig();
            if (HasChange())
            {
                ApplyChanges();
            }
            return ReadSettings();
        }

        public static string GetCombresUrl()
        {
            var filePath = HostingEnvironment.MapPath(Config.DefinitionUrl);
            return ConfigReader.ReadCombresUrl(filePath);
        }

        private static void InitializeConfig()
        {
            if (IsInitialized) // IsInitialized is volatile, lock-free read is safe
                return;
            lock (InitializationSyncLock)
            {
                if (IsInitialized)
                    return;
                Monitor = new ChangeMonitor();
                Monitor.ChangeDetected += ChangeDetectedHandler;
                ReloadSettings();
                IsInitialized = true;
            }
        }

        private static bool HasChange()
        {
            using (ChangeLock.Read())
            {
                return Changes.Count > 0;
            }
        }

        private static Settings ReadSettings()
        {
            using (ConfigLock.Read())
            {
                return HttpContext.Current.Application[AppKey] as Settings;
            }
        }

        private static void ApplyChanges()
        {
            var copiedChanges = GetChangeSnapshot();
            if (copiedChanges.Count == 0)
                return;

            if (Log.IsDebugEnabled)
                Log.Debug("Apply changes");
            foreach (var change in copiedChanges)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Apply change: " + change);
             
                switch (change.ChangeType)
                {
                    case ChangeType.Config:
                        ReloadSettings();
                        break;
                    case ChangeType.Resource:
                        ApplyChange(change);
                        break;
                }
            }
        }

        private static void ApplyChange(ChangeDetectedEventArg change)
        {
            /*
             * We may be dealing with a new setting object not existing
             * when the change.ModifiedResourcePaths is created, the code should
             * make sure it deals with the latest resource sets only
             */
            var settings = ReadSettings();
            foreach (var set in settings.ResourceSets)
            {
                if (!set.IsAutoVersion)
                    continue;

                var modifiedResources = set.Where(
                    r => change.ModifiedResourcePaths.Any(
                        path => path.Equals(r.Path, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                // Clear the content cache before recomputing the hash
                modifiedResources.ForEach(r => r.RemoveInCache());

                /*
                 * ComputeHash() must be kept out of the ConfigLock and ChangeLock in case 
                 * it makes HTTP request to local dynamic resources which happen to 
                 * require reading settings (i.e. invoke GetSettings()) and result in a 
                 * deadlock.  
                 * 
                 * This type of lock-smart approach has an issue:  
                 * - T1 finishes computing a hash
                 * - T2 picks up new change and finishes cimputing another hash
                 * - T2 save the hash
                 * - T1 save the hash -> this is the old hash
                 * But this shouldn't be a big deal because the only problem with it is
                 * when the app restarts and the new hash is recomputed, which is different
                 * from the hash in the client's browser cache and thus force the browser
                 * to make new request to the server.
                 */
                set.Hash = set.ComputeHash();
            }
        }

        private static List<ChangeDetectedEventArg> GetChangeSnapshot()
        {
            using (ChangeLock.Write())
            {
                var copiedChanges = new List<ChangeDetectedEventArg>();
                if (Changes.Count > 0)
                {
                    copiedChanges.AddRange(Changes.Select(c => new ChangeDetectedEventArg(c)));
                    Changes.Clear(); // Clean up - we don't want a change to be picked by more than 1 thread
                }
                return copiedChanges;
            }
        }
    }
}
