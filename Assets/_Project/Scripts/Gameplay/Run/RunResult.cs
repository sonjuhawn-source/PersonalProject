namespace Game.Gameplay.Run
{
    internal enum RunOutcome
    {
        Cleared,
        Died,
    }

    // 런이 끝난 시점의 값만 담는다. 끝난 뒤에는 안 바뀌므로 읽기 전용이다.
    internal readonly struct RunResult
    {
        internal readonly RunOutcome Outcome;
        internal readonly int Floor;
        internal readonly int Kills;
        internal readonly int Health;

        internal RunResult(RunOutcome outcome, int floor, int kills, int health)
        {
            Outcome = outcome;
            Floor = floor;
            Kills = kills;
            Health = health;
        }
    }
}
