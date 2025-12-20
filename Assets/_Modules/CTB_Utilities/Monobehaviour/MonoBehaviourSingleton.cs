using System;
using UnityEngine;

namespace CTB_Utils
{
    public class MonoBehaviourSingleton<T>: MonoBehaviour where T : Component
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
                    }
                }
                return instance;
            }
        }
    }
}