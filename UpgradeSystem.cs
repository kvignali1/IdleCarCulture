using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// System for purchasing and applying car upgrades.
    /// </summary>
    public class UpgradeSystem : MonoBehaviour
    {
        /// <summary>
        /// Attempts to purchase an upgrade for the active car.
        /// Returns true if successful, false if insufficient funds or already at max level.
        /// </summary>
        public bool TryUpgrade(UpgradeType upgradeType)
        {
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

            var upgradeState = manager.GetActiveUpgradeState();
            if (upgradeState == null)
            {
                Debug.LogError("Active upgrade state not found.");
                return false;
            }

            // Find the upgrade definition
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

            // Get current level
            int currentLevel = upgradeState.GetLevel(upgradeType);

            // Check if at max level
            if (currentLevel >= upgradeDef.maxLevel)
            {
                Debug.LogWarning($"{upgradeType} already at max level {upgradeDef.maxLevel}");
                return false;
            }

            // Calculate cost with prestige reduction
            var prestigeSystem = FindObjectOfType<PrestigeSystem>();
            float costMultiplier = prestigeSystem != null ? prestigeSystem.GetCostMultiplier() : 1f;
            int cost = (int)(upgradeDef.GetCostForNextLevel(currentLevel) * costMultiplier);

            if (profile.money < cost)
            {
                Debug.LogWarning($"Insufficient funds. Cost: {cost}, Available: {profile.money}");
                return false;
            }

            // Spend money
            var economy = FindObjectOfType<EconomyManager>();
            if (economy == null || !economy.TrySpend(cost))
            {
                Debug.LogError("Failed to spend money for upgrade.");
                return false;
            }

            // Apply upgrade
            upgradeState.SetLevel(upgradeType, currentLevel + 1);
            manager.Save();

            Debug.Log($"Upgrade successful: {upgradeType} to level {currentLevel + 1}. Cost: {cost}");
            return true;
        }

        /// <summary>
        /// Returns the cost to upgrade the specified type, or -1 if at max level or error.
        /// </summary>
        public int GetUpgradeCost(UpgradeType upgradeType)
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return -1;

            var upgradeState = manager.GetActiveUpgradeState();
            if (upgradeState == null)
                return -1;

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
                return -1;

            int currentLevel = upgradeState.GetLevel(upgradeType);
            if (currentLevel >= upgradeDef.maxLevel)
                return -1;

            // Apply prestige reduction
            var prestigeSystem = FindObjectOfType<PrestigeSystem>();
            float costMultiplier = prestigeSystem != null ? prestigeSystem.GetCostMultiplier() : 1f;
            return (int)(upgradeDef.GetCostForNextLevel(currentLevel) * costMultiplier);
        }

        /// <summary>
        /// Returns the current level of an upgrade type, or 0 if none found.
        /// </summary>
        public int GetUpgradeLevel(UpgradeType upgradeType)
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return 0;

            var upgradeState = manager.GetActiveUpgradeState();
            if (upgradeState == null)
                return 0;

            return upgradeState.GetLevel(upgradeType);
        }

        /// <summary>
        /// Returns the max level for an upgrade type, or 0 if not found.
        /// </summary>
        public int GetMaxLevel(UpgradeType upgradeType)
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return 0;

            foreach (var def in manager.upgradeDefs)
            {
                if (def != null && def.upgradeType == upgradeType)
                    return def.maxLevel;
            }

            return 0;
        }
    }
}
