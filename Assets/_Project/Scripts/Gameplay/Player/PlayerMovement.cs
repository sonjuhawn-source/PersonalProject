using UnityEngine;

namespace Game.Gameplay
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float maxSpeed = 8f;
        [SerializeField] float groundAccel = 60f;
        [SerializeField] float groundDecel = 60f;
        [SerializeField] float airAccel = 30f;

        [SerializeField] float jumpHeight = 3.5f;
        [SerializeField] float fallGravityMult = 2.5f;   // 하강 중
        [SerializeField] float lowJumpGravityMult = 4f;  // 상승 중 버튼 뗌
        [SerializeField] float maxFallSpeed = 20f;

        private bool jumpRequested;
        private Rigidbody2D body;
        private PlayerInputReader input;
        private GroundChecker ground;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
            ground = GetComponent<GroundChecker>();
        }

        private void Update()
        {
            if(input.JumpPressed)
                jumpRequested = true;
        }

        private void FixedUpdate()
        {
            HandleHorizontal();
            if(jumpRequested)
            {
                HandleJump();
                jumpRequested = false;
            }
            ApplyGravity();
            ClampFallSpeed();
        }

        private void HandleHorizontal()
        {
            float accelate;
            var targetSpeed = input.MoveInput * maxSpeed;
            if (ground.IsGrounded)
            {
                if (Mathf.Abs(input.MoveInput) > 0.01f)
                {
                    accelate = groundAccel;
                }
                else
                {
                    accelate = groundDecel;
                }
            }
            else
            {
                accelate = airAccel;
            }

            accelate = Mathf.MoveTowards(body.linearVelocityX, targetSpeed, accelate * Time.fixedDeltaTime);

            body.linearVelocityX = accelate;
        }

        private void HandleJump()
        {
            if (ground.IsGrounded)
            {
                float g = Mathf.Abs(Physics2D.gravity.y);
                float v0 = Mathf.Sqrt(2 * g * jumpHeight);
                body.linearVelocityY = v0;
            }
        }

        private void ApplyGravity()
        {
            if (body.linearVelocityY > 0)             // 상승 중
                body.gravityScale = input.JumpHeld ? 1 : lowJumpGravityMult;
            else if (body.linearVelocityY < 0)        // 하강 중
                body.gravityScale = fallGravityMult;
            else
                body.gravityScale = 1;
        }

        private void ClampFallSpeed()
        {
            if (body.linearVelocityY < -maxFallSpeed)
                body.linearVelocityY = -maxFallSpeed;
        }
    }
}