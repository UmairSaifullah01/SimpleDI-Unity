using UnityEngine;

namespace THEBADDEST.UnityDI.ServiceDemo
{
    /// <summary>
    /// Interface for analytics service.
    /// </summary>
    public interface IAnalyticsService
    {
        /// <summary>
        /// Logs an event.
        /// </summary>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="parameters">Event parameters.</param>
        void LogEvent(string eventName, params (string key, object value)[] parameters);

        /// <summary>
        /// Logs a user property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="value">Property value.</param>
        void SetUserProperty(string propertyName, object value);
    }
}