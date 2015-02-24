using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NodaTime;
using Raven.Tests.Helpers;
using Xunit;

namespace RavenDBHacking
{
    //public class SetupTests : RavenTestBase
    //{
    //    const bool RUN_THIS_TIME = true;

    //    [Fact]
    //    public void Run()
    //    {
    //        if (RUN_THIS_TIME)
    //            PopulateSpotLines();
    //    }

    //    Random _random = new Random();
    //    Dictionary<int, Contract> _contracts = new Dictionary<int, Contract>(1000);

    //    void PopulateSpotLines()
    //    {
    //        using (var documentStore = NewRemoteDocumentStore(databaseName: "Booking", runInMemory:false))
    //        {
    //            using (var session = documentStore.OpenSession())
    //            {
    //                for (int index = 0; index < 100000; index++)
    //                {
    //                    var month = _random.Next(0, 12);
    //                    var spotLine = new ContractSpotLine
    //                    {
    //                        Contract = GetContract(),
    //                        Month = (new LocalDate(2015, 02, 01)).PlusMonths(month)
    //                    };

    //                    session.Store(spotLine);
    //                }

    //                session.SaveChanges();
    //            }
    //        }
    //    }

    //    Contract GetContract()
    //    {
    //        var index = _random.Next(0, 999);
    //        if (_contracts.ContainsKey(index))
    //            return _contracts[index];

    //        var code = _random.Next(253778, 365123);
    //        var contract = new Contract
    //        {
    //            Id = "contracts/" + (index + 997).ToString(),
    //            Code = code.ToString()
    //        };

    //        return contract;
    //    }
    //}
}
