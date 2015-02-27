using System;
using System.Diagnostics;
using System.Linq;
using NodaTime;
using Raven.Client;
using Raven.Tests.Helpers;
using Xunit;

namespace Learning.RavenDB
{
    public class QueryRaven : RavenTestBase
    {
        [Fact]
        public void Query()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    // No static index defined on Month column of ContractSpotLine documents so
                    // this query will generate a dynamic index. That index will be created and populated
                    // by RavenDB before the results of the query are returned.
                    var spotLines = session.Query<LearningContractSpotLine>()
                                            .Where(c => c.Month == new LocalDate(2015, 02, 01))
                        // "Safe By Default" enforces default page size of 128
                        //.Take(128)
                                            .ToList();
                }
            }
        }
    }

    public class QueryRavenWithStatistics : RavenTestBase
    {
        [Fact]
        public void WithStatistics()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    // declare the statistics container before registering it with the query
                    // via the .Statistics(out statistics) call
                    RavenQueryStatistics statistics;
                    var spotLines = session.Query<LearningContractSpotLine>()
                                            .Statistics(out statistics)
                                            .Where(c => c.Month == new LocalDate(2015, 02, 01))
                                            .ToList();


                    // interrogate the statistics of the query...
                    //statistics.TotalResults
                }
            }
        }
    }

    public class QueryRavenGetTotalResultSetSizeWhenPaging : RavenTestBase
    {
        [Fact]
        public void GetTotalResultSetSizeWhenPaging()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    RavenQueryStatistics statistics;
                    var spotLines = session.Query<LearningContractSpotLine>()
                                            .Statistics(out statistics)
                                            .Where(c => c.Month == new LocalDate(2015, 02, 01))
                        // get page 3 assuming page size of 10 - skip 2 pages and take next 10
                                            .Skip(20)
                                            .Take(10)
                                            .ToList();

                    // total results property of query statistics exposes total count of docs
                    // that match the query criteria, ignoring paging
                    //statistics.TotalResults

                }
            }
        }
    }

    public class QueryRavenCheckingForStaleResults : RavenTestBase
    {

        [Fact]
        public void CheckingForStaleResults()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    RavenQueryStatistics statistics;
                    // as soon as we use Query(), we are hitting an index (dynamic or static) - Load() doesn't use an index
                    // and so results from Load will never be stale
                    var spotLines = session.Query<LearningContractSpotLine>()
                                            .Statistics(out statistics)
                                            .Where(c => c.Month == new LocalDate(2015, 02, 01))
                                            .ToList();

                    // check whether the returned results are stale or not
                    if (statistics.IsStale)
                    { }
                }
            }
        }
    }

    public class QueryRavenWaitForNonStaleResults : RavenTestBase
    {
        [Fact]
        public void WaitForNonStaleResults()
        {
            using (var documentStore = NewDocumentStore())
            {
                using (var session = documentStore.OpenSession())
                {
                    RavenQueryStatistics statistics;
                    var spotLines = session.Query<LearningContractSpotLine>()
                                            .Statistics(out statistics)
                        // configure the query to wait for non-stale results i.e. the index that is being query
                        // should be up to date (generally shouldn't need this outside of unit tests) -> embrace eventual consistency
                                            .Customize(q => q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                                            .Where(c => c.Month == new LocalDate(2015, 02, 01))
                                            .ToList();

                    // can also use WaitForNonStaleResultsAsOf(new DateTime(2015, 2, 1, 10, 0, 0, 0)) to wait for results up
                    // to a point in time - all pending change tasks after this cut off will not be considered
                }
            }
        }
    }
}
