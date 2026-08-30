using UnityEngine;

namespace ARPG.Game.Character.Player
{
    public sealed class PlayerCharacter
        : CharacterEntity
    {
        protected override void OnInitialized()
        {
            Debug.Log(
                $"PlayerCharacter initialized: " +
                $"{Context.Config.DisplayName}");
        }
    }
}