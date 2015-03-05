using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Prototype.One;
using Raven.Client.Document;
using Raven.Client.Extensions;
using Raven.Client.NodaTime;
using Raven.Imports.Newtonsoft.Json.Serialization;
using Prototype.One.Test.Data;
using Xunit;
using System;

namespace Prototype.One.Test
{
    public class IntegrationSuite
    {
        [Fact]
        public void x()
        {
            var documentStore = new DocumentStore { Url = "http://localhost:8080" };
            documentStore.Initialize();

            documentStore.Conventions.JsonContractResolver = new ExcludeReadOnlyCollectionsContractResolver();
            documentStore.ConfigureForNodaTime();
            //documentStore.Conventions.CustomizeJsonSerializer =
            //                serializer => serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            documentStore.DatabaseCommands.EnsureDatabaseExists("Booking");
            documentStore.DefaultDatabase = "Booking";

            //IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), documentStore);

            using (var session = documentStore.OpenSession())
            {
                var quantity = 5;
                var airingOn = Clock.Today.PlusDays(5);
                var station = Builder.Station.Build();
                var line = Builder.BookingLine.ForStation(station)
                                                .WithoutId()
                                                .Build();

                //
                line.ChangeBooking(quantity, airingOn);
                line.ChangeBooking(quantity + 2, airingOn.PlusDays(2));
                line.ChangeBooking(quantity + 4, airingOn.PlusDays(4));

                session.Store(line);


                foreach (var @event in line.GetUncommittedEvents())
                {
                    //Bus.Raise(@event)
                }
                
                line.ClearUncommittedEvents();
                session.SaveChanges();
            }
        }

        [Fact]
        public void x1()
        {
            var documentStore = new DocumentStore { Url = "http://localhost:8080" };
            documentStore.Initialize();

            documentStore.Conventions.JsonContractResolver = new ExcludeReadOnlyCollectionsContractResolver();
            documentStore.ConfigureForNodaTime();
            //documentStore.Conventions.CustomizeJsonSerializer =
            //                serializer => serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            documentStore.DatabaseCommands.EnsureDatabaseExists("Booking");
            documentStore.DefaultDatabase = "Booking";

            //IndexCreation.CreateIndexes(Assembly.GetExecutingAssembly(), documentStore);

            using (var session = documentStore.OpenSession())
            {
                var line = session.Load<BookingLine>("bookinglines/1");
                foreach (var booking in line.SpotBookings)
                    Console.WriteLine(booking);
            }
        }

    }


    public class ExcludeReadOnlyCollectionsContractResolver : DefaultContractResolver
    {
        public ExcludeReadOnlyCollectionsContractResolver()
        {
            DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            var members = base.GetSerializableMembers(objectType);
            return members.Where(m => !IsReadOnlyCollectionProperty(m))
                          .ToList();
        }

        bool IsReadOnlyCollectionProperty(MemberInfo info)
        {
            var asProperty = info as PropertyInfo;
            if (asProperty == null)
                return false;

            return asProperty.PropertyType.IsGenericType
                    && asProperty.PropertyType.GetGenericTypeDefinition() == typeof(IReadOnlyCollection<>);
        }
    }
}
