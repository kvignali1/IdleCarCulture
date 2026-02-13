namespace IdleCarCulture
{
    /// <summary>
    /// Types of race events that can occur in the game.
    /// </summary>
    public enum RaceEventType
    {
        /// <summary>
        /// Illegal event: a dig (ambush-style) race.
        /// </summary>
        IllegalDig,

        /// <summary>
        /// Illegal event: a roll (rolling start) race.
        /// </summary>
        IllegalRoll,

        /// <summary>
        /// Legal local racing event.
        /// </summary>
        LegalLocal,

        /// <summary>
        /// Legal regional racing event.
        /// </summary>
        LegalRegional
    }
}
