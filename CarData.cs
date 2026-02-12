using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// ScriptableObject containing base car metadata and stats used by the game.
    /// </summary>
    [CreateAssetMenu(fileName = "CarData", menuName = "IdleCarCulture/Car Data", order = 0)]
    public class CarData : ScriptableObject
    {
        /// <summary>
        /// Unique identifier for this car (slug). If empty, it will be generated from <see cref="displayName"/>.
        /// </summary>
        public string id;

        /// <summary>
        /// Human-readable name displayed in the UI.
        /// </summary>
        public string displayName;

        /// <summary>
        /// Drivetrain configuration (FWD/RWD/AWD).
        /// </summary>
        public Drivetrain drivetrain;

        /// <summary>
        /// Base horsepower.
        /// </summary>
        public int baseHP;

        /// <summary>
        /// Base torque.
        /// </summary>
        public int baseTorque;

        /// <summary>
        /// Base vehicle weight (units as used by game).
        /// </summary>
        public int baseWeight;

        /// <summary>
        /// Base grip value affecting cornering.
        /// </summary>
        public int baseGrip;

        /// <summary>
        /// Base suspension rating affecting ride and handling.
        /// </summary>
        public int baseSuspension;

        /// <summary>
        /// Tier or rarity level of the car.
        /// </summary>
        public int tier;

        /// <summary>
        /// Base monetary value for buying/selling.
        /// </summary>
        public int baseValue;

        private static string GenerateIdFromDisplayName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            var s = name.Trim().ToLowerInvariant();
            s = Regex.Replace(s, "\\s+", "-");
            s = Regex.Replace(s, "[^a-z0-9\-_]", string.Empty);
            s = Regex.Replace(s, "^-+|-+$", string.Empty);
            return s;
        }

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(displayName))
                displayName = string.Empty;

            if (string.IsNullOrWhiteSpace(id))
            {
                id = GenerateIdFromDisplayName(displayName);
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                id = Guid.NewGuid().ToString("N");
            }

            baseHP = Mathf.Max(0, baseHP);
            baseTorque = Mathf.Max(0, baseTorque);
            baseWeight = Mathf.Max(0, baseWeight);
            baseGrip = Mathf.Max(0, baseGrip);
            baseSuspension = Mathf.Max(0, baseSuspension);
            tier = Mathf.Max(0, tier);
            baseValue = Mathf.Max(0, baseValue);
        }
    }
}
