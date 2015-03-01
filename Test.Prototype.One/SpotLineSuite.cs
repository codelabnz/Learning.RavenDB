using System.Linq;
using NodaTime;
using NodaTime.Testing;
using Prototype.One;
using Shouldly;
using Xunit;

namespace Test.Prototype.One
{
    public class SpotLineSuite
    {
        public SpotLineSuite()
        {
            var today = DateTimeZoneProviders.Tzdb.GetSystemDefault()
                                                    .AtStrictly(new LocalDateTime(2015, 02, 05, 00, 00))
                                                    .ToInstant();
            Clock.Current = new FakeClock(today);
        }

        [Fact]
        public void single_station_spot_line_accept_booking_for_a_day_with_no_bookings()
        {
            var spotLine = new SpotLine(new[] { new StationId { Id = "stations/1" } });
            var spots = 2;
            var airingOn = Clock.Today.PlusMonths(1);
            var booking = Booking.For(spots, airingOn);

            spotLine.PlaceBooking(booking);

            spotLine.Bookings.Count().ShouldBe(1);
            spotLine.Bookings.First().Spots.ShouldBe(spots);
        }

        [Fact]
        public void spot_line_start_date_should_be_date_of_first_booking()
        {
            var spotLine = new SpotLine();
            var firstAiringOn = Clock.Today.PlusDays(2);
            var firstBooking = Booking.For(2, firstAiringOn);
            var secondBooking = Booking.For(2, firstAiringOn.PlusDays(2));

            spotLine.PlaceBooking(firstBooking);
            spotLine.PlaceBooking(secondBooking);

            spotLine.StartDate.ShouldBe(firstAiringOn);
        }

        [Fact]
        public void spot_line_end_date_should_be_date_of_last_booking()
        {
            var spotLine = new SpotLine();
            var lastAiringOn = Clock.Today.PlusDays(6);
            var lastBooking = Booking.For(2, lastAiringOn);
            var firstBooking = Booking.For(2, lastAiringOn.PlusDays(-2));

            spotLine.PlaceBooking(lastBooking);
            spotLine.PlaceBooking(firstBooking);

            spotLine.EndDate.ShouldBe(lastAiringOn);
        }
    }
}
