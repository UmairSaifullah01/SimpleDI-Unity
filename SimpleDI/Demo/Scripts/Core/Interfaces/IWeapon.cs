using UnityEngine;

namespace THEBADDEST.UnityDI.Demo
{
    /// <summary>
    /// Interface defining the contract for weapons in the game.
    /// </summary>
    public interface IWeapon
    {
        /// <summary>
        /// Performs the weapon's attack action.
        /// </summary>
        void Attack();

        /// <summary>
        /// Gets the weapon's damage value.
        /// </summary>
        float Damage { get; }

        /// <summary>
        /// Gets the weapon's name.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the weapon's configuration.
        /// </summary>
        WeaponConfig Config { get; }
    }
}