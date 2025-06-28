using UnityEngine;

namespace THEBADDEST.UnityDI.Demo
{
    /// <summary>
    /// Configuration for the Gun weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "GunConfig", menuName = "SimpleDI/Demo/Weapons/Gun Config")]
    public class GunConfig : WeaponConfig
    {
        [Header("Gun Specific")]
        [SerializeField] private float fireRate = 0.5f;
        [SerializeField] private int maxAmmo = 30;

        public float FireRate => fireRate;
        public int MaxAmmo => maxAmmo;
    }
}