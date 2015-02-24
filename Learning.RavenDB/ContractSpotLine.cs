using System.Collections.Generic;
using NodaTime;

namespace Learning.RavenDB
{
    public class ContractSpotLine
    {
        public string Id { get; set; }
        public Contract Contract { get; set; }

        public LocalDate Month { get; set; }

        public string StationDescription { get; set; }

        public IEnumerable<string> StationIds { get; set; }
    }

    public class Contract
    {
        public string Id { get; set; }
        public string Code { get; set; }
    }

    public class Station
    {
        public string Id { get; set; }
        public string Code { get; set; }
    }
}
