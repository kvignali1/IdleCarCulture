using System;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Spawns race opportunities in the City scene at intervals.
    /// Unlocks event types and tiers based on player cred (illegal) and reputation (legal).
    /// </summary>
    public class RaceOpportunitySpawner : MonoBehaviour
    {
        /// <summary>
        /// Base spawn interval in seconds.
        /// </summary>
        [SerializeField]
        private float spawnInterval = 10f;

        /// <summary>
        /// Random variation on spawn interval (+/- this value).
        /// </summary>
        [SerializeField]
        private float spawnIntervalRandomness = 3f;

        /// <summary>
        /// Cred required per illegal tier unlock (e.g., tier 2 at 200 cred).
        /// </summary>
        private const int CredPerTierUnlock = 100;

        /// <summary>
        /// Reputation required per legal tier unlock (e.g., tier 2 at 300 rep).
        /// </summary>
        private const int ReputationPerTierUnlock = 150;

        /// <summary>
        /// Fired when a new race opportunity is spawned.
        /// </summary>
        public event Action<RaceOpportunity> OnRaceOpportunitySpawned;

        private float timeSinceLastSpawn = 0f;
        private float nextSpawnTime;

        private void Start()
        {
            ResetSpawnTimer();
        }

        private void Update()
        {
            timeSinceLastSpawn += Time.deltaTime;
            if (timeSinceLastSpawn >= nextSpawnTime)
            {
                SpawnOpportunity();
                ResetSpawnTimer();
            }
        }

        private void ResetSpawnTimer()
        {
            float randomness = UnityEngine.Random.Range(-spawnIntervalRandomness, spawnIntervalRandomness);
            nextSpawnTime = spawnInterval + randomness;
            timeSinceLastSpawn = 0f;
        }

        private void SpawnOpportunity()
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

            // Determine available event types based on player stats
            bool canIllegal = profile.cred >= CredPerTierUnlock;
            bool canLegal = profile.reputation >= ReputationPerTierUnlock;

            // Pick an event type
            RaceEventType eventType = SelectEventType(canIllegal, canLegal);

            // Determine maximum tier unlock for this event type
            int maxTier = GetMaxAvailableTier(eventType, profile);
            if (maxTier < 0)
            {
                Debug.LogWarning("No available race tiers. Skipping spawn.");
                return;
            }

            // Select a random tier within available range
            int tier = UnityEngine.Random.Range(0, maxTier + 1);

            // Generate bet suggestion based on tier and profile wealth
            long betSuggestion = GenerateBetSuggestion(tier, profile.money);

            // Create display text
            string displayText = GenerateDisplayText(eventType, tier);

            // Pick a random opponent car name (if available)
            string opponentName = "Unknown Driver";

            var opportunity = new RaceOpportunity(eventType, tier, betSuggestion, displayText, opponentName);

            Debug.Log($"Race opportunity spawned: {displayText}, Bet: ${betSuggestion}, Tier: {tier}");
            OnRaceOpportunitySpawned?.Invoke(opportunity);
        }

        private RaceEventType SelectEventType(bool canIllegal, bool canLegal)
        {
            if (!canIllegal && !canLegal)
            {
                // Default to lowest tier illegal if nothing is unlocked
                return RaceEventType.IllegalDig;
            }

            if (canIllegal && canLegal)
            {
                // Random choice between illegal and legal
                return UnityEngine.Random.value > 0.5f ? RaceEventType.IllegalDig : RaceEventType.LegalLocal;
            }

            return canIllegal ? RaceEventType.IllegalDig : RaceEventType.LegalLocal;
        }

        private int GetMaxAvailableTier(RaceEventType eventType, PlayerProfile profile)
        {
            int maxTier = 0;

            switch (eventType)
            {
                case RaceEventType.IllegalDig:
                case RaceEventType.IllegalRoll:
                    // Illegal tiers unlock at 100, 200, 300, 400 cred
                    maxTier = Mathf.Min(profile.cred / CredPerTierUnlock, 4);
                    break;
                case RaceEventType.LegalLocal:
                case RaceEventType.LegalRegional:
                    // Legal tiers unlock at 150, 300, 450, 600 reputation
                    maxTier = Mathf.Min(profile.reputation / ReputationPerTierUnlock, 4);
                    break;
            }

            return Mathf.Clamp(maxTier, 0, 4);
        }

        private long GenerateBetSuggestion(int tier, long playerMoney)
        {
            // Base bet scales with tier (500 * (tier + 1))
            long baseBet = 500 * (tier + 1);

            // Scale based on player wealth (suggestion is 5-10% of liquid cash)
            long wealthScaled = Mathf.Max(baseBet, (long)(playerMoney * 0.05f));

            return Mathf.Min(wealthScaled, playerMoney / 2);
        }

        private string GenerateDisplayText(RaceEventType eventType, int tier)
        {
            string eventName = eventType switch
            {
                RaceEventType.IllegalDig => "Street Dig",
                RaceEventType.IllegalRoll => "Street Roll",
                RaceEventType.LegalLocal => "Local Sprint",
                RaceEventType.LegalRegional => "Regional Championship",
                _ => "Unknown Race"
            };

            return $"{eventName} - Tier {tier + 1}";
        }
    }
}
