using Game.Core;
using System;
using UnityEngine;

namespace Game.Gameplay.Enemies
{
    public class EnemyBrain : MonoBehaviour
    {
        [SerializeField]
        private AttackPattern[] patterns;
        [SerializeField]
        private HitBox hitBox;
        [SerializeField]
        private Animator animator;
        [SerializeField] 
        private bool patrols = true;
        [SerializeField]
        private float moveSpeed;
        [SerializeField]
        private float detectRange;
        [SerializeField]
        private float detectHeight = 2;
        [SerializeField]
        private float loseMult = 1.5f;
        [SerializeField]
        private float staggerTime = 0.15f;
        [SerializeField]
        private float deathDelay = 0.35f;
        [SerializeField]
        private float facingDeadzone = 0.1f;
        [SerializeField]
        private float keepDistance = 0;
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
        private Transform target;
        private IPatternSelector selector;
        private Color baseColor;
        private TerrainProbe probe;

        private float facing = 1;
        private const float standMult = 1.5f;

        internal IdleState Idle { get; private set; }
        internal ChaseState Chase { get; private set; }
        internal TelegraphState Telegraph { get; private set; }
        internal AttackState Attack { get; private set; }
        internal RecoverState Recover { get; private set; }
        internal StaggerState Stagger { get; private set; }
        internal DeadState Dead { get; private set; }

        internal AttackPattern Current { get; private set; }
        internal bool IsAdvancing { get; private set; }
        internal bool Patrols => patrols;
        internal float DistanceToTarget => Mathf.Abs(target.position.x - transform.position.x);
        internal float DirectionToTarget
        {
            get
            {
                float dx = target.position.x - transform.position.x;
                return Mathf.Abs(dx) < facingDeadzone ? 0f : Mathf.Sign(dx);
            }
        }
        internal float DetectHeight => detectHeight;
        internal float HeightToTarget => Mathf.Abs(target.position.y - transform.position.y);
        internal float DetectRange => detectRange;
        internal float LoseRange => detectRange * loseMult;
        internal float LoseHeight => detectHeight * loseMult;
        internal float StaggerTime => staggerTime;
        internal float DeathDelay => deathDelay;
        internal float KeepDistance => keepDistance;
        internal float StandRange => keepDistance * standMult;
        internal EnemyState CurrentState => (EnemyState)machine.Current;
        internal HitBox HitBox => hitBox;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            impulse = GetComponent<VelocityImpulse>();
            health = GetComponent<Health>();
            target = GameObject.FindWithTag("Player")?.transform;
            machine = new StateMachine<EnemyBrain>(this);
            probe = GetComponent<TerrainProbe>();

            bool fatal = false;

            if (target == null)
            {
                Debug.LogWarning($"{gameObject.name}: 씬에 Player 태그를 가진 오브젝트가 없다 — 추적할 대상이 없다", this);
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
            if (probe == null)
            {
                Debug.LogWarning($"{gameObject.name}: TerrainProbe 가 없다 — 지형을 무시하고 발판에서 걸어 나간다", this);
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

            selector = new WeightedPatternSelector(patterns ?? Array.Empty<AttackPattern>());

            if (sprite != null)
                baseColor = sprite.color;
            else
                Debug.LogWarning($"{gameObject.name}: sprite 미지정 — 예고가 눈에 안 보인다", this);


            Idle = new IdleState(machine);
            Chase = new ChaseState(machine);
            Telegraph = new TelegraphState(machine);
            Attack = new AttackState(machine);
            Recover = new RecoverState(machine);
            Stagger = new StaggerState(machine);
            Dead = new DeadState(machine);

            if (health != null)
            {
                health.Died += OnDied;
                health.Damaged += OnDamaged;
            }
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
            selector.Tick(Time.fixedDeltaTime);
            machine.FixedTick();
            HandleMovement();
        }

        private void HandleMovement()
        {
            if (impulse.IsActive)
                return;

            float dir = CurrentState.MoveDirection;

            if (CurrentState.AllowsFacing)
                SetFacing(dir);

            if (dir == 0f || !CanAdvance(dir))
            {
                body.linearVelocityX = 0;
                IsAdvancing = false;
                return;
            }

            body.linearVelocityX = dir * moveSpeed;
            IsAdvancing = dir != 0f;
        }

        internal bool CanAdvance(float dir) => probe == null || probe.CanAdvance(dir);

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

        internal void Despawn()
        {
            Destroy(gameObject);
        }

        internal bool TrySelectPattern()
        {
            var picked = selector.Select(DistanceToTarget);

            if (picked == null)
                return false;

            if (HeightToTarget > picked.MaxHeightDiff)
                return false;

            if (picked.ForwardSpeed > 0)
            {
                float dashDistance = picked.ForwardSpeed * picked.Attack.startup / 2;
                if (probe != null && !probe.HasGroundAt(DirectionToTarget, dashDistance))
                    return false;
            }

            Current = picked;
            return true;
        }

        internal void SetTint(bool on)
        {
            if (sprite == null)
                return;
            sprite.color = on ? telegraphColor : baseColor;
        }

        private void OnDamaged()
        {
            if (CurrentState.CanBeInterrupted)
                machine.Change(Stagger);
        }

        private void OnDied()
        {
            machine.Change(Dead);
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.Died -= OnDied;
                health.Damaged -= OnDamaged;
            }
        }
    }
}
