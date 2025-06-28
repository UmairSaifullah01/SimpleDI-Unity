using UnityEngine;
using System.Threading.Tasks;

namespace THEBADDEST.UnityDI.ServiceDemo
{
    /// <summary>
    /// Interface for save service.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Saves data asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of data to save.</typeparam>
        /// <param name="key">Save key.</param>
        /// <param name="data">Data to save.</param>
        Task SaveDataAsync<T>(string key, T data);

        /// <summary>
        /// Loads data asynchronously.
        /// </summary>
        /// <typeparam name="T">Type of data to load.</typeparam>
        /// <param name="key">Save key.</param>
        /// <returns>Loaded data.</returns>
        Task<T> LoadDataAsync<T>(string key);

        /// <summary>
        /// Checks if data exists.
        /// </summary>
        /// <param name="key">Save key.</param>
        /// <returns>True if data exists.</returns>
        bool HasData(string key);
    }
}