using UnityEngine;
using System.Collections.Generic;

namespace THEBADDEST.SimpleDependencyInjection.ServiceDemo
{
    /// <summary>
    /// Implementation of analytics service.
    /// </summary>
    [Injectable(Lifetime.Singleton)]
    public class AnalyticsService : MonoBehaviour, IAnalyticsService
    {
        private readonly Dictionary<string, object> _userProperties = new Dictionary<string, object>();

        public void LogEvent(string eventName, params (string key, object value)[] parameters)
        {
            var logMessage = $"Analytics Event: {eventName}";
            if (parameters != null && parameters.Length > 0)
            {
                logMessage += "\nParameters:";
                foreach (var param in parameters)
                {
                    logMessage += $"\n- {param.key}: {param.value}";
                }
            }
            Debug.Log(logMessage);
        }

        public void SetUserProperty(string propertyName, object value)
        {
            _userProperties[propertyName] = value;
            Debug.Log($"User Property Set: {propertyName} = {value}");
        }
    }
}