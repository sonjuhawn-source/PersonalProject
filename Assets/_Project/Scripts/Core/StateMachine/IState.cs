namespace Game.Core
{
    public interface IState
    {
        void Enter();       
        void Exit();        
        void Tick();        // Update 주기
        void FixedTick();   // FixedUpdate 주기
    }
}
