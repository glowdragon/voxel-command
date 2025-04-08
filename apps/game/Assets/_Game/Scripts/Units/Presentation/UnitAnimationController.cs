using NaughtyAttributes;
using UnityEngine;
using UnityEngine.AI;

namespace VoxelCommand.Client
{
    [RequireComponent(typeof(Animator))]
    public class UnitAnimationController : MonoBehaviour
    {
        private Unit _unit;
        private Animator _animator;
        private NavMeshAgent _navMeshAgent;

        [SerializeField]
        private float _movementThreshold = 0.1f;

        // Animator parameters
        private readonly int _walkForwardHash = Animator.StringToHash("Walk Forward");
        private readonly int _runHash = Animator.StringToHash("Run");
        private readonly int _punchTriggerHash = Animator.StringToHash("PunchTrigger");
        private readonly int _deathTriggerHash = Animator.StringToHash("DeathTrigger");
        private readonly int _lightHitTriggerHash = Animator.StringToHash("LightHitTrigger");
        private readonly int _rangeAttack1TriggerHash = Animator.StringToHash("RangeAttack1Trigger");
        private readonly int _idleHash = Animator.StringToHash("Idle");
        private readonly int _reviveTriggerHash = Animator.StringToHash("ReviveTrigger");
        private readonly int _victory1TriggerHash = Animator.StringToHash("Victory1Trigger");
        private readonly int _victory2TriggerHash = Animator.StringToHash("Victory2Trigger");

        public bool IsDead => _unit.State.IsDead;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void Initialize(Unit unit)
        {
            _unit = unit;
            _navMeshAgent = unit.GetComponent<NavMeshAgent>();
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
            if (IsDead)
                return;

            // Update movement animations based on NavMeshAgent velocity
            float speed = _navMeshAgent.velocity.magnitude;
            bool isMoving = speed > _movementThreshold;

            // Update movement animations
            if (isMoving)
            {
                // Run if speed is above threshold, otherwise walk
                bool isRunning = speed > 1.5f;
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
            _animator.SetTrigger(_punchTriggerHash);
        }

        [Button("Play Range Attack Animation")]
        public void PlayRangeAttackAnimation()
        {
            _animator.SetTrigger(_rangeAttack1TriggerHash);
        }

        [Button("Play Hit Reaction Animation")]
        public void PlayHitReactionAnimation()
        {
            _animator.SetTrigger(_lightHitTriggerHash);
        }

        [Button("Play Death Animation")]
        public void PlayDeathAnimation()
        {
            _animator.SetTrigger(_deathTriggerHash);

            // Disable NavMeshAgent when unit dies
            if (_navMeshAgent != null)
            {
                _navMeshAgent.isStopped = true;
                _navMeshAgent.enabled = false;
            }
        }

        /// <summary>
        /// Reset the death state and play the revive animation if available
        /// </summary>
        public void PlayReviveAnimation()
        {
            // Try to play a revive animation if it exists
            try
            {
                _animator.SetTrigger(_reviveTriggerHash);
            }
            catch (System.Exception)
            {
                // If revive animation doesn't exist, just set to idle
                _animator.SetBool(_idleHash, true);
            }

            // Ensure all other animation states are reset
            _animator.SetBool(_runHash, false);
            _animator.SetBool(_walkForwardHash, false);
        }

        /// <summary>
        /// Play a victory dance animation
        /// </summary>
        public void PlayVictoryAnimation()
        {
            // Randomly choose between the two victory animations
            bool useFirstVictoryAnim = UnityEngine.Random.value > 0.5f;

            try
            {
                if (useFirstVictoryAnim)
                {
                    _animator.SetTrigger(_victory1TriggerHash);
                }
                else
                {
                    _animator.SetTrigger(_victory2TriggerHash);
                }
            }
            catch (System.Exception)
            {
                // If victory animations don't exist, just set to idle
                _animator.SetBool(_idleHash, true);
            }
        }
    }
}
