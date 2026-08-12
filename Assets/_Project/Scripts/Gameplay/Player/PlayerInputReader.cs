using UnityEngine;

namespace Game.Gameplay
{
    public class PlayerInputReader : MonoBehaviour
    {
        InputSystem_Actions actions;
        public float MoveInput => actions.Player.Move.ReadValue<Vector2>().x;
        public bool JumpPressed => actions.Player.Jump.WasPressedThisFrame();
        public bool JumpHeld => actions.Player.Jump.IsPressed();
        public bool AttackPressed => actions.Player.Attack.WasPressedThisFrame();
        public bool SwapPressed => actions.Player.Swap.WasPressedThisFrame();

        private void Awake()
        {
            actions = new InputSystem_Actions();
        }

        private void OnEnable()
        {
            actions.Player.Enable();
        }

        private void OnDisable()
        {
            actions.Player.Disable();
        }

        private void OnDestroy()
        {
            actions.Dispose();
        }
    }
}
