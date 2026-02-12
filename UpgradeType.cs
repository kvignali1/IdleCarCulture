namespace IdleCarCulture
{
    /// <summary>
    /// Types of vehicle upgrades and the systems they affect.
    /// </summary>
    public enum UpgradeType
    {
        /// <summary>
        /// Engine upgrades increase horsepower and torque by improving internal components,
        /// fueling, or tuning.
        /// </summary>
        Engine,

        /// <summary>
        /// Turbo upgrades add forced induction (turbochargers/superchargers) to increase
        /// boost pressure and overall engine power.
        /// </summary>
        Turbo,

        /// <summary>
        /// Transmission upgrades affect gear ratios and shifting behavior, improving
        /// acceleration, driveability, or top-speed delivery.
        /// </summary>
        Transmission,

        /// <summary>
        /// Tires upgrades improve grip, traction, and handling characteristics.
        /// </summary>
        Tires,

        /// <summary>
        /// Suspension upgrades modify ride height, damping, and stiffness to enhance
        /// handling, stability, and comfort.
        /// </summary>
        Suspension
    }
}
