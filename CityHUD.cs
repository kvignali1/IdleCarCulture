using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleCarCulture
{
    /// <summary>
    /// City HUD displaying player stats and active car info.
    /// Subscribes to GameManager events and updates UI accordingly.
    /// Includes an Advanced Stats panel toggled by button.
    /// </summary>
    public class CityHUD : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI moneyText;

        [SerializeField]
        private TextMeshProUGUI heatText;

        [SerializeField]
        private TextMeshProUGUI credText;

        [SerializeField]
        private TextMeshProUGUI reputationText;

        [SerializeField]
        private TextMeshProUGUI prText;

        [SerializeField]
        private TextMeshProUGUI activeCarNameText;

        [SerializeField]
        private Button advancedStatsToggleButton;

        [SerializeField]
        private GameObject advancedStatsPanel;

        // Advanced stats text fields
        [SerializeField]
        private TextMeshProUGUI hpText;

        [SerializeField]
        private TextMeshProUGUI torqueText;

        [SerializeField]
        private TextMeshProUGUI weightText;

        [SerializeField]
        private TextMeshProUGUI gripText;

        [SerializeField]
        private TextMeshProUGUI suspensionText;

        [SerializeField]
        private TextMeshProUGUI drivetrainText;

        [SerializeField]
        private TextMeshProUGUI upgradeStatsText;

        private bool advancedStatsVisible = false;

        private void OnEnable()
        {
            var manager = GameManager.Instance;
            if (manager != null)
            {
                manager.OnMoneyChanged += UpdateMoney;
                manager.OnHeatChanged += UpdateHeat;
                manager.OnCredChanged += UpdateCred;
                manager.OnReputationChanged += UpdateReputation;
                manager.OnActiveCarChanged += OnActiveCarChanged;
            }

            if (advancedStatsToggleButton != null)
                advancedStatsToggleButton.onClick.AddListener(ToggleAdvancedStats);

            RefreshAll();
        }

        private void OnDisable()
        {
            var manager = GameManager.Instance;
            if (manager != null)
            {
                manager.OnMoneyChanged -= UpdateMoney;
                manager.OnHeatChanged -= UpdateHeat;
                manager.OnCredChanged -= UpdateCred;
                manager.OnReputationChanged -= UpdateReputation;
                manager.OnActiveCarChanged -= OnActiveCarChanged;
            }

            if (advancedStatsToggleButton != null)
                advancedStatsToggleButton.onClick.RemoveListener(ToggleAdvancedStats);
        }

        /// <summary>
        /// Refreshes all UI elements with current player data.
        /// </summary>
        public void RefreshAll()
        {
            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogWarning("GameManager not found. Cannot refresh HUD.");
                return;
            }

            var profile = manager.GetProfile();
            if (profile != null)
            {
                UpdateMoney(profile.money);
                UpdateHeat(profile.heat);
                UpdateCred(profile.cred);
                UpdateReputation(profile.reputation);
                RefreshActiveCarInfo();
                RefreshAdvancedStats();
            }
        }

        private void UpdateMoney(long amount)
        {
            if (moneyText != null)
                moneyText.text = $"Money: ${amount:N0}";
        }

        private void UpdateHeat(float heat)
        {
            if (heatText != null)
                heatText.text = $"Heat: {heat:F1}";
        }

        private void UpdateCred(int cred)
        {
            if (credText != null)
                credText.text = $"Cred: {cred}";
        }

        private void UpdateReputation(int reputation)
        {
            if (reputationText != null)
                reputationText.text = $"Reputation: {reputation}";
        }

        private void OnActiveCarChanged(string carId)
        {
            RefreshActiveCarInfo();
            RefreshAdvancedStats();
        }

        private void RefreshActiveCarInfo()
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var carData = manager.GetActiveCarData();
            if (carData != null)
            {
                if (activeCarNameText != null)
                    activeCarNameText.text = $"Active Car: {carData.displayName}";

                var stats = manager.ComputeActiveStats();
                if (stats.HasValue)
                {
                    float pr = RaceCalculator.ComputePR(stats.Value, RaceEventType.LegalLocal);
                    if (prText != null)
                        prText.text = $"PR: {pr:F0}";
                }
            }
            else
            {
                if (activeCarNameText != null)
                    activeCarNameText.text = "Active Car: None";
                if (prText != null)
                    prText.text = "PR: --";
            }
        }

        private void RefreshAdvancedStats()
        {
            if (!advancedStatsVisible)
                return;

            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var stats = manager.ComputeActiveStats();
            if (!stats.HasValue)
            {
                Debug.LogWarning("Cannot compute active car stats for advanced panel.");
                return;
            }

            if (hpText != null)
                hpText.text = $"HP: {stats.Value.hp}";

            if (torqueText != null)
                torqueText.text = $"Torque: {stats.Value.torque}";

            if (weightText != null)
                weightText.text = $"Weight: {stats.Value.weight}";

            if (gripText != null)
                gripText.text = $"Grip: {stats.Value.grip}";

            if (suspensionText != null)
                suspensionText.text = $"Suspension: {stats.Value.suspension}";

            if (drivetrainText != null)
                drivetrainText.text = $"Drivetrain: {stats.Value.drivetrain}";

            // Upgrade levels
            var upgradeState = manager.GetActiveUpgradeState();
            if (upgradeState != null && upgradeStatsText != null)
            {
                upgradeStatsText.text = $"Upgrades:\n" +
                    $"Engine: {upgradeState.engineLevel}\n" +
                    $"Turbo: {upgradeState.turboLevel}\n" +
                    $"Transmission: {upgradeState.transmissionLevel}\n" +
                    $"Tires: {upgradeState.tiresLevel}\n" +
                    $"Suspension: {upgradeState.suspensionLevel}";
            }
        }

        private void ToggleAdvancedStats()
        {
            advancedStatsVisible = !advancedStatsVisible;

            if (advancedStatsPanel != null)
                advancedStatsPanel.SetActive(advancedStatsVisible);

            if (advancedStatsVisible)
                RefreshAdvancedStats();

            Debug.Log($"Advanced Stats Panel: {(advancedStatsVisible ? "Shown" : "Hidden")}");
        }
    }
}
