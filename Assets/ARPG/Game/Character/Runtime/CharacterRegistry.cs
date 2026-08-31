using System;
using System.Collections.Generic;
using ARPG.Game.Character.Player;

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
                        moveSpeed: 5f)
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