using NServiceBus;

namespace Prototype.One
{
    public class AddMonthFrame : ICommand { }

    public class AddMonthFrameHandler : IHandleMessages<AddMonthFrame>
    {
        public void Handle(AddMonthFrame message)
        {
            
        }
    }
}
