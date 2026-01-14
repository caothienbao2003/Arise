using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GridTool
{
    public class ObjectPoolingService : IObjectPoolingService, IInitializable
    {
        private readonly Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();
        private readonly Dictionary<GameObject, GameObject> activeObjects = new Dictionary<GameObject, GameObject>();
        private readonly Dictionary<GameObject, int> activeCountPerPrefab = new Dictionary<GameObject, int>();
        
        private Transform poolParent;
        private readonly IObjectResolver resolver;

        public ObjectPoolingService(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        public void Initialize()
        {
            GameObject poolParentGO = new GameObject("Object Pools");
            Object.DontDestroyOnLoad(poolParentGO);
            poolParent = poolParentGO.transform;
            
            Debug.Log("[ObjectPoolService] Initialized");
        }

        public GameObject GetPooledObject(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolService] Cannot get pooled object: prefab is null");
                return null;
            }

            if (!poolDictionary.ContainsKey(prefab))
            {
                poolDictionary[prefab] = new Queue<GameObject>();
            }

            GameObject obj;

            if (poolDictionary[prefab].Count > 0)
            {
                obj = poolDictionary[prefab].Dequeue();
                obj.SetActive(true);
                
                resolver?.Inject(obj);
            }
            else
            {
                if (resolver != null)
                {
                    obj = resolver.Instantiate(prefab, poolParent);
                }
                else
                {
                    obj = Object.Instantiate(prefab, poolParent);
                }
                obj.name = $"{prefab.name} (Pooled)";
            }

            if (parent != null)
            {
                obj.transform.SetParent(parent);
            }

            activeObjects[obj] = prefab;
            
            activeCountPerPrefab.TryAdd(prefab, 0);
            activeCountPerPrefab[prefab]++;

            return obj;
        }

        public void ReturnToPool(GameObject obj)
        {
            if (obj == null)
            {
                Debug.LogWarning("[ObjectPoolService] Attempted to return null object to pool");
                return;
            }

            if (!activeObjects.Remove(obj, out var prefab))
            {
                Debug.LogWarning($"[ObjectPoolService] Object {obj.name} is not tracked by pool service");
                Object.Destroy(obj);
                return;
            }

            if (activeCountPerPrefab.ContainsKey(prefab))
            {
                activeCountPerPrefab[prefab]--;
                if (activeCountPerPrefab[prefab] < 0)
                {
                    activeCountPerPrefab[prefab] = 0;
                }
            }

            obj.SetActive(false);
            obj.transform.SetParent(poolParent);
            
            if (!poolDictionary.ContainsKey(prefab))
            {
                poolDictionary[prefab] = new Queue<GameObject>();
            }
            
            poolDictionary[prefab].Enqueue(obj);
        }

        public void PrewarmPool(GameObject prefab, int count)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolService] Cannot prewarm pool: prefab is null");
                return;
            }

            if (!poolDictionary.ContainsKey(prefab))
            {
                poolDictionary[prefab] = new Queue<GameObject>();
            }

            for (int i = 0; i < count; i++)
            {
                GameObject obj;
                
                if (resolver != null)
                {
                    obj = resolver.Instantiate(prefab, poolParent);
                }
                else
                {
                    obj = Object.Instantiate(prefab, poolParent);
                }
                
                obj.name = $"{prefab.name} (Pooled)";
                obj.SetActive(false);
                poolDictionary[prefab].Enqueue(obj);
            }

            Debug.Log($"[ObjectPoolService] Prewarmed pool for {prefab.name} with {count} objects");
        }

        public void ClearPool(GameObject prefab)
        {
            if (prefab == null || !poolDictionary.ContainsKey(prefab))
            {
                return;
            }

            while (poolDictionary[prefab].Count > 0)
            {
                GameObject obj = poolDictionary[prefab].Dequeue();
                if (obj != null)
                {
                    Object.Destroy(obj);
                }
            }

            poolDictionary.Remove(prefab);
            
            activeCountPerPrefab.Remove(prefab);

            Debug.Log($"[ObjectPoolService] Cleared pool for {prefab.name}");
        }

        public void ClearAllPools()
        {
            foreach (var kvp in poolDictionary)
            {
                while (kvp.Value.Count > 0)
                {
                    GameObject obj = kvp.Value.Dequeue();
                    if (obj != null)
                    {
                        Object.Destroy(obj);
                    }
                }
            }

            poolDictionary.Clear();
            activeObjects.Clear();
            activeCountPerPrefab.Clear();

            Debug.Log("[ObjectPoolService] Cleared all pools");
        }

        public int GetPoolSize(GameObject prefab)
        {
            if (prefab == null || !poolDictionary.TryGetValue(prefab, out var value))
            {
                return 0;
            }

            return value.Count;
        }

        public int GetActiveCount(GameObject prefab)
        {
            if (prefab == null || !activeCountPerPrefab.TryGetValue(prefab, out var activeCount))
            {
                return 0;
            }

            return activeCount;
        }
    }
}