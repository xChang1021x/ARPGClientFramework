using System;
using UnityEngine;

namespace ARPG.Framework.Pool
{
    /// <summary>
    /// Unity GameObject对象池。
    /// 每个实例管理一种Prefab。
    /// </summary>
    public sealed class GameObjectPool : IDisposable
    {
        private readonly GameObject _prefab;
        private readonly Transform _poolRoot;

        private readonly ObjectPool<GameObject> _pool;

        public int ActiveCount => _pool.ActiveCount;

        public int InactiveCount => _pool.InactiveCount;

        public int TotalCount => _pool.TotalCount;

        public GameObjectPool(
            GameObject prefab,
            Transform poolRoot,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            if (poolRoot == null)
            {
                throw new ArgumentNullException(nameof(poolRoot));
            }

            _prefab = prefab;
            _poolRoot = poolRoot;

            _pool = new ObjectPool<GameObject>(
                createFunc: CreateInstance,
                onRelease: OnRelease,
                onDestroy: OnDestroy,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize);
        }

        /// <summary>
        /// 从池中获取GameObject。
        /// </summary>
        public GameObject Get(
    Vector3 position,
    Quaternion rotation,
    Transform parent = null)
        {
            GameObject instance = _pool.Get();

            Transform instanceTransform = instance.transform;
            instanceTransform.SetParent(parent);
            instanceTransform.SetPositionAndRotation(position, rotation);

            Activate(instance);

            return instance;
        }

        private static void Activate(GameObject instance)
        {
            PooledObject pooledObject =
                instance.GetComponent<PooledObject>();

            pooledObject.MarkSpawned();

            instance.SetActive(true);

            InvokePoolableSpawn(instance);
        }

        /// <summary>
        /// 回收GameObject。
        /// </summary>
        public void Release(GameObject instance)
        {
            _pool.Release(instance);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        private GameObject CreateInstance()
        {
            GameObject instance =
                UnityEngine.Object.Instantiate(
                    _prefab,
                    _poolRoot);

            instance.name = $"{_prefab.name}_Pooled";

            PooledObject pooledObject =
                instance.GetComponent<PooledObject>();

            if (pooledObject == null)
            {
                pooledObject =
                    instance.AddComponent<PooledObject>();
            }

            pooledObject.Initialize(Release);

            return instance;
        }

        private void OnGet(GameObject instance)
        {
            PooledObject pooledObject =
                instance.GetComponent<PooledObject>();

            pooledObject.MarkSpawned();

            instance.SetActive(true);

            InvokePoolableSpawn(instance);
        }

        private void OnRelease(GameObject instance)
        {
            InvokePoolableDespawn(instance);

            PooledObject pooledObject =
                instance.GetComponent<PooledObject>();

            /*
             * 构造函数预热时，对象从未进入Spawn状态。
             * 因此只有已Spawn对象才执行MarkDespawned。
             */
            if (pooledObject.IsSpawned)
            {
                pooledObject.MarkDespawned();
            }

            instance.transform.SetParent(_poolRoot);
            instance.SetActive(false);
        }

        private static void OnDestroy(GameObject instance)
        {
            if (instance != null)
            {
                UnityEngine.Object.Destroy(instance);
            }
        }

        private static void InvokePoolableSpawn(
            GameObject instance)
        {
            IPoolable[] poolableComponents =
                instance.GetComponents<IPoolable>();

            foreach (IPoolable poolable in poolableComponents)
            {
                poolable.OnSpawn();
            }
        }

        private static void InvokePoolableDespawn(
            GameObject instance)
        {
            IPoolable[] poolableComponents =
                instance.GetComponents<IPoolable>();

            foreach (IPoolable poolable in poolableComponents)
            {
                poolable.OnDespawn();
            }
        }
    }
}