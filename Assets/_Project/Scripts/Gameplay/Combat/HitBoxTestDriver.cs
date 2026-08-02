using UnityEngine;

namespace Game.Gameplay
{
    public class HitBoxTestDriver : MonoBehaviour // 임시, #10 에서 삭제
    {
        [SerializeField]
        HitBox hitbox;

        private bool isOn = false;
        private PlayerInputReader input;
        private DamageInfo damageInfo;

        private void Awake()
        {
            input = GetComponent<PlayerInputReader>();
            damageInfo = new DamageInfo(10, Vector2.right * 5f, 0.08f, this.gameObject);
        }

        private void Update()
        {
            if (input.AttackPressed)
            {
                isOn = !isOn;
                if (isOn)
                {
                    hitbox.HitBoxActivate(damageInfo);
                }
                else
                {
                    hitbox.HitBoxDeactivate();
                }
            }
        }
    }
}
