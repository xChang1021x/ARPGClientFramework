using ARPG.Framework.Pool;
using UnityEngine;

namespace ARPG.Game.Tests.Pool
{
    [RequireComponent(typeof(PooledObject))]
    public sealed class TestBullet : MonoBehaviour, IPoolable
    {
        [SerializeField]
        private float _speed = 10f;

        [SerializeField]
        private float _lifeTime = 2f;

        private float _remainingLifeTime;
        private PooledObject _pooledObject;

        private void Awake()
        {
            _pooledObject = GetComponent<PooledObject>();
        }

        private void Update()
        {
            transform.position +=
                transform.forward * (_speed * Time.deltaTime);

            _remainingLifeTime -= Time.deltaTime;

            if (_remainingLifeTime <= 0f)
            {
                _pooledObject.Release();
            }
        }

        public void OnSpawn()
        {
            _remainingLifeTime = _lifeTime;
        }

        public void OnDespawn()
        {
            _remainingLifeTime = 0f;
        }
    }
}