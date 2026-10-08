using System;
using System.Collections.Generic;
using ARPG.Game.Character.Player;
using ARPG.Game.Character.TrainingDummy;

namespace ARPG.Game.Character
{
    /// <summary>
    /// Character类型到CharacterConfig的集中注册表。
    /// </summary>
    public static class CharacterRegistry
    {
        private static readonly Dictionary<Type, CharacterConfig>
            Configs = new()
            {
                {
                    typeof(PlayerCharacter),
                    new CharacterConfig(
                        "ARPG/Character/Player",
                        "Player",
                        moveSpeed: 5f,
                        gravity: -20f,
                        rotationSpeed: 720f,
                        maxHealth: 100,
                        hitRecoveryDuration: 0.2f,
                        attackDamage: 20,
                        attackDuration: 0.5f,
                        attackHitTime: 0.2f,
                        attackRange: 1.5f,
                        attackRadius: 0.8f)
                },
                {
                    typeof(TrainingDummyCharacter),
                    new CharacterConfig(
                        "ARPG/Character/Dummy",
                        "TrainingDummy",
                        moveSpeed: 5f,
                        gravity: -20f,
                        rotationSpeed: 720f,
                        maxHealth: 100,
                        hitRecoveryDuration: 0.2f,
                        attackDamage: 20,
                        attackDuration: 0.5f,
                        attackHitTime: 0.2f,
                        attackRange: 1.5f,
                        attackRadius: 0.8f)
                }
            };

        public static CharacterConfig Get<TCharacter>()
            where TCharacter : CharacterEntity
        {
            return Get(
                typeof(TCharacter));
        }

        public static CharacterConfig Get(
            Type characterType)
        {
            if (characterType == null)
            {
                throw new ArgumentNullException(
                    nameof(characterType));
            }

            if (!Configs.TryGetValue(
                    characterType,
                    out CharacterConfig config))
            {
                throw new InvalidOperationException(
                    $"Character config for " +
                    $"'{characterType.Name}' " +
                    "has not been registered.");
            }

            return config;
        }
    }
}