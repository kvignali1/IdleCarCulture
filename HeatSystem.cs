using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Heat system that accumulates from illegal races, decays over time in safe zones,
    /// and triggers police encounters based on heat level.
    /// </summary>
    public class HeatSystem : MonoBehaviour
    {
        /// <summary>
        /// Heat threshold percentages for police event triggers.
        /// See Tuning class for adjustable thresholds.
        /// </summary>

        /// <summary>
        /// Base trigger chance per update at different heat levels.
        /// See Tuning class for adjustable chances.
        /// </summary>

        /// <summary>
        /// Adds heat to the player profile from an illegal race.
        /// Heat is capped at 100.
        /// </summary>
        public void AddHeat(float amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("AddHeat called with negative amount.");
                return;
            }

            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GameManager not found.");
                return;
            }

            var profile = manager.GetProfile();
            if (profile == null)
            {
                Debug.LogError("PlayerProfile not found.");
                return;
            }

            profile.heat = Mathf.Clamp(profile.heat + amount, 0f, 100f);
            manager.OnHeatChanged?.Invoke(profile.heat);
            manager.Save();
            Debug.Log($"Heat increased by {amount}. Current heat: {profile.heat:F1}");
        }

        /// <summary>
        /// Decays heat over time (called in Update when in City/safe zone).
        /// Typically called from scene managers when player is not in a race.
        /// </summary>
        public void DecayHeat(float perSecond)
        {
            if (perSecond < 0)
            {
                Debug.LogWarning("DecayHeat called with negative rate.");
                return;
            }

            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var profile = manager.GetProfile();
            if (profile == null)
                return;

            float decay = perSecond * Time.deltaTime;
            profile.heat = Mathf.Clamp(profile.heat - decay, 0f, 100f);
            manager.OnHeatChanged?.Invoke(profile.heat);
            manager.Save();
        }

        /// <summary>
        /// Determines if a police event should trigger based on current heat and random chance.
        /// Higher heat = higher trigger probability.
        /// </summary>
        public bool ShouldTriggerPoliceEvent()
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return false;

            var profile = manager.GetProfile();
            if (profile == null)
                return false;

            float heat = profile.heat;
            float triggerChance = 0f;

            if (heat >= Tuning.HEAT_THRESHOLD_HIGH)
                triggerChance = Tuning.HEAT_TRIGGER_CHANCE_HIGH;
            else if (heat >= Tuning.HEAT_THRESHOLD_MEDIUM)
                triggerChance = Tuning.HEAT_TRIGGER_CHANCE_MEDIUM;
            else if (heat >= Tuning.HEAT_THRESHOLD_LOW)
                triggerChance = Tuning.HEAT_TRIGGER_CHANCE_LOW;

            bool triggered = Random.value < triggerChance;

            if (triggered)
                Debug.Log($"Police event triggered! Heat: {heat:F1}, Chance: {triggerChance:P}");

            return triggered;
        }

        /// <summary>
        /// Resolves a police encounter based on car stats (grip and suspension for escape chance).
        /// Returns outcome with fine or escape status.
        /// </summary>
        public PoliceOutcome ResolvePoliceEvent(ComputedCarStats stats)
        {
            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GameManager not found.");
                return new PoliceOutcome { escaped = false, fineAmount = 1000, heatLost = 0f };
            }

            var profile = manager.GetProfile();
            if (profile == null)
            {
                Debug.LogError("PlayerProfile not found.");
                return new PoliceOutcome { escaped = false, fineAmount = 1000, heatLost = 0f };
            }

            // Escape chance based on car grip and suspension (handling)
            float escapeChanceBase = Tuning.POLICE_ESCAPE_CHANCE_BASE + (stats.grip + stats.suspension) * Tuning.POLICE_ESCAPE_STAT_BONUS;
            escapeChanceBase = Mathf.Clamp01(escapeChanceBase);

            bool escaped = Random.value < escapeChanceBase;

            long fineAmount = 0;
            float heatLost = 0f;

            if (escaped)
            {
                heatLost = profile.heat * Tuning.POLICE_ESCAPE_HEAT_REDUCTION_PERCENT;
                Debug.Log($"Escaped police! Heat lost: {heatLost:F1}");
            }
            else
            {
                // Fine scales with current heat
                fineAmount = (long)(Tuning.POLICE_BASE_FINE + profile.heat * Tuning.POLICE_FINE_HEAT_MULTIPLIER);
                heatLost = profile.heat; // Lose all heat if caught
                Debug.Log($"Caught by police! Fine: {fineAmount}, Heat lost: {heatLost:F1}");
            }

            // Apply heat loss
            profile.heat = Mathf.Clamp(profile.heat - heatLost, 0f, 100f);
            manager.OnHeatChanged?.Invoke(profile.heat);

            // If caught, deduct fine via EconomyManager
            if (!escaped && fineAmount > 0)
            {
                var economy = FindObjectOfType<EconomyManager>();
                if (economy != null)
                {
                    economy.TrySpend(fineAmount);
                }
                else
                {
                    Debug.LogWarning("EconomyManager not found. Fine not deducted.");
                }
            }

            manager.Save();

            return new PoliceOutcome
            {
                escaped = escaped,
                fineAmount = fineAmount,
                heatLost = heatLost
            };
        }
    }
}
