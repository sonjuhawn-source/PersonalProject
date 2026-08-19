using Game.Core;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class EnemyBrain : MonoBehaviour
    {
        [SerializeField]
        private AttackPattern[] patterns;
        [SerializeField]
        private Transform target;
        [SerializeField]
        private HitBox hitBox;
        [SerializeField]
        private Animator animator;
        [SerializeField]
        private float moveSpeed;
        [SerializeField]
        private float detectRange;
        [SerializeField] 
        private SpriteRenderer sprite;
        [SerializeField] 
        private Color telegraphColor = new Color(1f, 0.78f, 0.78f, 1f);

        [SerializeField]
        Transform muzzle;

        private Rigidbody2D body;
        private VelocityImpulse impulse;
        private Health health;
        private StateMachine<EnemyBrain> machine;
        private Color baseColor;

        private float facing = 1;

        internal IdleState Idle { get; private set; }
        internal ChaseState Chase { get; private set; }
        internal TelegraphState Telegraph { get; private set; }
        internal AttackState Attack { get; private set; }
        internal RecoverState Recover { get; private set; }
        internal DeadState Dead { get; private set; }

        internal AttackPattern Current { get; private set; }
        internal float DistanceToTarget => Mathf.Abs(target.position.x - transform.position.x);
        internal float DirectionToTarget => Mathf.Sign(target.position.x - transform.position.x);
        internal float DetectRange => detectRange;
        internal EnemyState CurrentState => (EnemyState)machine.Current;
        internal HitBox HitBox => hitBox;   

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            impulse = GetComponent<VelocityImpulse>();
            health = GetComponent<Health>();

            machine = new StateMachine<EnemyBrain>(this);

            bool fatal = false;

            if (target == null)
            {
                Debug.LogWarning($"{gameObject.name}: target 미지정 — 추적할 대상이 없다", this);
                fatal = true;
            }
            if (body == null)
            {
                Debug.LogWarning($"{gameObject.name}: Rigidbody2D 가 없다 — 이동을 적용할 수 없다", this);
                fatal = true;
            }
            if (impulse == null)
            {
                Debug.LogWarning($"{gameObject.name}: VelocityImpulse 가 없다 — 전진과 넉백을 처리할 수 없다", this);
                fatal = true;
            }

            if (fatal)
            {
                Debug.LogWarning($"{gameObject.name}: 위 문제로 EnemyBrain 을 껐다", this);
                enabled = false;
                return;
            }

            if (patterns == null || patterns.Length == 0)
                Debug.LogWarning($"{gameObject.name}: patterns 가 비었다 — 공격 패턴이 없어 Chase 에서 멈춘다", this);

            if (animator == null)
                Debug.LogWarning($"{gameObject.name}: animator 미지정 — 예고·공격 모션이 안 나온다", this);

            if (hitBox == null)
                Debug.LogWarning($"{gameObject.name}: hitBox 미지정 — 근접 공격 판정이 안 나간다", this);

            if ((patterns != null && patterns.Length > 0) && animator != null)
            {
                for (int i = 0; i < patterns.Length; i++)
                {
                    var pattern = patterns[i];
                    if (pattern == null)
                    {
                        Debug.LogWarning($"{gameObject.name}: patterns[{i}] 가 비어 있다 — 그 칸은 선택되지 않는다", this);
                        continue;
                    }
                    if (animator.HasState(0, Animator.StringToHash(pattern.TelegraphStateName)) == false)
                        Debug.LogWarning($"{pattern.name}: 예고 상태 '{pattern.TelegraphStateName}' 이 컨트롤러에 없다. 예고 모션이 안 나온다", this);
                    if (animator.HasState(0, Animator.StringToHash(pattern.AttackStateName)) == false)
                        Debug.LogWarning($"{pattern.name}: 공격 상태 '{pattern.AttackStateName}' 이 없다. 공격 모션이 안 나온다", this);
                    if (pattern.Kind == AttackKind.Projectile && pattern.ProjectilePrefab == null)
                        Debug.LogWarning($"{pattern.name}: 원거리인데 투사체 프리팹이 없다. 공격해도 아무것도 안 나간다", this);
                }
            }

            if (sprite != null)
                baseColor = sprite.color;
            else
                Debug.LogWarning($"{gameObject.name}: sprite 미지정 — 예고가 눈에 안 보인다", this);


            Idle = new IdleState(machine);
            Chase = new ChaseState(machine);
            Telegraph = new TelegraphState(machine);
            Attack = new AttackState(machine);
            Recover = new RecoverState(machine);
            Dead = new DeadState(machine);

            if (health != null)
                health.Died += OnDied;
            else
                Debug.LogWarning($"{gameObject.name}: Health 가 없다 — 죽어도 Dead 로 못 간다", this);


            machine.Change(Idle);
        }

        private void Update()
        {
            machine.Tick();
        }

        private void FixedUpdate()
        {
            machine.FixedTick();
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (impulse.IsActive)
                return;
            if (CurrentState.AllowsFacing)
                SetFacing(DirectionToTarget);
            if (!CurrentState.AllowsMovement)
            {
                body.linearVelocityX = 0;
                return;
            }
            body.linearVelocityX = DirectionToTarget * moveSpeed;
        }

        internal void SetFacing(float dir)
        {
            if (dir == 0)
                return;
            facing = dir;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * dir;
            transform.localScale = s;
        }

        internal void PlayClip(string stateName, float speed = 1)
        {
            if (animator == null)
                return;

            animator.speed = speed;
            animator.Play(stateName, -1, 0f);
        }

        internal DamageInfo BuildDamageInfo(AttackData data)
        {
            return new DamageInfo(data.damage,
               Vector2.right * facing * data.knockbackForce,
               data.hitStopTime,
               gameObject,
               data.shakeStrength);
        }

        internal void ApplyForward(float duration)
        {
            if (Current.ForwardSpeed <= 0)
                return;

            impulse.Apply(Vector2.right * facing * Current.ForwardSpeed, duration);
        }

        internal void FireProjectile(AttackData data)
        {
            var p = Instantiate(Current.ProjectilePrefab, muzzle.position, Quaternion.identity);
            p.Init(BuildDamageInfo(data), facing);
        }

        internal void Stop()
        {
            body.linearVelocityX = 0;
            hitBox?.HitBoxDeactivate();
        }

        internal bool TrySelectPattern()
        {
            var distance = DistanceToTarget;
            foreach (var pattern in patterns)
            {
                if (pattern == null)
                    continue;
                if (distance < pattern.MinRange)
                    continue;
                if (distance > pattern.MaxRange)
                    continue;
                Current = pattern;
                return true;
            }
            return false;
        }

        internal void SetTint(bool on)
        {
            if (sprite == null)
                return;
            sprite.color = on ? telegraphColor : baseColor;
        }

        private void OnDied()
        {
            machine.Change(Dead);
        }

        private void OnDestroy()
        {
            if (health != null)
                health.Died -= OnDied;
        }
    }
}
