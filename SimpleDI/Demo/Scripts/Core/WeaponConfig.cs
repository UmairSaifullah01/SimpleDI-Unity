using UnityEngine;

namespace THEBADDEST.UnityDI.Demo
{
    /// <summary>
    /// Base configuration for weapons.
    /// </summary>
    public abstract class WeaponConfig : ScriptableObject
    {
        [Header("Basic Stats")]
        [SerializeField] protected float damage = 25f;
        [SerializeField] protected string weaponName = "Weapon";

        public float Damage => damage;
        public string Name => weaponName;
    }
}