using System;
using System.Collections.Generic;
using Sitecore.ContentSearch;
using Sitecore.Diagnostics;
using Sitecore.Tasks;

namespace Sitecore.Support.ContentSearch.SolrProvider.Agents
{
    [UsedImplicitly]
    public class IsSolrAliveAgent : BaseAgent
    {
        [UsedImplicitly]
        public void Run()
        {
            var indexesCount = Sitecore.ContentSearch.SolrProvider.SolrStatus.GetIndexesForInitialization().Count;

            if (indexesCount <= 0)
            {
                Log.Info("IsSolrAliveAgent: No indexes are pending for re-initialization. Terminating execution", this);
                return;
            }

            Log.Info(string.Format("IsSolrAliveAgent: {0} indexes are pending for re-initialization. Checking SOLR status...", indexesCount), this);

            bool currentStatus = Sitecore.Support.ContentSearch.SolrProvider.SolrStatus.OkSolrStatus();

            if (!currentStatus)
            {
                Log.Info("IsSolrAliveAgent: SOLR is unavailable. Terminating execution", this);
                return;
            }

            Log.Debug("IsSolrAliveAgent: Start indexes re-initialization");
            var reinitializedIndexes = new List<ISearchIndex>();
            foreach (var index in Sitecore.ContentSearch.SolrProvider.SolrStatus.GetIndexesForInitialization())
            {
                try
                {
                    Log.Debug(string.Format(" - Re-initializing index '{0}' ...", index.Name), this);
                    index.Initialize();
                    Log.Debug(" - DONE", this);
                    reinitializedIndexes.Add(index);
                }
                catch (Exception ex)
                {
                    Log.Warn(string.Format("{0} index intialization failed", index.Name), ex, this);
                }
            }

            foreach (var index in reinitializedIndexes)
            {
                Log.Debug(string.Format("IsSolrAliveAgent: Un-registering {0} index after successfull re-initialization...", index.Name), this);
                Sitecore.ContentSearch.SolrProvider.SolrStatus.UnsetIndexForInitialization(index);
                Log.Debug("IsSolrAliveAgent: DONE", this);
            }
        }
    }
}