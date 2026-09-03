using System;
using ARPG.Game.Character.Movement;
using UnityEngine;

namespace ARPG.Game.Character.Player.Input
{
    /// <summary>
    /// Unity Update边界。
    ///
    /// 负责：
    /// PlayerInputReader
    /// →
    /// CharacterMovementIntent
    /// →
    /// CharacterMotor.Tick。
    ///
    /// 不直接修改Transform。
    /// </summary>
    public sealed class PlayerInputDriver
        : MonoBehaviour
    {
        private PlayerInputReader _inputReader;
        private CharacterEntity _character;

        public void Bind(
            CharacterEntity character)
        {
            if (character == null)
            {
                throw new ArgumentNullException(
                    nameof(character));
            }

            if (!character.IsInitialized)
            {
                throw new InvalidOperationException(
                    "Cannot bind an uninitialized character.");
            }

            _character =
                character;

            _inputReader ??=
                new PlayerInputReader();
        }

        public void Unbind()
        {
            _character =
                null;
        }

        private void Update()
        {
            if (_character == null ||
                !_character.IsInitialized)
            {
                return;
            }

            Vector2 movement =
                _inputReader.ReadMovement();

            var intent =
                new CharacterMovementIntent(
                    movement);

            _character.Context
                .Motor
                .Tick(
                    intent,
                    Time.deltaTime);
        }
    }
}