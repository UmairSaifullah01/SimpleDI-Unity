using UnityEngine;

namespace THEBADDEST.SimpleDependencyInjection.UIDemo
{
    /// <summary>
    /// Interface for UI screens in the game.
    /// </summary>
    public interface IScreen
    {
        /// <summary>
        /// Shows the screen.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides the screen.
        /// </summary>
        void Hide();

        /// <summary>
        /// Gets whether the screen is currently visible.
        /// </summary>
        bool IsVisible { get; }
    }
}