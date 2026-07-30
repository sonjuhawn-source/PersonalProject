using UnityEngine;

namespace Game.Gameplay
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField]
        private float maxSpeed = 8f;
        [SerializeField]
        private float groundAccel = 60f;
        [SerializeField]
        private float groundDecel = 60f;
        [SerializeField]
        private float airAccel = 30f;

        [SerializeField]
        private float jumpHeight = 3.5f;
        [SerializeField]
        private float fallGravityMult = 2.5f;   // 하강 중
        [SerializeField]
        private float lowJumpGravityMult = 4f;  // 상승 중 버튼 뗌
        [SerializeField]
        private float maxFallSpeed = 20f;
        [SerializeField]
        private float jumpBufferTime = 0.15f;
        [SerializeField]
        private float coyoteTime = 0.1f;

        private float jumpPressedTime = float.NegativeInfinity;
        private float leaveGroundTime = float.NegativeInfinity;
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
            if (input.JumpPressed)
                jumpPressedTime = Time.time;
        }

        private void FixedUpdate()
        {
            if (ground.IsGrounded && body.linearVelocityY <= 0f)
                leaveGroundTime = Time.time;

            HandleHorizontal();
            float window = Mathf.Max(jumpBufferTime, Time.fixedDeltaTime);  //Update와 FixedUpdate의 간격을 메꾸기 위한 장치
            if (Time.time - jumpPressedTime <= window)
            {
                if (TryJump())
                    jumpPressedTime = float.NegativeInfinity;
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

        private bool TryJump()
        {
            if (body.linearVelocityY > 0f)
                return false;

            if (!ground.IsGrounded && (Time.time - leaveGroundTime) > coyoteTime)
                return false;

            float g = Mathf.Abs(Physics2D.gravity.y);
            body.linearVelocityY = Mathf.Sqrt(2 * g * jumpHeight);
            leaveGroundTime = float.NegativeInfinity;
            return true;
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