using UnityEngine;

namespace THEBADDEST.UnityDI.Demo
{
    /// <summary>
    /// Demo class demonstrating the usage of dependency injection with weapons.
    /// </summary>
    public class Demo : MonoBehaviour
    {
        [SerializeField] private Player player;
        private WeaponInstaller weaponInstaller;

        private void Awake()
        {
            DemoForMonoClasses();
        }

        private void DemoForMonoClasses()
        {
            // Get the weapon installer
            weaponInstaller = GetComponent<WeaponInstaller>();
            if (weaponInstaller == null)
            {
                Debug.LogError("WeaponInstaller not found!");
                return;
            }

            // Install dependencies
            weaponInstaller.Install();

            // Demo weapon attacks
            Debug.Log("=== Starting Weapon Demo ===");
            player.Attack();

            // Demo player taking damage
            Debug.Log("\n=== Testing Player Damage ===");
            player.TakeDamage(30f);
            player.TakeDamage(20f);

            // Demo player healing
            Debug.Log("\n=== Testing Player Healing ===");
            player.Heal(25f);

            // Demo weapon durability/ammo
            Debug.Log("\n=== Testing Weapon States ===");
            for (int i = 0; i < 5; i++)
            {
                player.Attack();
            }
        }
    }
}