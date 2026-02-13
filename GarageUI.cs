using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IdleCarCulture
{
    /// <summary>
    /// Garage UI for viewing owned cars, selecting active car, viewing stats, and purchasing upgrades.
    /// </summary>
    public class GarageUI : MonoBehaviour
    {
        [SerializeField]
        private ScrollRect carListScroll;

        [SerializeField]
        private Transform carListContent;

        [SerializeField]
        private TextMeshProUGUI activeCarNameText;

        [SerializeField]
        private TextMeshProUGUI prText;

        [SerializeField]
        private TextMeshProUGUI statsText;

        // Upgrade buttons and level displays
        [SerializeField]
        private Button engineUpgradeButton;

        [SerializeField]
        private TextMeshProUGUI engineLevelText;

        [SerializeField]
        private Button turboUpgradeButton;

        [SerializeField]
        private TextMeshProUGUI turboLevelText;

        [SerializeField]
        private Button transmissionUpgradeButton;

        [SerializeField]
        private TextMeshProUGUI transmissionLevelText;

        [SerializeField]
        private Button tiresUpgradeButton;

        [SerializeField]
        private TextMeshProUGUI tiresLevelText;

        [SerializeField]
        private Button suspensionUpgradeButton;

        [SerializeField]
        private TextMeshProUGUI suspensionLevelText;

        [SerializeField]
        private Prefab carListItemPrefab;

        private UpgradeSystem upgradeSystem;

        private void Start()
        {
            upgradeSystem = GetComponent<UpgradeSystem>() ?? FindObjectOfType<UpgradeSystem>();
            if (upgradeSystem == null)
            {
                upgradeSystem = gameObject.AddComponent<UpgradeSystem>();
                Debug.LogWarning("[GarageUI] UpgradeSystem not found. Created new instance on this GameObject.");
            }

            if (carListContent == null)
                Debug.LogError("[GarageUI] carListContent is not assigned in Inspector. Car list will not display.");

            SetupUpgradeButtons();
            RefreshUI();
        }

        private void OnEnable()
        {
            var manager = GameManager.Instance;
            if (manager != null)
            {
                manager.OnActiveCarChanged += OnActiveCarChanged;
                manager.OnMoneyChanged += OnMoneyChanged;
            }
        }

        private void OnDisable()
        {
            var manager = GameManager.Instance;
            if (manager != null)
            {
                manager.OnActiveCarChanged -= OnActiveCarChanged;
                manager.OnMoneyChanged -= OnMoneyChanged;
            }
        }

        private void SetupUpgradeButtons()
        {
            if (engineUpgradeButton != null)
                engineUpgradeButton.onClick.AddListener(() => AttemptUpgrade(UpgradeType.Engine));
            else
                Debug.LogWarning("[GarageUI] engineUpgradeButton is not assigned in Inspector.");

            if (turboUpgradeButton != null)
                turboUpgradeButton.onClick.AddListener(() => AttemptUpgrade(UpgradeType.Turbo));
            else
                Debug.LogWarning("[GarageUI] turboUpgradeButton is not assigned in Inspector.");

            if (transmissionUpgradeButton != null)
                transmissionUpgradeButton.onClick.AddListener(() => AttemptUpgrade(UpgradeType.Transmission));
            else
                Debug.LogWarning("[GarageUI] transmissionUpgradeButton is not assigned in Inspector.");

            if (tiresUpgradeButton != null)
                tiresUpgradeButton.onClick.AddListener(() => AttemptUpgrade(UpgradeType.Tires));
            else
                Debug.LogWarning("[GarageUI] tiresUpgradeButton is not assigned in Inspector.");

            if (suspensionUpgradeButton != null)
                suspensionUpgradeButton.onClick.AddListener(() => AttemptUpgrade(UpgradeType.Suspension));
            else
                Debug.LogWarning("[GarageUI] suspensionUpgradeButton is not assigned in Inspector.");
        }

        private void AttemptUpgrade(UpgradeType upgradeType)
        {
            if (upgradeSystem == null)
                return;

            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var carData = manager.GetActiveCarData();
            if (carData == null)
                return;

            long cost = upgradeSystem.GetUpgradeCost(carData.id, upgradeType);
            if (cost < 0)
            {
                Debug.LogWarning($"{upgradeType} already at max level");
                return;
            }

            if (upgradeSystem.TryUpgrade(carData.id, upgradeType))
            {
                Debug.Log($"Upgrade successful: {upgradeType}");
                RefreshUI();
            }
            else
            {
                Debug.LogWarning($"Upgrade failed: {upgradeType}");
            }
        }

        public void RefreshUI()
        {
            RefreshOwnedCarsList();
            RefreshActiveCarInfo();
            RefreshUpgradeButtons();
        }

        private void RefreshOwnedCarsList()
        {
            if (carListContent == null)
                return;

            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var profile = manager.GetProfile();
            if (profile == null)
                return;

            // Clear existing list
            foreach (Transform child in carListContent)
                Destroy(child.gameObject);

            // Populate with owned cars
            if (profile.ownedCarIds != null)
            {
                foreach (var carId in profile.ownedCarIds)
                {
                    var carData = manager.GetCar(carId);
                    if (carData != null)
                    {
                        AddCarListItem(carData, carId == profile.activeCarId);
                    }
                }
            }
        }

        private void AddCarListItem(CarData carData, bool isActive)
        {
            if (carListContent == null)
                return;

            var itemGo = new GameObject($"CarItem_{carData.id}");
            itemGo.transform.SetParent(carListContent, false);

            var layout = itemGo.AddComponent<LayoutElement>();
            layout.preferredHeight = 60f;

            var button = itemGo.AddComponent<Button>();
            var textComp = itemGo.AddComponent<TextMeshProUGUI>();
            textComp.text = carData.displayName + (isActive ? " (Active)" : "");
            textComp.alignment = TextAlignmentOptions.Center;

            button.onClick.AddListener(() =>
            {
                GameManager.Instance?.SetActiveCar(carData.id);
            });
        }

        private void RefreshActiveCarInfo()
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var carData = manager.GetActiveCarData();
            if (carData == null)
            {
                if (activeCarNameText != null)
                    activeCarNameText.text = "No active car";
                if (prText != null)
                    prText.text = "PR: --";
                if (statsText != null)
                    statsText.text = "--";
                return;
            }

            if (activeCarNameText != null)
                activeCarNameText.text = carData.displayName;

            var stats = manager.ComputeActiveStats();
            if (stats.HasValue)
            {
                float pr = RaceCalculator.ComputePR(stats.Value, RaceEventType.LegalLocal);
                if (prText != null)
                    prText.text = $"PR: {pr:F0}";

                if (statsText != null)
                    statsText.text = $"HP: {stats.Value.hp} | TQ: {stats.Value.torque} | Weight: {stats.Value.weight}\n" +
                        $"Grip: {stats.Value.grip} | Suspension: {stats.Value.suspension}";
            }
        }

        private void RefreshUpgradeButtons()
        {
            if (upgradeSystem == null)
                return;

            RefreshUpgradeDisplay(UpgradeType.Engine, engineLevelText, engineUpgradeButton);
            RefreshUpgradeDisplay(UpgradeType.Turbo, turboLevelText, turboUpgradeButton);
            RefreshUpgradeDisplay(UpgradeType.Transmission, transmissionLevelText, transmissionUpgradeButton);
            RefreshUpgradeDisplay(UpgradeType.Tires, tiresLevelText, tiresUpgradeButton);
            RefreshUpgradeDisplay(UpgradeType.Suspension, suspensionLevelText, suspensionUpgradeButton);
        }

        private void RefreshUpgradeDisplay(UpgradeType upgradeType, TextMeshProUGUI levelText, Button upgradeButton)
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var carData = manager.GetActiveCarData();
            if (carData == null)
                return;

            // Get upgrade state for this car
            var profile = manager.GetProfile();
            if (profile == null)
                return;

            var upgradeState = profile.GetOrCreateUpgradeState(carData.id);
            if (upgradeState == null)
                return;

            int currentLevel = upgradeState.GetLevel(upgradeType);
            int maxLevel = 5; // Default max level

            // Find max level from definition
            foreach (var def in manager.upgradeDefs)
            {
                if (def != null && def.upgradeType == upgradeType)
                {
                    maxLevel = def.maxLevel;
                    break;
                }
            }

            long cost = upgradeSystem.GetUpgradeCost(carData.id, upgradeType);

            if (levelText != null)
                levelText.text = $"Level: {currentLevel}/{maxLevel}";

            if (upgradeButton != null)
            {
                bool isAtMax = currentLevel >= maxLevel;
                upgradeButton.interactable = !isAtMax;

                var textComp = upgradeButton.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null)
                {
                    if (isAtMax)
                        textComp.text = "MAX";
                    else
                        textComp.text = $"Upgrade\n${cost:N0}";
                }
            }
        }

        private void OnActiveCarChanged(string carId)
        {
            RefreshUI();
        }

        private void OnMoneyChanged(long amount)
        {
            RefreshUpgradeButtons();
        }
    }
}
