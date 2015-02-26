using System.Linq;
using Raven.Client.Indexes;

namespace Learning.RavenDB
{
    public class ContractSpotLineStationTransformer : AbstractTransformerCreationTask<ContractSpotLine>
    {
        public class ContractSpotLineStation
        {
            public string Station { get; set; }
        }

        public ContractSpotLineStationTransformer()
        {
            TransformResults = spotLines => from line in spotLines
                                            from station in line.StationIds
                                            select new
                                            {
                                                Station = LoadDocument<Station>(station).Code
                                            };
        }
    }
}
