namespace Game.Core
{
    public abstract class BaseState<TOwner> : IState
    {
        protected TOwner Owner { get; }
        protected StateMachine<TOwner> Machine { get; }

        protected BaseState(StateMachine<TOwner> machine)
        {
            Machine = machine;
            Owner = machine.Owner;
        }

        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Tick() { }
        public virtual void FixedTick() { }
    }
}
