using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace IdleCarCulture
{
    /// <summary>
    /// UI for displaying race opportunity popups and handling Accept/Decline actions.
    /// </summary>
    public class RaceOpportunityUI : MonoBehaviour
    {
        [SerializeField]
        private GameObject popupPanel;

        [SerializeField]
        private TextMeshProUGUI titleText;

        [SerializeField]
        private TextMeshProUGUI descriptionText;

        [SerializeField]
        private TextMeshProUGUI eventTypeText;

        [SerializeField]
        private TextMeshProUGUI betSuggestionText;

        [SerializeField]
        private Button acceptButton;

        [SerializeField]
        private Button declineButton;

        private RaceOpportunity currentOpportunity;

        private void OnEnable()
        {
            var spawner = FindObjectOfType<RaceOpportunitySpawner>();
            if (spawner != null)
                spawner.OnRaceOpportunitySpawned += DisplayOpportunity;

            if (acceptButton != null)
                acceptButton.onClick.AddListener(OnAcceptClicked);

            if (declineButton != null)
                declineButton.onClick.AddListener(OnDeclineClicked);

            if (popupPanel != null)
                popupPanel.SetActive(false);
        }

        private void OnDisable()
        {
            var spawner = FindObjectOfType<RaceOpportunitySpawner>();
            if (spawner != null)
                spawner.OnRaceOpportunitySpawned -= DisplayOpportunity;

            if (acceptButton != null)
                acceptButton.onClick.RemoveListener(OnAcceptClicked);

            if (declineButton != null)
                declineButton.onClick.RemoveListener(OnDeclineClicked);
        }

        /// <summary>
        /// Displays the race opportunity popup with details.
        /// </summary>
        public void DisplayOpportunity(RaceOpportunity opportunity)
        {
            if (opportunity == null)
            {
                Debug.LogWarning("DisplayOpportunity called with null opportunity.");
                return;
            }

            currentOpportunity = opportunity;

            if (titleText != null)
                titleText.text = opportunity.displayText;

            if (descriptionText != null)
                descriptionText.text = $"Opponent: {opportunity.opponentCarName}";

            if (eventTypeText != null)
                eventTypeText.text = $"Type: {opportunity.eventType}";

            if (betSuggestionText != null)
                betSuggestionText.text = $"Suggested Bet: ${opportunity.betSuggestion:N0}";

            if (popupPanel != null)
                popupPanel.SetActive(true);

            Debug.Log($"Race opportunity displayed: {opportunity.displayText}");
        }

        private void OnAcceptClicked()
        {
            if (currentOpportunity == null)
            {
                Debug.LogWarning("Accept clicked but no opportunity selected.");
                return;
            }

            var manager = GameManager.Instance;
            if (manager == null)
            {
                Debug.LogError("GameManager not found.");
                return;
            }

            // Store the opportunity in GameManager
            manager.SetCurrentRaceOpportunity(currentOpportunity);
            manager.Save();

            Debug.Log($"Race accepted: {currentOpportunity.displayText}. Loading Race scene...");

            // Close popup
            if (popupPanel != null)
                popupPanel.SetActive(false);

            // Load Race scene
            SceneManager.LoadScene("Race");
        }

        private void OnDeclineClicked()
        {
            if (popupPanel != null)
                popupPanel.SetActive(false);

            currentOpportunity = null;
            Debug.Log("Race opportunity declined.");
        }
    }
}
