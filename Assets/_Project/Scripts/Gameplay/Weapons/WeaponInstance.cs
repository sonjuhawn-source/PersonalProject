namespace Game.Gameplay
{
    public class WeaponInstance 
    {
        internal readonly WeaponData Data;

        private float swapCooldownRemaining;

        internal WeaponInstance(WeaponData data)
        {
            Data = data;
        }

        internal bool IsSwapReady => swapCooldownRemaining <= 0;

        internal void Tick(float dt)
        {
            if(swapCooldownRemaining >0)
                swapCooldownRemaining -= dt;
        }

        internal void StartSwapCooldown()
        {
            swapCooldownRemaining = Data.SwapCooldown;
        }
    }
}
