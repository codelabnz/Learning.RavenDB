using System.Collections.Generic;
using System.Linq;
using Prototype.One.Extensions;
using Shouldly;
using Test.Prototype.One.Data;
using Xunit;

namespace Test.Prototype.One
{
    public class StationBookingSuite
    {
        [Fact]
        public void create_station_booking_creates_station_booking_added_event()
        {
            //
            var stationDescription = "MCH(ROCK, EDGE)";
            var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

            //
            var booking = new StationBooking(stationDescription, stationIds);

            //
            booking.Stations.ShouldBe(stationIds);
            booking.GetUncommittedEvents().ShouldContain(e => (e as StationBookingAddedEvent) != null
                                                            && (e as StationBookingAddedEvent).AggregateId == booking.Id
                                                            && (e as StationBookingAddedEvent).Stations.ContainsAll(stationIds));
        }

        [Fact]
        public void change_stations_creates_station_added_event()
        {
            //
            var initalStationDescription = "MCH(ROCK, EDGE)";
            var initialStationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
            var booking = new StationBooking(initalStationDescription, initialStationIds);
            var changedStationDescription = "MCH(ROCK, BREEZE)";
            var changedStationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/3" } };

            //
            booking.ChangeStations(changedStationDescription, changedStationIds);

            //
            booking.Stations.ShouldBe(changedStationIds);
            booking.GetUncommittedEvents().ShouldContain(e => (e as StationAddedEvent) != null
                                                            && (e as StationAddedEvent).AggregateId == booking.Id
                                                            && (e as StationAddedEvent).Station == changedStationIds[1]);
        }

        [Fact]
        public void add_line_to_station_booking_creates_booking_line_added_event()
        {
            //
            var booking = Builder.StationBooking.Build();
            var line = Builder.BookingLine.Build();

            //
            booking.AddBookingLine(line);

            //
            booking.Lines.ShouldContain(b => b.ToString() == line.Id);
        }
    }

    public class StationBooking : Aggregate
    {
        StationBooking()
        {
            _lines = new List<BookingLineId>();
            _stations = new List<StationId>();
        }

        public StationBooking(string stationDescription, IEnumerable<StationId> stationIds)
            : this()
        {
            _stationDescription = stationDescription;
            SetStations(stationIds);
            RaiseEvent(new StationBookingAddedEvent(Id, _stations));
        }

        string _stationDescription;

        List<StationId> _stations;
        public IEnumerable<StationId> Stations { get { return _stations; } }

        List<BookingLineId> _lines;
        public IEnumerable<BookingLineId> Lines { get { return _lines; } }

        public void ChangeStations(string stationDescription, IEnumerable<StationId> stationIds)
        {
            _stationDescription = stationDescription;
            SetStations(stationIds);
        }

        public void AddBookingLine(BookingLine line)
        {
            _lines.Add(new BookingLineId { Id = line.Id });
        }

        void SetStations(IEnumerable<StationId> stationIds)
        {
            AddStations(stationIds);
            RemoveStations(stationIds);
        }

        void AddStations(IEnumerable<StationId> stationIds)
        {
            foreach (var station in _stations.Where(s => stationIds.DoesNotContain(s))
                                            .ToArray())
            {
                _stations.Remove(station);
                RaiseEvent(new StationRemovedEvent(Id, station));
            }
        }

        void RemoveStations(IEnumerable<StationId> stationIds)
        {
            foreach (var station in stationIds.Where(s => _stations.DoesNotContain(s)))
            {
                _stations.Add(station);
                RaiseEvent(new StationAddedEvent(Id, station));
            }
        }

        
    }

    #region events

    public class StationBookingAddedEvent : DomainEvent
    {
        public StationBookingAddedEvent(string aggregateId, IEnumerable<StationId> stations)
        {
            AggregateId = aggregateId;
            Stations = stations;
        }

        public string AggregateId { get; private set; }
        public IEnumerable<StationId> Stations { get; private set; }
    }

    #endregion
}
