using UnityEngine;

namespace THEBADDEST.SimpleDependencyInjection.Demo
{
    /// <summary>
    /// Represents a gun weapon in the game.
    /// </summary>
    [Injectable(Lifetime.Scoped)]
    public class Gun : MonoBehaviour, IWeapon
    {
        [SerializeField] private GunConfig config;
        private int currentAmmo;

        public float Damage => config.Damage;
        public string Name => config.Name;
        public WeaponConfig Config => config;

        private void Start()
        {
            currentAmmo = config.MaxAmmo;
        }

        public void Attack()
        {
            if (currentAmmo <= 0)
            {
                Debug.LogWarning("Out of ammo!");
                return;
            }

            currentAmmo--;
            Debug.Log($"Shooting with a gun! Damage: {Damage}, Ammo: {currentAmmo}/{config.MaxAmmo}");
        }

        public void Reload()
        {
            currentAmmo = config.MaxAmmo;
            Debug.Log("Gun reloaded!");
        }
    }
}