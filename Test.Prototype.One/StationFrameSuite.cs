using System.Linq;
using Prototype.One;
using Shouldly;
using Xunit;

namespace Test.Prototype.One
{
    public class StationFrameSuite
    {
        [Fact]
        public void add_spot_line_to_station_frame()
        {
            var stations = new[] { new Station { Id = "stations/1", Code = "WKOMORE" }, new Station { Id = "stations/2", Code = "WKOEDGE" } };
            var frame = StationFrame.ForStations(stations, "");
            frame.Id = "stationframes/1";

            var line = frame.AddLine();

            line.ShouldNotBe(null);
            //line.FrameId.ShouldBe(frame.Id);
        }
    }
}
