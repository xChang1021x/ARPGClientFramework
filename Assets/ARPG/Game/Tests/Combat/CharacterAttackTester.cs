using System;
using ARPG.Framework.Core;
using ARPG.Game.Bootstrap;
using ARPG.Game.Character;
using ARPG.Game.Character.Control;
using ARPG.Game.Character.Movement;
using ARPG.Game.Character.Player;
using ARPG.Game.Character.Player.Input;
using ARPG.Game.Character.TrainingDummy;
using ARPG.Game.GameCamera;
using UnityEngine;

namespace ARPG.Game.Tests.Combat
{
    /// <summary>
    /// Day29 Character攻击命中链路测试。
    ///
    /// 测试：
    /// Player Input
    /// → AttackState
    /// → CharacterAttackExecutor
    /// → TrainingDummy
    /// → DamageReceiver
    /// → Health
    /// → Hit / Dead。
    /// </summary>
    public sealed class CharacterAttackTester : MonoBehaviour
    {
        private static readonly CharacterControlIntent
            NoControlIntent =
                new CharacterControlIntent(
                    new CharacterMovementIntent(
                        Vector3.zero),
                    false);

        [Header("Player")]
        [SerializeField]
        private PlayerInputDriver _inputDriver;

        [SerializeField]
        private ThirdPersonCameraController
            _cameraController;

        [Header("Spawn")]
        [SerializeField]
        private Vector3 _playerSpawnPosition =
            new Vector3(
                0f,
                1f,
                0f);

        [SerializeField]
        private Vector3 _dummySpawnPosition =
            new Vector3(
                0f,
                1f,
                2f);

        private CharacterFactory
            _characterFactory;

        private CharacterHandle
            _playerHandle;

        private CharacterHandle
            _dummyHandle;

        private int _lastDummyHealth;

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
            TickDummy();

            HandleTestInput();

            TrackDummyHealth();
        }

        /// <summary>
        /// Dummy没有PlayerInputDriver或AI，
        /// 所以测试脚本临时负责驱动它的FSM。
        /// </summary>
        private void TickDummy()
        {
            if (_dummyHandle == null ||
                _dummyHandle.IsDisposed)
            {
                return;
            }

            CharacterEntity dummy =
                _dummyHandle.Character;

            if (!dummy.IsInitialized)
            {
                return;
            }

            dummy.Context
                .StateMachine
                .Tick(
                    NoControlIntent,
                    Time.deltaTime);
        }

        private void HandleTestInput()
        {
            /*
             * 1：同时创建Player和Dummy。
             */
            if (Input.GetKeyDown(
                    KeyCode.Alpha1))
            {
                CreateCombatActorsAsync();
            }

            /*
             * 2：销毁Player。
             */
            if (Input.GetKeyDown(
                    KeyCode.Alpha2))
            {
                DestroyPlayer();
            }

            /*
             * 3：销毁Dummy。
             */
            if (Input.GetKeyDown(
                    KeyCode.Alpha3))
            {
                DestroyDummy();
            }

            /*
             * G：打印双方运行时信息。
             */
            if (Input.GetKeyDown(
                    KeyCode.G))
            {
                LogCombatState();
            }

            /*
             * R：重新创建测试角色。
             */
            if (Input.GetKeyDown(
                    KeyCode.R))
            {
                ResetCombatActorsAsync();
            }

            /*
             * Escape：释放鼠标。
             */
            if (UnityEngine.InputSystem
                    .Keyboard.current != null &&
                UnityEngine.InputSystem
                    .Keyboard.current
                    .escapeKey
                    .wasPressedThisFrame)
            {
                Cursor.lockState =
                    CursorLockMode.None;

                Cursor.visible =
                    true;
            }
        }

        private async void CreateCombatActorsAsync()
        {
            try
            {
                if (HasLivingHandle(
                        _playerHandle) ||
                    HasLivingHandle(
                        _dummyHandle))
                {
                    Debug.LogWarning(
                        "[Day29] Combat actors already exist.");

                    return;
                }

                /*
                 * Player面向世界正Z。
                 *
                 * Dummy默认放在Player正前方，
                 * 保证第一版攻击球能够命中。
                 */
                _playerHandle =
                    await _characterFactory
                        .CreateAsync<PlayerCharacter>(
                            _playerSpawnPosition,
                            Quaternion.identity);

                _dummyHandle =
                    await _characterFactory
                        .CreateAsync<TrainingDummyCharacter>(
                            _dummySpawnPosition,
                            Quaternion.identity);

                _inputDriver.Bind(
                    _playerHandle.Character);

                _cameraController.Bind(
                    _playerHandle
                        .Character
                        .transform);

                _lastDummyHealth =
                    _dummyHandle
                        .Character
                        .Context
                        .Health
                        .CurrentHealth;

                Debug.Log(
                    "[Day29] Combat actors created. " +
                    "Left click to attack.");
            }
            catch (Exception exception)
            {
                Debug.LogException(
                    exception);

                /*
                 * 如果第二次Create失败，
                 * 避免留下半初始化测试现场。
                 */
                DestroyCombatActors();
            }
        }

        private async void ResetCombatActorsAsync()
        {
            DestroyCombatActors();

            /*
             * Destroy是延迟销毁。
             * 对当前测试而言重新创建到不同实例即可，
             * 不依赖旧对象立即从Scene消失。
             */
            CreateCombatActorsAsync();

            await System.Threading.Tasks.Task.CompletedTask;
        }

        private void TrackDummyHealth()
        {
            if (_dummyHandle == null ||
                _dummyHandle.IsDisposed)
            {
                return;
            }

            CharacterEntity dummy =
                _dummyHandle.Character;

            int currentHealth =
                dummy.Context
                    .Health
                    .CurrentHealth;

            if (currentHealth ==
                _lastDummyHealth)
            {
                return;
            }

            int damage =
                _lastDummyHealth -
                currentHealth;

            _lastDummyHealth =
                currentHealth;

            Type stateType =
                dummy.Context
                    .StateMachine
                    .CurrentStateType;

            Debug.Log(
                $"[Day29] Dummy damaged. " +
                $"Damage={damage}, " +
                $"HP={currentHealth}/" +
                $"{dummy.Context.Health.MaxHealth}, " +
                $"Alive={dummy.Context.Health.IsAlive}, " +
                $"State={stateType?.Name}");
        }

        private void LogCombatState()
        {
            LogCharacter(
                "Player",
                _playerHandle);

            LogCharacter(
                "Dummy",
                _dummyHandle);
        }

        private static void LogCharacter(
            string name,
            CharacterHandle handle)
        {
            if (handle == null ||
                handle.IsDisposed)
            {
                Debug.Log(
                    $"[Day29] {name}: Not created.");

                return;
            }

            CharacterEntity character =
                handle.Character;

            Type stateType =
                character.Context
                    .StateMachine
                    .CurrentStateType;

            Debug.Log(
                $"[Day29] {name}: " +
                $"HP=" +
                $"{character.Context.Health.CurrentHealth}/" +
                $"{character.Context.Health.MaxHealth}, " +
                $"Alive=" +
                $"{character.Context.Health.IsAlive}, " +
                $"State={stateType?.Name}, " +
                $"Position={character.transform.position}");
        }

        private void DestroyPlayer()
        {
            if (_playerHandle == null)
            {
                return;
            }

            _inputDriver?.Unbind();
            _cameraController?.Unbind();

            _playerHandle.Dispose();
            _playerHandle = null;

            Debug.Log(
                "[Day29] Player destroyed.");
        }

        private void DestroyDummy()
        {
            if (_dummyHandle == null)
            {
                return;
            }

            _dummyHandle.Dispose();
            _dummyHandle = null;

            Debug.Log(
                "[Day29] Dummy destroyed.");
        }

        private void DestroyCombatActors()
        {
            DestroyPlayer();
            DestroyDummy();
        }

        private static bool HasLivingHandle(
            CharacterHandle handle)
        {
            return handle != null &&
                   !handle.IsDisposed;
        }

        private void OnDestroy()
        {
            DestroyCombatActors();
        }
    }
}