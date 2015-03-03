using System.Linq;
using System.Collections.Generic;
using Raven.Client;
using NodaTime;

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
            return null;// new SpotLine(this.Id);
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

    //public class StationId
    //{
    //    public string Id { get; set; }

    //}

    public class SpotLine
    {
        public SpotLine()
        {
            _bookings = new List<Booking>();
        }

        //public SpotLine(IEnumerable<StationId> stations) { }

        //public SpotLine(string frameId)
        //{
        //    FrameId = frameId;
        //}

        //public string FrameId { get; private set; }

        public LocalDate? StartDate { get; private set; }
        public LocalDate? EndDate { get; private set; }

        List<Booking> _bookings;
        public IEnumerable<Booking> Bookings { get { return _bookings.AsReadOnly(); } }

        public void PlaceBooking(Booking booking)
        {
            CheckDates(booking.AiringOn);

            _bookings.Add(booking);
        }

        void CheckDates(LocalDate newBookingDate)
        {
            if (IsBeforeStartDate(newBookingDate))
                StartDate = newBookingDate;

            if (IsAfterEndDate(newBookingDate))
                EndDate = newBookingDate;
        }

        bool IsAfterEndDate(LocalDate newBookingDate)
        {
            return !EndDate.HasValue || newBookingDate > EndDate;
        }

        bool IsBeforeStartDate(LocalDate newBookingDate)
        {
            return !StartDate.HasValue || newBookingDate < StartDate;
        }
    }

    public class Booking
    {
        Booking(int spots, LocalDate airingOn)
        {
            Spots = spots;
            AiringOn = airingOn;
        }

        public int Spots { get; private set; }
        public LocalDate AiringOn { get; private set; }

        public static Booking For(int spotCount, LocalDate airDate)
        {
            return new Booking(spotCount, airDate);
        }
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
