using UnityEngine;

namespace GridTool
{
    public interface IObjectPoolingService
    {
        GameObject GetPooledObject(GameObject prefab, Transform parent = null);
        
        void PrewarmPool(GameObject prefab, int count);
    }
}