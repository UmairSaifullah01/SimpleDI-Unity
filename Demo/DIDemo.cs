using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using THEBADDEST.SimpleDependencyInjection;

// Example interfaces
public interface ILogger
{
    void Log(string message);
}

public interface IDataService
{
    string GetData();
}

public interface IAnalyticsService
{
    void TrackEvent(string eventName);
}

// Example implementations
public class UnityLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log($"[UnityLogger] {message}");
    }
}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log($"[FileLogger] {message}");
    }
}

public class DataService : IDataService
{
    private readonly ILogger _logger;

    public DataService([Inject(name: "unity")] ILogger logger)
    {
        _logger = logger;
    }

    public string GetData()
    {
        _logger.Log("Getting data...");
        return "Sample Data";
    }
}

public class AnalyticsService : IAnalyticsService
{
    private readonly ILogger _logger;
    private readonly IDataService _dataService;

    public AnalyticsService(
        [Inject(name:"file")] ILogger logger,
        IDataService                  dataService)
    {
        _logger = logger;
        _dataService = dataService;
    }

    public void TrackEvent(string eventName)
    {
        var data = _dataService.GetData();
        _logger.Log($"Tracking event: {eventName} with data: {data}");
    }
}

// Example decorator
public class LoggingAnalyticsDecorator : IAnalyticsService
{
    private  IAnalyticsService _analytics;
    private readonly ILogger _logger;

    public LoggingAnalyticsDecorator(
        [Inject(name:  "unity")] ILogger logger)
    {
        _logger = logger;
        // The analytics service will be set by the container
        _analytics = null;
    }

    public void SetAnalyticsService(IAnalyticsService analytics)
    {
        _analytics = analytics;
    }

    public void TrackEvent(string eventName)
    {
        if (_analytics == null)
            throw new InvalidOperationException("Analytics service not set");

        _logger.Log($"Before tracking: {eventName}");
        _analytics.TrackEvent(eventName);
        _logger.Log($"After tracking: {eventName}");
    }
}

public class DIDemo : MonoBehaviour
{
    private IContainer _container;
    private IAnalyticsService _analytics;

    // Start is called before the first frame update
    void Start()
    {
        // Create container builder
        var builder = new ContainerBuilder();

        // Register named loggers
        builder.Register<ILogger, UnityLogger>(name:"unity");
        builder.Register<ILogger, FileLogger>(name:"file");

        // Register data service
        builder.Register<IDataService, DataService>();

        // Register analytics service with decorator
        builder.Register<IAnalyticsService, AnalyticsService>();
        builder.RegisterDecorator<IAnalyticsService, LoggingAnalyticsDecorator>();

        // Build container
        _container = builder.Build();

        // Resolve analytics service
        _analytics = _container.Resolve<IAnalyticsService>();

        // Demonstrate scoped lifetime
        using (var scope = _container.CreateScope())
        {
            var scopedAnalytics = scope.Resolve<IAnalyticsService>();
            scopedAnalytics.TrackEvent("Scoped Event");
        }

        // Demonstrate named resolution
        var unityLogger = _container.ResolveNamed<ILogger>("unity");
        var fileLogger = _container.ResolveNamed<ILogger>("file");

        // Demonstrate collection resolution
        var allLoggers = _container.ResolveAll<ILogger>();
        foreach (var logger in allLoggers)
        {
            logger.Log("Testing all loggers");
        }

        // Test analytics
        _analytics.TrackEvent("Game Started");
    }

    // Update is called once per frame
    void Update()
    {
        // Example of using the analytics service
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _analytics.TrackEvent("Space Pressed");
        }
    }

    void OnDestroy()
    {
        // Clean up container
        _container?.Dispose();
    }
}
