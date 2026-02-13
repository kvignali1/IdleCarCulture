namespace IdleCarCulture
{
    /// <summary>
    /// Represents a race opportunity available to the player.
    /// </summary>
    public class RaceOpportunity
    {
        /// <summary>
        /// Type of race event.
        /// </summary>
        public RaceEventType eventType;

        /// <summary>
        /// Tier of the opponent (0-4).
        /// </summary>
        public int opponentTier;

        /// <summary>
        /// Suggested bet amount for the player.
        /// </summary>
        public long betSuggestion;

        /// <summary>
        /// Display text for the UI card (e.g., "Street Dig: Tier 2").
        /// </summary>
        public string displayText;

        /// <summary>
        /// Opponent car display name (if available).
        /// </summary>
        public string opponentCarName;

        public RaceOpportunity(
            RaceEventType type,
            int tier,
            long bet,
            string display,
            string opponentName = "Unknown")
        {
            eventType = type;
            opponentTier = tier;
            betSuggestion = bet;
            displayText = display;
            opponentCarName = opponentName;
        }
    }
}
