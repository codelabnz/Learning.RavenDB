using System.Linq;
using Raven.Client.Indexes;

namespace Learning.RavenDB
{
    //public class SpotLines_ByStationAsDynamicField : AbstractIndexCreationTask<ContractSpotLine>
    //{
    //    public SpotLines_ByStationAsDynamicField()
    //    {
    //        Map = spotLines => from line in spotLines
    //                           select new
    //                                      {
    //                                          // generate a dynamic field - silly example (use case would be IEnumerable<KeyValuePair> where KVPs are custom attributes)
    //                                          _ = line.StationIds.Take(1)
    //                                                 .Select(s => CreateField("StationOne", LoadDocument<Station>(s).Code))
    //                                      };
    //    }
    //}
}
