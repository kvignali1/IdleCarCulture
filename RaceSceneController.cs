using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IdleCarCulture
{
    /// <summary>
    /// Manages the race scene: minigame phases, result calculation, and reward application.
    /// </summary>
    public class RaceSceneController : MonoBehaviour
    {
        /// <summary>
        /// Multiplier for opponent stats scaling by tier.
        /// </summary>
        private const float OpponentStatScalePerTier = 1.1f;

        [SerializeField]
        private GameObject minigamePanel;

        [SerializeField]
        private GameObject resultsPanel;

        // Tire heat phase UI
        [SerializeField]
        private Slider tireHeatMeter;

        [SerializeField]
        private Button tireHeatHoldButton;

        [SerializeField]
        private Image tireHeatGreenZone;

        // Launch phase UI
        [SerializeField]
        private TextMeshProUGUI launchCountdownText;

        [SerializeField]
        private Button launchTapButton;

        // Shifting phase UI
        [SerializeField]
        private Slider shiftMarkerSlider;

        [SerializeField]
        private Button shiftTapButton;

        [SerializeField]
        private TextMeshProUGUI shiftCountText;

        // Results UI
        [SerializeField]
        private TextMeshProUGUI resultTitleText;

        [SerializeField]
        private TextMeshProUGUI resultDetailsText;

        [SerializeField]
        private TextMeshProUGUI payoutText;

        [SerializeField]
        private TextMeshProUGUI statsChangeText;

        [SerializeField]
        private Button returnToCityButton;

        // Car sprites for animation
        [SerializeField]
        private Image playerCarSprite;

        [SerializeField]
        private Image opponentCarSprite;

        private RaceOpportunity raceOpportunity;
        private SkillResult playerSkillResult;
        private ComputedCarStats playerStats;
        private ComputedCarStats opponentStats;

        private int currentShiftCount = 0;
        private float shiftMarkerPosition = 0f;

        private void Start()
        {
            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("[RaceSceneController] GameManager.Instance is null. Cannot initialize race.");
                SceneManager.LoadScene("City");
                return;
            }

            raceOpportunity = manager.GetCurrentRaceOpportunity();
            if (raceOpportunity == null)
            {
                Debug.LogError("[RaceSceneController] No race opportunity set in GameManager. Returning to City.");
                SceneManager.LoadScene("City");
                return;
            }

            // Get player stats
            var playerStats = manager.ComputeActiveStats();
            if (!playerStats.HasValue)
            {
                Debug.LogError("[RaceSceneController] Could not compute player stats. Returning to City.");
                SceneManager.LoadScene("City");
                return;
            }

            this.playerStats = playerStats.Value;

            // Generate opponent stats
            GenerateOpponentStats();

            // Setup minigame
            SetupMinigame();

            if (resultsPanel != null)
                resultsPanel.SetActive(false);
            else
                Debug.LogWarning("[RaceSceneController] resultsPanel is not assigned in Inspector.");
        }

        private void GenerateOpponentStats()
        {
            // Simple scaling based on tier
            float tierScale = Mathf.Pow(OpponentStatScalePerTier, raceOpportunity.opponentTier);

            opponentStats = new ComputedCarStats
            {
                hp = (int)(100 * tierScale),
                torque = (int)(150 * tierScale),
                weight = (int)(300 * tierScale),
                grip = (int)(50 * tierScale),
                suspension = (int)(40 * tierScale),
                drivetrain = Drivetrain.RWD
            };

            Debug.Log($"Opponent stats generated - HP: {opponentStats.hp}, Tier: {raceOpportunity.opponentTier}");
        }

        private void SetupMinigame()
        {
            if (minigamePanel != null)
                minigamePanel.SetActive(true);
            else
                Debug.LogWarning("[RaceSceneController] minigamePanel is not assigned in Inspector.");

            // Setup tire heat phase
            if (tireHeatHoldButton != null)
                tireHeatHoldButton.onClick.AddListener(OnTireHeatPhaseStart);
            else
                Debug.LogWarning("[RaceSceneController] tireHeatHoldButton is not assigned in Inspector.");

            // Setup launch phase
            if (launchTapButton != null)
                launchTapButton.onClick.AddListener(OnLaunchTap);
            else
                Debug.LogWarning("[RaceSceneController] launchTapButton is not assigned in Inspector.");

            // Setup shifting phase
            if (shiftTapButton != null)
                shiftTapButton.onClick.AddListener(OnShiftTap);
            else
                Debug.LogWarning("[RaceSceneController] shiftTapButton is not assigned in Inspector.");

            if (returnToCityButton != null)
                returnToCityButton.onClick.AddListener(ReturnToCity);
            else
                Debug.LogWarning("[RaceSceneController] returnToCityButton is not assigned in Inspector.");

            StartCoroutine(RunMinigame());
        }

        private System.Collections.IEnumerator RunMinigame()
        {
            // Phase 1: Tire Heat
            yield return StartCoroutine(TireHeatPhase());

            // Phase 2: Launch
            yield return StartCoroutine(LaunchPhase());

            // Phase 3: Shifting
            yield return StartCoroutine(ShiftingPhase());

            // Resolve race and show results
            ResolveRace();
        }

        private System.Collections.IEnumerator TireHeatPhase()
        {
            Debug.Log("Tire Heat Phase started");
            float score = 0f;
            float holdTime = 0f;
            bool holding = false;

            if (tireHeatMeter != null)
            {
                tireHeatMeter.value = 0f;
                tireHeatMeter.gameObject.SetActive(true);
            }

            if (tireHeatHoldButton != null)
            {
                tireHeatHoldButton.onClick.RemoveAllListeners();
                tireHeatHoldButton.onClick.AddListener(() => holding = true);
            }

            float phaseTime = 0f;
            while (phaseTime < 3f)
            {
                phaseTime += Time.deltaTime;

                if (holding)
                {
                    holdTime += Time.deltaTime;
                    if (tireHeatMeter != null)
                        tireHeatMeter.value = holdTime / 3f;

                    // Green zone: 0.4-0.6
                    if (holdTime > 1.2f && holdTime < 1.8f)
                        score = Mathf.Lerp(score, 1f, Time.deltaTime * 2f);
                    else if (holdTime > 0.9f && holdTime < 2.1f)
                        score = Mathf.Lerp(score, 0.7f, Time.deltaTime);
                    else
                        score = Mathf.Lerp(score, 0.3f, Time.deltaTime);
                }

                yield return null;
            }

            playerSkillResult.tireHeatScore = Mathf.Clamp01(score);
            Debug.Log($"Tire Heat Score: {playerSkillResult.tireHeatScore:F2}");

            if (tireHeatMeter != null)
                tireHeatMeter.gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator LaunchPhase()
        {
            Debug.Log("Launch Phase started");
            float score = 0f;
            bool tapped = false;
            float targetTime = Random.Range(0.8f, 2f);
            float elapsed = 0f;

            if (launchCountdownText != null)
                launchCountdownText.gameObject.SetActive(true);

            if (launchTapButton != null)
            {
                launchTapButton.onClick.RemoveAllListeners();
                launchTapButton.onClick.AddListener(() => tapped = true);
            }

            while (elapsed < 3f)
            {
                elapsed += Time.deltaTime;

                if (launchCountdownText != null)
                    launchCountdownText.text = Mathf.Max(0, 3f - elapsed).ToString("F1");

                if (tapped && !score.Equals(0f))
                {
                    // Already scored
                    yield return null;
                    continue;
                }

                if (tapped)
                {
                    // Calculate score based on timing
                    float diff = Mathf.Abs(elapsed - targetTime);
                    if (diff < 0.1f)
                        score = 1f;
                    else if (diff < 0.3f)
                        score = 0.8f;
                    else if (diff < 0.5f)
                        score = 0.5f;
                    else
                        score = 0.2f;

                    tapped = false; // Reset for next phase
                }

                yield return null;
            }

            playerSkillResult.launchScore = Mathf.Clamp01(score);
            Debug.Log($"Launch Score: {playerSkillResult.launchScore:F2}");

            if (launchCountdownText != null)
                launchCountdownText.gameObject.SetActive(false);
        }

        private System.Collections.IEnumerator ShiftingPhase()
        {
            Debug.Log("Shifting Phase started");
            float totalScore = 0f;
            currentShiftCount = 0;

            if (shiftMarkerSlider != null)
                shiftMarkerSlider.gameObject.SetActive(true);

            // 3 shifts
            for (int i = 0; i < 3; i++)
            {
                currentShiftCount = i + 1;

                if (shiftCountText != null)
                    shiftCountText.text = $"Shift {currentShiftCount}/3";

                shiftMarkerPosition = Random.Range(0.3f, 0.7f);
                float shiftScore = 0f;
                bool tapped = false;
                float shiftTime = 0f;

                if (shiftTapButton != null)
                {
                    shiftTapButton.onClick.RemoveAllListeners();
                    shiftTapButton.onClick.AddListener(() => tapped = true);
                }

                while (shiftTime < 2f && !tapped)
                {
                    shiftTime += Time.deltaTime;

                    if (shiftMarkerSlider != null)
                    {
                        // Animate marker across slider
                        float markerPos = Mathf.PingPong(shiftTime, 1f);
                        shiftMarkerSlider.value = markerPos;
                    }

                    yield return null;
                }

                if (tapped)
                {
                    // Calculate score based on marker distance to target
                    float diff = Mathf.Abs(shiftMarkerPosition - (shiftMarkerSlider != null ? shiftMarkerSlider.value : 0.5f));
                    if (diff < 0.1f)
                        shiftScore = 1f;
                    else if (diff < 0.2f)
                        shiftScore = 0.8f;
                    else if (diff < 0.4f)
                        shiftScore = 0.5f;
                    else
                        shiftScore = 0.2f;
                }

                totalScore += shiftScore;
                yield return new WaitForSeconds(0.5f);
            }

            playerSkillResult.shiftScore = Mathf.Clamp01(totalScore / 3f);
            Debug.Log($"Shift Score: {playerSkillResult.shiftScore:F2}");

            if (shiftMarkerSlider != null)
                shiftMarkerSlider.gameObject.SetActive(false);
        }

        private void ResolveRace()
        {
            var manager = GameManager.Instance;
            if (manager == null)
                return;

            var profile = manager.GetProfile();
            if (profile == null)
                return;

            // Calculate player PR with skill modifier
            float playerPR = RaceCalculator.ComputePR(playerStats, raceOpportunity.eventType);
            float skillBonus = (raceOpportunity.eventType == RaceEventType.IllegalDig || raceOpportunity.eventType == RaceEventType.IllegalRoll)
                ? 0.10f
                : 0.15f;
            playerPR *= (1f + skillBonus * playerSkillResult.Overall());

            // Resolve race
            var outcome = RaceCalculator.ResolveRace(
                playerStats,
                opponentStats,
                raceOpportunity.eventType,
                playerSkillResult,
                profile.prestigeCurrency,
                -1f);

            // Calculate payout
            var economy = FindObjectOfType<EconomyManager>();
            long payout = 0;
            if (economy != null)
            {
                payout = economy.CalculateRacePayout(
                    raceOpportunity.eventType,
                    raceOpportunity.opponentTier,
                    outcome.win,
                    outcome.payoutMultiplier,
                    profile.prestigeCurrency);

                if (outcome.win)
                    economy.AddMoney(payout);
            }

            // Apply heat changes
            var heatSystem = FindObjectOfType<HeatSystem>();
            if (outcome.win && raceOpportunity.eventType == RaceEventType.IllegalDig)
            {
                float heatGain = 15f * (1f - FindObjectOfType<PrestigeSystem>()?.GetHeatGainMultiplier() ?? 1f);
                if (heatSystem != null)
                    heatSystem.AddHeat(heatGain);
            }

            // Award cred/reputation
            int credGain = outcome.win ? (10 + raceOpportunity.opponentTier * 5) : 0;
            int repGain = outcome.win ? (5 + raceOpportunity.opponentTier * 2) : 0;

            if (credGain > 0)
            {
                profile.cred += credGain;
                manager.OnCredChanged?.Invoke(profile.cred);
            }

            if (repGain > 0)
            {
                profile.reputation += repGain;
                manager.OnReputationChanged?.Invoke(profile.reputation);
            }

            manager.Save();

            // Show results
            ShowResults(outcome, payout, credGain, repGain);
        }

        private void ShowResults(RaceOutcome outcome, long payout, int credGain, int repGain)
        {
            if (minigamePanel != null)
                minigamePanel.SetActive(false);

            if (resultsPanel != null)
                resultsPanel.SetActive(true);
            else
                Debug.LogWarning("[RaceSceneController] resultsPanel is not assigned in Inspector. Results cannot be displayed.");

            if (resultTitleText != null)
                resultTitleText.text = outcome.win ? "YOU WON!" : "YOU LOST";
            else
                Debug.LogWarning("[RaceSceneController] resultTitleText is not assigned in Inspector.");

            if (resultDetailsText != null)
                resultDetailsText.text = $"Player PR: {outcome.playerPR:F0}\nOpponent PR: {outcome.opponentPR:F0}";
            else
                Debug.LogWarning("[RaceSceneController] resultDetailsText is not assigned in Inspector.");

            if (payoutText != null)
                payoutText.text = $"Payout: ${payout:N0}";
            else
                Debug.LogWarning("[RaceSceneController] payoutText is not assigned in Inspector.");

            if (statsChangeText != null)
                statsChangeText.text = $"Cred: +{credGain}\nReputation: +{repGain}";
            else
                Debug.LogWarning("[RaceSceneController] statsChangeText is not assigned in Inspector.");

            // Simple animation: move cars based on outcome
            if (outcome.win && playerCarSprite != null)
            {
                StartCoroutine(AnimatePlayerWin());
            }
            else if (!outcome.win && opponentCarSprite != null)
            {
                StartCoroutine(AnimatePlayerLose());
            }

            Debug.Log($"Race Results: Win={outcome.win}, Payout={payout}, Cred+{credGain}, Rep+{repGain}");
        }

        private System.Collections.IEnumerator AnimatePlayerWin()
        {
            // Simple animation: move player car to right
            if (playerCarSprite != null)
            {
                Vector3 start = playerCarSprite.transform.localPosition;
                Vector3 end = start + Vector3.right * 100f;

                for (float t = 0; t < 1f; t += Time.deltaTime)
                {
                    playerCarSprite.transform.localPosition = Vector3.Lerp(start, end, t);
                    yield return null;
                }
            }
        }

        private System.Collections.IEnumerator AnimatePlayerLose()
        {
            // Simple animation: move opponent car to right
            if (opponentCarSprite != null)
            {
                Vector3 start = opponentCarSprite.transform.localPosition;
                Vector3 end = start + Vector3.right * 100f;

                for (float t = 0; t < 1f; t += Time.deltaTime)
                {
                    opponentCarSprite.transform.localPosition = Vector3.Lerp(start, end, t);
                    yield return null;
                }
            }
        }

        private void ReturnToCity()
        {
            SceneManager.LoadScene("City");
        }
    }
}
