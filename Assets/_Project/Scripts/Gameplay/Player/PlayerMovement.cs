using UnityEngine;

namespace Game.Gameplay
{
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float maxSpeed = 8f;
        [SerializeField] float groundAccel = 60f;
        [SerializeField] float groundDecel = 60f;
        [SerializeField] float airAccel = 30f;

        Rigidbody2D body;
        PlayerInputReader input;
        GroundChecker ground;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            input = GetComponent<PlayerInputReader>();
            ground = GetComponent<GroundChecker>();
        }

        void FixedUpdate()
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
    }
}