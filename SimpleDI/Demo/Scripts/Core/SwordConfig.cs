using UnityEngine;

namespace THEBADDEST.UnityDI.Demo
{
    /// <summary>
    /// Configuration for the Sword weapon.
    /// </summary>
    [CreateAssetMenu(fileName = "SwordConfig", menuName = "SimpleDI/Demo/Weapons/Sword Config")]
    public class SwordConfig : WeaponConfig
    {
        [Header("Sword Specific")]
        [SerializeField] private float swingSpeed = 1f;
        [SerializeField] private float durability = 100f;
        [SerializeField] private float durabilityLossPerSwing = 5f;

        public float SwingSpeed => swingSpeed;
        public float Durability => durability;
        public float DurabilityLossPerSwing => durabilityLossPerSwing;
    }
}