namespace Game.Core
{
    public class StateMachine<TOwner>
    {
        public TOwner Owner { get; }
        public IState Current { get; private set; }
        public StateMachine(TOwner owner)
        {
            Owner = owner;
        }

        public void Change(IState next)
        {
            if (Current == next)
                return;

            Current?.Exit();
            Current = next;
            Current.Enter();
        }
        public void Tick()
        {
            if (Current == null)
                return;

            Current.Tick();
        }
        public void FixedTick()
        {
            if (Current == null)
                return;

            Current.FixedTick();
        }
        public string CurrentName => $"{Current}"; // 디버그용 — 상태 클래스 이름
    }
}
