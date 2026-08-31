using System;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.Character;
using ARPG.Game.Character.Player;
using ARPG.Game.Character.Player.Input;
using UnityEngine;

namespace ARPG.Game.Tests.Character
{
    public sealed class CharacterFactoryTester
        : MonoBehaviour
    {
        [SerializeField]
        private PlayerInputDriver _inputDriver;

        private CharacterFactory
            _characterFactory;

        private CharacterHandle
            _playerHandle;

        private void Awake()
        {
            ServiceContainer services =
                GameLauncher.Instance
                    .GameContext
                    .Services;

            _characterFactory =
                services.Get<CharacterFactory>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                CreatePlayerAsync();
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                DestroyPlayer();
            }
        }

        private async void CreatePlayerAsync()
        {
            try
            {
                if (_playerHandle != null &&
                    !_playerHandle.IsDisposed)
                {
                    Debug.LogWarning(
                        "Player already exists.");

                    return;
                }

                _playerHandle =
                    await _characterFactory
                        .CreateAsync<PlayerCharacter>(
                            Vector3.zero,
                            Quaternion.identity);

                _inputDriver.Bind(
                    _playerHandle.Character);

                Debug.Log(
                    "[Day19] Player created.");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);
            }
        }

        private void DestroyPlayer()
        {
            if (_playerHandle == null)
            {
                return;
            }

            _playerHandle.Dispose();
            _playerHandle = null;

            Debug.Log(
                "[Day19] Player destroyed.");
        }

        private void OnDestroy()
        {
            _inputDriver?.Unbind();
            _playerHandle?.Dispose();
            _playerHandle = null;
        }
    }
}