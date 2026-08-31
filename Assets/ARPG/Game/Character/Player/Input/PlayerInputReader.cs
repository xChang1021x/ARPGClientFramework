using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPG.Game.Character.Player.Input
{
    /// <summary>
    /// 读取本地玩家物理输入。
    ///
    /// 只负责：
    /// Keyboard / Gamepad
    /// →
    /// 标准化输入值。
    ///
    /// 不知道CharacterEntity，
    /// 不修改Transform。
    /// </summary>
    public sealed class PlayerInputReader
    {
        public Vector2 ReadMovement()
        {
            Vector2 movement =
                Vector2.zero;

            Keyboard keyboard =
                Keyboard.current;

            if (keyboard != null)
            {
                if (keyboard.wKey.isPressed)
                {
                    movement.y += 1f;
                }

                if (keyboard.sKey.isPressed)
                {
                    movement.y -= 1f;
                }

                if (keyboard.dKey.isPressed)
                {
                    movement.x += 1f;
                }

                if (keyboard.aKey.isPressed)
                {
                    movement.x -= 1f;
                }
            }

            Gamepad gamepad =
                Gamepad.current;

            if (gamepad != null)
            {
                Vector2 gamepadMovement =
                    gamepad.leftStick.ReadValue();

                /*
                 * 当前第一版：
                 * Gamepad存在输入时优先使用摇杆。
                 */
                if (gamepadMovement.sqrMagnitude >
                    0.0001f)
                {
                    movement =
                        gamepadMovement;
                }
            }

            return Vector2.ClampMagnitude(
                movement,
                1f);
        }
    }
}