using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// MonoBehaviour that manages player economy: money spending, earning, and race payouts.
    /// Depends on GameManager singleton for profile access.
    /// </summary>
    public class EconomyManager : MonoBehaviour
    {
        /// <summary>
        /// Base payout per tier for illegal events.
        /// </summary>
        private static readonly int[] IllegalBasePayout = { 500, 1000, 1500, 2500, 4000 };

        /// <summary>
        /// Base payout per tier for legal events.
        /// </summary>
        private static readonly int[] LegalBasePayout = { 1000, 2000, 3500, 5500, 8000 };

        /// <summary>
        /// Attempts to spend the specified amount of money.
        /// Returns true if successful, false if insufficient funds.
        /// Fires GameManager.OnMoneyChanged and calls GameManager.Save() on success.
        /// </summary>
        public bool TrySpend(long amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("TrySpend called with negative amount.");
                return false;
            }

            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GameManager not found.");
                return false;
            }

            var profile = manager.GetProfile();
            if (profile == null)
            {
                Debug.LogError("PlayerProfile not found.");
                return false;
            }

            if (profile.money < amount)
            {
                Debug.LogWarning($"Insufficient funds. Required: {amount}, Available: {profile.money}");
                return false;
            }

            profile.money -= amount;
            manager.OnMoneyChanged?.Invoke(profile.money);
            manager.Save();
            Debug.Log($"Spent {amount}. New balance: {profile.money}");
            return true;
        }

        /// <summary>
        /// Adds the specified amount of money to the player balance.
        /// Fires GameManager.OnMoneyChanged and calls GameManager.Save().
        /// </summary>
        public void AddMoney(long amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("AddMoney called with negative amount.");
                return;
            }

            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GameManager not found.");
                return;
            }

            manager.AddMoney(amount);
            manager.Save();
            Debug.Log($"Added {amount}. New balance: {manager.GetProfile()?.money}");
        }

        /// <summary>
        /// Calculates the payout for a race based on event type, opponent tier, win status,
        /// payout multiplier, and prestige currency bonus.
        /// 
        /// Formula:
        /// - Base payout determined by event type (illegal vs legal) and tier (0-4)
        /// - Multiplied by payoutMultiplier
        /// - Prestige bonus: +2% per prestigeCurrency
        /// - Only awarded if player won
        /// </summary>
        public long CalculateRacePayout(
            RaceEventType eventType,
            int opponentTier,
            bool win,
            float payoutMultiplier,
            int prestigeCurrency)
        {
            if (!win)
                return 0;

            // Clamp opponent tier to valid range [0, 4]
            opponentTier = Mathf.Clamp(opponentTier, 0, 4);

            // Select base payout table based on event type
            int[] basePayoutTable = (eventType == RaceEventType.IllegalDig || eventType == RaceEventType.IllegalRoll)
                ? IllegalBasePayout
                : LegalBasePayout;

            long basePayout = basePayoutTable[opponentTier];

            // Apply payout multiplier
            float adjustedPayout = basePayout * payoutMultiplier;

            // Apply prestige bonus: +2% per prestigeCurrency
            float prestigeMultiplier = 1f + (0.02f * prestigeCurrency);
            adjustedPayout *= prestigeMultiplier;

            long finalPayout = (long)adjustedPayout;
            Debug.Log($"Race payout calculated: {finalPayout} (base: {basePayout}, multiplier: {payoutMultiplier}, prestige: {prestigeMultiplier:F2}x)");
            return finalPayout;
        }
    }
}
