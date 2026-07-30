// 검증용 임시 파일. #7 에서 실제 플레이어 상태로 대체하며 삭제한다.
using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class StateMachineSandbox : MonoBehaviour
    {
        StateMachine<StateMachineSandbox> machine;
        PingState ping;
        PongState pong;

        void Awake()
        {
            machine = new StateMachine<StateMachineSandbox>(this);
            ping = new PingState(machine);
            pong = new PongState(machine);
        }

        void Start()
        {
            machine.Change(ping);
            machine.Change(ping);
            machine.Change(pong);
        }

        void Update()
        {
            machine.Tick();
        }
        void FixedUpdate()
        {
            machine.FixedTick();
        }
    }

    class PingState : BaseState<StateMachineSandbox>
    {
        int tickCount;

        public PingState(StateMachine<StateMachineSandbox> m) : base(m) { }

        public override void Enter() { Debug.Log("Ping Enter"); }
        public override void Exit() { Debug.Log($"Ping Exit — tick {tickCount}"); }
        public override void Tick() { tickCount++; }
        public override void FixedTick() { }
    }

    class PongState : BaseState<StateMachineSandbox>
    {
        int tickCount;

        public PongState(StateMachine<StateMachineSandbox> m) : base(m) { }

        public override void Enter() { Debug.Log("Pong Enter"); }
        public override void Exit() { Debug.Log($"Pong Exit — tick {tickCount}"); }
        public override void Tick() { tickCount++; }
        public override void FixedTick() { }
    }
}