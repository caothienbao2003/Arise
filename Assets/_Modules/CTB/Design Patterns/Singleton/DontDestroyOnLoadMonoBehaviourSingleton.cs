using UnityEngine;

namespace CTB
{
    public abstract class DontDestroyOnLoadMonoBehaviourSingleton<T>: MonoBehaviour where T : Component
    {
        private static T instance;
        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindAnyObjectByType<T>();

                    if (instance == null)
                    {
                        GameObject newGameObject = new GameObject(typeof(T).Name);
                        instance = newGameObject.AddComponent<T>();
                        
                        DontDestroyOnLoad(instance.gameObject);
                    }
                }
                return instance;
            }
        }
        
        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                DontDestroyOnLoad(gameObject);
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }
        }
    }
}