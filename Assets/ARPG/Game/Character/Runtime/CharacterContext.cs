using System;
using ARPG.Game.Character.Attribute;
using ARPG.Game.Character.Movement;
using ARPG.Game.Character.StateMachine;
using ARPG.Game.Combat.Character;

namespace ARPG.Game.Character
{
    public sealed class CharacterContext
    {
        public CharacterContext(
            CharacterConfig config,
            CharacterMotor motor,
            CharacterStateMachine stateMachine,
            CharacterHealth health,
            CharacterDamageReceiver damageReceiver)
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

            Health =
                health
                ?? throw new ArgumentNullException(
                    nameof(health));

            DamageReceiver =
                damageReceiver
                ?? throw new ArgumentNullException(
                    nameof(damageReceiver));
        }

        public CharacterConfig Config { get; }

        public CharacterMotor Motor { get; }

        public CharacterStateMachine StateMachine { get; }

        public CharacterHealth Health { get; }

        public CharacterDamageReceiver DamageReceiver { get; }
    }
}