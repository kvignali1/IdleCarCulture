namespace IdleCarCulture
{
    /// <summary>
    /// Result of a race between two cars.
    /// </summary>
    public struct RaceOutcome
    {
        /// <summary>
        /// True if the player won, false otherwise.
        /// </summary>
        public bool win;

        /// <summary>
        /// Player's computed PR for this race.
        /// </summary>
        public float playerPR;

        /// <summary>
        /// Opponent's computed PR for this race.
        /// </summary>
        public float opponentPR;

        /// <summary>
        /// Recommended payout multiplier based on race difficulty and outcome.
        /// </summary>
        public float payoutMultiplier;
    }
}
