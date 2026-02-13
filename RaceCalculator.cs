using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Pure C# utility class for computing race outcomes and player rating (PR).
    /// </summary>
    public static class RaceCalculator
    {
        /// <summary>
        /// Computes Performance Rating (PR) for a car based on its stats and race type.
        /// Event-specific weighting emphasizes HP+Torque, penalizes Weight, and incorporates Grip and Suspension.
        /// </summary>
        public static float ComputePR(ComputedCarStats stats, RaceEventType eventType)
        {
            float powerScore = stats.hp * 0.6f + stats.torque * 0.4f;
            float weightPenalty = stats.weight * 0.1f;
            float gripBonus = stats.grip * 0.05f;
            float suspensionBonus = stats.suspension * 0.03f;

            float pr = powerScore - weightPenalty + gripBonus + suspensionBonus;

            // Event-specific modifiers
            switch (eventType)
            {
                case RaceEventType.IllegalDig:
                case RaceEventType.IllegalRoll:
                    // Illegal races emphasize power and acceleration
                    pr *= 1.1f;
                    break;
                case RaceEventType.LegalLocal:
                case RaceEventType.LegalRegional:
                    // Legal races slightly reduce raw power emphasis
                    pr *= 0.95f;
                    break;
            }

            return Mathf.Max(0, pr);
        }

        /// <summary>
        /// Resolves a race between player and opponent, applying skill multipliers, randomness,
        /// and prestige bonuses to compute outcome and payout.
        /// </summary>
        public static RaceOutcome ResolveRace(
            ComputedCarStats playerStats,
            ComputedCarStats opponentStats,
            RaceEventType eventType,
            SkillResult skill,
            int prestigeBonus,
            float rngSeedOptional = -1f)
        {
            // Set random seed if provided
            if (rngSeedOptional >= 0)
                Random.InitState((int)rngSeedOptional);

            // Compute base PR
            float playerPR = ComputePR(playerStats, eventType);
            float opponentPR = ComputePR(opponentStats, eventType);

            // Apply skill multiplier based on event legality
            float skillMultiplier = (eventType == RaceEventType.IllegalDig || eventType == RaceEventType.IllegalRoll)
                ? (1f + 0.10f * skill.Overall())
                : (1f + 0.15f * skill.Overall());

            playerPR *= skillMultiplier;

            // Apply prestige bonus (flat addition)
            playerPR += prestigeBonus;

            // Apply small randomness (+/- 3%)
            float randomness = Random.Range(0.97f, 1.03f);
            playerPR *= randomness;

            // Opponent gets slight randomness too
            float opponentRandomness = Random.Range(0.97f, 1.03f);
            opponentPR *= opponentRandomness;

            // Determine winner
            bool playerWins = playerPR > opponentPR;

            // Compute payout multiplier based on difficulty
            float prRatio = opponentPR > 0 ? playerPR / opponentPR : 1f;
            float payoutMultiplier = playerWins
                ? Mathf.Clamp(1.5f / prRatio, 0.5f, 3.0f)  // Harder opponents = higher payout
                : 0.1f;  // Losing gives minimal payout

            return new RaceOutcome
            {
                win = playerWins,
                playerPR = playerPR,
                opponentPR = opponentPR,
                payoutMultiplier = payoutMultiplier
            };
        }
    }
}
