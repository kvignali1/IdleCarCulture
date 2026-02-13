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
        /// </summary>
        private const float LowHeatThreshold = 30f;
        private const float MediumHeatThreshold = 60f;
        private const float HighHeatThreshold = 85f;

        /// <summary>
        /// Base trigger chance per update at different heat levels.
        /// </summary>
        private const float LowHeatTriggerChance = 0.01f;
        private const float MediumHeatTriggerChance = 0.05f;
        private const float HighHeatTriggerChance = 0.15f;

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

            if (heat >= HighHeatThreshold)
                triggerChance = HighHeatTriggerChance;
            else if (heat >= MediumHeatThreshold)
                triggerChance = MediumHeatTriggerChance;
            else if (heat >= LowHeatThreshold)
                triggerChance = LowHeatTriggerChance;

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
            float escapeChanceBase = 0.3f + (stats.grip + stats.suspension) * 0.001f;
            escapeChanceBase = Mathf.Clamp01(escapeChanceBase);

            bool escaped = Random.value < escapeChanceBase;

            long fineAmount = 0;
            float heatLost = 0f;

            if (escaped)
            {
                heatLost = profile.heat * 0.5f; // Lose 50% heat on escape
                Debug.Log($"Escaped police! Heat lost: {heatLost:F1}");
            }
            else
            {
                // Fine scales with current heat
                fineAmount = (long)(500 + profile.heat * 50);
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
