using System.Linq;
using Raven.Client.Indexes;

namespace RavenDBHacking
{
    public class SpotLines_ByMonth : AbstractIndexCreationTask<ContractSpotLine>
    {
        // simple index definition - just has a map and no reduce. The projection contains the properties
        // to index, in this case any query against the month property of spot lines could use this index
        public SpotLines_ByMonth()
        {
            Map = spotLines => from spotLine in spotLines
                               select new { spotLine.Month };
        }
    }
}
