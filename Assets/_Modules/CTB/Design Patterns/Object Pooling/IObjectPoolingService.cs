using UnityEngine;

namespace GridTool
{
    public interface IObjectPoolingService
    {
        GameObject GetPooledObject(GameObject prefab, Transform parent = null);

        void ReturnToPool(GameObject obj);

        void PrewarmPool(GameObject prefab, int count);

        void ClearPool(GameObject prefab);

        void ClearAllPools();

        int GetPoolSize(GameObject prefab);

        int GetActiveCount(GameObject prefab);
    }
}