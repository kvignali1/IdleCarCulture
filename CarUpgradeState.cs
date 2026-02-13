using System;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Serializable state container for a car's applied upgrade levels.
    /// </summary>
    [Serializable]
    public class CarUpgradeState
    {
        /// <summary>
        /// Identifier of the car this state belongs to.
        /// </summary>
        public string carId;

        /// <summary>
        /// Engine upgrade level.
        /// </summary>
        public int engineLevel;

        /// <summary>
        /// Turbo upgrade level.
        /// </summary>
        public int turboLevel;

        /// <summary>
        /// Transmission upgrade level.
        /// </summary>
        public int transmissionLevel;

        /// <summary>
        /// Tires upgrade level.
        /// </summary>
        public int tiresLevel;

        /// <summary>
        /// Suspension upgrade level.
        /// </summary>
        public int suspensionLevel;

        /// <summary>
        /// Returns the current level for the given <paramref name="type"/>.
        /// </summary>
        public int GetLevel(UpgradeType type)
        {
            switch (type)
            {
                case UpgradeType.Engine: return engineLevel;
                case UpgradeType.Turbo: return turboLevel;
                case UpgradeType.Transmission: return transmissionLevel;
                case UpgradeType.Tires: return tiresLevel;
                case UpgradeType.Suspension: return suspensionLevel;
                default: return 0;
            }
        }

        /// <summary>
        /// Sets the level for the given <paramref name="type"/> to <paramref name="level"/>.
        /// </summary>
        public void SetLevel(UpgradeType type, int level)
        {
            switch (type)
            {
                case UpgradeType.Engine: engineLevel = level; break;
                case UpgradeType.Turbo: turboLevel = level; break;
                case UpgradeType.Transmission: transmissionLevel = level; break;
                case UpgradeType.Tires: tiresLevel = level; break;
                case UpgradeType.Suspension: suspensionLevel = level; break;
            }
        }
    }
}
