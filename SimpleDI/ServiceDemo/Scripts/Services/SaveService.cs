using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace THEBADDEST.UnityDI.ServiceDemo
{
    /// <summary>
    /// Implementation of save service.
    /// </summary>
    [Injectable(Lifetime.Singleton)]
    public class SaveService : MonoBehaviour, ISaveService
    {
        private readonly Dictionary<string, object> _saveData = new Dictionary<string, object>();

        public async Task SaveDataAsync<T>(string key, T data)
        {
            // Simulate async operation
            await Task.Delay(100);
            _saveData[key] = data;
            Debug.Log($"Data saved for key: {key}");
        }

        public async Task<T> LoadDataAsync<T>(string key)
        {
            // Simulate async operation
            await Task.Delay(100);
            if (_saveData.TryGetValue(key, out var data))
            {
                return (T)data;
            }
            Debug.LogWarning($"No data found for key: {key}");
            return default;
        }

        public bool HasData(string key)
        {
            return _saveData.ContainsKey(key);
        }
    }
}