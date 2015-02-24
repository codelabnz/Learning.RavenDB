using System;
using System.Linq;
using NodaTime;
using Raven.Client;
using Raven.Client.Indexes;
using Raven.Client.NodaTime;
using Raven.Tests.Helpers;
using Xunit;

namespace RavenDBHacking
{
    public class ExecuteRavenQueryAgainstSimpleStaticIndex : RavenTestBase
    {
        [Fact]
        public void QuerySimpleStaticIndex()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    var spotLine = new ContractSpotLine
                    {
                        Month = new LocalDate(2015, 02, 01),
                        Contract = new Contract { Code = "11223344", Id = "contracts/12345" }
                    };
                    session.Store(spotLine);
                    session.SaveChanges();
                }

                // have to create the index in the embedded test database per test run...
                //IndexCreation.CreateIndexes(typeof(SpotLines_ByMonth).Assembly, documentStore);
                // just create the one index, not all in this assembly (as our assert checks the index used)
                new SpotLines_ByMonth().Execute(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var monthToQuery = new LocalDate(2015, 02, 01);
                    // the index to query can be explictly specified - more useful in the case of
                    // a more complex index/query
                    //var linesForMonth = session.Query<ContractSpotLine, SpotLines_ByMonth>()
                    //                            .Where(c => c.Month == monthToQuery)
                    //                            .ToList();

                    RavenQueryStatistics statistics;
                    var linesForMonth = session.Query<ContractSpotLine>()
                                            .Statistics(out statistics)
                                            .Customize(q => q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                                            .Where(c => c.Month == monthToQuery)
                                            .ToList();

                    Assert.Equal("SpotLines/ByMonth", statistics.IndexName);
                    Assert.Equal(1, linesForMonth.Count);
                }
            }
        }
    }

    public class ExecuteRavenQueryAgainstMapReduceStaticIndex : RavenTestBase
    {
        [Fact]
        public void QueryMapReduceStaticIndex()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    var spotLine = new ContractSpotLine
                    {
                        Month = new LocalDate(2015, 02, 01),
                        Contract = new Contract { Code = "11223344", Id = "contracts/12345" }
                    };
                    session.Store(spotLine);
                    session.SaveChanges();
                }

                // have to create the index in the embedded test database per test run...
                IndexCreation.CreateIndexes(typeof(SpotLines_LineCountByMonth).Assembly, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var monthToQuery = new LocalDate(2015, 02, 01);

                    RavenQueryStatistics statistics;
                    var linesForMonth = session.Query<SpotLines_LineCountByMonth.ReduceResult, SpotLines_LineCountByMonth>()
                                            .Statistics(out statistics)
                                            .Customize(q => q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                                            .FirstOrDefault(c => c.Month == monthToQuery);

                    Assert.Equal("SpotLines/LineCountByMonth", statistics.IndexName);
                    Assert.Equal(1, linesForMonth.Count);
                }
            }
        }
    }

    public class ExecuteRavenQueryAgainstFullTextAnalysedStaticIndex : RavenTestBase
    {
        [Fact]
        public void QueryFullTextStaticIndex()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    var spotLine = new ContractSpotLine
                    {
                        Month = new LocalDate(2015, 02, 01),
                        StationDescription = "WKO(MORE, ROCK, EDGE), AKL(MORE, ROCK, EDGE)",
                        Contract = new Contract { Code = "11223344", Id = "contracts/12345" }
                    };
                    session.Store(spotLine);
                    session.SaveChanges();
                }

                // have to create the index in the embedded test database per test run...
                IndexCreation.CreateIndexes(typeof(SpotLines_ByStationDescription_FullText).Assembly, documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var monthToQuery = new LocalDate(2015, 02, 01);

                    RavenQueryStatistics statistics;
                    //var linesForMonth = session.Advanced
                    //                            .LuceneQuery<ContractSpotLine>("SpotLines/ByStationDescription/FullText")
                    //                            .WaitForNonStaleResults(TimeSpan.FromSeconds(5))
                    //                            .Where("StationDescription:WKO -StationDescription:MCH")
                    //                            .Statistics(out statistics)
                    //                            .ToList();
                    // above is explicit use of LuceneQuery and is equivalent to:
                    var linesForMonth = session.Query<ContractSpotLine, SpotLines_ByStationDescription_FullText>()
                                                .Customize(q => q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                                                .Search(c => c.StationDescription, "WKO")
                                                .Search(c => c.StationDescription, "MCH", options: SearchOptions.Not | SearchOptions.And)
                                                .Statistics(out statistics)
                                                .ToList();

                    Assert.Equal("SpotLines/ByStationDescription/FullText", statistics.IndexName);
                    Assert.Equal(1, linesForMonth.Count);

                    //linesForMonth = session.Advanced
                    //                        .LuceneQuery<ContractSpotLine>("SpotLines/ByStationDescription/FullText")
                    //                        .Where("StationDescription:WKO AND StationDescription:MCH")
                    //                        .ToList();
                    linesForMonth = session.Query<ContractSpotLine, SpotLines_ByStationDescription_FullText>()
                                            .Search(c => c.StationDescription, "WKO")
                                            .Search(c => c.StationDescription, "MCH", options: SearchOptions.And)
                                            .ToList();

                    Assert.Equal(0, linesForMonth.Count);
                }
            }
        }
    }

    public class ExecuteRavenQueryAndUseSuggestionsOfStaticIndex : RavenTestBase
    {
        [Fact]
        public void QueryAndUseSuggestsionsOfStaticIndex()
        {
            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();

                using (var session = documentStore.OpenSession())
                {
                    var spotLine = new ContractSpotLine
                    {
                        Month = new LocalDate(2015, 02, 01),
                        StationDescription = "WKOMORE",
                        Contract = new Contract { Code = "11223344", Id = "contracts/12345" }
                    };
                    session.Store(spotLine);
                    session.SaveChanges();
                }

                // have to create the index in the embedded test database per test run...
                new SpotLines_ByStationDescription_FullText().Execute(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    var monthToQuery = new LocalDate(2015, 02, 01);

                    RavenQueryStatistics statistics;
                    var linesForMonth = session.Query<ContractSpotLine, SpotLines_ByStationDescription_FullText>()
                                            .Customize(q => q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                                            .Statistics(out statistics)
                                            .Where(c => c.StationDescription == "WKOE");

                    var resultCount = linesForMonth.ToList().Count;
                    Assert.Equal("SpotLines/ByStationDescription/FullText", statistics.IndexName);
                    Assert.Equal(0, resultCount);

                    if(resultCount <= 0)
                    {
                        var suggestionResult = linesForMonth.Suggest();
                        foreach (var suggestion in suggestionResult.Suggestions)
                            Assert.NotNull(suggestion);
                    }
                }
            }
        }
    }
}
