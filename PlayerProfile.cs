using System;
using System.Collections.Generic;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Serializable player profile containing currencies, owned cars and upgrade states.
    /// </summary>
    [Serializable]
    public class PlayerProfile
    {
        /// <summary>
        /// Player money balance.
        /// </summary>
        public long money;

        /// <summary>
        /// Premium or prestige currency.
        /// </summary>
        public int prestigeCurrency;

        /// <summary>
        /// Heat value (float-based meter).
        /// </summary>
        public float heat;

        /// <summary>
        /// Street cred.
        /// </summary>
        public int cred;

        /// <summary>
        /// Reputation points.
        /// </summary>
        public int reputation;

        /// <summary>
        /// Currently active car id.
        /// </summary>
        public string activeCarId;

        /// <summary>
        /// List of owned car ids.
        /// </summary>
        public List<string> ownedCarIds = new List<string>();

        /// <summary>
        /// Upgrade states per car.
        /// </summary>
        public List<CarUpgradeState> upgradesByCar = new List<CarUpgradeState>();

        /// <summary>
        /// Returns the existing upgrade state for <paramref name="carId"/>, or creates and
        /// returns a new state if none exists.
        /// </summary>
        public CarUpgradeState GetOrCreateUpgradeState(string carId)
        {
            if (string.IsNullOrEmpty(carId))
                throw new ArgumentException("carId cannot be null or empty", nameof(carId));

            if (upgradesByCar == null)
                upgradesByCar = new List<CarUpgradeState>();

            for (int i = 0; i < upgradesByCar.Count; i++)
            {
                var s = upgradesByCar[i];
                if (s != null && s.carId == carId)
                    return s;
            }

            var state = new CarUpgradeState { carId = carId };
            upgradesByCar.Add(state);
            return state;
        }

        /// <summary>
        /// Sets the active car id for the profile.
        /// </summary>
        public void SetActiveCar(string carId)
        {
            activeCarId = carId;
        }
    }
}
