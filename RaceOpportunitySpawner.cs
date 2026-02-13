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
        /// Tier unlock thresholds defined in Tuning class.
        /// </summary>

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
                    // Illegal tiers unlock at Tuning.OPPORTUNITY_CRED_PER_TIER, 2x, 3x, 4x cred
                    maxTier = Mathf.Min(profile.cred / Tuning.OPPORTUNITY_CRED_PER_TIER, 4);
                    break;
                case RaceEventType.LegalLocal:
                case RaceEventType.LegalRegional:
                    // Legal tiers unlock at Tuning.OPPORTUNITY_REPUTATION_PER_TIER, 2x, 3x, 4x reputation
                    maxTier = Mathf.Min(profile.reputation / Tuning.OPPORTUNITY_REPUTATION_PER_TIER, 4);
                    break;
            }

            return Mathf.Clamp(maxTier, 0, 4);
        }

        private long GenerateBetSuggestion(int tier, long playerMoney)
        {
            // Base bet scales with tier (Tuning.OPPORTUNITY_BASE_BET * (tier + 1))
            long baseBet = Tuning.OPPORTUNITY_BASE_BET * (tier + 1);

            // Scale based on player wealth (suggestion is Tuning % of liquid cash)
            long wealthScaled = Mathf.Max(baseBet, (long)(playerMoney * Tuning.OPPORTUNITY_BET_MIN_PERCENT));

            return Mathf.Min(wealthScaled, (long)(playerMoney * Tuning.OPPORTUNITY_BET_MAX_MONEY_FRACTION));
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
