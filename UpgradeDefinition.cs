using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Definition of an upgrade including per-level modifiers and cost scaling.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "IdleCarCulture/Upgrade Definition", order = 1)]
    public class UpgradeDefinition : ScriptableObject
    {
        /// <summary>
        /// Type of upgrade (Engine, Turbo, etc.).
        /// </summary>
        public UpgradeType upgradeType;

        /// <summary>
        /// Maximum level for this upgrade.
        /// Default from Tuning.UPGRADE_MAX_LEVEL.
        /// </summary>
        public int maxLevel = Tuning.UPGRADE_MAX_LEVEL;

        /// <summary>
        /// Base cost for level 1.
        /// </summary>
        public int baseCost;

        /// <summary>
        /// Exponential multiplier applied per level to compute cost.
        /// Default from Tuning.UPGRADE_COST_EXPONENT.
        /// </summary>
        public float costExponent = Tuning.UPGRADE_COST_EXPONENT;

        /// <summary>
        /// Per-level stat modifiers. Index 0 corresponds to level 1.
        /// </summary>
        public List<LevelModifiers> levelModifiers = new List<LevelModifiers>();

        /// <summary>
        /// Per-level stat modifiers container.
        /// </summary>
        [Serializable]
        public struct LevelModifiers
        {
            /// <summary>
            /// HP modifier at this level (can be negative for reductions).
            /// </summary>
            public int hp;

            /// <summary>
            /// Torque modifier at this level (can be negative).
            /// </summary>
            public int torque;

            /// <summary>
            /// Weight modifier at this level (negative allowed to indicate weight reduction).
            /// </summary>
            public int weight;

            /// <summary>
            /// Grip modifier at this level.
            /// </summary>
            public int grip;

            /// <summary>
            /// Suspension modifier at this level.
            /// </summary>
            public int suspension;
        }

        /// <summary>
        /// Returns the cost required to reach the next level from <paramref name="currentLevel"/>.
        /// Returns 0 if already at or above <see cref="maxLevel"/>.
        /// </summary>
        public int GetCostForNextLevel(int currentLevel)
        {
            if (maxLevel <= 0) return 0;

            currentLevel = Mathf.Max(0, currentLevel);
            if (currentLevel >= maxLevel) return 0;

            // Cost for next level uses currentLevel as the exponent step (level 0 -> baseCost)
            float cost = baseCost * Mathf.Pow(costExponent, currentLevel);
            return Mathf.Max(0, Mathf.RoundToInt(cost));
        }

        /// <summary>
        /// Returns the <see cref="LevelModifiers"/> for the requested 1-based <paramref name="level"/>.
        /// If out of range, the level will be clamped to [1, maxLevel].
        /// </summary>
        public LevelModifiers GetModifiersAtLevel(int level)
        {
            if (maxLevel <= 0 || level <= 0)
                return default;

            int clamped = Mathf.Clamp(level, 1, maxLevel);
            EnsureModifiersSize();
            return levelModifiers[clamped - 1];
        }

        private void EnsureModifiersSize()
        {
            if (maxLevel < 0) maxLevel = 0;

            // Resize list to match maxLevel
            while (levelModifiers.Count < maxLevel)
            {
                levelModifiers.Add(default);
            }

            while (levelModifiers.Count > maxLevel && maxLevel >= 0)
            {
                levelModifiers.RemoveAt(levelModifiers.Count - 1);
            }
        }

        private void OnValidate()
        {
            if (maxLevel <= 0) maxLevel = 5;
            if (costExponent < 1f) costExponent = 1f;
            if (baseCost < 0) baseCost = 0;

            EnsureModifiersSize();
        }
    }
}
