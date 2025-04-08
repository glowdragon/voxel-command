using System.Linq;
using System.Text;
using NaughtyAttributes;
using UnityEngine;

namespace VoxelCommand.Client
{
    [RequireComponent(typeof(Animator))]
    public class UnitAnimationController : MonoBehaviour
    {
        private Unit _unit;
        private Animator _animator;

        [SerializeField]
        private float _movementThreshold = 0.1f;

        // Animator parameters
        private readonly int _idleHash = Animator.StringToHash("Idle");
        private readonly int _walkForwardHash = Animator.StringToHash("Walk Forward");
        private readonly int _runHash = Animator.StringToHash("Run");
        private readonly int _punchTriggerHash = Animator.StringToHash("PunchTrigger");
        private readonly int _lightHitTriggerHash = Animator.StringToHash("LightHitTrigger");
        private readonly int _deathTriggerHash = Animator.StringToHash("DeathTrigger");
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
        }

        private void Update()
        {
            if (IsDead)
                return;

            // Update movement animations based on NavMeshAgent velocity
            float speed = _unit.Controller.NavMeshAgent.velocity.magnitude;
            bool isMoving = speed > _movementThreshold;

            // Update movement animations
            if (isMoving)
            {
                // Run if speed is above threshold, otherwise walk
                bool isRunning = speed > 0.5f;
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
            if (IsDead)
                return;

            _animator.SetTrigger(_punchTriggerHash);
        }

        [Button("Play Hit Reaction Animation")]
        public void PlayHitReactionAnimation()
        {
            if (IsDead)
                return;

            _animator.SetTrigger(_lightHitTriggerHash);
        }

        [Button("Play Victory Animation")]
        public void PlayVictoryAnimation()
        {
            if (IsDead)
                return;

            _animator.SetTrigger(Random.value > 0.5f ? _victory1TriggerHash : _victory2TriggerHash);
        }

        [Button("Play Death Animation")]
        public void PlayDeathAnimation()
        {
            // if (!IsDead)
            //     return;

            _animator.SetTrigger(_deathTriggerHash);
        }

        [Button("Play Revive Animation")]
        public void PlayReviveAnimation()
        {
            // Ensure all other animation states are reset
            _animator.SetBool(_runHash, false);
            _animator.SetBool(_walkForwardHash, false);

            // Play the revive animation
            _animator.SetTrigger(_reviveTriggerHash);
        }

        [Button("List Animator Parameters")]
        private void ListAnimatorParameters()
        {
            if (IsDead)
                return;

            var animator = GetComponent<Animator>();
            Debug.Log("Parameters\n" + string.Join("\n", animator.parameters.Select(p => $"- {p.name} ({p.type})")));
        }
    }
}
