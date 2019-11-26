namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch;
    using Sitecore.ContentSearch.Diagnostics;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.Diagnostics;
    using Sitecore.StringExtensions;
    using SolrNet;
    using SolrNet.Exceptions;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    public static class SolrStatus
    {
        /// <summary>
        /// The lock object.
        /// </summary>
        private static object lockObject = new object();

        /// <summary>
        /// The list of indexes that failed to initialize due to some errors and should be initialized again.
        /// </summary>
        private static List<ISearchIndex> IndexesToInit { get; set; }

        public static bool InitStatusOk { get; private set; }

        static SolrStatus()
        {
            InitStatusOk = OkSolrStatus();
            IndexesToInit = new List<ISearchIndex>();
        }

        /// <summary>
        /// Sets the index that failed to initialize due to some errors and should be initialized again. 
        /// </summary>
        /// <param name="solrIndex">The index.</param>
        public static void SetIndexForInitialization(ISearchIndex solrIndex)
        {
            if (solrIndex != null)
            {
                lock (lockObject)
                {
                    if (!IndexesToInit.Contains(solrIndex))
                    {
                        IndexesToInit.Add(solrIndex);
                    }
                    else
                    {
                        CrawlingLog.Log.Warn("Index re-initialization list already contains '{0}' index. Skipping this operation to avoid duplicating the entry.".FormatWith(solrIndex.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Unsets the index that has been already initialized. 
        /// </summary>
        /// <param name="solrIndex">The index.</param>
        public static void UnsetIndexForInitialization(ISearchIndex solrIndex)
        {
            lock (lockObject)
            {
                if (solrIndex != null && IndexesToInit.Contains(solrIndex))
                {
                    IndexesToInit.Remove(solrIndex);
                }
            }
        }

        /// <summary>
        /// Gets the list of indexes that failed to initialize due to some errors and should be initialized again. 
        /// </summary>
        /// <returns>The list of <see cref="SolrSearchIndex"/> instances.</returns>
        public static List<ISearchIndex> GetIndexesForInitialization()
        {
            List<ISearchIndex> indexes;

            lock (lockObject)
            {
                indexes = IndexesToInit.ToList();
            }

            return indexes;
        }

        public static bool OkSolrStatus()
        {
            try
            {
                ISolrCoreAdmin solrAdmin = SolrContentSearchManager.SolrAdmin;
                if (solrAdmin != null)
                {
                    var list = solrAdmin.Status();
                    return true;
                }

                return false;
            }
            catch (SolrConnectionException solrException)
            {
                if (solrException.Message.Contains("java.lang.IllegalStateException") && solrException.Message.Contains("appears both in delegate and in cache"))
                {
                    Log.Warn("SUPPORT: Solr STATUS check core failed. Exception is suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188", solrException);
                    return true;
                }
                CrawlingLog.Log.Warn("Unable to connect to Solr: [{0}], ".FormatWith(SolrContentSearchManager.ServiceAddress) + "the [{0}] was caught.".FormatWith(typeof(SolrConnectionException).FullName), solrException);
                return false;
            }
        }
    }
}