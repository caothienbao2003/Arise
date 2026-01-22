using System.Collections.Generic;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace GridTool
{
    public class ObjectPoolingService : IObjectPoolingService, IInitializable
    {
        private readonly Dictionary<GameObject, Queue<PoolObject>> pools = new();
        private Transform poolParent;

        private readonly IObjectResolver resolver;

        public ObjectPoolingService(IObjectResolver resolver)
        {
            this.resolver = resolver;
        }

        public void Initialize()
        {
            var go = new GameObject("Object Pooling Parent");
            Object.DontDestroyOnLoad(go);
            poolParent = go.transform;

            Debug.Log("[ObjectPoolingService] Initialized");
        }

        public GameObject GetPooledObject(GameObject prefab, Transform parent = null)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPoolingService] Prefab is null");
                return null;
            }

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<PoolObject>();
                pools[prefab] = pool;
            }

            PoolObject poolObject;

            if (pool.Count > 0)
            {
                poolObject = pool.Dequeue();
            }
            else
            {
                var obj = resolver != null
                    ? resolver.Instantiate(prefab, poolParent)
                    : Object.Instantiate(prefab, poolParent);

                obj.name = $"{prefab.name} (Pooled)";

                poolObject = obj.GetComponent<PoolObject>() 
                          ?? obj.AddComponent<PoolObject>();

                poolObject.Initialize(this, prefab);

                resolver?.Inject(obj);
            }

            var goObj = poolObject.gameObject;

            goObj.transform.SetParent(parent, false);
            goObj.SetActive(true);

            foreach (var poolable in goObj.GetComponents<IPoolable>())
            {
                poolable.OnSpawned();
            }

            return goObj;
        }

        public void ReturnToPool(PoolObject poolObject)
        {
            if (poolObject == null)
                return;

            var obj = poolObject.gameObject;
            var prefab = poolObject.Prefab;

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<PoolObject>();
                pools[prefab] = pool;
            }

            foreach (var poolable in obj.GetComponents<IPoolable>())
            {
                poolable.OnDespawned();
            }

            obj.SetActive(false);
            obj.transform.SetParent(poolParent, false);

            pool.Enqueue(poolObject);
        }

        public void PrewarmPool(GameObject prefab, int count)
        {
            if (prefab == null || count <= 0)
                return;

            if (!pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<PoolObject>();
                pools[prefab] = pool;
            }

            for (int i = 0; i < count; i++)
            {
                var obj = resolver != null
                    ? resolver.Instantiate(prefab, poolParent)
                    : Object.Instantiate(prefab, poolParent);

                obj.name = $"{prefab.name} (Pooled)";
                obj.SetActive(false);

                var poolObject = obj.GetComponent<PoolObject>() 
                              ?? obj.AddComponent<PoolObject>();

                poolObject.Initialize(this, prefab);
                resolver?.Inject(obj);

                pool.Enqueue(poolObject);
            }

            Debug.Log($"[ObjectPoolingService] Prewarmed {count} objects for {prefab.name}");
        }

        public void ClearPool(GameObject prefab)
        {
            if (prefab == null || !pools.TryGetValue(prefab, out var pool))
                return;

            while (pool.Count > 0)
            {
                var poolObj = pool.Dequeue();
                if (poolObj != null)
                    Object.Destroy(poolObj.gameObject);
            }

            pools.Remove(prefab);
        }

        public void ClearAllPools()
        {
            foreach (var pool in pools.Values)
            {
                while (pool.Count > 0)
                {
                    var poolObj = pool.Dequeue();
                    if (poolObj != null)
                        Object.Destroy(poolObj.gameObject);
                }
            }

            pools.Clear();
            Debug.Log("[ObjectPoolingService] Cleared all pools");
        }

        public int GetPoolSize(GameObject prefab)
        {
            return prefab != null && pools.TryGetValue(prefab, out var pool)
                ? pool.Count
                : 0;
        }
    }
}
