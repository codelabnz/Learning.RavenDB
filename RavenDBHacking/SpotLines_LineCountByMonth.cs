using System.Linq;
using NodaTime;
using Raven.Client.Indexes;

namespace RavenDBHacking
{
    public class SpotLines_LineCountByMonth : AbstractIndexCreationTask<ContractSpotLine, SpotLines_LineCountByMonth.ReduceResult>
    {
        public class ReduceResult
        {
            public LocalDate Month { get; set; }
            public int Count { get; set; }
        }

        public SpotLines_LineCountByMonth()
        {
            Map = spotLines => from spotLine in spotLines
                               select new
                               {
                                   spotLine.Month,
                                   Count = 1
                               };

            Reduce = results => from result in results
                                group result by result.Month
                                    into grouped
                                    select new
                                    {
                                        Month = grouped.Key,
                                        Count = grouped.Sum(c => c.Count)
                                    };
        }
    }
}
