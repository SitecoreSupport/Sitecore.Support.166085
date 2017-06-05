namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch.Diagnostics;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.Diagnostics;
    using Sitecore.StringExtensions;
    using SolrNet;
    using SolrNet.Exceptions;
    using System.Reflection;
    public static class SolrStatus
    {
        static SolrStatus()
        {
            MethodInfo methodInfo = typeof(Sitecore.ContentSearch.SolrProvider.SolrStatus).GetProperty("InitStatusOk").GetSetMethod(true);
            methodInfo.Invoke(null, new object[] {OkSolrStatus()});
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
                    Log.Warn("SUPPORT: Solr STATUS check core failed. Error suppressed as not related to Solr core availability. Details: https://issues.apache.org/jira/browse/LUCENE-7188", solrException);
                    return true;
                }
                CrawlingLog.Log.Warn("Unable to connect to Solr: [{0}], ".FormatWith(SolrContentSearchManager.ServiceAddress) + "the [{0}] was caught.".FormatWith(typeof(SolrConnectionException).FullName), solrException);
                return false;
            }
        }
    }
}