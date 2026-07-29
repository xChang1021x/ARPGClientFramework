using System;
using System.Collections.Generic;

namespace ARPG.Framework.Pool
{
    /// <summary>
    /// 通用同步对象池。
    /// 适用于纯C#对象以及由上层封装管理的Unity对象。
    /// </summary>
    /// <typeparam name="T">池中对象类型。</typeparam>
    public sealed class ObjectPool<T> : IDisposable
        where T : class
    {
        private readonly Stack<T> _inactiveObjects;
        private readonly HashSet<T> _inactiveLookup;

        private readonly Func<T> _createFunc;
        private readonly Action<T> _onGet;
        private readonly Action<T> _onRelease;
        private readonly Action<T> _onDestroy;

        private readonly int _maxSize;

        private bool _isDisposed;

        /// <summary>
        /// 当前池中未使用对象数量。
        /// </summary>
        public int InactiveCount => _inactiveObjects.Count;

        /// <summary>
        /// 当前已借出对象数量。
        /// </summary>
        public int ActiveCount { get; private set; }

        /// <summary>
        /// 当前由对象池管理的总对象数量。
        /// </summary>
        public int TotalCount => ActiveCount + InactiveCount;

        public ObjectPool(
            Func<T> createFunc,
            Action<T> onGet = null,
            Action<T> onRelease = null,
            Action<T> onDestroy = null,
            int defaultCapacity = 10,
            int maxSize = 100)
        {
            if (createFunc == null)
            {
                throw new ArgumentNullException(nameof(createFunc));
            }

            if (defaultCapacity < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(defaultCapacity),
                    "Default capacity cannot be negative.");
            }

            if (maxSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxSize),
                    "Max size must be greater than zero.");
            }

            if (defaultCapacity > maxSize)
            {
                throw new ArgumentException(
                    "Default capacity cannot be greater than max size.");
            }

            _createFunc = createFunc;
            _onGet = onGet;
            _onRelease = onRelease;
            _onDestroy = onDestroy;
            _maxSize = maxSize;

            _inactiveObjects = new Stack<T>(defaultCapacity);
            _inactiveLookup = new HashSet<T>();

            Prewarm(defaultCapacity);
        }

        /// <summary>
        /// 从对象池获取对象。
        /// 如果池中没有可用对象，则创建新对象。
        /// </summary>
        public T Get()
        {
            ThrowIfDisposed();

            T instance;

            if (_inactiveObjects.Count > 0)
            {
                instance = _inactiveObjects.Pop();
                _inactiveLookup.Remove(instance);
            }
            else
            {
                instance = CreateInstance();
            }

            ActiveCount++;

            _onGet?.Invoke(instance);

            return instance;
        }

        /// <summary>
        /// 将对象归还到对象池。
        /// </summary>
        public void Release(T instance)
        {
            ThrowIfDisposed();

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            if (_inactiveLookup.Contains(instance))
            {
                throw new InvalidOperationException(
                    $"Object of type '{typeof(T).Name}' has already been released.");
            }

            if (ActiveCount <= 0)
            {
                throw new InvalidOperationException(
                    $"Pool '{typeof(T).Name}' has no active objects to release.");
            }

            ActiveCount--;

            _onRelease?.Invoke(instance);

            if (_inactiveObjects.Count >= _maxSize)
            {
                _onDestroy?.Invoke(instance);
                return;
            }

            _inactiveObjects.Push(instance);
            _inactiveLookup.Add(instance);
        }

        /// <summary>
        /// 预创建指定数量的对象并放入池中。
        /// </summary>
        public void Prewarm(int count)
        {
            ThrowIfDisposed();

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            int availableCapacity = _maxSize - _inactiveObjects.Count;
            int createCount = Math.Min(count, availableCapacity);

            for (int index = 0; index < createCount; index++)
            {
                T instance = CreateInstance();

                _onRelease?.Invoke(instance);

                _inactiveObjects.Push(instance);
                _inactiveLookup.Add(instance);
            }
        }

        /// <summary>
        /// 销毁池内所有未使用对象。
        /// 已借出的对象不会被自动回收。
        /// </summary>
        public void Clear()
        {
            ThrowIfDisposed();

            while (_inactiveObjects.Count > 0)
            {
                T instance = _inactiveObjects.Pop();
                _inactiveLookup.Remove(instance);

                _onDestroy?.Invoke(instance);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            Clear();

            _isDisposed = true;
        }

        private T CreateInstance()
        {
            T instance = _createFunc();

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"Create function returned null for pool '{typeof(T).Name}'.");
            }

            return instance;
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(
                    $"ObjectPool<{typeof(T).Name}>");
            }
        }
    }
}