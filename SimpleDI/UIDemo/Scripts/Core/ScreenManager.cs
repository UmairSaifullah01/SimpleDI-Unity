using UnityEngine;
using System.Collections.Generic;

namespace THEBADDEST.SimpleDependencyInjection.UIDemo
{
    /// <summary>
    /// Manages UI screens in the game.
    /// </summary>
    [Injectable(Lifetime.Singleton)]
    public class ScreenManager : MonoBehaviour, IScreenManager
    {
        private readonly Dictionary<System.Type, IScreen> _screens = new Dictionary<System.Type, IScreen>();

        public void RegisterScreen<T>(T screen) where T : IScreen
        {
            _screens[typeof(T)] = screen;
        }

        public void ShowScreen<T>() where T : IScreen
        {
            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                screen.Show();
            }
            else
            {
                Debug.LogError($"Screen of type {typeof(T)} not found!");
            }
        }

        public void HideScreen<T>() where T : IScreen
        {
            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                screen.Hide();
            }
            else
            {
                Debug.LogError($"Screen of type {typeof(T)} not found!");
            }
        }

        public T GetScreen<T>() where T : IScreen
        {
            if (_screens.TryGetValue(typeof(T), out var screen))
            {
                return (T)screen;
            }
            Debug.LogError($"Screen of type {typeof(T)} not found!");
            return default;
        }
    }
}