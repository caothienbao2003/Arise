using UnityEngine;

namespace CTB
{
    public abstract class ScriptableSingleton<T> : ScriptableObject where T : ScriptableObject
    {
        private static T _instance = null;

        protected static T GetInstance(string path)
        {
            if (_instance == null)
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<T>(path);

                    if (_instance == null)
                    {
                        Debug.LogError($"Failed to load {typeof(T).Name} from Resources/{path}. " +
                                       $"Ensure the asset exists in a Resources folder at that path.");
                    }
                }
            }

            return _instance;
        }
    }
}