using System.Collections.Generic;
using NodaTime;

namespace Learning.RavenDB
{
    public class LearningContractSpotLine
    {
        public string Id { get; set; }
        public LearningContract Contract { get; set; }

        public LocalDate Month { get; set; }

        public string StationDescription { get; set; }

        public IEnumerable<string> StationIds { get; set; }
    }

    public class LearningContract
    {
        public string Id { get; set; }
        public string Code { get; set; }
    }

    public class LearningStation
    {
        public string Id { get; set; }
        public string Code { get; set; }
    }
}
