using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;
using Prototype.One.Extensions;
using Prototype.One.Test.Data;
using Xunit;
using Prototype.One;

namespace Prototype.One.Test
{
    public class ComboBookingSuite
    {
        [Fact]
        public void create_combo_booking_line_creates_station_added_to_combo_booking_event()
        {
            //
            var description = "COMBO_STATION_DESCRIPTION";
            var stations = new[] { Builder.Station.Build(), Builder.Station.Build() };

            //
            var combo = new ComboBooking(description, stations);

            //
            combo.GetUncommittedEvents().ShouldContain(e => (e as StationAddedToComboBooking) != null
                                                            && (e as StationAddedToComboBooking).AggregateId == combo.Id
                                                            && (e as StationAddedToComboBooking).Station == stations[0]);
            combo.GetUncommittedEvents().ShouldContain(e => (e as StationAddedToComboBooking) != null
                                                            && (e as StationAddedToComboBooking).AggregateId == combo.Id
                                                            && (e as StationAddedToComboBooking).Station == stations[1]);
        }

        [Fact]
        public void change_station_for_combo_booking_creates_station_added_event()
        {
            //
            var initialDescription = "COMBO_STATION_DESCRIPTION";
            var initialStations = new[] { Builder.Station.Build(), Builder.Station.Build() };
            var newDescription = "COMBO_STATION_DESCRIPTION_CHANGED";
            var newStations = new[] { initialStations[0], Builder.Station.Build() };
            var combo = new ComboBooking(initialDescription, initialStations);

            //
            combo.ChangeStations(newDescription, newStations);

            //
            combo.Stations.ShouldBe(newStations);
            combo.GetUncommittedEvents().ShouldContain(e => (e as StationAddedToComboBooking) != null
                                                            && (e as StationAddedToComboBooking).AggregateId == combo.Id
                                                            && (e as StationAddedToComboBooking).Station == newStations[1]);
        }

        [Fact]
        public void change_station_for_combo_booking_creates_station_removed_event()
        {
            throw new NotImplementedException();
        }
    }

    public class ComboBooking : Aggregate
    {
        public ComboBooking(string stationDescription, IEnumerable<StationId> stations)
            : base()
        {
            _stations = new List<StationId>(stations.Count());

            _stationDescription = stationDescription;
            SetStations(stations);
        }

        string _stationDescription;
        List<StationId> _stations;
        public IEnumerable<StationId> Stations { get { return _stations; } }

        public void ChangeStations(string newDescription, IEnumerable<StationId> newStations)
        {
            _stationDescription = newDescription;
            SetStations(newStations);
        }

        void SetStations(IEnumerable<StationId> stations)
        {
            foreach (var station in stations.Where(s => _stations.DoesNotContain(s)))
            {
                _stations.Add(station);
                this.RaiseEvent(new StationAddedToComboBooking(station));
            }

            foreach (var station in _stations.Where(s => stations.DoesNotContain(s))
                                                .ToList())
            {
                _stations.Remove(station);
            }
        }
    }

    public class StationAddedToComboBooking : DomainEvent
    {
        public StationAddedToComboBooking(StationId station)
        {
            Station = station;
        }

        public StationId Station { get; private set; }
    }
}
