using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NodaTime;
using Prototype.One;
using Prototype.One.Extensions;
using Raven.Imports.Newtonsoft.Json;
using Shouldly;
using Prototype.One.Test.Data;
using Xunit;

namespace Prototype.One.Test
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
            line.ChangeBooking(quantity, airingOn);

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
            int initialQuantity = 5, newQuantity = 2;
            var airingOn = Clock.Today.PlusDays(5);
            var line = Builder.BookingLine.WithSpots(initialQuantity, airingOn).Build();

            //
            line.ChangeBooking(newQuantity, airingOn);

            //
            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsRemoved) != null
                                                            && (e as SpotsRemoved).AggregateId == line.Id
                                                            && (e as SpotsRemoved).Count == initialQuantity - newQuantity
                                                            && (e as SpotsRemoved).AiringOn == airingOn);
        }

        [Fact]
        public void remove_spots_more_spots_than_are_booked_throws()
        {
            //
            int initialQuantity = 5, newQuantity = -1;
            var airingOn = Clock.Today.PlusDays(5);
            var line = Builder.BookingLine.WithSpots(initialQuantity, airingOn).Build();

            //
            Action act = () => line.ChangeBooking(newQuantity, airingOn);

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
            var moveTo = firstBookingDate.MonthBegin().PlusMonths(3);


            //
            line.MoveTo(moveTo);

            //
            line.SpotBookings.GroupBy(b => b.AiringOn.DayOfWeek)
                                .Select(b => new
                                {
                                    DayOfWeek = b.Key,
                                    SpotCount = b.Sum(s => s.Count)
                                });
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
            _bookings = new Bookings();
        }

        public BookingLine(StationId stationId)
            : this()
        {
            Station = stationId;
            RaiseEvent(new BookingLineCreated(stationId));
        }

        Bookings _bookings;
        public IReadOnlyCollection<Booking> SpotBookings { get { return _bookings; } }

        public StationId Station { get; private set; }

        public void ChangeBooking(int count, LocalDate airingOn)
        {
            if (count < 0) throw new InvalidOperationException("Cannot book less than zero spots");

            RaiseEvent(_bookings.ChangeFor(count, airingOn).Execute());
        }

        public void ChangeStation(StationId newStation)
        {
            Station = newStation;
            RaiseEvent(new BookingLineStationChanged(Station));
        }

        public void MoveTo(LocalDate moveTo)
        {

        }
    }

    public class Bookings : KeyedCollection<LocalDate, Booking>
    {
        protected override LocalDate GetKeyForItem(Booking item)
        {
            return item.AiringOn;
        }

        internal BookingChange ChangeFor(int count, LocalDate airingOn)
        {
            var existing = Contains(airingOn) ? this[airingOn] : Booking.Empty(airingOn);
            return (count - existing.Count) > 0 ? (BookingChange)new AddSpotsChange(this, count, airingOn) : new RemoveSpotsChange(this, count, airingOn);
        }

        internal abstract class BookingChange
        {
            internal BookingChange(Bookings bookings, int count, LocalDate airingOn)
            {
                _bookings = bookings;
                _count = count;
                _airingOn = airingOn;
            }

            protected Bookings _bookings;
            protected int _count;
            protected LocalDate _airingOn;
            public abstract DomainEvent Execute();
        }

        class AddSpotsChange : BookingChange
        {
            internal AddSpotsChange(Bookings bookings, int count, LocalDate airingOn)
                : base(bookings, count, airingOn)
            { }

            public override DomainEvent Execute()
            {
                var existing = _bookings.Contains(_airingOn) ? _bookings[_airingOn] : Booking.Empty(_airingOn);
                _bookings.Remove(_airingOn);

                _bookings.Add(existing.ChangeSpotCount(_count));

                return new SpotsAdded(_count - existing.Count, _airingOn);
            }
        }

        class RemoveSpotsChange : BookingChange
        {
            internal RemoveSpotsChange(Bookings bookings, int count, LocalDate airingOn)
                : base(bookings, count, airingOn)
            { }

            public override DomainEvent Execute()
            {
                var existing = _bookings.Contains(_airingOn) ? _bookings[_airingOn] : Booking.Empty(_airingOn);
                _bookings.Remove(_airingOn);

                if (_count > 0)
                    _bookings.Add(existing.ChangeSpotCount(_count));

                return new SpotsRemoved(existing.Count - _count, _airingOn);
            }
        }
    }

    public class Booking
    {
        Booking() { }

        internal Booking(int count, LocalDate airingOn)
        {
            Count = count;
            AiringOn = airingOn;
        }

        public int Count { get; private set; }
        public LocalDate AiringOn { get; private set; }

        public Booking ChangeSpotCount(int count)
        {
            return new Booking(count, AiringOn);
        }

        public static Booking Empty(LocalDate airingOn)
        {
            return new Booking(0, airingOn);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Booking;
            if (other == null)
                return false;

            return other.AiringOn == this.AiringOn;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 29 + AiringOn.GetHashCode();

            return hash;
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

        [JsonIgnore]
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
            foreach (var @event in _events)
                @event.AggregateId = Id;

            return _events.ToArray();
        }

        public void ClearUncommittedEvents()
        {
            _events.Clear();
        }
    }

    #endregion
}
