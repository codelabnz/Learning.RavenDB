using System.Collections.Generic;
using System.Linq;
using NodaTime;
using NodaTime.Testing;
using Prototype.One;
using Shouldly;
using Xunit;

namespace Test.Prototype.One
{
    public class LineBookingSuite
    {
        public LineBookingSuite()
        {
            var today = DateTimeZoneProviders.Tzdb.GetSystemDefault()
                                                    .AtStrictly(new LocalDateTime(2015, 02, 05, 00, 00))
                                                    .ToInstant();
            Clock.Current = new FakeClock(today);
        }

        [Fact]
        public void create_line_for_single_station()
        {
            var stationId = new StationId { Id = "stations/1" };

            var line = new BookingLine(stationId);

            line.Stations.ShouldContain(stationId);
        }

        [Fact]
        public void create_line_for_combo_of_stations()
        {
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var line = new BookingLine(stationIds);

            stationIds.ShouldAllBe(s => line.Stations.Contains(s));
        }

        [Fact]
        public void add_booking_to_line_for_single_station()
        {
            var quantity = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var stationId = new StationId { Id = "stations/1" };
            var line = new BookingLine(stationId);

            line.AddBooking(quantity, airingOn);

            line.Bookings[airingOn].ShouldBe(quantity);
            line.TotalSpots.ShouldBe(quantity);
        }

        public void amend_booking_for_combo_results_in_amended_booking()
        {
            int quantity = 5, remove = 2;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
            var line = new BookingLine(stationIds);

            line.AddBooking(quantity, airingOn);
            line.Bookings[airingOn].RemoveSpotsFromStation(stationIds[0], remove);

            line.TotalSpots.ShouldBe((quantity * stationIds.Length) - remove);
        }

    }

    public class BookingLine
    {
        BookingLine()
        {
            Bookings = new _Bookings();
        }

        public BookingLine(StationId stationId)
            : this()
        {
            _stations = new List<StationId> { stationId };
        }

        public BookingLine(IEnumerable<StationId> stationIds)
            : this()
        {
            _stations = new List<StationId>(stationIds);
        }

        List<StationId> _stations;
        public IEnumerable<StationId> Stations { get { return _stations; } }

        //public int TotalSpots 
        //{
        //    get {  return Bookings}
        //    }

        public _Bookings Bookings { get; private set; }

        public void AddBooking(int quantity, LocalDate airingOn)
        {
            Bookings[airingOn] = quantity;
        }

        public class _Bookings
        {
            Dictionary<LocalDate, int> _bookings = new Dictionary<LocalDate, int>();

            public int this[LocalDate index]
            {
                get
                {
                    return _bookings[index];
                }
                internal set
                {
                    _bookings[index] = value;
                }
            }
        }
    }

    public abstract class SpBooking
    {
        public abstract int TotalSpots { get; }
    }

    public abstract class Amendment : SpBooking { }

    public class StandardBooking : SpBooking { }

    public class SpotRemovalAmendment : Amendment
    {
        int _adjustment;
        SpBooking _original;
        public SpotRemovalAmendment(SpBooking original, int removalAdjustment)
        {
            _original = original;
            _adjustment = removalAdjustment;
        }

        public override int TotalSpots
        {
            get { return _original.TotalSpots + _adjustment; }
        }
    }
}
