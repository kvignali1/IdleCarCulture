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
        /// Returns the weighted overall score: launch 0.4, shift 0.4, heat 0.2.
        /// The result is clamped to the range [0,1].
        /// </summary>
        public float Overall()
        {
            float overall = launchScore * 0.4f + shiftScore * 0.4f + tireHeatScore * 0.2f;
            return Mathf.Clamp01(overall);
        }
    }
}
