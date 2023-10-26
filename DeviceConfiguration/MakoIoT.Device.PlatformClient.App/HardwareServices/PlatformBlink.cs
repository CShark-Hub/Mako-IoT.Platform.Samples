using System.Device.Gpio;
using System.Threading;
using MakoIoT.Device.PlatformClient.App.Configuration;
using MakoIoT.Device.PlatformClient.App.Events;
using MakoIoT.Device.Services.Configuration;
using MakoIoT.Device.Services.Interface;
using MakoIoT.Device.Services.Mediator;

namespace MakoIoT.Device.PlatformClient.App.HardwareServices
{
    public class PlatformBlink : IEventHandler
    {
        private readonly IConfigurationService _configurationService;
        private readonly GpioPin _pin;
        private Thread _displayThread;
        private CancellationTokenSource _displayTokenSource;
        private CancellationToken _cancellationToken;

        public PlatformBlink(IConfigurationService configurationService, GpioController gpio)
        {
            _configurationService = configurationService;

            _pin = gpio.OpenPin(2, PinMode.Output);
        }

        public void Initialize()
        {
            _displayTokenSource = new CancellationTokenSource();
            _cancellationToken = _displayTokenSource.Token;
            _displayThread = new Thread(Start);
            _displayThread.Start();
        }

        public void Start()
        {
            PlatformBlinkConfig config;
            try
            {
                config =
                    (PlatformBlinkConfig)_configurationService.GetConfigSection(PlatformBlinkConfig.SectionName,
                        typeof(PlatformBlinkConfig));
            }
            catch (ConfigurationException e)
            {
                return;
            }

            var interval = config.Interval;

            if (interval == 0)
                return;

            while (!_cancellationToken.IsCancellationRequested)
            {
                _pin.Write(PinValue.High);
                Thread.Sleep(50);
                _pin.Write(PinValue.Low);
                _cancellationToken.WaitHandle.WaitOne(interval, false);
            }
        }

        public void Handle(IEvent @event)
        {
            if (@event is not ConfigUpdatedEvent) return;
            if (_displayThread == null) return;

            _displayTokenSource.Cancel();
            Thread.Sleep(100);

            Initialize();
        }
    }
}
