using System;
using System.Linq;
using NodaTime;
using NServiceBus;
using Raven.Client;
using Prototype.One.Extensions;

namespace Prototype.One
{
    public class CreateStationFrame : ICommand
    {
        public string ContractId { get; set; }

        public LocalDate Month { get; set; }

        public int[] StationIds { get; set; }
    }

    public class StationFrameCreated : IEvent { }

    public class CreateStationFrameHandler : IHandleMessages<CreateStationFrame>
    {
        public CreateStationFrameHandler(IBus bus, IDocumentSession session)
        {
            _bus = bus;
            _session = session;
        }

        IBus _bus;
        IDocumentSession _session;

        public void Handle(CreateStationFrame message)
        {
            //var stations = message.StationIds
            //                        .Select(s => new Station { Id = "stations/{0}".Format(s) });

            //_session.Store(StationFrame.ForStations(stations));
            //_session.SaveChanges();

            //_bus.Publish<StationFrameCreated>();
        }
    }
}
