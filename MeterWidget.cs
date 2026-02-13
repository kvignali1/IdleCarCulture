using UnityEngine;
using UnityEngine.UI;

namespace IdleCarCulture
{
    /// <summary>
    /// Reusable UI meter widget with a fillable bar and target green zone.
    /// Tracks current meter value and calculates score based on proximity to green zone center.
    /// </summary>
    public class MeterWidget : MonoBehaviour
    {
        /// <summary>
        /// Image component for the meter fill bar.
        /// </summary>
        [SerializeField]
        private Image fillImage;

        /// <summary>
        /// Start of the green zone target range (0..1).
        /// </summary>
        [SerializeField]
        [Range(0f, 1f)]
        private float greenZoneStart = 0.4f;

        /// <summary>
        /// End of the green zone target range (0..1).
        /// </summary>
        [SerializeField]
        [Range(0f, 1f)]
        private float greenZoneEnd = 0.6f;

        /// <summary>
        /// Current meter fill value (0..1).
        /// </summary>
        private float currentValue = 0f;

        /// <summary>
        /// Whether the meter is currently active.
        /// </summary>
        private bool isActive = false;

        /// <summary>
        /// Rate at which the meter fills when active (fill units per second).
        /// Set to 0 for manual control.
        /// </summary>
        private float autoFillRate = 0f;

        private void OnEnable()
        {
            ResetMeter();
        }

        private void Update()
        {
            if (!isActive)
                return;

            // Auto-fill if rate is set
            if (autoFillRate > 0)
            {
                currentValue += autoFillRate * Time.deltaTime;
                currentValue = Mathf.Clamp01(currentValue);
                UpdateDisplay();
            }
        }

        /// <summary>
        /// Starts the meter. Optionally sets auto-fill rate.
        /// </summary>
        public void StartMeter(float autoFillRate = 0f)
        {
            isActive = true;
            this.autoFillRate = autoFillRate;
            ResetMeter();
            Debug.Log($"Meter started. Auto-fill rate: {autoFillRate}");
        }

        /// <summary>
        /// Stops the meter and freezes current value.
        /// </summary>
        public void StopMeter()
        {
            isActive = false;
            Debug.Log($"Meter stopped at value: {currentValue:F2}");
        }

        /// <summary>
        /// Resets the meter to 0 and updates display.
        /// </summary>
        public void ResetMeter()
        {
            currentValue = 0f;
            UpdateDisplay();
        }

        /// <summary>
        /// Manually sets the meter value and updates display.
        /// </summary>
        public void SetValue(float value)
        {
            currentValue = Mathf.Clamp01(value);
            UpdateDisplay();
        }

        /// <summary>
        /// Increments the meter value by the specified amount.
        /// </summary>
        public void AddValue(float amount)
        {
            currentValue = Mathf.Clamp01(currentValue + amount);
            UpdateDisplay();
        }

        /// <summary>
        /// Returns the current meter value (0..1).
        /// </summary>
        public float GetCurrentValue()
        {
            return currentValue;
        }

        /// <summary>
        /// Returns a score (0..1) based on how close the current value is to the center of the green zone.
        /// Perfect center = 1.0, edges of green zone = 0.8, outside zone = lower scores.
        /// </summary>
        public float GetScore()
        {
            float greenZoneCenter = (greenZoneStart + greenZoneEnd) / 2f;
            float greenZoneWidth = greenZoneEnd - greenZoneStart;

            // Distance from center in normalized units
            float distanceFromCenter = Mathf.Abs(currentValue - greenZoneCenter);

            // If outside green zone, apply heavy penalty
            if (currentValue < greenZoneStart || currentValue > greenZoneEnd)
            {
                // Score decreases as we get further from green zone
                float distanceOutside = Mathf.Max(
                    greenZoneStart - currentValue,
                    currentValue - greenZoneEnd);
                return Mathf.Max(0f, 0.5f - distanceOutside * 2f);
            }

            // Inside green zone: score based on distance from center
            // Center = 1.0, edges = 0.8
            float normalizedDistance = distanceFromCenter / (greenZoneWidth / 2f);
            return Mathf.Lerp(1f, 0.8f, normalizedDistance);
        }

        /// <summary>
        /// Returns the center value of the green zone (0..1).
        /// </summary>
        public float GetGreenZoneCenter()
        {
            return (greenZoneStart + greenZoneEnd) / 2f;
        }

        /// <summary>
        /// Returns true if the current value is within the green zone.
        /// </summary>
        public bool IsInGreenZone()
        {
            return currentValue >= greenZoneStart && currentValue <= greenZoneEnd;
        }

        /// <summary>
        /// Sets the green zone range (both values clamped 0..1).
        /// </summary>
        public void SetGreenZoneRange(float start, float end)
        {
            greenZoneStart = Mathf.Clamp01(start);
            greenZoneEnd = Mathf.Clamp01(end);
            if (greenZoneStart > greenZoneEnd)
            {
                var temp = greenZoneStart;
                greenZoneStart = greenZoneEnd;
                greenZoneEnd = temp;
            }
        }

        /// <summary>
        /// Updates the fill image display based on current value.
        /// </summary>
        private void UpdateDisplay()
        {
            if (fillImage != null)
                fillImage.fillAmount = currentValue;
        }
    }
}
