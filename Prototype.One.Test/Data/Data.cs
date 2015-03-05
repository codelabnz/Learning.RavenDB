using System.Collections.Generic;
using NodaTime;
using NodaTime.Testing;
using Prototype.One;
using Prototype.One.Extensions;
using Raven.Client;

namespace Prototype.One.Test.Data
{
    public class StationData
    {
        public static void Add(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                session.Store(new Station { Id = "stations/1", Code = "WKOMORE" });
                session.Store(new Station { Id = "stations/2", Code = "WKOEDGE" });
                session.Store(new Station { Id = "stations/3", Code = "AKLGRG" });
                session.Store(new Station { Id = "stations/4", Code = "AKLROCK" });

                session.SaveChanges();
            }
        }
    }

    public class StationBuilder
    {
        static StationBuilder _builder;

        StationBuilder() { }

        int _lastId = 1;

        public static StationBuilder Get()
        {
            if (_builder == null)
                _builder = new StationBuilder();

            return _builder;
        }

        public StationId Build()
        {
            return new StationId { Id = "stations/{0}".Format(_lastId++) };
        }
    }

    public abstract class AggregateBuilder<T> where T : class
    {
        int _lastId = 1;
        bool _withId = true;
        protected abstract string _CollectionName { get; }

        protected TAggregate SetAggregateId<TAggregate>(TAggregate aggregate)
            where TAggregate : Aggregate
        {
            if(_withId)
            {
            var idProperty = typeof(Aggregate).GetProperty("Id");
            idProperty.SetValue(aggregate, "{0}/{1}".Format((object)_CollectionName, _lastId++));
            }

            return aggregate;
        }

        public T WithoutId()
        {
            _withId = false;
            return this as T;
        }
    }

    public class BookingLineBuilder : AggregateBuilder<BookingLineBuilder>
    {
        static BookingLineBuilder _builder;

        BookingLineBuilder()
        {
            _defaultStation = Builder.Station.Build();
            _spots = new Dictionary<LocalDate, int>();
        }

        int _lastId = 1;

        StationId _defaultStation;
        StationId _station;

        Dictionary<LocalDate, int> _spots;

        protected override string _CollectionName
        {
            get { return "bookinglines"; }
        }

        public static BookingLineBuilder Get()
        {
            if (_builder == null)
                _builder = new BookingLineBuilder();

            return _builder;
        }

        public static BookingLineBuilder Get(bool withNoId)
        {
            _builder = new BookingLineBuilder();

            return _builder;
        }

        public BookingLineBuilder ForStation(StationId station)
        {
            _station = station;
            return this;
        }

        public BookingLineBuilder WithSpots(int count, LocalDate airingOn)
        {
            _spots[airingOn] = count;
            return this;
        }

        public BookingLine Build()
        {
            var line = new BookingLine(_station ?? _defaultStation);
            foreach (var kvp in _spots)
                line.ChangeBooking(kvp.Value, kvp.Key);

            return SetAggregateId(line);
        }
    }

    public class StationBookingBuilder : AggregateBuilder<StationBookingBuilder>
    {
        static StationBookingBuilder _builder;

        StationBookingBuilder() { }

        int _lastId = 1;
        string _defaultStationDescription = "MCH(ROCK, BREEZE)";
        IEnumerable<StationId> _defaultStations = new[] { new StationId { Id = "stations/1" }, new StationId { Id = "stations/2" } };

        string _stationDescription = null;
        IEnumerable<StationId> _stations = null;


        protected override string _CollectionName
        {
            get { return "stationbookings"; }
        }

        public static StationBookingBuilder Get()
        {
            if (_builder == null)
                _builder = new StationBookingBuilder();

            return _builder;
        }

        public StationBooking Build()
        {
            var booking = new StationBooking(_stationDescription ?? _defaultStationDescription, _stations ?? _defaultStations);
            return SetAggregateId(booking);
        }
    }

    public static class Builder
    {
        public static StationBookingBuilder StationBooking { get { return StationBookingBuilder.Get(); } }

        public static BookingLineBuilder BookingLine { get { return BookingLineBuilder.Get(); } }

        public static StationBuilder Station { get { return StationBuilder.Get(); } }
    }

    public static class Testing
    {
        public static void Today(LocalDate today)
        {
            var todayAsInstant = DateTimeZoneProviders.Tzdb
                                                    .GetSystemDefault()
                                                    .AtStrictly(today.AtMidnight())
                                                    .ToInstant();
            
            Clock.Current = new FakeClock(todayAsInstant);
        }
    }

}
