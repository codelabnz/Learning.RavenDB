using System.Linq;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Learning.RavenDB
{
    public class SpotLines_ByStationDescription_FullText : AbstractIndexCreationTask<ContractSpotLine>
    {
        public SpotLines_ByStationDescription_FullText()
        {
            Map = spotLines => from spotLine in spotLines
                               select new { spotLine.StationDescription };

            // the default analyzer is the lower case analyzer, if this is all you want then just
            // need the field to analyze in the projection as above. So the following line is not required
            // as that is the default behaviour.
            // Index(s=>s.StationDescription, FieldIndexing.Default);

            // the FieldIndexing.Analyzed option is used to change from the default lower case analyzer provided
            // by RavenDB. Without calling Analyze(...) and just calling Index(s => s.StationDescription, FieldIndexing.Analyzed)
            // the default Lucene analyzer is used which is the standard analyzer. So the following Analyze call is not required when
            // calling Index(s => s.StationDescription, FieldIndexing.Analyzed) as StandardAnalyzer will be used by default
            // Analyze(s => s.StationDescription, "Lucene.Net.Analysis.Standard.StandardAnalyzer, Lucene.Net");

            // to use another of the Lucene.Net analyzers you must first call Analyze to register the analyzer to use
            // and then call index, with FieldIndexing.Analyzed as the 2nd param.
            // the following configures the Whitespace analyzer which only splits on whitespace and does not do any changes
            // to case of tokens or filtering of standard english terms (e.g. 'the') like the standard analyzer does.
            //Analyze(s => s.StationDescription, "Lucene.Net.Analysis.WhitespaceAnalyzer, Lucene.Net");
            //Index(s => s.StationDescription, FieldIndexing.Analyzed);

            // for this test we are going to assume the Lucene.Net StandardAnalyzer so just the following is required
            // to change from RavenDB default of LowerCase analyzer
            Index(s => s.StationDescription, FieldIndexing.Analyzed);
        }
    }
}
