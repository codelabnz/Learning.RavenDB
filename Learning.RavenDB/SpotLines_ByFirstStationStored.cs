using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Learning.RavenDB
{
    public class SpotLines_ByFirstStationStored : AbstractIndexCreationTask<ContractSpotLine>
    {
        public class StationViewModel
        {
            public string StationCode { get; set; }
        }

        public SpotLines_ByFirstStationStored()
        {
            Map = spotLines => from line in spotLines
                               from station in line.StationIds
                               select new
                               {
                                   StationCode = LoadDocument<Station>(station).Code
                               };


            // this example is a bit dumb as Station Code isn't a property on the indexed doc (ContractSpotLine)
            // but what this is doing is creating a property of StationCode in the index which means when transforming
            // results of this index to type StationViewModel, the whole result object can be built from the index
            // fields without having to go back to the indexed document
            Store("StationCode", FieldStorage.Yes);
        }
    }
}
