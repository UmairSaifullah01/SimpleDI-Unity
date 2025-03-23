using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace THEBADDEST.SimpleDependencyInjection.UIDemo
{
    /// <summary>
    /// Main menu screen implementation.
    /// </summary>
    [Injectable(Lifetime.Scoped)]
    public class MainMenuScreen : MonoBehaviour, IScreen
    {
        [SerializeField] private GameObject screenObject;
        [SerializeField] private Button playButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private TextMeshProUGUI titleText;

        private void Start()
        {
            // Register with screen manager
            var screenManager = GetComponent<ScreenManager>();
            if (screenManager != null)
            {
                screenManager.RegisterScreen(this);
            }

            // Setup button listeners
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayClicked);
            }
            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(OnSettingsClicked);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
        }

        public void Show()
        {
            screenObject.SetActive(true);
            Debug.Log("Main Menu Screen shown");
        }

        public void Hide()
        {
            screenObject.SetActive(false);
            Debug.Log("Main Menu Screen hidden");
        }

        public bool IsVisible => screenObject.activeSelf;

        private void OnPlayClicked()
        {
            Debug.Log("Play button clicked");
            // Add play logic here
        }

        private void OnSettingsClicked()
        {
            Debug.Log("Settings button clicked");
            // Add settings logic here
        }

        private void OnQuitClicked()
        {
            Debug.Log("Quit button clicked");
            Application.Quit();
        }
    }
}