using System;
using NaughtyAttributes;
using UniRx;
using UnityEngine;
using UnityEngine.AI;

namespace VoxelCommand.Client
{
    /**
    === Animator Parameters ===
    Idle (bool)
    Walk Forward (bool)
    Walk Backward (bool)
    Crouch (bool)
    Block (bool)
    SweepTrigger (trigger)
    LowPunchTrigger (trigger)
    LowKickTrigger (trigger)
    KickTrigger (trigger)
    PunchTrigger (trigger)
    JabTrigger (trigger)
    UppercutTrigger (trigger)
    LightHitTrigger (trigger)
    KnockdownTrigger (trigger)
    Stunned (bool)
    DashForwardTrigger (trigger)
    DashBackwardTrigger (trigger)
    Intro1Trigger (trigger)
    Intro2Trigger (trigger)
    DeathTrigger (trigger)
    Victory1Trigger (trigger)
    Victory2Trigger (trigger)
    JumpTrigger (trigger)
    JumpForwardTrigger (trigger)
    JumpBackwardTrigger (trigger)
    InAir (bool)
    JumpHitReactTrigger (trigger)
    HighKickTrigger (trigger)
    HighPunchTrigger (trigger)
    Choke (trigger)
    RangeAttack1Trigger (trigger)
    RangeAttack2Trigger (trigger)
    MoveAttack1Trigger (trigger)
    MoveAttack2Trigger (trigger)
    SpecialAttack1Trigger (trigger)
    SpecialAttack2Trigger (trigger)
    CrouchBlockHitReactTrigger (trigger)
    BlockHitReactTrigger (trigger)
    ReviveTrigger (trigger)
    Run (bool)
    WalkLeft (bool)
    WalkRight (bool)
    DashLeftTrigger (trigger)
    DashRightTrigger (trigger)
    WalkSlow (bool)
    RollBackwardTrigger (trigger)
    RollForwardTrigger (trigger)
    BlockBreakTrigger (trigger)
    HighSmashTrigger (trigger)
    SmashComboTrigger (trigger)
    AxeKickTrigger (trigger)
    Combo1Trigger (trigger)
    HeavySmashTrigger (trigger)
    ForwardSmashTrigger (trigger)
    DownSmashTrigger (trigger)
**/
    [RequireComponent(typeof(Animator))]
    public class UnitAnimationController : DisposableComponent
    {
        private Unit _unit;
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;

        [SerializeField]
        private float _movementThreshold = 0.1f;

        // Animation parameter names
        // Using actual existing parameters from the animation controller
        private readonly int _walkForwardHash = Animator.StringToHash("Walk Forward");
        private readonly int _runHash = Animator.StringToHash("Run");
        private readonly int _punchTriggerHash = Animator.StringToHash("PunchTrigger");
        private readonly int _deathTriggerHash = Animator.StringToHash("DeathTrigger");
        private readonly int _lightHitTriggerHash = Animator.StringToHash("LightHitTrigger");
        private readonly int _rangeAttack1TriggerHash = Animator.StringToHash("RangeAttack1Trigger");
        private readonly int _idleHash = Animator.StringToHash("Idle");

        // Tracking animation states internally since there are no direct parameters for these
        private bool _isAttacking = false;

        private bool _isDead;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Initialize(Unit unit)
        {
            Dispose();
            _unit = unit;
            _navMeshAgent = unit.GetComponent<NavMeshAgent>();
            _isDead = false;

            SetupSubscriptions();
        }

        private void SetupSubscriptions()
        {
            // Subscribe to health changes to play death animation
            _unit.State.Health.Where(health => health <= 0 && !_isDead).Subscribe(_ => PlayDeathAnimation()).AddTo(_disposables);

            // Subscribe to health changes for hit reactions
            _unit
                .State.Health.Pairwise()
                .Where(pair => pair.Current < pair.Previous && pair.Current > 0)
                .Subscribe(_ => PlayHitReactionAnimation())
                .AddTo(_disposables);
        }

        [Button("List Animator Parameters")]
        private void ListAnimatorParameters()
        {
            if (_animator == null)
            {
                _animator = GetComponent<Animator>();
                if (_animator == null)
                {
                    Debug.LogError("Animator component not found!");
                    return;
                }
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder("=== Animator Parameters ===\n");
            foreach (AnimatorControllerParameter param in _animator.parameters)
            {
                sb.AppendLine($"Name: {param.name}, Type: {param.type}, Hash: {Animator.StringToHash(param.name)}");
            }
            sb.AppendLine("=========================");
            Debug.Log(sb.ToString());
        }

        private void Update()
        {
            if (_isDead || _unit == null || _animator == null || _navMeshAgent == null)
                return;

            // Update movement animations based on NavMeshAgent velocity
            float speed = _navMeshAgent.velocity.magnitude;
            bool isMoving = speed > _movementThreshold;

            // Update movement animations
            if (isMoving)
            {
                // Run if speed is above threshold, otherwise walk
                bool isRunning = speed > 2.5f;
                _animator.SetBool(_runHash, isRunning);
                _animator.SetBool(_walkForwardHash, !isRunning && isMoving);
                _animator.SetBool(_idleHash, false);
            }
            else
            {
                // Reset movement animations when not moving
                _animator.SetBool(_runHash, false);
                _animator.SetBool(_walkForwardHash, false);
                _animator.SetBool(_idleHash, true);
            }
        }

        [Button("Play Attack Animation")]
        public void PlayAttackAnimation()
        {
            if (_isDead)
                return;

            _animator.SetTrigger(_punchTriggerHash);
            _isAttacking = true;

            // Reset attack state after animation completes
            Observable.Timer(TimeSpan.FromSeconds(1.0f)).Subscribe(_ => _isAttacking = false).AddTo(_disposables);
        }

        [Button("Play Range Attack Animation")]
        public void PlayRangeAttackAnimation()
        {
            if (_isDead)
                return;

            _animator.SetTrigger(_rangeAttack1TriggerHash);
            _isAttacking = true;

            // Reset attack state after animation completes
            Observable.Timer(TimeSpan.FromSeconds(1.0f)).Subscribe(_ => _isAttacking = false).AddTo(_disposables);
        }

        [Button("Play Hit Reaction Animation")]
        public void PlayHitReactionAnimation()
        {
            if (_isDead)
                return;

            _animator.SetTrigger(_lightHitTriggerHash);
        }

        [Button("Play Death Animation")]
        private void PlayDeathAnimation()
        {
            _isDead = true;
            _animator.SetTrigger(_deathTriggerHash);

            // Disable NavMeshAgent when unit dies
            if (_navMeshAgent != null)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled = false;
            }
        }
    }
}
