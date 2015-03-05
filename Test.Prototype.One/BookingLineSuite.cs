using System;
using System.Collections.Generic;
using NodaTime;
using NodaTime.Testing;
using Prototype.One;
using Shouldly;
using Test.Prototype.One.Data;
using Xunit;

namespace Test.Prototype.One
{
    public class BookingLineSuite
    {
        public BookingLineSuite() { }

        [Fact]
        public void create_line_creates_station_added_event()
        {
            //
            var station = Builder.Station.Build();

            //
            var line = new BookingLine(station);

            //
            line.Station.ShouldBe(station);
            line.GetUncommittedEvents().ShouldContain(e => (e as BookingLineCreated) != null
                                                            && (e as BookingLineCreated).AggregateId == line.Id
                                                            && (e as BookingLineCreated).Station == station);
        }

        [Fact]
        public void add_spots_to_line_creates_spots_added_event()
        {
            //
            var quantity = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(station).Build();

            //
            line.AddSpots(quantity, airingOn);

            //
            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsAdded) != null
                                                            && (e as SpotsAdded).AggregateId == line.Id
                                                            && (e as SpotsAdded).Count == quantity
                                                            && (e as SpotsAdded).AiringOn == airingOn);
        }

        [Fact]
        public void remove_spots_from_line_creates_spots_removed_event()
        {
            //
            int initinalQuantity = 5, removeQuantity = 2;
            var airingOn = Clock.Today.PlusDays(5);
            var line = Builder.BookingLine.WithSpots(initinalQuantity, airingOn).Build();

            //
            line.RemoveSpots(removeQuantity, airingOn);

            //
            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsRemoved) != null
                                                            && (e as SpotsRemoved).AggregateId == line.Id
                                                            && (e as SpotsRemoved).Count == removeQuantity
                                                            && (e as SpotsRemoved).AiringOn == airingOn);
        }

        [Fact]
        public void remove_spots_more_spots_than_are_booked_throws()
        {
            //
            int initinalQuantity = 5, removeQuantity = 6;
            var airingOn = Clock.Today.PlusDays(5);
            var line = Builder.BookingLine.WithSpots(initinalQuantity, airingOn).Build();

            //
            Action act = () => line.RemoveSpots(removeQuantity, airingOn);

            //
            Should.Throw<InvalidOperationException>(act);
        }

        [Fact]
        public void change_station_for_line_creates_station_changed_event()
        {
            //
            StationId initialStation = Builder.Station.Build(), newStation = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(initialStation).Build();

            //
            line.ChangeStation(newStation);

            //
            line.Station.ShouldBe(newStation);
            line.GetUncommittedEvents().ShouldContain(e => (e as BookingLineStationChanged) != null
                                                            && (e as BookingLineStationChanged).AggregateId == line.Id
                                                            && (e as BookingLineStationChanged).Station == newStation);
        }

        [Fact]
        public void move_bookings_by_number_of_months_bookings_should_fall_on_same_day_of_week()
        {
            //
            Testing.Today(new LocalDate(2015, 03, 05));
            var firstBookingDate = Clock.Today.PlusDays(1);
            var secondBookingDate = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(station)
                                        .WithSpots(5, firstBookingDate)
                                        .WithSpots(5, secondBookingDate).Build();
            
            var duration = Duration.FromStandardWeeks(4);

            //
            //line.MoveBookingsBy(duration);

            //
            //line.Bookings
            throw new Exception("to complete");
        }

        [Fact]
        public void x()
        {
            // to move between to dates
            var d1 = new LocalDate(2015, 06, 01);
            var d2 = new LocalDate(2015, 10, 01);

            // find the number of days between the dates
            var days = Period.Between(d1, d2, PeriodUnits.Days).Days;

            // determine how many FULL weeks fall between the dates
            days = days - (days % 7);

            // add the resulting number of weeks (as days) to each booking in the existing line
        }
    }

    public class BookingLine : Aggregate
    {
        protected BookingLine()
        {
            _bookings = new _Bookings();
        }

        public BookingLine(StationId stationId)
            : this()
        {
            Station = stationId;
            RaiseEvent(new BookingLineCreated(stationId));
        }

        _Bookings _bookings;

        public StationId Station { get; private set; }

        public void AddSpots(int count, LocalDate airingOn)
        {
            IncreaseBooking(count, airingOn);
            RaiseEvent(new SpotsAdded(count, airingOn));
        }

        public void RemoveSpots(int count, LocalDate airingOn)
        {
            DecreaseBooking(count, airingOn);
            RaiseEvent(new SpotsRemoved(count, airingOn));
        }

        public void MoveBookingsBy(Duration duration)
        {
            //foreach(var booking in _bookings.All())
            //    _bookings.Move(booking, booking.)
        }

        public void ChangeStation(StationId newStation)
        {
            Station = newStation;
            RaiseEvent(new BookingLineStationChanged(Station));
        }

        void IncreaseBooking(int count, LocalDate airingOn)
        {
            _bookings[airingOn] = _bookings[airingOn].Add(count);
        }

        void DecreaseBooking(int count, LocalDate airingOn)
        {
            _bookings[airingOn] = _bookings[airingOn].Remove(count);
        }

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

            public IEnumerable<_Booking> All()
            {
                return _bookings.Values;
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

        public override string ToString()
        {
            return Id;
        }

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

    public abstract class DomainEvent
    {
        public string AggregateId { get; set; }
    }

    public class BookingLineCreated : DomainEvent
    {
        public BookingLineCreated(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }

    public class BookingLineStationChanged : DomainEvent
    {
        public BookingLineStationChanged(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }

    public class SpotsAdded : DomainEvent
    {
        public SpotsAdded(int count, LocalDate airingOn)
        {
            Count = count;
            AiringOn = airingOn;
        }

        public int Count { get; private set; }
        public LocalDate AiringOn { get; private set; }
    }

    public class SpotsRemoved : DomainEvent
    {
        public SpotsRemoved(int count, LocalDate airingOn)
        {
            Count = count;
            AiringOn = airingOn;
        }

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

        public virtual IEnumerable<DomainEvent> GetUncommittedEvents()
        {
            foreach (var @event in _events)
                @event.AggregateId = Id;

            return _events.ToArray();
        }
    }

    // is this useful for wrapping Aggregate Ids when used as references?
    // e.g. see StationBooking.Lines
    public abstract class AggregateId
    {
        public string Id { get; set; }

        public override string ToString()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            var other = obj as AggregateId;
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

    #endregion
}
