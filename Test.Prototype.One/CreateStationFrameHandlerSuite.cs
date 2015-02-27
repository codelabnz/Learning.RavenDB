using NodaTime;
using NodaTime.Testing;
using Prototype.One;
using Xunit;
using Shouldly;

namespace Test.Prototype.One
{
    using NServiceBus.Testing;
    using Raven.Tests.Helpers;

    public class CreateStationFrameHandlerSuite : RavenTestBase
    {
        public CreateStationFrameHandlerSuite()
        {
            var fakeClock = new FakeClock(_today);
            Clock.Current = fakeClock;
        }

        Instant _today = DateTimeZoneProviders.Tzdb.GetSystemDefault()
                                                    .AtStrictly(new LocalDateTime(2015, 01, 15, 00, 00))
                                                    .ToInstant();

        [Fact]
        public void AddStationFrameCommand()
        {
            var session = NewDocumentStore().OpenSession();
            Test.Initialize();

            Test.Handler<CreateStationFrameHandler>(b => new CreateStationFrameHandler(b, session))
                .ExpectPublish<StationFrameCreated>(e => true)
                .OnMessage<CreateStationFrame>(c =>
                {
                    c.ContractId = "contracts/1";
                    c.Month = Clock.Today;
                    c.StationIds = new[] { 20, 25 };
                });

            session.Load<StationFrame>("stationframes/1")
                    .Stations.ShouldNotBeEmpty();
        }
    }
}
