using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Prestige system allowing players to reset progress for permanent bonuses.
    /// </summary>
    public class PrestigeSystem : MonoBehaviour
    {
        /// <summary>
        /// Money threshold required to prestige.
        /// </summary>
        private const long MoneyPrestigeThreshold = 500000;

        /// <summary>
        /// Reputation threshold required to prestige (alternative).
        /// </summary>
        private const int ReputationPrestigeThreshold = 1000;

        /// <summary>
        /// Cred threshold required to prestige (alternative).
        /// </summary>
        private const int CredPrestigeThreshold = 500;

        /// <summary>
        /// Starting money after prestige.
        /// </summary>
        private const long StartingMoney = 5000;

        /// <summary>
        /// Determines if the player can prestige based on current profile stats.
        /// Returns true if money >= threshold OR (reputation >= threshold OR cred >= threshold).
        /// </summary>
        public bool CanPrestige(PlayerProfile profile)
        {
            if (profile == null)
            {
                Debug.LogWarning("CanPrestige called with null profile.");
                return false;
            }

            bool moneyThreshold = profile.money >= MoneyPrestigeThreshold;
            bool reputationThreshold = profile.reputation >= ReputationPrestigeThreshold;
            bool credThreshold = profile.cred >= CredPrestigeThreshold;

            return moneyThreshold || reputationThreshold || credThreshold;
        }

        /// <summary>
        /// Applies prestige: increments prestigeCurrency, resets progress stats,
        /// keeps owned cars, and saves the profile.
        /// </summary>
        public void ApplyPrestige()
        {
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

            if (!CanPrestige(profile))
            {
                Debug.LogWarning("Prestige requirements not met.");
                return;
            }

            // Increment prestige currency
            profile.prestigeCurrency++;

            // Reset progress stats
            profile.money = StartingMoney;
            profile.heat = 0f;
            profile.cred = 0;
            profile.reputation = 0;

            // Keep owned cars and upgrades
            // Reset upgrade costs via cost reduction (applied in UpgradeDefinition.GetCostForNextLevel)

            // Fire events for reset stats
            manager.OnMoneyChanged?.Invoke(profile.money);
            manager.OnHeatChanged?.Invoke(profile.heat);
            manager.OnCredChanged?.Invoke(profile.cred);
            manager.OnReputationChanged?.Invoke(profile.reputation);

            // Save
            manager.Save();

            Debug.Log($"Prestige applied! New prestige level: {profile.prestigeCurrency}");
            Debug.Log($"Cost Reduction: {GetCostReductionPercent():F1}%, Heat Gain Reduction: {GetHeatGainReductionPercent():F1}%, Income Bonus: {GetIncomeBonusPercent():F1}%");
        }

        /// <summary>
        /// Computed bonus: cost reduction percentage based on prestige level.
        /// Scales from 0% at prestige 0 to higher values per prestige rank.
        /// Formula: 5% per prestige level.
        /// </summary>
        public float GetCostReductionPercent()
        {
            var manager = GameManager.Instance;
            if (manager == null) return 0f;

            var profile = manager.GetProfile();
            if (profile == null) return 0f;

            return profile.prestigeCurrency * 5f;
        }

        /// <summary>
        /// Computed bonus: heat gain reduction percentage from illegal races.
        /// Formula: 3% per prestige level.
        /// </summary>
        public float GetHeatGainReductionPercent()
        {
            var manager = GameManager.Instance;
            if (manager == null) return 0f;

            var profile = manager.GetProfile();
            if (profile == null) return 0f;

            return profile.prestigeCurrency * 3f;
        }

        /// <summary>
        /// Computed bonus: income multiplier from races.
        /// Formula: 10% per prestige level.
        /// </summary>
        public float GetIncomeBonusPercent()
        {
            var manager = GameManager.Instance;
            if (manager == null) return 0f;

            var profile = manager.GetProfile();
            if (profile == null) return 0f;

            return profile.prestigeCurrency * 10f;
        }

        /// <summary>
        /// Returns the multiplier for upgrade costs after prestige reduction.
        /// E.g., 20% reduction = 0.8x multiplier.
        /// </summary>
        public float GetCostMultiplier()
        {
            float reduction = GetCostReductionPercent();
            return Mathf.Clamp01(1f - reduction / 100f);
        }

        /// <summary>
        /// Returns the multiplier for heat gain from illegal races.
        /// E.g., 10% reduction = 0.9x multiplier on heat gain.
        /// </summary>
        public float GetHeatGainMultiplier()
        {
            float reduction = GetHeatGainReductionPercent();
            return Mathf.Clamp01(1f - reduction / 100f);
        }

        /// <summary>
        /// Returns the income multiplier for race payouts.
        /// E.g., 20% bonus = 1.2x multiplier on payout.
        /// </summary>
        public float GetIncomeMultiplier()
        {
            float bonus = GetIncomeBonusPercent();
            return 1f + bonus / 100f;
        }
    }
}
