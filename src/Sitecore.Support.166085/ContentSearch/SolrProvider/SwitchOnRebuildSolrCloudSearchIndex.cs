namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System.Linq;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.SolrProvider.SolrOperations;
    using SolrNet.Impl;
    using SolrNet;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.ContentSearch.SolrProvider.SolrNetIntegration;

    public class SwitchOnRebuildSolrCloudSearchIndex :
        Sitecore.ContentSearch.SolrProvider.SwitchOnRebuildSolrCloudSearchIndex
    {
        internal ISolrCoreAdmin solrAdmin;
        public SwitchOnRebuildSolrCloudSearchIndex(string name, string mainalias, string rebuildalias,
            string activecollection, string rebuildcollection, ISolrOperationsFactory solrOperationsFactory,
            IIndexPropertyStore propertyStore)
            : base(
                name, mainalias, rebuildalias, activecollection, rebuildcollection, solrOperationsFactory, propertyStore
            )
        {
        }

        protected override CoreResult RequestStatus()
        {
            return this.solrAdmin.Status(this.ActiveCollection).Single<CoreResult>();
        }

        public override void Initialize()
        {
            this.solrAdmin = (SolrContentSearchManager.SolrAdmin as ISolrCoreAdminEx);
            base.Initialize();
        }
    }
}