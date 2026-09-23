using System;
using ARPG.Game.Character.Movement;
using UnityEngine;

namespace ARPG.Game.Character.Player.Input
{
    public sealed class PlayerInputDriver
        : MonoBehaviour
    {
        [SerializeField]
        private Camera _movementCamera;

        private PlayerInputReader _inputReader;

        private PlayerMovementIntentBuilder
            _movementIntentBuilder;

        private CharacterEntity _character;

        private void Awake()
        {
            if (_movementCamera == null)
            {
                _movementCamera =
                    Camera.main;
            }

            if (_movementCamera == null)
            {
                throw new InvalidOperationException(
                    "Player movement camera is missing.");
            }

            _inputReader =
                new PlayerInputReader();

            _movementIntentBuilder =
                new PlayerMovementIntentBuilder(
                    _movementCamera.transform);
        }

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

            Vector2 input =
                _inputReader.ReadMovement();

            CharacterMovementIntent movementIntent =
                _movementIntentBuilder.Build(
                    input);

            _character.Context
                .StateMachine
                .Tick(
                    movementIntent,
                    Time.deltaTime);
        }
    }
}