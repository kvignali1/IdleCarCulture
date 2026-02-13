using System;
using System.IO;
using UnityEngine;

namespace IdleCarCulture
{
    /// <summary>
    /// Static save system that serializes <see cref="PlayerProfile"/> to JSON
    /// in <see cref="Application.persistentDataPath"/> and restores it.
    /// </summary>
    public static class SaveSystem
    {
        private const string SaveFileName = "player_profile.json";

        private static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);

        /// <summary>
        /// Saves the provided <paramref name="profile"/> to disk as JSON.
        /// Exceptions are caught and logged with <see cref="Debug.LogError(string)"/>.
        /// </summary>
        public static void Save(PlayerProfile profile)
        {
            try
            {
                if (profile == null) throw new ArgumentNullException(nameof(profile));

                string json = JsonUtility.ToJson(profile, true);
                Directory.CreateDirectory(Application.persistentDataPath);
                File.WriteAllText(SaveFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save PlayerProfile to '{SaveFilePath}': {ex}");
            }
        }

        /// <summary>
        /// Loads a saved <see cref="PlayerProfile"/> if it exists; otherwise creates
        /// a default via <paramref name="defaultFactory"/>, saves it and returns it.
        /// Any errors are caught and logged; a fallback profile will be returned.
        /// </summary>
        public static PlayerProfile LoadOrCreateDefault(Func<PlayerProfile> defaultFactory)
        {
            try
            {
                if (defaultFactory == null) throw new ArgumentNullException(nameof(defaultFactory));

                if (File.Exists(SaveFilePath))
                {
                    string json = File.ReadAllText(SaveFilePath);
                    var profile = JsonUtility.FromJson<PlayerProfile>(json);
                    if (profile != null)
                        return profile;

                    Debug.LogError("Deserialized PlayerProfile was null; creating default profile.");
                }

                var def = defaultFactory();
                Save(def);
                return def;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load PlayerProfile from '{SaveFilePath}': {ex}");
                try
                {
                    var fallback = defaultFactory != null ? defaultFactory() : new PlayerProfile();
                    Save(fallback);
                    return fallback;
                }
                catch (Exception inner)
                {
                    Debug.LogError($"Failed to create or save fallback PlayerProfile: {inner}");
                    return new PlayerProfile();
                }
            }
        }

        /// <summary>
        /// Deletes the saved PlayerProfile file if it exists. Errors are logged.
        /// </summary>
        public static void DeleteSave()
        {
            try
            {
                if (File.Exists(SaveFilePath))
                    File.Delete(SaveFilePath);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete PlayerProfile save '{SaveFilePath}': {ex}");
            }
        }
    }
}
