using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using Prototype.One;
using Prototype.One.Extensions;
using Shouldly;
using Xunit;

namespace Test.Prototype.One
{
    public class StandardSpotBookingSuite
    {
        [Fact]
        public void create_spot_booking_for_single_station()
        {
            var spotCount = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var stationId = new StationId { Id = "stations/1" };

            var booking = new SpotBooking(spotCount, airingOn, stationId);

            booking.BookedSpots.ShouldBe(spotCount);
            booking.AiringOn.ShouldBe(airingOn);
            booking.Stations.ShouldContain(stationId);
        }

        [Fact]
        public void create_spot_booking_for_combo_stations()
        {
            var spotCount = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(spotCount, airingOn, stationIds);

            booking.BookedSpots.ShouldBe(spotCount);
            booking.AiringOn.ShouldBe(airingOn);
            booking.Stations.ShouldContain(stationIds[0]);
            booking.Stations.ShouldContain(stationIds[1]);
        }

        [Fact]
        public void spot_booking_for_combo_stations_total_spots_returns_count_across_stations()
        {
            var spotCount = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(spotCount, airingOn, stationIds);

            booking.TotalSpots.ShouldBe(spotCount * stationIds.Count());
        }

        [Fact]
        public void create_spot_booking_with_no_spots_throws()
        {
            var spotCount = 0;
            var airingOn = Clock.Today.PlusDays(5);
            var stationId = new StationId { Id = "stations/1" };

            Action createBooking = () => new SpotBooking(spotCount, airingOn, stationId);

            Should.Throw<ArgumentOutOfRangeException>(createBooking)
                    .ParamName.ShouldBe("spots");
        }

        [Fact]
        public void create_spot_booking_on_past_date_throws()
        {
            var spotCount = 5;
            var airingOn = Clock.Today.PlusDays(-5);
            var stationId = new StationId { Id = "stations/1" };

            Action createBooking = () => new SpotBooking(spotCount, airingOn, stationId);

            Should.Throw<ArgumentOutOfRangeException>(createBooking)
                    .ParamName.ShouldBe("airingOn");
        }

        [Fact]
        public void change_number_of_spots_for_booking()
        {
            int originalCount = 5, newCount = 8;
            var airingOn = Clock.Today.PlusDays(5);
            var stationId = new StationId { Id = "stations/1" };

            var booking = new SpotBooking(originalCount, airingOn, stationId);
            booking.ChangeBooking(newCount);

            booking.BookedSpots.ShouldBe(newCount);
        }

        [Fact]
        public void amend_booking_for_station_not_in_booking_combo_throws()
        {
            int originalCount = 5, amendedCount = 6;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
            var amendmentStationId = new StationId { Id = "stations/3" };

            var booking = new SpotBooking(originalCount, airingOn, stationIds);
            var amendment = new BookingAmendment(amendmentStationId, amendedCount);
            Action amend = () => booking.AmendBooking(amendment);

            Should.Throw<InvalidOperationException>(amend);
        }

        [Fact]
        public void amend_booking_for_station_in_combo_results_in_amended_booking()
        {
            int originalCount = 5, amendedCount = 6;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(originalCount, airingOn, stationIds);
            var amendment = new BookingAmendment(stationIds[1], amendedCount);
            booking.AmendBooking(amendment);

            booking.BookedSpots.ShouldBe(originalCount);
            booking.Amended.ShouldBe(true);
        }

        [Fact]
        public void booking_has_correct_total_spots_after_addition_amendment()
        {
            int originalCount = 5, amendmentAdjustment = 1;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(originalCount, airingOn, stationIds);
            var amendment = new BookingAmendment(stationIds[1], amendmentAdjustment);
            booking.AmendBooking(amendment);

            booking.BookedSpots.ShouldBe(originalCount);
            booking.TotalSpots.ShouldBe((booking.BookedSpots * stationIds.Count()) + amendmentAdjustment);
        }

        [Fact]
        public void booking_has_correct_total_spots_after_removal_amendment()
        {
            int originalCount = 5, amendmentAdjustment = -2;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(originalCount, airingOn, stationIds);
            var amendment = new BookingAmendment(stationIds[1], amendmentAdjustment);
            booking.AmendBooking(amendment);

            booking.BookedSpots.ShouldBe(originalCount);
            booking.TotalSpots.ShouldBe((booking.BookedSpots * stationIds.Count()) + amendmentAdjustment);
        }

        [Fact]
        public void booking_can_be_amended_multiple_times()
        {
            int originalCount = 5, amendmentOneAdjustment = -2, amendmentTwoAdjustment = -1;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(originalCount, airingOn, stationIds);
            var amendmentOne = new BookingAmendment(stationIds[1], amendmentOneAdjustment);
            booking.AmendBooking(amendmentOne);
            var amendmentTwo = new BookingAmendment(stationIds[1], amendmentTwoAdjustment);
            booking.AmendBooking(amendmentTwo);

            booking.BookedSpots.ShouldBe(originalCount);
            booking.TotalSpots.ShouldBe((booking.BookedSpots * stationIds.Count()) + amendmentOneAdjustment + amendmentTwoAdjustment);
        }

        [Fact]
        public void booking_not_amended_when_multiple_amendments_have_zero_total_effect()
        {
            int originalCount = 5, amendmentOneAdjustment = -2, amendmentTwoAdjustment = 2;
            var airingOn = Clock.Today.PlusDays(5);
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            var booking = new SpotBooking(originalCount, airingOn, stationIds);
            var amendmentOne = new BookingAmendment(stationIds[1], amendmentOneAdjustment);
            booking.AmendBooking(amendmentOne);
            var amendmentTwo = new BookingAmendment(stationIds[1], amendmentTwoAdjustment);
            booking.AmendBooking(amendmentTwo);

            booking.Amended.ShouldBe(false);
            booking.TotalSpots.ShouldBe((booking.BookedSpots * stationIds.Count()));
        }

        //[Fact]
        //public void reduce_number_of_spots_for_booking_should_raise_spots_removed()
        //{
        //    throw new Exception("TODO");
        //}

        //[Fact]
        //public void reduce_number_of_spots_for_booking_should_raise_spots_added()
        //{
        //    throw new Exception("TODO");
        //}
    }

    public class SpotBooking
    {
        SpotBooking(int spots, LocalDate airingOn)
        {
            if (spots <= 0) throw new ArgumentOutOfRangeException("spots");
            if (airingOn < Clock.Today) throw new ArgumentOutOfRangeException("airingOn");

            BookedSpots = spots;
            AiringOn = airingOn;

            _amendments = new List<BookingAmendment>();
        }

        public SpotBooking(int spots, LocalDate airingOn, IEnumerable<StationId> stations)
            : this(spots, airingOn)
        {
            _stations = new List<StationId>(stations);
        }

        public SpotBooking(int spots, LocalDate airingOn, StationId station)
            : this(spots, airingOn)
        {
            _stations = new List<StationId>();
            _stations.Add(station);
        }

        List<StationId> _stations;
        List<BookingAmendment> _amendments;

        public int BookedSpots { get; private set; }
        public LocalDate AiringOn { get; private set; }
        public IEnumerable<StationId> Stations { get { return _stations; } }
        public bool Amended { get; private set; }

        public int TotalSpots
        {
            get
            {
                if (!_amendments.Any())
                    return BookedSpots * _stations.Count;

                return (from station in _stations
                        let amendments = _amendments.Where(a => a.Station == station)
                        select new
                        {
                            Station = station,
                            Spots = BookedSpots + amendments.Sum(a => a.Adjustment)
                        }).Sum(s => s.Spots);
            }
        }

        public void ChangeBooking(int spots)
        {
            BookedSpots = spots;
        }

        public void AmendBooking(BookingAmendment amendment)
        {
            if (_stations.DoesNotContain(amendment.Station)) throw new InvalidOperationException("Station {0} not included in booking".Format((object)amendment.Station.Id));

            _amendments.Add(amendment);
            CheckTotalAmendmentEffect();
        }

        void CheckTotalAmendmentEffect()
        {
            Amended = (from station in _stations
                       let amendments = _amendments.Where(a => a.Station == station)
                       select BookedSpots + amendments.Sum(a => a.Adjustment))
                       .Any(total => total != BookedSpots);
        }
    }

    public class BookingAmendment
    {
        public BookingAmendment(StationId station, int adjustment)
        {
            Station = station;
            Adjustment = adjustment;
        }

        public StationId Station { get; private set; }
        public int Adjustment { get; private set; }
    }
}
