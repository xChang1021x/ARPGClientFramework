using ARPG.Game.Character.Control;

namespace ARPG.Game.Character.StateMachine
{
    /// <summary>
    /// Character运行时状态。
    ///
    /// 每个状态负责：
    /// 1. Enter；
    /// 2. Tick；
    /// 3. Exit；
    /// 4. 根据当前运行时条件决定状态切换。
    /// </summary>
    public interface ICharacterState
    {
        void Enter();

        void Tick(
            CharacterControlIntent intent,
            float deltaTime);

        void Exit();
    }
}