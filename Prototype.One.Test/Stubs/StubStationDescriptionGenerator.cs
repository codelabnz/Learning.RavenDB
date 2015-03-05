using System.Collections.Generic;
using Prototype.One;

namespace Prototype.One.Test.Stubs
{
    public class StubStationDescriptionGenerator : IStationDescriptionGenerator
    {
        StubStationDescriptionGenerator(string returnDescription)
        {
            _returnDescription = returnDescription;
            WasInvoked = false;
        }

        string _returnDescription;
        public bool WasInvoked { get; private set; }

        public static StubStationDescriptionGenerator WithReturnDescription(string returnDescription)
        {
            return new StubStationDescriptionGenerator(returnDescription);
        }

        public string GenerateDescriptionForStations(IEnumerable<string> stationIds)
        {
            WasInvoked = true;
            return _returnDescription;
        }
    }
}
