using UnityEngine;

namespace GridTool
{
    public class PoolObject : MonoBehaviour
    {
        public GameObject Prefab { get; private set; }
        private ObjectPoolingService pool;

        public void Initialize(ObjectPoolingService pool, GameObject prefab)
        {
            this.pool = pool;
            Prefab = prefab;
        }

        public void Despawn()
        {
            pool.ReturnToPool(this);
        }
    }
}