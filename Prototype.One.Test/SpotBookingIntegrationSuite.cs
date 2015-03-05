using NodaTime;
using Prototype.One.Test.Data;
using Raven.Tests.Helpers;
using Xunit;

namespace Prototype.One.Test
{
    using NServiceBus.Testing;

    public class SpotBookingIntegrationSuite : RavenTestBase
    {
        public SpotBookingIntegrationSuite()
        {
            Testing.Today(new LocalDate(2015, 03, 01));
        }

        [Fact]
        public void x()
        {
            var session = NewDocumentStore().OpenSession();
            Test.Initialize();

            //Test.Handler<CreateStationFrameHandler>(b => new CreateStationFrameHandler(b, session))
            //    .ExpectPublish<StationFrameCreated>(e => true)
            //    .OnMessage<CreateStationFrame>(c =>
            //    {
            //        c.ContractId = "contracts/1";
            //        c.Month = Clock.Today;
            //        c.StationIds = new[] { 20, 25 };
            //    });

            //session.Load<StationFrame>("stationframes/1")
            //        .Stations.ShouldNotBeEmpty();
        }
    }
}
