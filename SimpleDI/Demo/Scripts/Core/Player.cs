using UnityEngine;
using System.Linq;

namespace THEBADDEST.SimpleDependencyInjection.Demo
{
    /// <summary>
    /// Player class that demonstrates dependency injection with weapons.
    /// </summary>
    [Injectable]
    public class Player : MonoBehaviour
    {
        [Inject] private IWeapon[] _weapons;
        [SerializeField] private float health = 100f;
        private float currentHealth;

        private void Start()
        {
            currentHealth = health;
            Debug.Log($"Player started with {_weapons.Length} weapons:");
            foreach (var weapon in _weapons)
            {
                Debug.Log($"- {weapon.Name} (Damage: {weapon.Damage})");
            }
        }

        public void Attack()
        {
            if (_weapons == null || _weapons.Length == 0)
            {
                Debug.LogWarning("No weapons available!");
                return;
            }

            foreach (var weapon in _weapons)
            {
                weapon.Attack();
            }
        }

        public void TakeDamage(float damage)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            Debug.Log($"Player took {damage} damage. Health: {currentHealth:F1}/{health}");

            if (currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            Debug.Log("Player died!");
            // Add death logic here
        }

        public void Heal(float amount)
        {
            currentHealth = Mathf.Min(health, currentHealth + amount);
            Debug.Log($"Player healed for {amount}. Health: {currentHealth:F1}/{health}");
        }
    }
}