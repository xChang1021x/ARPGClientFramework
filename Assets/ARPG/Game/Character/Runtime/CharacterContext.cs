using System;
using ARPG.Game.Character.Movement;
using ARPG.Game.Character.StateMachine;

namespace ARPG.Game.Character
{
    public sealed class CharacterContext
    {
        public CharacterContext(
            CharacterConfig config,
            CharacterMotor motor,
            CharacterStateMachine stateMachine)
        {
            Config = config;

            Motor =
                motor
                ?? throw new ArgumentNullException(
                    nameof(motor));

            StateMachine =
                stateMachine
                ?? throw new ArgumentNullException(
                    nameof(stateMachine));
        }

        public CharacterConfig Config { get; }

        public CharacterMotor Motor { get; }

        public CharacterStateMachine StateMachine { get; }
    }
}