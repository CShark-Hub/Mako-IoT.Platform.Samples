using System.Device.Gpio;
using MakoIoT.Device.PlatformClient.App.HardwareServices;
using MakoIoT.Device.Services.ConfigurationManager.Interface;
using MakoIoT.Device.Services.Logging.Configuration;
using Microsoft.Extensions.Logging;
using nanoFramework.DependencyInjection;
using System.Threading;
using MakoIoT.Device.Platform.Interface.Configuration;
using MakoIoT.Device.Platform.LocalConfig.Extensions;
using MakoIoT.Device.PlatformClient.Extensions;
using MakoIoT.Device.Services.ConfigurationManager;
using MakoIoT.Device.Services.ConfigurationManager.Events;
using MakoIoT.Device.Services.ConfigurationManager.Extensions;
using MakoIoT.Device.Services.FileStorage.Extensions;
using MakoIoT.Device.Services.Logging.Extensions;
using MakoIoT.Device.Services.Mediator.Extensions;
using MakoIoT.Device.Services.Server.Extensions;
using MakoIoT.Device.Services.Server.WebServer;
using MakoIoT.Device.Services.WiFi.AP.Configuration;
using MakoIoT.Device.Services.WiFi.AP.Extensions;
using MakoIoT.Device.Services.WiFi.Configuration;
using MakoIoT.Device.Services.WiFi.Extensions;
using MakoIoT.Device.PlatformClient.App.Events;
using MakoIoT.Device.PlatformClient.App.Services;

namespace MakoIoT.Device.PlatformClient.App
{
    public class Program
    {
        public static void Main()
        {
            var builder = DeviceBuilder.Create()
                .ConfigureDI(s =>
                {
                    s.AddSingleton(typeof(GpioController));

                    s.AddSingleton(typeof(ConfigButton));
                    s.AddSingleton(typeof(UpdateButton));
                    s.AddSingleton(typeof(ConfigUpdater));
                    s.AddSingleton(typeof(PlatformBlink));

                    s.AddTransient(typeof(IDeviceControl), typeof(DeviceControlService));
                })
                .AddMediator(o =>
                {
                    o.AddSubscriber(typeof(ConfigModeToggleEvent), typeof(IConfigManager));
                    o.AddSubscriber(typeof(ResetToDefaultsEvent), typeof(IConfigManager));
                    o.AddSubscriber(typeof(ConfigUpdateEvent), typeof(ConfigUpdater));
                    o.AddSubscriber(typeof(ConfigUpdatedEvent), typeof(PlatformBlink));
                })
                .AddLogging(new LoggerConfig(LogLevel.Debug))
                .AddWiFi()
                .AddFileStorage()
                .AddWiFiInterfaceManager()
                .AddConfigurationManager()
                .AddWebServer(o =>
                {
                    o.Port = 80;
                    o.Protocol = HttpProtocol.Http;
                    o.AddConfigurationWebsite();
                })
                .AddPlatformClient(c =>
                {
                    c.WriteDefault(WiFiAPConfig.SectionName, new WiFiAPConfig
                    {
                        Ssid = "MAKO-IoT Device",
                        Password = "csharkmakers",
                    });

                    c.WriteDefault(WiFiConfig.SectionName, new WiFiConfig());

                    c.WriteDefault(PlatformConfig.SectionName, new PlatformConfig
                    {
                        PlatformServiceUrl = "https://mako-iot-platform-deviceapi-url.com/",
                    });
                });

            var device = builder.Build();
            device.Start();

            //initialize hardware buttons
            var configButton = (ConfigButton)device.ServiceProvider.GetService(typeof(ConfigButton));
            var updateButton = (UpdateButton)device.ServiceProvider.GetService(typeof(UpdateButton));

            var blink = (PlatformBlink)device.ServiceProvider.GetService(typeof(PlatformBlink));
            blink.Initialize();
          
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
