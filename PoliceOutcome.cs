namespace IdleCarCulture
{
    /// <summary>
    /// Result of a police encounter during a race.
    /// </summary>
    public struct PoliceOutcome
    {
        /// <summary>
        /// True if the player escaped, false if caught.
        /// </summary>
        public bool escaped;

        /// <summary>
        /// Fine amount if caught. Zero if escaped.
        /// </summary>
        public long fineAmount;

        /// <summary>
        /// Heat lost from the encounter.
        /// </summary>
        public float heatLost;
    }
}
