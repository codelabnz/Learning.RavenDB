using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NodaTime;
using Prototype.One;
using Prototype.One.Extensions;
using Raven.Imports.Newtonsoft.Json;
using Shouldly;
using FluentAssertions;
using Prototype.One.Test.Data;
using Xunit;
using System.Collections;

namespace Prototype.One.Test
{
    public class BookingLineSuite
    {
        public BookingLineSuite() { }

        [Fact]
        public void create_line_creates_station_added_event()
        {
            //
            var bookingStart = Clock.Today.MonthBegin();
            var station = Builder.Station.Build();


            //
            var line = new BookingLine(bookingStart, station);

            //
            line.Station.ShouldBe(station);
            line.GetUncommittedEvents().ShouldContain(e => (e as BookingLineCreatedEvent) != null
                                                            && (e as BookingLineCreatedEvent).AggregateId == line.Id
                                                            && (e as BookingLineCreatedEvent).Station == station);
        }

        [Fact]
        public void add_new_booking_has_correct_quantity()
        {
            //
            var quantity = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(station).Build();

            //
            line.ChangeBooking(quantity, airingOn);

            //
            line.SpotBookings.Single()
                            .Quantity.Should().Be(quantity);

        }

        [Fact]
        public void change_booking_multiple_times_final_quantity_is_correct()
        {
            //
            var finalQuantity = 10;
            var airingOn = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(station).Build();

            //
            line.ChangeBooking(finalQuantity - 3, airingOn);
            line.ChangeBooking(finalQuantity - 8, airingOn);
            line.ChangeBooking(finalQuantity, airingOn);

            //
            line.SpotBookings.Single()
                            .Quantity.Should().Be(finalQuantity);

        }

        [Fact]
        public void increasing_booking_creates_spots_added_event()
        {
            //
            var quantity = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(station).Build();

            //
            line.ChangeBooking(quantity, airingOn);

            //
            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsAddedEvent) != null
                                                            && (e as SpotsAddedEvent).AggregateId == line.Id
                                                            && (e as SpotsAddedEvent).Count == quantity
                                                            && (e as SpotsAddedEvent).AiringOn == airingOn);
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
            line.GetUncommittedEvents().ShouldContain(e => (e as SpotsRemovedEvent) != null
                                                            && (e as SpotsRemovedEvent).AggregateId == line.Id
                                                            && (e as SpotsRemovedEvent).Count == initialQuantity - newQuantity
                                                            && (e as SpotsRemovedEvent).AiringOn == airingOn);
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
            line.GetUncommittedEvents().ShouldContain(e => (e as BookingLineStationChangedEvent) != null
                                                            && (e as BookingLineStationChangedEvent).AggregateId == line.Id
                                                            && (e as BookingLineStationChangedEvent).Station == newStation);
        }

        [Fact]
        public void move_bookings_by_number_of_months_bookings_should_fall_on_same_day_of_week()
        {
            //
            Testing.Today(new LocalDate(2015, 03, 05));
            var airingOn = Clock.Today.PlusDays(1);
            int bookingQuantity = 5;
            var station = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(station)
                                        .WithSpots(bookingQuantity, airingOn).Build();
            var moveTo = airingOn.MonthBegin().PlusMonths(3);

            //
            line.MoveTo(moveTo);

            //
            var movedBooking = line.SpotBookings.SingleOrDefault(b => b.Quantity == bookingQuantity);

            movedBooking.Should().NotBeNull("the quantity of spots shouldn't have changed");
            movedBooking.AiringOn.Should().NotBe(airingOn, "the spots should have moved");
            movedBooking.AiringOn.DayOfWeek.Should().Be(airingOn.DayOfWeek, "the spots should still fall on {0:dddd}".Format(airingOn));
            movedBooking.AiringOn.MonthBegin().Should().Be(moveTo, "the spots should have moved to {0:MMMM}".Format(moveTo));
        }
    }

    public class BookingLine : Aggregate
    {
        protected BookingLine()
        {
            _bookings = new Bookings();
        }

        public BookingLine(LocalDate bookingStart, StationId stationId)
            : this()
        {
            _bookingStart = bookingStart;
            Station = stationId;
            RaiseEvent(new BookingLineCreatedEvent(stationId));
        }

        LocalDate _bookingStart;

        Bookings _bookings;
        public IReadOnlyCollection<Booking> SpotBookings
        {
            get
            {
                return _bookings.ToList()
                                .AsReadOnly();
            }
        }

        public StationId Station { get; private set; }

        public void ChangeBooking(int quantity, LocalDate airingOn)
        {
            if (quantity < 0) throw new InvalidOperationException("Cannot book less than zero spots");

            RaiseEvent(_bookings.ChangeQuantity(airingOn, quantity));
        }

        public void ChangeStation(StationId newStation)
        {
            Station = newStation;
            RaiseEvent(new BookingLineStationChangedEvent(Station));
        }

        public void MoveTo(LocalDate moveTo)
        {
            var daysToMove = DetermineDaysToMove(moveTo);
            foreach (var booking in _bookings)
                RaiseEvent(_bookings.Move(booking.AiringOn, daysToMove));
        }

        int DetermineDaysToMove(LocalDate moveTo)
        {
            moveTo = moveTo.MonthBegin();
            var days = (int)Period.Between(_bookingStart, moveTo, PeriodUnits.Days).Days;

            // return FULL weeks between the dates
            return days - (days % 7);
        }
    }

    public class Bookings : IEnumerable<Booking>
    {
        internal Bookings()
        {
            _bookings = new Dictionary<string, Booking>();
        }

        Dictionary<string, Booking> _bookings;

        Booking this[LocalDate index]
        {
            get
            {
                var key = index.ToString();
                if (!_bookings.ContainsKey(key))
                    _bookings.Add(key, Booking.Empty(index));

                return _bookings[key];
            }
        }

        internal DomainEvent ChangeQuantity(LocalDate airingOn, int count)
        {
            var existing = this[airingOn];
            var change = count - existing.Quantity;
            if (change > 0)
                return AddSpots(airingOn, change);
            else
                return RemoveSpots(airingOn, Math.Abs(change));
        }

        internal DomainEvent Move(LocalDate bookedOn, int daysToMove)
        {
            var existing = this[bookedOn];
            existing.Move(daysToMove);

            return new BookingMovedEvent();
        }

        DomainEvent AddSpots(LocalDate airingOn, int quantityToAdd)
        {
            var booking = _bookings[airingOn.ToString()];
            booking.Add(quantityToAdd);

            return new SpotsAddedEvent(quantityToAdd, airingOn);
        }

        DomainEvent RemoveSpots(LocalDate airingOn, int quantityToRemove)
        {
            var booking = _bookings[airingOn.ToString()];
            booking.Remove(quantityToRemove);

            if (booking.Quantity <= 0)
                _bookings.Remove(airingOn.ToString());

            return new SpotsRemovedEvent(quantityToRemove, airingOn);
        }

        #region IEnumerable

        public IEnumerator<Booking> GetEnumerator()
        {
            foreach (var booking in _bookings.Values.ToList())
                yield return booking;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    public class Booking
    {
        Booking() { }

        internal Booking(int quantity, LocalDate airingOn)
        {
            Quantity = quantity;
            AiringOn = airingOn;
        }

        public int Quantity { get; private set; }
        public LocalDate AiringOn { get; private set; }

        internal void Add(int quantityToAdd)
        {
            Quantity += quantityToAdd;
        }

        internal void Remove(int quantityToRemove)
        {
            Quantity -= quantityToRemove;
        }

        internal void Move(int days)
        {
            AiringOn = AiringOn.PlusDays(days);
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

            return other.AiringOn == this.AiringOn
                    && other.Quantity == this.Quantity;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 29 + AiringOn.GetHashCode();
            hash = hash * 29 + Quantity.GetHashCode();

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

    public class BookingLineCreatedEvent : DomainEvent
    {
        public BookingLineCreatedEvent(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }

    public class BookingLineStationChangedEvent : DomainEvent
    {
        public BookingLineStationChangedEvent(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }

    public class SpotsAddedEvent : DomainEvent
    {
        public SpotsAddedEvent(int count, LocalDate airingOn)
        {
            Count = count;
            AiringOn = airingOn;
        }

        public int Count { get; private set; }
        public LocalDate AiringOn { get; private set; }
    }

    public class SpotsRemovedEvent : DomainEvent
    {
        public SpotsRemovedEvent(int count, LocalDate airingOn)
        {
            Count = count;
            AiringOn = airingOn;
        }

        public int Count { get; private set; }
        public LocalDate AiringOn { get; private set; }
    }

    public class BookingMovedEvent : DomainEvent
    { }

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
