namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.ContentSearch.SolrProvider.SolrNetIntegration;
    using SolrNet.Impl;
    using System.Linq;
    using SolrNet;
    public class SwitchOnRebuildSolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SwitchOnRebuildSolrSearchIndex
    {
        internal ISolrCoreAdmin solrAdmin;
        public SwitchOnRebuildSolrSearchIndex(string name, string core, string rebuildcore, IIndexPropertyStore propertyStore) : base(name, core, rebuildcore, propertyStore)
        {

        }

        protected override CoreResult RequestStatus()
        {
            return this.solrAdmin.Status(base.Core).Single<CoreResult>();
        }

        public override void Initialize()
        {
            this.solrAdmin = SolrContentSearchManager.SolrAdmin as ISolrCoreAdminEx;
            base.Initialize();
        }
    }
}