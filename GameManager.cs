using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Singleton GameManager that manages player profile, car database, and upgrade definitions.
    /// Persists across scene loads via DontDestroyOnLoad.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        /// <summary>
        /// Singleton instance of GameManager.
        /// </summary>
        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// Current player profile.
        /// </summary>
        private PlayerProfile _playerProfile;

        /// <summary>
        /// Current selected race opportunity (from RaceOpportunitySpawner).
        /// </summary>
        private RaceOpportunity _currentRaceOpportunity;

        /// <summary>
        /// Database of all car definitions.
        /// </summary>
        [SerializeField]
        public List<CarData> carDatabase = new List<CarData>();

        /// <summary>
        /// Database of all upgrade definitions.
        /// </summary>
        [SerializeField]
        public List<UpgradeDefinition> upgradeDefs = new List<UpgradeDefinition>();

        /// <summary>
        /// Fired when player money changes.
        /// </summary>
        public event Action<long> OnMoneyChanged;

        /// <summary>
        /// Fired when player heat changes.
        /// </summary>
        public event Action<float> OnHeatChanged;

        /// <summary>
        /// Fired when player cred changes.
        /// </summary>
        public event Action<int> OnCredChanged;

        /// <summary>
        /// Fired when player reputation changes.
        /// </summary>
        public event Action<int> OnReputationChanged;

        /// <summary>
        /// Fired when active car changes.
        /// </summary>
        public event Action<string> OnActiveCarChanged;

        private void Awake()
        {
            // Enforce singleton pattern
            if (_instance != null && _instance != this)
            {
                Debug.LogWarning("GameManager singleton already exists. Destroying duplicate.");
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            // Load or create player profile
            _playerProfile = SaveSystem.LoadOrCreateDefault(() => new PlayerProfile
            {
                money = 5000,
                prestigeCurrency = 0,
                heat = 0f,
                cred = 0,
                reputation = 0,
                activeCarId = string.Empty,
                ownedCarIds = new List<string>(),
                upgradesByCar = new List<CarUpgradeState>()
            });

            if (_playerProfile == null)
            {
                Debug.LogError("Failed to load or create PlayerProfile. Creating empty profile.");
                _playerProfile = new PlayerProfile();
            }

            Debug.Log($"GameManager initialized. Player money: {_playerProfile.money}, Active car: {_playerProfile.activeCarId}");
        }

        /// <summary>
        /// Gets car data by id from the carDatabase.
        /// </summary>
        public CarData GetCar(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("GetCar called with empty id.");
                return null;
            }

            foreach (var car in carDatabase)
            {
                if (car != null && car.id == id)
                    return car;
            }

            Debug.LogWarning($"Car with id '{id}' not found in database.");
            return null;
        }

        /// <summary>
        /// Returns the CarData for the currently active car, or null if none is active.
        /// </summary>
        public CarData GetActiveCarData()
        {
            if (string.IsNullOrEmpty(_playerProfile?.activeCarId))
            {
                Debug.LogWarning("No active car set.");
                return null;
            }

            return GetCar(_playerProfile.activeCarId);
        }

        /// <summary>
        /// Returns the upgrade state for the currently active car, or null if none is active.
        /// </summary>
        public CarUpgradeState GetActiveUpgradeState()
        {
            if (string.IsNullOrEmpty(_playerProfile?.activeCarId))
            {
                Debug.LogWarning("No active car set.");
                return null;
            }

            return _playerProfile.GetOrCreateUpgradeState(_playerProfile.activeCarId);
        }

        /// <summary>
        /// Computes the final stats for the active car by applying all upgrades.
        /// Returns null if active car or upgrades are not available.
        /// </summary>
        public ComputedCarStats? ComputeActiveStats()
        {
            var carData = GetActiveCarData();
            if (carData == null)
            {
                Debug.LogWarning("Cannot compute stats: no active car.");
                return null;
            }

            var upgrades = GetActiveUpgradeState();
            if (upgrades == null)
            {
                Debug.LogWarning("Cannot compute stats: upgrade state not found.");
                return null;
            }

            try
            {
                return ComputedCarStats.Compute(carData, upgrades, upgradeDefs);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error computing active car stats: {ex}");
                return null;
            }
        }

        /// <summary>
        /// Sets the active car by id and fires OnActiveCarChanged event.
        /// </summary>
        public void SetActiveCar(string carId)
        {
            if (string.IsNullOrEmpty(carId))
            {
                Debug.LogWarning("SetActiveCar called with empty id.");
                return;
            }

            if (_playerProfile == null)
            {
                Debug.LogError("PlayerProfile is null.");
                return;
            }

            _playerProfile.SetActiveCar(carId);
            Debug.Log($"Active car set to: {carId}");
            OnActiveCarChanged?.Invoke(carId);
        }

        /// <summary>
        /// Sets player money and fires OnMoneyChanged event.
        /// </summary>
        public void SetMoney(long amount)
        {
            if (_playerProfile == null) return;
            _playerProfile.money = amount;
            OnMoneyChanged?.Invoke(amount);
        }

        /// <summary>
        /// Adds money to player balance and fires OnMoneyChanged event.
        /// </summary>
        public void AddMoney(long amount)
        {
            if (_playerProfile == null) return;
            _playerProfile.money += amount;
            OnMoneyChanged?.Invoke(_playerProfile.money);
        }

        /// <summary>
        /// Sets player heat and fires OnHeatChanged event.
        /// </summary>
        public void SetHeat(float heat)
        {
            if (_playerProfile == null) return;
            _playerProfile.heat = heat;
            OnHeatChanged?.Invoke(heat);
        }

        /// <summary>
        /// Sets player cred and fires OnCredChanged event.
        /// </summary>
        public void SetCred(int cred)
        {
            if (_playerProfile == null) return;
            _playerProfile.cred = cred;
            OnCredChanged?.Invoke(cred);
        }

        /// <summary>
        /// Sets player reputation and fires OnReputationChanged event.
        /// </summary>
        public void SetReputation(int reputation)
        {
            if (_playerProfile == null) return;
            _playerProfile.reputation = reputation;
            OnReputationChanged?.Invoke(reputation);
        }

        /// <summary>
        /// Returns the current PlayerProfile.
        /// </summary>
        public PlayerProfile GetProfile()
        {
            return _playerProfile;
        }

        /// <summary>
        /// Sets the current race opportunity.
        /// </summary>
        public void SetCurrentRaceOpportunity(RaceOpportunity opportunity)
        {
            _currentRaceOpportunity = opportunity;
        }

        /// <summary>
        /// Gets the current race opportunity.
        /// </summary>
        public RaceOpportunity GetCurrentRaceOpportunity()
        {
            return _currentRaceOpportunity;
        }

        /// <summary>
        /// Saves the current PlayerProfile to disk using SaveSystem.
        /// </summary>
        public void Save()
        {
            if (_playerProfile == null)
            {
                Debug.LogError("Cannot save: PlayerProfile is null.");
                return;
            }

            SaveSystem.Save(_playerProfile);
            Debug.Log("PlayerProfile saved successfully.");
        }
    }
}
