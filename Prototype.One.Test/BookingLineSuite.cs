﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NodaTime;
using Prototype.One.Extensions;
using Prototype.One.Test.Data;
using Raven.Imports.Newtonsoft.Json;
using Xunit;

namespace Prototype.One.Test
{
    public class BookingLineSuite
    {
        public BookingLineSuite() { }

        [Fact]
        public void change_quantity_multiple_times_rebuild_object_in_versions()
        {
            //
            var finalQuantity = 10;
            var airingOn = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();
            var originalLine = Builder.BookingLine.ForStation(station).Build();

            //
            originalLine.ChangeBooking(finalQuantity - 3, airingOn);
            originalLine.ChangeBooking(finalQuantity - 8, airingOn);
            originalLine.ChangeBooking(finalQuantity, airingOn);

            //
            originalLine.SpotBookings.Single()
                            .Quantity.Should().Be(finalQuantity);

            //rebuild object to play back from start to end
            var rebuildLine = new BookingLine(originalLine.Id, originalLine.Events);

            rebuildLine.Should().Be(originalLine);

            //rebuild object to version 2 (0 based index)
            var rebuildToVersion = new BookingLine(originalLine.Id, originalLine.Events.Take(3));
            rebuildToVersion.SpotBookings.Single().Quantity.Should().Be(2);
            rebuildToVersion.Station.Should().Be(station);

        }

        [Fact]
        public void create_line_change_quantity_rebuild_object()
        {
            //
            var quantity = 5;
            var airingOn = Clock.Today.PlusDays(5);
            var station = Builder.Station.Build();

            //first event will be the created event
            var line = Builder.BookingLine.ForStation(station).Build();
            //second event will be change booking event
            line.ChangeBooking(quantity, airingOn);

            //test quantity of spot bookings
            line.SpotBookings.Single()
                            .Quantity.Should().Be(quantity);

            //these are all the past events that made up the current state of the line booking
            var pastEvents = line.Events.ToList();
            var lineId = line.Id;

            var newLine = new BookingLine(lineId, pastEvents);
            //compare the state of the old and new objects
            newLine.Should().Be(line);


        }

        [Fact]
        public void create_line_creates_station_added_event()
        {
            //
            var bookingStart = Clock.Today.MonthBegin();
            var station = Builder.Station.Build();


            //
            var line = new BookingLine(bookingStart, station);

            //
            line.Station.Should().Be(station, "the line was created for station {0}".Format(station));
            line.GetUncommittedEvents().Should()
                                        .ContainSingle(e => e.GetType() == typeof(BookingLineCreatedEvent)
                                                            && (e as BookingLineCreatedEvent).AggregateId == line.Id
                                                            && (e as BookingLineCreatedEvent).Station == station, "the line was created for station {0}".Format(station));
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
            line.GetUncommittedEvents().Should()
                                        .ContainSingle(e => e.GetType() == typeof(SpotsAddedEvent)
                                                            && ((SpotsAddedEvent)e).AggregateId == line.Id
                                                            && ((SpotsAddedEvent)e).Count == quantity
                                                            && ((SpotsAddedEvent)e).AiringOn == airingOn, "the booking increased to {0}".Format(quantity));
        }

        [Fact]
        public void decreasing_booking_creates_spots_removed_event()
        {
            //
            int initialQuantity = 5, newQuantity = 2;
            var airingOn = Clock.Today.PlusDays(5);
            var line = Builder.BookingLine.WithSpots(initialQuantity, airingOn).Build();

            //
            line.ChangeBooking(newQuantity, airingOn);

            //
            line.GetUncommittedEvents().Should()
                                        .ContainSingle(e => e.GetType() == typeof(SpotsRemovedEvent)
                                                            && ((SpotsRemovedEvent)e).AggregateId == line.Id
                                                            && ((SpotsRemovedEvent)e).Count == initialQuantity - newQuantity
                                                            && ((SpotsRemovedEvent)e).AiringOn == airingOn, "the booking was decreased to {0}".Format(newQuantity));
        }

        [Fact]
        public void change_booking_negative_quantity_throws()
        {
            //
            int initialQuantity = 5, newQuantity = -1;
            var airingOn = Clock.Today.PlusDays(5);
            var line = Builder.BookingLine.WithSpots(initialQuantity, airingOn).Build();

            //
            Action act = () => line.ChangeBooking(newQuantity, airingOn);

            //
            act.ShouldThrow<InvalidOperationException>("the quantity to book was a negative value");
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
            line.Station.Should().Be(newStation, "the station was changed to {0}".Format(newStation));
            line.GetUncommittedEvents().Should()
                                        .ContainSingle(e => e.GetType() == typeof(BookingLineStationChangedEvent)
                                                            && ((BookingLineStationChangedEvent)e).AggregateId == line.Id
                                                            && ((BookingLineStationChangedEvent)e).Station == newStation, "the station was changed to {0}".Format(newStation));
        }

        [Fact]
        public void change_station_for_line_rebuild_object()
        {
            //
            StationId initialStation = Builder.Station.Build(), newStation = Builder.Station.Build();
            var line = Builder.BookingLine.ForStation(initialStation).Build();

            //
            line.ChangeStation(newStation);

            //
            line.Station.Should().Be(newStation, "the station was changed to {0}".Format(newStation));
          
            //check that the event was raised
            line.Events.Should().ContainSingle(e => e.GetType() == typeof(ChangeStationVersionedEvent) && ((ChangeStationVersionedEvent)e).Station == newStation);

            var rebuildLine = new BookingLine(line.Id, line.Events);
            rebuildLine.Station.Should().Be(newStation);

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
        public BookingLine()
        {
            _bookings = new Bookings();

            Handles<BookingLineCreatedVersionedEvent>(x =>
            {
                this.Station = x.StationId;
                this._bookingStart = x.BookingStart;

                RaiseEvent(new BookingLineCreatedEvent(this.Station));
            });

            Handles<ChangeBookingVersionedEvent>(x =>
            {
                if (x.Quantity < 0) throw new InvalidOperationException("Cannot book less than zero spots");
                RaiseEvent(_bookings.ChangeQuantity(x.AiringOn, x.Quantity));
            });

            Handles<ChangeStationVersionedEvent>(x =>
                {
                    Station = x.Station;
                    RaiseEvent(new BookingLineStationChangedEvent(Station));
                });

            Handles<MoveToVersionedEvent>(x =>
            {
                var daysToMove = DetermineDaysToMove(x.MoveTo);
                foreach (var booking in _bookings)
                    RaiseEvent(_bookings.Move(booking.AiringOn, daysToMove));
            });

        }

        public BookingLine(string id) : this()
        {
            this.Id = id;
        }

        public BookingLine(LocalDate bookingStart, StationId stationId)
            : this(Guid.NewGuid().ToString())
        {
       
            Update(new BookingLineCreatedVersionedEvent() { BookingStart = bookingStart, StationId = stationId });

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
            Update(new ChangeBookingVersionedEvent() { AiringOn = airingOn, Quantity = quantity });
        }

        public void ChangeStation(StationId newStation)
        {
            Update(new ChangeStationVersionedEvent() { Station = newStation });
        }

        public void MoveTo(LocalDate moveTo)
        {
            Update(new MoveToVersionedEvent() { MoveTo = moveTo });
        }

        int DetermineDaysToMove(LocalDate moveTo)
        {
            moveTo = moveTo.MonthBegin();
            var days = (int)Period.Between(_bookingStart, moveTo, PeriodUnits.Days).Days;

            // return FULL weeks between the dates
            return days - (days % 7);
        }

        public override bool Equals(object obj)
        {
            var other = obj as BookingLine;
            if (other == null)
                return false;

            return other.Id == this.Id && other.Station.Equals(this.Station);
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 29 + Id.GetHashCode();
            hash = hash * 29 + Station.GetHashCode();

            return hash;
        }

        #region eventSourcing
       

        public BookingLine(string Id, IEnumerable<IVersionedEvent> history)
            : this(Id)
        {
            this.LoadFrom(history);
        }
        #endregion
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

    #region event sourced setup
    public interface IVersionedEvent
    {
        string SourceId { get; set; }
        int Version { get; set; }
    }

    public abstract class VersionedEvent : IVersionedEvent
    {
        public string SourceId { get; set; }
        public int Version { get; set; }
    }

    public interface IEventSourced
    {
        string Id { get; }
        int Version { get; }
        IEnumerable<IVersionedEvent> Events { get; }
    }

    #endregion

    #region event source events

    public class MoveToVersionedEvent : VersionedEvent
    {
        public LocalDate MoveTo { get; set; }
    }
    public class BookingLineCreatedVersionedEvent : VersionedEvent
    {
        public StationId StationId { get; set; }
        public LocalDate BookingStart { get; set; }
    }

    public class ChangeBookingVersionedEvent : VersionedEvent
    {
        public int Quantity { get; set;}
        public LocalDate AiringOn { get; set; }
    }

    public class ChangeStationVersionedEvent : VersionedEvent
    {
        public StationId Station { get; set; }
    }

    #endregion

    #region events

    public abstract class DomainEvent
    {
        public string AggregateId { get; set; }
        int Version { get; set; }
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

    public abstract class Aggregate : IEventSourced
    {
        public string Id { get; set; }

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

        #region Event Source Events
        private int version = -1;
        public int Version { get { return this.version; } }

        [JsonIgnore]
        private readonly Dictionary<Type, Action<IVersionedEvent>> handlers = new Dictionary<Type, Action<IVersionedEvent>>();
        [JsonIgnore]
        private readonly List<IVersionedEvent> pendingEvents = new List<IVersionedEvent>();


        protected void Handles<TEvent>(Action<TEvent> handler)
        {
            this.handlers.Add(typeof(TEvent), @event => handler((TEvent)@event));
        }

        protected void LoadFrom(IEnumerable<IVersionedEvent> pastEvents)
        {
            foreach (var e in pastEvents)
            {
                this.handlers[e.GetType()].Invoke(e);
                this.version = e.Version;
            }
        }

        protected void Update(VersionedEvent e)
        {
            e.SourceId = this.Id;
            e.Version = this.version + 1;
            this.handlers[e.GetType()].Invoke(e);
            this.version = e.Version;
            this.pendingEvents.Add(e);
        }
        public IEnumerable<IVersionedEvent> Events
        {
            get { return this.pendingEvents; }
        }
        #endregion


    }

    #endregion
}
