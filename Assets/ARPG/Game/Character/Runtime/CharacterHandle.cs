using System;
using ARPG.Game.Resource;
using UnityEngine;

namespace ARPG.Game.Character
{
    /// <summary>
    /// 一个Character实例的生命周期Handle。
    ///
    /// 它同时拥有：
    /// 1. Character运行时GameObject；
    /// 2. Character Prefab ResourceHandle。
    ///
    /// Dispose：
    /// 销毁实例并释放Prefab资源所有权。
    /// </summary>
    public sealed class CharacterHandle
        : IDisposable
    {
        private ResourceHandle<GameObject>
            _resourceHandle;

        private CharacterEntity _character;

        private bool _isDisposed;

        internal CharacterHandle(
            CharacterEntity character,
            ResourceHandle<GameObject> resourceHandle)
        {
            _character =
                character
                ?? throw new ArgumentNullException(
                    nameof(character));

            _resourceHandle =
                resourceHandle
                ?? throw new ArgumentNullException(
                    nameof(resourceHandle));
        }

        public CharacterEntity Character =>
            _character;

        public bool IsDisposed =>
            _isDisposed;

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (_character != null)
            {
                UnityEngine.Object.Destroy(
                    _character.gameObject);

                _character = null;
            }

            _resourceHandle?.Dispose();
            _resourceHandle = null;
        }
    }
}