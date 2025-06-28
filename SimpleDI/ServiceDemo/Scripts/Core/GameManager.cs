using UnityEngine;
using System.Threading.Tasks;

namespace THEBADDEST.UnityDI.ServiceDemo
{
    /// <summary>
    /// Game manager that demonstrates service usage.
    /// </summary>
    [Injectable]
    public class GameManager : MonoBehaviour
    {
        [Inject] private IAnalyticsService _analytics;
        [Inject] private ISaveService _saveService;

        private int _score;
        private const string ScoreKey = "PlayerScore";

        private async void Start()
        {
            // Load saved score
            if (_saveService.HasData(ScoreKey))
            {
                _score = await _saveService.LoadDataAsync<int>(ScoreKey);
                Debug.Log($"Loaded score: {_score}");
            }

            // Log game start
            _analytics.LogEvent("GameStart",
                ("score", _score),
                ("time", Time.time));
        }

        public async void AddScore(int points)
        {
            _score += points;
            Debug.Log($"Score updated: {_score}");

            // Save score
            await _saveService.SaveDataAsync(ScoreKey, _score);

            // Log score update
            _analytics.LogEvent("ScoreUpdate",
                ("points", points),
                ("total_score", _score));
        }

        public void SetPlayerName(string name)
        {
            _analytics.SetUserProperty("PlayerName", name);
        }
    }
}