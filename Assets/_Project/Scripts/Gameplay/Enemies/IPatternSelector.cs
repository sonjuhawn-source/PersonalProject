namespace Game.Gameplay.Enemies
{
    internal interface IPatternSelector
    {
        void Tick(float deltaTime);
        AttackPattern Select(float distance);
    }
}
