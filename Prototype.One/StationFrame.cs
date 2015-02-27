using System.Linq;
using System.Collections.Generic;
using Raven.Client;

namespace Prototype.One
{
    public class StationFrame
    {
        StationFrame(IEnumerable<Station> stations, string stationDescription)
        {
            Stations = stations;
            StationDescription = stationDescription;
        }

        public string Id { get; set; }

        public string StationDescription { get; private set; }
        public IEnumerable<Station> Stations { get; private set; }

        public static StationFrame ForStations(IEnumerable<Station> stations, string stationDescription)
        {
            return new StationFrame(stations, stationDescription);
        }

        public SpotLine AddLine()
        {
            return new SpotLine(this.Id);
        }
    }

    public class Station
    {
        public string Id { get; set; }
        public string Code { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Station;
            if (other == null)
                return false;

            return other.Id == this.Id;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 29 + Id.GetHashCode();

            return hash;
        }
    }

    public class SpotLine
    {
        public SpotLine(string frameId)
        {
            FrameId = frameId;
        }

        public string FrameId { get; private set; }
    }

    public class StationFrameFactory
    {
        public StationFrameFactory(IStationDescriptionGenerator stationDescriptionGenerator, IDocumentSession session)
        {
            _stationDescriptionGenerator = stationDescriptionGenerator;
            _session = session;
        }

        IStationDescriptionGenerator _stationDescriptionGenerator;
        IDocumentSession _session;

        public StationFrame FrameForStations(IEnumerable<string> stationIds)
        {
            stationIds = stationIds.Distinct();

            var description = _stationDescriptionGenerator.GenerateDescriptionForStations(stationIds);
            var stations = _session.Load<Station>(stationIds);
            return StationFrame.ForStations(stations, description);
        }
    }

    public interface IStationDescriptionGenerator
    {
        string GenerateDescriptionForStations(IEnumerable<string> stationIds);
    }
}
