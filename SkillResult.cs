using System;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Result of player skill checks for a race segment. Scores are in the range [0,1].
    /// </summary>
    [Serializable]
    public struct SkillResult
    {
        /// <summary>
        /// Tire heat score (0..1).
        /// </summary>
        public float tireHeatScore;

        /// <summary>
        /// Launch score (0..1).
        /// </summary>
        public float launchScore;

        /// <summary>
        /// Shift score (0..1).
        /// </summary>
        public float shiftScore;

        /// <summary>
        /// Returns the weighted overall score based on Tuning weights:
        /// Launch (Tuning.SKILL_WEIGHT_LAUNCH), Shift (Tuning.SKILL_WEIGHT_SHIFT), Heat (Tuning.SKILL_WEIGHT_TIRE_HEAT).
        /// The result is clamped to the range [0,1].
        /// </summary>
        public float Overall()
        {
            float overall = launchScore * Tuning.SKILL_WEIGHT_LAUNCH + shiftScore * Tuning.SKILL_WEIGHT_SHIFT + tireHeatScore * Tuning.SKILL_WEIGHT_TIRE_HEAT;
            return Mathf.Clamp01(overall);
        }
    }
}
