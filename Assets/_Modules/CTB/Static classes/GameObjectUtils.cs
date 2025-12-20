using UnityEngine;

namespace CTB
{
    public static class GameObjectUtils
    {
        public static T FindOrCreateComponent<T>(string gameObjectName) where T : Component
        {
            T monobehaviour = Object.FindAnyObjectByType<T>();
            if (monobehaviour == null)
            {
                GameObject newGameObject = FindOrCreateGameObject(gameObjectName);
                monobehaviour = newGameObject.AddComponent<T>();
            }

            return monobehaviour;
        }

        public static GameObject FindOrCreateGameObject(string gameObjectName)
        {
            GameObject gameObject = GameObject.Find(gameObjectName);

            if (gameObject == null)
            {
                gameObject = new GameObject(gameObjectName);
            }

            return gameObject;
        }
    }
}