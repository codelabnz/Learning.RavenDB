using Prototype.One;
using Xunit;

namespace Test.Prototype.One
{
    using NServiceBus.Testing;

    public class MonthFrameSuite
    {
        [Fact]
        public void AddBookingFrameCommand()
        {
            Test.Initialize();

            Test.Handler<AddMonthFrameHandler>()
                .OnMessage<AddMonthFrame>(m => { });
        }
    }
}
