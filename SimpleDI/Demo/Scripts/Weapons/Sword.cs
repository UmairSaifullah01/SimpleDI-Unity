using UnityEngine;

namespace THEBADDEST.SimpleDependencyInjection.Demo
{
    /// <summary>
    /// Represents a sword weapon in the game.
    /// </summary>
    [Injectable(Lifetime.Scoped)]
    public class Sword : MonoBehaviour, IWeapon
    {
        [SerializeField] private SwordConfig config;
        private float currentDurability;

        public float Damage => config.Damage;
        public string Name => config.Name;
        public WeaponConfig Config => config;

        private void Start()
        {
            currentDurability = config.Durability;
        }

        public void Attack()
        {
            if (currentDurability <= 0)
            {
                Debug.LogWarning("Sword is broken!");
                return;
            }

            currentDurability -= config.DurabilityLossPerSwing;
            Debug.Log($"Swinging sword! Damage: {Damage}, Durability: {currentDurability:F1}/{config.Durability}");
        }

        public void Repair()
        {
            currentDurability = config.Durability;
            Debug.Log("Sword repaired!");
        }
    }
}