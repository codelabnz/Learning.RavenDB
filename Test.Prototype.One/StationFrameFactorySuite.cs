using System.Linq;
using Prototype.One;
using Shouldly;
using Test.Prototype.One.Data;
using Test.Prototype.One.Stubs;
using Xunit;

namespace Test.Prototype.One
{
    public class StationFrameFactorySuite : DriveRavenTestBase
    {
        public StationFrameFactorySuite()
        {
            StationData.Add(_store);
        }

        [Fact]
        public void create_station_frame_for_single_station()
        {
            //
            var stations = new[] { "stations/1" };
            var generatorStub = StubStationDescriptionGenerator.WithReturnDescription("");
            var factory = new StationFrameFactory(generatorStub, _session);

            //
            var frame = factory.FrameForStations(stations);

            //
            frame.Stations.Count().ShouldBe(1);
            frame.Stations.First().Id.ShouldBe(stations[0]);
        }

        [Fact]
        public void create_station_frame_for_multiple_stations()
        {
            //
            var stations = new[] { "stations/1", "stations/2" };
            var generatorStub = StubStationDescriptionGenerator.WithReturnDescription("");
            var factory = new StationFrameFactory(generatorStub, _session);

            //
            var frame = factory.FrameForStations(stations);

            //
            frame.Stations.Count().ShouldBe(2);
            frame.Stations.First().Id.ShouldBe(stations[0]);
            frame.Stations.Skip(1).First().Id.ShouldBe(stations[1]);
        }

        [Fact]
        public void station_frame_should_not_contain_duplicate_stations()
        {
            //
            var stations = new[] { "stations/1", "stations/1" };
            var generatorStub = StubStationDescriptionGenerator.WithReturnDescription("");
            var factory = new StationFrameFactory(generatorStub, _session);

            //
            var frame = factory.FrameForStations(stations);

            frame.Stations.Count().ShouldBe(1);
            frame.Stations.First().Id.ShouldBe(stations[0]);

        }

        [Fact]
        public void frame_factory_uses_station_description_generator_to_generate_description()
        {
            //
            var stations = new[] { "stations/1", "stations/2" };
            var description = "WKO(MORE, EDGE)";
            var generatorStub = StubStationDescriptionGenerator.WithReturnDescription(description);
            var factory = new StationFrameFactory(generatorStub, _session);

            //
            var frame = factory.FrameForStations(stations);

            //
            frame.StationDescription.ShouldBe(description);
            generatorStub.WasInvoked.ShouldBe(true);
        }
    }
}
