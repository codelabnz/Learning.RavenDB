using Raven.Client;
using Raven.Client.Document;
using Raven.Tests.Helpers;

namespace Test.Prototype.One
{
    public class DriveRavenTestBase : RavenTestBase
    {
        public DriveRavenTestBase()
        {
            _store = NewDocumentStore();
            _store.Conventions.DefaultQueryingConsistency = ConsistencyOptions.AlwaysWaitForNonStaleResultsAsOfLastWrite;

            _session = _store.OpenSession();
        }

        protected IDocumentStore _store;
        protected IDocumentSession _session;

        public override void Dispose()
        {
            _session.Dispose();
            _store.Dispose();

            base.Dispose();
        }
    }
}
