using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CTB_Utils
{
    public static class SceneUtils
    {
        public enum SceneExecutionMode
        {
            CloseTargetAfter,
            CloseOriginalAfter
        }

        /// <summary>
        /// Create a new scene at the specified path.
        /// </summary>
        public static bool CreateScene(
            string sceneName,
            string sceneFolderPath,
            NewSceneSetup newSceneSetup = NewSceneSetup.DefaultGameObjects,
            NewSceneMode newSceneMode = NewSceneMode.Single
            )
        {
            AssetDatabaseUtils.EnsureFolderExists(sceneFolderPath);

            string scenePath = $"{sceneFolderPath}/{sceneName}.unity";

            Scene newScene = EditorSceneManager.NewScene(newSceneSetup, newSceneMode);
            bool saveSuccess = EditorSceneManager.SaveScene(newScene, scenePath);

            if (!saveSuccess) return false;

            SceneManager.SetActiveScene(newScene);
            AssetDatabase.Refresh();

            return true;
        }

        /// <summary>
        /// Opens a scene at the specified path.
        /// </summary>
        public static Scene OpenScene(string scenePath, OpenSceneMode mode = OpenSceneMode.Single)
        {
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            {
                Debug.LogError($"Scene asset not found at path: {scenePath}");
                return new Scene();
            }

            Scene loadedScene = EditorSceneManager.OpenScene(scenePath, mode);

            if (loadedScene.IsValid() && loadedScene.isLoaded)
            {
                Debug.Log($"Scene '{loadedScene.name}' opened successfully in {mode} mode.");
            }
            else
            {
                Debug.LogError($"Failed to open scene at path: {scenePath}");
            }

            return loadedScene;
        }

        /// <summary>
        /// Closes a scene.
        /// </summary>
        public static bool CloseScene(Scene targetScene, bool saveIfDirty = true)
        {
            string sceneName = targetScene.IsValid() ? targetScene.name : "Unknown";
            string scenePath = targetScene.IsValid() ? targetScene.path : "Unknown";

            if (!targetScene.IsValid() || !targetScene.isLoaded)
            {
                Debug.LogWarning($"Scene '{sceneName}' (path: {scenePath}) is not valid or not loaded. Nothing to close.");
                return true;
            }

            bool success = EditorSceneManager.CloseScene(targetScene, saveIfDirty);

            if (success)
            {
                Debug.Log($"Scene '{sceneName}' closed successfully.");
            }
            else
            {
                Debug.LogError($"Failed to close scene '{sceneName}'.");
            }

            return success;
        }

        /// <summary>
        /// Opens a scene, executes an action, then closes a scene based on the mode.
        /// </summary>
        public static bool ExecuteInScene(
            Scene targetScene,
            Action<Scene> action,
            SceneExecutionMode mode = SceneExecutionMode.CloseTargetAfter,
            bool saveAfterAction = true,
            bool saveOnClose = true
            )
        {
            if (!targetScene.IsValid() || string.IsNullOrEmpty(targetScene.path))
            {
                Debug.LogError("Target scene is invalid or has no path.");
                return false;
            }

            return ExecuteInScene(targetScene.path, action, mode, saveAfterAction, saveOnClose);
        }

        /// <summary>
        /// Opens a scene, executes an action, then closes a scene based on the mode.
        /// </summary>
        public static bool ExecuteInScene(
            SceneAsset sceneAsset,
            Action<Scene> action,
            SceneExecutionMode mode = SceneExecutionMode.CloseTargetAfter,
            bool saveAfterAction = true,
            bool saveOnClose = true
            )
        {
            if (sceneAsset == null)
            {
                Debug.LogError("Scene asset is null.");
                return false;
            }

            string scenePath = AssetDatabase.GetAssetPath(sceneAsset);

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("Failed to get scene path from scene asset.");
                return false;
            }

            return ExecuteInScene(scenePath, action, mode, saveAfterAction, saveOnClose);
        }

        /// <summary>
        /// Opens a scene, executes an action, then closes a scene based on the mode.
        /// </summary>
        public static bool ExecuteInScene(
            string scenePath,
            Action<Scene> action,
            SceneExecutionMode mode = SceneExecutionMode.CloseTargetAfter,
            bool saveAfterAction = true,
            bool saveOnClose = true
            )
        {
            if (action == null)
            {
                Debug.LogError("Action cannot be null.");
                return false;
            }

            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("Scene path is null or empty.");
                return false;
            }

            Scene originalScene = SceneManager.GetActiveScene();

            bool isTargetSceneAlreadyActive = originalScene.IsValid() &&
                                              originalScene.isLoaded &&
                                              originalScene.path == scenePath;

            if (isTargetSceneAlreadyActive)
            {
                Debug.Log($"Target scene '{originalScene.name}' is already active. Executing action in current scene.");
                return ExecuteInCurrentScene(originalScene, action, saveAfterAction);
            }

            Scene targetScene = OpenScene(scenePath, OpenSceneMode.Additive);

            if (!IsSceneLoaded(targetScene))
            {
                Debug.LogError($"Failed to load target scene at path: {scenePath}");
                return false;
            }

            bool success = false;

            try
            {
                SceneManager.SetActiveScene(targetScene);
                action.Invoke(targetScene);

                if (saveAfterAction)
                {
                    SaveScene(targetScene);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred while executing action in scene '{targetScene.name}': {ex.Message}");
            }
            finally
            {
                HandleSceneCleanup(targetScene, originalScene, mode, saveOnClose);
            }

            return success;
        }

        /// <summary>
        /// Executes an action in the current scene without opening or closing.
        /// </summary>
        private static bool ExecuteInCurrentScene(Scene currentScene, Action<Scene> action, bool saveAfterAction)
        {
            try
            {
                action.Invoke(currentScene);

                if (saveAfterAction)
                {
                    SaveScene(currentScene);
                }

                Debug.Log($"Action executed successfully in current scene '{currentScene.name}'.");
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Exception occurred while executing action in current scene '{currentScene.name}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Checks if a scene is valid and loaded.
        /// </summary>
        private static bool IsSceneLoaded(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Saves a scene.
        /// </summary>
        private static void SaveScene(Scene scene)
        {
            // Force the dirty flag so Unity doesn't skip the save
            EditorSceneManager.MarkSceneDirty(scene);

            bool saveSuccess = EditorSceneManager.SaveScene(scene);

            if (!saveSuccess)
            {
                Debug.LogWarning($"Failed to save scene '{scene.name}' at {scene.path}.");
            }
            else
            {
                // Optional: Ensures any SO or Prefab changes are also committed
                AssetDatabase.SaveAssets();
                Debug.Log($"Scene '{scene.name}' saved successfully.");
            }
        }

        /// <summary>
        /// Handles scene cleanup based on the execution mode.
        /// </summary>
        private static void HandleSceneCleanup(
            Scene targetScene,
            Scene originalScene,
            SceneExecutionMode mode,
            bool saveOnClose
            )
        {
            switch (mode)
            {
                case SceneExecutionMode.CloseTargetAfter:
                    if (IsSceneLoaded(targetScene))
                    {
                        CloseScene(targetScene, saveOnClose);
                    }

                    if (IsSceneLoaded(originalScene))
                    {
                        SceneManager.SetActiveScene(originalScene);
                    }
                    break;

                case SceneExecutionMode.CloseOriginalAfter:
                    if (IsSceneLoaded(originalScene))
                    {
                        CloseScene(originalScene, saveOnClose);
                    }
                    break;
            }
        }
    }
}