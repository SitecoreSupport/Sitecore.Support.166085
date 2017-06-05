namespace Sitecore.Support.ContentSearch.SolrProvider
{
    using System.Linq;
    using Sitecore.ContentSearch.Maintenance;
    using Sitecore.ContentSearch.SolrProvider;
    using Sitecore.ContentSearch.SolrProvider.SolrNetIntegration;
    using SolrNet;
    using SolrNet.Impl;

    public class SolrSearchIndex : Sitecore.ContentSearch.SolrProvider.SolrSearchIndex
    {
        internal ISolrCoreAdmin solrAdmin;
        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore, string @group) : base(name, core, propertyStore, @group)
        {
        }

        public SolrSearchIndex(string name, string core, IIndexPropertyStore propertyStore) : base(name, core, propertyStore)
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