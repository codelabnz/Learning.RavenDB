using System;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using NodaTime.Testing;
using Prototype.One;
using Prototype.One.Extensions;
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

        //[Fact]
        //public void create_line_for_single_station_creates_station_added_event()
        //{
        //    var stationId = new StationId { Id = "stations/1" };

        //    var line = new BookingLine(stationId);

        //    line.Stations.ShouldContain(stationId);
        //    line.GetUncommittedEvents().ShouldContain(e => (e as StationAddedEvent) != null
        //                                                    && (e as StationAddedEvent).AggregateId == line.Id
        //                                                    && (e as StationAddedEvent).Station == stationId);
        //}

        //[Fact]
        //public void create_line_for_combo_of_stations_creates_all_station_added_events()
        //{
        //    var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

        //    var line = new BookingLine(stationIds);

        //    stationIds.ShouldAllBe(s => line.Stations.Contains(s));
        //    line.GetUncommittedEvents().ShouldContain(e => (e as StationAddedEvent) != null
        //                                                    && (e as StationAddedEvent).AggregateId == line.Id
        //                                                    && (e as StationAddedEvent).Station == stationIds[0]);
        //    line.GetUncommittedEvents().ShouldContain(e => (e as StationAddedEvent) != null
        //                                                    && (e as StationAddedEvent).AggregateId == line.Id
        //                                                    && (e as StationAddedEvent).Station == stationIds[1]);
        //}

        [Fact]
        public void add_spots_to_line_creates_spots_added_event()
        {
            var quantity = 5;
            var airingOn = Clock.Today.PlusDays(5);
            //var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
            //var line = new BookingLine(stationIds);
            var line = new BookingLine();

            line.AddSpots(quantity, airingOn);

            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsAddedEvent) != null
                                                            && (e as SpotsAddedEvent).AggregateId == line.Id
                                                            && (e as SpotsAddedEvent).Count == quantity
                                                            && (e as SpotsAddedEvent).AiringOn == airingOn);
        }

        [Fact]
        public void remove_spots_from_line_creates_spots_removed_event()
        {
            int initinalQuantity = 5, removeQuantity = 2;
            var airingOn = Clock.Today.PlusDays(5);
            //var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
            //var line = new BookingLine(stationIds);
            var line = new BookingLine();
            line.AddSpots(initinalQuantity, airingOn);

            line.RemoveSpots(removeQuantity, airingOn);

            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsRemovedEvent) != null
                                                            && (e as SpotsRemovedEvent).AggregateId == line.Id
                                                            && (e as SpotsRemovedEvent).Count == removeQuantity
                                                            && (e as SpotsRemovedEvent).AiringOn == airingOn);
        }

        [Fact]
        public void remove_spots_more_spots_than_are_booked_throws()
        {
            int initinalQuantity = 5, removeQuantity = 6;
            var airingOn = Clock.Today.PlusDays(5);
            //var stationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
            //var line = new BookingLine(stationIds);
            var line = new BookingLine();
            line.AddSpots(initinalQuantity, airingOn);

            Action act = () => line.RemoveSpots(removeQuantity, airingOn);

            Should.Throw<InvalidOperationException>(act);
        }

        //[Fact]
        //public void change_stations_for_line_creates_station_added_events()
        //{
        //    var initialStationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
        //    var line = new BookingLine(initialStationIds);
        //    var changedStationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/3" } };

        //    line.ChangeStations(changedStationIds);

        //    line.Stations.ShouldBe(changedStationIds);
        //    line.GetUncommittedEvents().ShouldContain(e => (e as StationAddedEvent) != null
        //                                                    && (e as StationAddedEvent).AggregateId == line.Id
        //                                                    && (e as StationAddedEvent).Station == changedStationIds[1]);
        //}

        //[Fact]
        //public void change_stations_for_line_creates_station_removed_events()
        //{
        //    var initialStationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };
        //    var line = new BookingLine(initialStationIds);
        //    var changedStationIds = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/3" } };

        //    line.ChangeStations(changedStationIds);

        //    line.GetUncommittedEvents().ShouldContain(e => (e as StationRemovedEvent) != null
        //                                                    && (e as StationRemovedEvent).AggregateId == line.Id
        //                                                    && (e as StationRemovedEvent).Station == initialStationIds[1]);
        //}
    }

    public class BookingLine : Aggregate
    {
        public BookingLine()
        {
            _bookings = new _Bookings();
        }

        //BookingLine()
        //{
        //    _stations = new List<StationId>();
        //    _bookings = new _Bookings();
        //}

        //public BookingLine(StationId stationId)
        //    : this()
        //{
        //    SetStations(new[] { stationId });
        //}

        //public BookingLine(IEnumerable<StationId> stationIds)
        //    : this()
        //{
        //    SetStations(stationIds);
        //}

        _Bookings _bookings;

        //List<StationId> _stations;
        //public IEnumerable<StationId> Stations { get { return _stations; } }

        public void AddSpots(int count, LocalDate airingOn)
        {
            IncreaseBooking(count, airingOn);
            RaiseEvent(new SpotsAddedEvent(this.Id, count, airingOn));
        }

        public void RemoveSpots(int count, LocalDate airingOn)
        {
            DecreaseBooking(count, airingOn);
            RaiseEvent(new SpotsRemovedEvent(this.Id, count, airingOn));
        }

        //public void ChangeStations(IEnumerable<StationId> newStationIds)
        //{
        //    SetStations(newStationIds);
        //}

        void IncreaseBooking(int count, LocalDate airingOn)
        {
            _bookings[airingOn] = _bookings[airingOn].Add(count);
        }

        void DecreaseBooking(int count, LocalDate airingOn)
        {
            _bookings[airingOn] = _bookings[airingOn].Remove(count);
        }

        //void SetStations(IEnumerable<StationId> stationIds)
        //{
        //    AddStations(stationIds);
        //    RemoveStations(stationIds);
        //}

        //void AddStations(IEnumerable<StationId> stationIds)
        //{
        //    foreach (var station in _stations.Where(s => stationIds.DoesNotContain(s))
        //                                    .ToArray())
        //    {
        //        _stations.Remove(station);
        //        RaiseEvent(new StationRemovedEvent(Id, station));
        //    }
        //}

        //void RemoveStations(IEnumerable<StationId> stationIds)
        //{
        //    foreach (var station in stationIds.Where(s => _stations.DoesNotContain(s)))
        //    {
        //        _stations.Add(station);
        //        RaiseEvent(new StationAddedEvent(Id, station));
        //    }
        //}

        class _Bookings
        {
            Dictionary<LocalDate, _Booking> _bookings = new Dictionary<LocalDate, _Booking>();

            public _Booking this[LocalDate index]
            {
                get
                {
                    return _bookings.ContainsKey(index) ? _bookings[index] : _Booking.Empty();
                }
                internal set
                {
                    _bookings[index] = value;
                }
            }
        }
    }

    public class _Booking
    {
        internal _Booking(int count)
        {
            Count = count;
        }

        public int Count { get; private set; }

        public _Booking Add(int toAdd)
        {
            return new _Booking(Count + toAdd);
        }

        public _Booking Remove(int toRemove)
        {
            var newCount = Count - toRemove;
            if (newCount < 0) throw new InvalidOperationException();

            return new _Booking(newCount);
        }

        public static _Booking Empty()
        {
            return new _Booking(0);
        }
    }

    public class StationId
    {
        public string Id { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as StationId;
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

    #region events

    public class DomainEvent
    { }

    public class StationAddedEvent : DomainEvent
    {
        public StationAddedEvent(string aggregateId, StationId station)
        {
            AggregateId = aggregateId;
            Station = station;
        }

        public string AggregateId { get; private set; }
        public StationId Station { get; private set; }
    }

    public class StationRemovedEvent : DomainEvent
    {
        public StationRemovedEvent(string aggregateId, StationId station)
        {
            AggregateId = aggregateId;
            Station = station;
        }

        public string AggregateId { get; private set; }
        public StationId Station { get; private set; }
    }

    public class SpotsAddedEvent : DomainEvent
    {
        public SpotsAddedEvent(string aggregateId, int count, LocalDate airingOn)
        {
            AggregateId = aggregateId;
            Count = count;
            AiringOn = airingOn;
        }

        public string AggregateId { get; private set; }
        public int Count { get; private set; }
        public LocalDate AiringOn { get; private set; }
    }

    public class SpotsRemovedEvent : DomainEvent
    {
        public SpotsRemovedEvent(string aggregateId, int count, LocalDate airingOn)
        {
            AggregateId = aggregateId;
            Count = count;
            AiringOn = airingOn;
        }

        public string AggregateId { get; private set; }
        public int Count { get; private set; }
        public LocalDate AiringOn { get; private set; }
    }

    #endregion

    #region aggregate...

    public abstract class Aggregate
    {
        public string Id { get; private set; }

        List<DomainEvent> _events = new List<DomainEvent>();
        protected void RaiseEvent(DomainEvent @event)
        {
            //var newVersion = this.Version + 1;
            //@event.AggregateVersion = newVersion;

            //this.uncommittedEvents.Add(@event);
            //this.Version = newVersion;
            _events.Add(@event);
        }

        public IEnumerable<DomainEvent> GetUncommittedEvents()
        {
            return _events.ToArray();
        }
    }

        #endregion
}
