using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class IdleState : PlayerState
    {
        public IdleState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter()
        {
            Debug.Log("플레이어: Idle");
        }

        public override void FixedTick()
        {
            if (!Owner.IsGrounded)
            {
                Machine.Change(Owner.Fall);
                return;
            }

            if (Owner.HasMoveInput)
            {
                Machine.Change(Owner.Move);
                return;
            }
        }
    }
}