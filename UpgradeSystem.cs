using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// System for purchasing and applying car upgrades.
    /// Supports upgrading any car by carId with prestige cost reductions.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        /// <summary>
        /// Returns the cost to upgrade a specific car, or -1 if at max level or error.
        /// Applies prestige cost reduction from PrestigeSystem.
        /// </summary>
        public long GetUpgradeCost(string carId, UpgradeType upgradeType)
        {
            if (string.IsNullOrEmpty(carId))
            {
                Debug.LogWarning("GetUpgradeCost called with empty carId.");
                return -1;
            }

            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("GameManager not found.");
                return -1;
            }

            var profile = manager.GetProfile();
            if (profile == null)
            {
                Debug.LogWarning("PlayerProfile not found.");
                return -1;
            }

            // Get upgrade state for the specific car
            var upgradeState = profile.GetOrCreateUpgradeState(carId);
            if (upgradeState == null)
            {
                Debug.LogWarning($"Upgrade state not found for car {carId}.");
                return -1;
            }

            // Find upgrade definition
            UpgradeDefinition upgradeDef = null;
            foreach (var def in manager.upgradeDefs)
            {
                if (def != null && def.upgradeType == upgradeType)
                {
                    upgradeDef = def;
                    break;
                }
            }

            if (upgradeDef == null)
            {
                Debug.LogWarning($"Upgrade definition not found for {upgradeType}");
                return -1;
            }

            int currentLevel = upgradeState.GetLevel(upgradeType);
            if (currentLevel >= upgradeDef.maxLevel)
                return -1;

            // Apply prestige reduction
            var prestigeSystem = FindObjectOfType<PrestigeSystem>();
            if (prestigeSystem == null)
                Debug.LogWarning("[UpgradeSystem] PrestigeSystem not found. Prestige cost reduction will not apply.");
            float costMultiplier = prestigeSystem != null ? prestigeSystem.GetCostMultiplier() : 1f;
            long cost = (long)(upgradeDef.GetCostForNextLevel(currentLevel) * costMultiplier);

            return cost;
        }

        /// <summary>
        /// Attempts to purchase an upgrade for the specified car.
        /// Returns true if successful, false if insufficient funds or already at max level.
        /// Applies prestige cost reduction and saves after successful upgrade.
        /// </summary>
        public bool TryUpgrade(string carId, UpgradeType upgradeType)
        {
            if (string.IsNullOrEmpty(carId))
            {
                Debug.LogWarning("TryUpgrade called with empty carId.");
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

            // Get upgrade state for the specific car
            var upgradeState = profile.GetOrCreateUpgradeState(carId);
            if (upgradeState == null)
            {
                Debug.LogError($"Upgrade state not found for car {carId}.");
                return false;
            }

            // Find upgrade definition
            UpgradeDefinition upgradeDef = null;
            foreach (var def in manager.upgradeDefs)
            {
                if (def != null && def.upgradeType == upgradeType)
                {
                    upgradeDef = def;
                    break;
                }
            }

            if (upgradeDef == null)
            {
                Debug.LogWarning($"Upgrade definition not found for {upgradeType}");
                return false;
            }

            int currentLevel = upgradeState.GetLevel(upgradeType);

            // Check if at max level
            if (currentLevel >= upgradeDef.maxLevel)
            {
                Debug.LogWarning($"{upgradeType} already at max level {upgradeDef.maxLevel}");
                return false;
            }

            // Calculate cost with prestige reduction
            var prestigeSystem = FindObjectOfType<PrestigeSystem>();
            if (prestigeSystem == null)
                Debug.LogWarning("[UpgradeSystem] PrestigeSystem not found. Prestige cost reduction will not apply.");
            float costMultiplier = prestigeSystem != null ? prestigeSystem.GetCostMultiplier() : 1f;
            long cost = (long)(upgradeDef.GetCostForNextLevel(currentLevel) * costMultiplier);

            if (profile.money < cost)
            {
                Debug.LogWarning($"Insufficient funds. Cost: {cost}, Available: {profile.money}");
                return false;
            }

            // Spend money
            var economy = FindObjectOfType<EconomyManager>();
            if (economy == null)
            {
                Debug.LogError("[UpgradeSystem] EconomyManager not found. Cannot deduct upgrade cost.");
                return false;
            }
            
            if (!economy.TrySpend(cost))
            {
                Debug.LogError("Failed to spend money for upgrade.");
                return false;
            }

            // Apply upgrade
            upgradeState.SetLevel(upgradeType, currentLevel + 1);
            manager.Save();

            Debug.Log($"Upgrade successful: {carId} - {upgradeType} to level {currentLevel + 1}. Cost: {cost}");
            return true;
        }
    }
}
