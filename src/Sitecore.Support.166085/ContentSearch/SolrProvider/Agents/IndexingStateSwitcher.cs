namespace Sitecore.Support.ContentSearch.SolrProvider.Agents
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Sitecore.ContentSearch;
    using Sitecore.Diagnostics;
    using Sitecore.StringExtensions;
    using Sitecore.Tasks;
    [UsedImplicitly]
    public class IndexingStateSwitcher : BaseAgent
    {
        private static bool lastSolrConnectionStatus;
        private static readonly MethodInfo OnPauseIndexingMI;
        private static readonly MethodInfo OnResumeIndexingMI;

        static IndexingStateSwitcher()
        {
            OnPauseIndexingMI = typeof(Sitecore.ContentSearch.Maintenance.EventHub).GetMethod("OnPauseIndexing", BindingFlags.NonPublic | BindingFlags.Static, null,
                new[] { typeof(object), typeof(Action<object>)}, null);
            OnResumeIndexingMI = typeof(Sitecore.ContentSearch.Maintenance.EventHub).GetMethod("OnResumeIndexing", BindingFlags.NonPublic | BindingFlags.Static, null,
                new[] { typeof(object), typeof(Action<object>) }, null);

            lastSolrConnectionStatus = Sitecore.ContentSearch.SolrProvider.SolrStatus.InitStatusOk;
            if (!lastSolrConnectionStatus)
            {
                OnPauseIndexingMI.Invoke(null, new object[]
                {
                    typeof(IndexingStateSwitcher), (System.Action<object>) ((sender) =>
                    {
                        ContentSearchManager.Indexes.ToList().ForEach(i =>
                        {
                            if (i as ISearchIndexSwitch != null)
                            {
                                i.PauseIndexing();
                            }
                        });
                    })
                });
            }
        }

        [UsedImplicitly]
        public void Run()
        {
            bool currentStatus = Sitecore.Support.ContentSearch.SolrProvider.SolrStatus.OkSolrStatus();

            this.LogAction(() => Log.Info("DEBUG: before lastSolrConnectionStatus update: lastSolrConnectionStatus='{0}' and currentStatus='{1}'".FormatWith(lastSolrConnectionStatus, currentStatus), this));

            if (lastSolrConnectionStatus && !currentStatus)
            {
                OnPauseIndexingMI.Invoke(null, new object[] { this, (System.Action<object>)(this.PausedAction) });

                lastSolrConnectionStatus = false;
                this.LogAction(() => Log.Warn("Solr Connection Failed. Indexing operation is put on pause.", this));
            }
            else if (!lastSolrConnectionStatus && currentStatus)
            {
                // If initial solr connection status is false, index is not been initialized fully.
                // Attempt to resume would cause further hard. We should wait until IsSolrAliveAgent re-initialize index.
                // We need to ensure that all indexes are initialized before resuming indexing.
                var canResume = !ContentSearchManager.Indexes.Any(x => x is AbstractSearchIndex && !((AbstractSearchIndex)x).IsInitialized);

                if (canResume)
                {
                    OnResumeIndexingMI.Invoke(null, new object[] { this, (System.Action<object>)(this.ResumeAction) });
                    lastSolrConnectionStatus = true;
                    this.LogAction(() => Log.Warn("Solr Connection up. Indexing operation resume.", this));
                }
            }

            this.LogAction(() => Log.Info("DEBUG: after lastSolrConnectionStatus update: lastSolrConnectionStatus='{0}' and currentStatus='{1}'".FormatWith(lastSolrConnectionStatus, currentStatus), this));
        }

        protected internal virtual void LogAction(Action logAction)
        {
            if (logAction != null)
            {
                logAction();
            }
        }

        protected internal virtual void PausedAction(object sender)
        {
            ContentSearchManager.Indexes.ToList().ForEach(i =>
            {
                if (i as ISearchIndexSwitch != null)
                {
                    i.PauseIndexing();
                }
            });
        }

        protected internal virtual void ResumeAction(object sender)
        {
            ContentSearchManager.Indexes.ToList().ForEach(i =>
            {
                if (i as ISearchIndexSwitch != null)
                {
                    i.ResumeIndexing();
                }
            });
        }
    }
}