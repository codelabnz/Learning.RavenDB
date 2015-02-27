using System.Linq;
using Raven.Client.Indexes;

namespace Learning.RavenDB
{
    public class SpotLines_ByStationCode : AbstractIndexCreationTask<LearningContractSpotLine>
    {
        // simple index definition - just has a map and no reduce. The projection contains the properties
        // to index, in this case any query against the month property of spot lines could use this index
        public SpotLines_ByStationCode()
        {
            Map = spotLines => from spotLine in spotLines
                               select new
                               {
                                   spotLine.Month,
                                   spotLine.StationDescription,
                                   Stations = spotLine.StationIds.Select(s => LoadDocument<LearningStation>(s).Code)
                               };
        }
    }
}
