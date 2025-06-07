using UnityEngine;

namespace THEBADDEST.UnityDI.UIDemo
{
    /// <summary>
    /// Interface for managing UI screens.
    /// </summary>
    public interface IScreenManager
    {
        /// <summary>
        /// Shows a screen by type.
        /// </summary>
        /// <typeparam name="T">The type of screen to show.</typeparam>
        void ShowScreen<T>() where T : IScreen;

        /// <summary>
        /// Hides a screen by type.
        /// </summary>
        /// <typeparam name="T">The type of screen to hide.</typeparam>
        void HideScreen<T>() where T : IScreen;

        /// <summary>
        /// Gets a screen by type.
        /// </summary>
        /// <typeparam name="T">The type of screen to get.</typeparam>
        /// <returns>The screen instance.</returns>
        T GetScreen<T>() where T : IScreen;
    }
}