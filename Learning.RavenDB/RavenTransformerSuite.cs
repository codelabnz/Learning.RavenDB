using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using Raven.Tests.Helpers;
using Raven.Client.NodaTime;
using Xunit;

namespace Learning.RavenDB
{
    public class TransformRavenQueryResult : RavenTestBase
    {
        [Fact]
        public void TransformQueryResult()
        {
            var station = new Station
            {
                Code = "WKOMORE"
            };

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                new ContractSpotLineStationTransformer().Execute(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    session.Store(station);

                    var spotLine = new ContractSpotLine
                    {
                        Month = new LocalDate(2015, 02, 01),
                        StationIds = new[] { station.Id }
                    };

                    session.Store(spotLine);

                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var results = session.Query<ContractSpotLine>()
                                                .TransformWith<ContractSpotLineStationTransformer, ContractSpotLineStationTransformer.ContractSpotLineStation>()
                                                .Customize(q => q.WaitForNonStaleResults(TimeSpan.FromSeconds(5)))
                                                .ToList();

                    Assert.Equal(results.FirstOrDefault().Station, station.Code);
                }
            }
        }
    }

    public class TransformRavenLoadResult : RavenTestBase
    {
        [Fact]
        public void TransformLoadResult()
        {
            var station = new Station
            {
                Code = "WKOMORE"
            };

            using (var documentStore = NewDocumentStore())
            {
                documentStore.ConfigureForNodaTime();
                new ContractSpotLineStationTransformer().Execute(documentStore);

                using (var session = documentStore.OpenSession())
                {
                    session.Store(station);

                    var spotLine = new ContractSpotLine
                    {
                        Month = new LocalDate(2015, 02, 01),
                        StationIds = new[] { station.Id }
                    };

                    session.Store(spotLine);

                    session.SaveChanges();
                }

                using (var session = documentStore.OpenSession())
                {
                    var result = session.Load<ContractSpotLineStationTransformer, ContractSpotLineStationTransformer.ContractSpotLineStation>("contractspotlines/1");

                    Assert.Equal(result.Station, station.Code);
                }
            }
        }
    }
}
