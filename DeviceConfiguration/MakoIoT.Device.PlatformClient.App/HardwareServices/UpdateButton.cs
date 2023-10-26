using System;
using System.Device.Gpio;
using Iot.Device.Button;
using MakoIoT.Device.PlatformClient.App.Events;
using MakoIoT.Device.Services.Mediator;

namespace MakoIoT.Device.PlatformClient.App.HardwareServices
{
    public class UpdateButton
    {
        private readonly IMediator _mediator;
        public UpdateButton(GpioController gpio, IMediator mediator)
        {
            _mediator = mediator;
            var updateButton = new GpioButton(gpio: gpio, buttonPin: 13);
            updateButton.Press += UpdateButtonOnPress;
        }

        private void UpdateButtonOnPress(object sender, EventArgs e)
        {
            _mediator.Publish(new ConfigUpdateEvent());
        }
    }
}
