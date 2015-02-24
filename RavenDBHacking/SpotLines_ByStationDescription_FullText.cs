using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace RavenDBHacking
{
    public class SpotLines_ByStationDescription_FullText : AbstractIndexCreationTask<ContractSpotLine>
    {
        public SpotLines_ByStationDescription_FullText()
        {
            Map = spotLines => from spotLine in spotLines
                               select new { spotLine.StationDescription };

            Analyze(s => s.StationDescription, "Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net");
            Index(s => s.StationDescription, FieldIndexing.Analyzed);
        }
    }
}
