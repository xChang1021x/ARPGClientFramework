using System;
using ARPG.Framework.Pool;
using UnityEngine;

namespace ARPG.Game.Tests.Pool
{
    public sealed class GameObjectPoolTester : MonoBehaviour
    {
        [SerializeField]
        private GameObject _bulletPrefab;

        [SerializeField]
        private Transform _spawnPoint;

        [SerializeField]
        private int _defaultCapacity = 10;

        [SerializeField]
        private int _maxSize = 30;

        private GameObjectPool _bulletPool;

        private void Awake()
        {
            if (_bulletPrefab == null)
            {
                throw new InvalidOperationException(
                    "Bullet prefab has not been assigned.");
            }

            if (_spawnPoint == null)
            {
                throw new InvalidOperationException(
                    "Spawn point has not been assigned.");
            }

            GameObject poolRoot =
                new GameObject("BulletPoolRoot");

            poolRoot.transform.SetParent(transform);

            _bulletPool = new GameObjectPool(
                prefab: _bulletPrefab,
                poolRoot: poolRoot.transform,
                defaultCapacity: _defaultCapacity,
                maxSize: _maxSize);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Space))
            {
                return;
            }

            _bulletPool.Get(
                _spawnPoint.position,
                _spawnPoint.rotation);
        }

        private void OnDestroy()
        {
            _bulletPool?.Dispose();
        }
    }
}