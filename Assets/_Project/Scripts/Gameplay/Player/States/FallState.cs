using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class FallState : PlayerState
    {
        public FallState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter() 
        {
            Debug.Log("플레이어: Fall");
        }

        public override void FixedTick()
        {
            if (Owner.IsGrounded)
            {
                if (Owner.HasMoveInput)
                {
                    Machine.Change(Owner.Move);
                }
                else
                {
                    Machine.Change(Owner.Idle);
                }
            }
        }
    }
}