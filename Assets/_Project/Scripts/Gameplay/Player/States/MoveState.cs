using Game.Core;
using UnityEngine;

namespace Game.Gameplay
{
    public class MoveState : PlayerState
    {
        public MoveState(StateMachine<PlayerMovement> machine) : base(machine) { }

        public override void Enter() 
        {
            Debug.Log("플레이어: Move");
        }

        public override void FixedTick()
        {
            if (!Owner.IsGrounded)
            {
                Machine.Change(Owner.Fall);
                return;
            }

            if (!Owner.HasMoveInput)
            {
                Machine.Change(Owner.Idle);
                return;
            }
        }
    }
}