using MakoIoT.Device.Services.Mediator;
using MakoIoT.Device.PlatformClient.Services;
using MakoIoT.Device.Services.Interface;
using Microsoft.Extensions.Logging;
using System;
using MakoIoT.Device.PlatformClient.App.Configuration;
using MakoIoT.Device.PlatformClient.App.Events;

namespace MakoIoT.Device.PlatformClient.App.Services
{
    public class ConfigUpdater : IEventHandler
    {
        private readonly INetworkProvider _networkProvider;
        private readonly IConfigUpdateService _configUpdateService;
        private readonly IConfigurationService _configurationService;
        private readonly IMediator _mediator;
        private readonly ILogger _logger;

        public ConfigUpdater(
            INetworkProvider networkProvider,
            IConfigUpdateService configUpdateService,
            IConfigurationService configurationService,
            ILogger logger, IMediator mediator)
        {
            _networkProvider = networkProvider;
            _configUpdateService = configUpdateService;
            _configurationService = configurationService;
            _logger = logger;
            _mediator = mediator;
        }

        public void Handle(IEvent @event)
        {
            if (@event is ConfigUpdateEvent)
            {
                if (!_networkProvider.IsConnected)
                {
                    _logger.LogDebug("connecting to wifi...");
                    _networkProvider.Connect();
                }

                if (_networkProvider.IsConnected)
                {
                    _logger.LogDebug("updating configuration");

                    try
                    {
                        _configUpdateService.UpdateConfiguration();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "error updating configs");
                    }

                    foreach (var s in _configurationService.GetSections())
                    {
                        _logger.LogDebug(s);
                    }

                }
                else
                {
                    _logger.LogError("network disconnected");
                }

                try
                {
                    var pbc = (PlatformBlinkConfig)
                        _configurationService.GetConfigSection(PlatformBlinkConfig.SectionName, typeof(PlatformBlinkConfig));

                    _logger.LogInformation(pbc.Interval.ToString());
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "platform blink config");
                }

                _mediator.Publish(new ConfigUpdatedEvent());
            }
        }
    }
}
