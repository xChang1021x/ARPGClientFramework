using UnityEngine;
using UnityEngine.InputSystem;

namespace ARPG.Game.GameCamera
{
    /// <summary>
    /// 读取第三人称镜头输入。
    /// 不负责修改Camera Transform。
    /// </summary>
    public sealed class ThirdPersonCameraInputReader
    {
        public Vector2 ReadLookDelta()
        {
            Mouse mouse =
                Mouse.current;

            if (mouse == null)
            {
                return Vector2.zero;
            }

            return mouse.delta.ReadValue();
        }
    }
}