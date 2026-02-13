using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Represents final car stats after all upgrades are applied.
    /// </summary>
    public struct ComputedCarStats
    {
        /// <summary>
        /// Final horsepower after upgrades.
        /// </summary>
        public int hp;

        /// <summary>
        /// Final torque after upgrades.
        /// </summary>
        public int torque;

        /// <summary>
        /// Final weight after upgrades.
        /// </summary>
        public int weight;

        /// <summary>
        /// Final grip after upgrades.
        /// </summary>
        public int grip;

        /// <summary>
        /// Final suspension after upgrades.
        /// </summary>
        public int suspension;

        /// <summary>
        /// Drivetrain type (unchanged by upgrades).
        /// </summary>
        public Drivetrain drivetrain;

        /// <summary>
        /// Computes final car stats by starting with base <see cref="CarData"/> values
        /// and applying per-level modifiers from <paramref name="upgrades"/> using the
        /// definitions in <paramref name="upgradeDefinitions"/>.
        /// </summary>
        /// <param name="carData">Base car data.</param>
        /// <param name="upgrades">Upgrade state for this car.</param>
        /// <param name="upgradeDefinitions">List of upgrade definitions indexed by type.</param>
        /// <returns>Computed stats struct.</returns>
        public static ComputedCarStats Compute(
            CarData carData,
            CarUpgradeState upgrades,
            List<UpgradeDefinition> upgradeDefinitions)
        {
            if (carData == null) throw new ArgumentNullException(nameof(carData));
            if (upgrades == null) throw new ArgumentNullException(nameof(upgrades));
            if (upgradeDefinitions == null) throw new ArgumentNullException(nameof(upgradeDefinitions));

            var result = new ComputedCarStats
            {
                hp = carData.baseHP,
                torque = carData.baseTorque,
                weight = carData.baseWeight,
                grip = carData.baseGrip,
                suspension = carData.baseSuspension,
                drivetrain = carData.drivetrain
            };

            // Build a map of UpgradeDefinition by UpgradeType for quick lookup
            var defs = new Dictionary<UpgradeType, UpgradeDefinition>();
            foreach (var def in upgradeDefinitions)
            {
                if (def != null)
                    defs[def.upgradeType] = def;
            }

            // Apply each upgrade type
            ApplyUpgradeModifiers(ref result, UpgradeType.Engine, upgrades.engineLevel, defs);
            ApplyUpgradeModifiers(ref result, UpgradeType.Turbo, upgrades.turboLevel, defs);
            ApplyUpgradeModifiers(ref result, UpgradeType.Transmission, upgrades.transmissionLevel, defs);
            ApplyUpgradeModifiers(ref result, UpgradeType.Tires, upgrades.tiresLevel, defs);
            ApplyUpgradeModifiers(ref result, UpgradeType.Suspension, upgrades.suspensionLevel, defs);

            // Ensure stats don't go negative
            result.hp = Mathf.Max(0, result.hp);
            result.torque = Mathf.Max(0, result.torque);
            result.weight = Mathf.Max(0, result.weight);
            result.grip = Mathf.Max(0, result.grip);
            result.suspension = Mathf.Max(0, result.suspension);

            return result;
        }

        private static void ApplyUpgradeModifiers(
            ref ComputedCarStats stats,
            UpgradeType upgradeType,
            int currentLevel,
            Dictionary<UpgradeType, UpgradeDefinition> definitions)
        {
            if (currentLevel <= 0 || !definitions.TryGetValue(upgradeType, out var def))
                return;

            // Apply modifiers cumulatively for levels 1 through currentLevel
            for (int level = 1; level <= currentLevel; level++)
            {
                var modifiers = def.GetModifiersAtLevel(level);
                stats.hp += modifiers.hp;
                stats.torque += modifiers.torque;
                stats.weight += modifiers.weight;
                stats.grip += modifiers.grip;
                stats.suspension += modifiers.suspension;
            }
        }
    }
}
