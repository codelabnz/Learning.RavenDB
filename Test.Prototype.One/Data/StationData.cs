﻿using System.Collections.Generic;
using NodaTime;
using Prototype.One;
using Prototype.One.Extensions;
using Raven.Client;

namespace Test.Prototype.One.Data
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

    public abstract class AggregateBuilder
    {
        int _lastId = 1;
        protected abstract string _CollectionName { get; }

        protected TAggregate SetAggregateId<TAggregate>(TAggregate aggregate)
            where TAggregate : Aggregate
        {
            var idProperty = typeof(Aggregate).GetProperty("Id");
            idProperty.SetValue(aggregate, "{0}/{1}".Format((object)_CollectionName, _lastId++));

            return aggregate;
        }
    }

    public class BookingLineBuilder : AggregateBuilder
    {
        static BookingLineBuilder _builder;

        BookingLineBuilder()
        {
            _spots = new Dictionary<LocalDate, int>();
        }

        int _lastId = 1;
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

        public BookingLineBuilder WithSpots(int count, LocalDate airingOn)
        {
            _spots[airingOn] = count;
            return this;
        }

        public BookingLine Build()
        {
            var line = new BookingLine();
            foreach (var kvp in _spots)
                line.AddSpots(kvp.Value, kvp.Key);
            
            return SetAggregateId(line);
        }
    }

    public class StationBookingBuilder : AggregateBuilder
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
    }

}
