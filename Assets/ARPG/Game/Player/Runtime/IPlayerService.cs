namespace ARPG.Game.Player
{
    public interface IPlayerService
    {
        int MaxHealth { get; }

        int Attack { get; }

        float MoveSpeed { get; }
    }
}