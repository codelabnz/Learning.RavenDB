using Prototype.One;
using Raven.Client;

namespace Test.Prototype.One.Data
{
    public class StationData
    {
        public static void Add(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                session.Store(new Station { Id = "stations/1", Code = "WKOMORE" });
                session.Store(new Station { Id = "stations/2", Code = "WKOEDGE" });
                session.Store(new Station { Id = "stations/3", Code = "AKLGRG" });
                session.Store(new Station { Id = "stations/4", Code = "AKLROCK" });

                session.SaveChanges();
            }
        }
    }
}
