using ARPG.Game.Character.Control;
using UnityEngine;

namespace ARPG.Game.Character
{
    public sealed class PassiveCharacterDriver
        : MonoBehaviour
    {
        private CharacterEntity _character;

        public void Bind(
            CharacterEntity character)
        {
            _character = character;
        }

        public void Unbind()
        {
            _character = null;
        }

        private void Update()
        {
            if (_character == null ||
                !_character.IsInitialized)
            {
                return;
            }

            _character.Context
                .StateMachine
                .Tick(
                    CharacterControlIntent.None,
                    Time.deltaTime);
        }
    }
}