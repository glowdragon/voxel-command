using UnityEngine;

namespace VoxelCommand.Client
{
    [RequireComponent(typeof(Collider))]
    public class AttackTrigger : MonoBehaviour
    {
        [SerializeField]
        private UnitController _unitController;

        private void Awake()
        {
            // Ensure the collider is set as a trigger
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            // Check if we hit a vulnerable collider
            var opponent = other.GetComponentInParent<Unit>();
            if (opponent == null || opponent == _unitController.Self)
                return;

            // Check if the unit is vulnerable (has a vulnerable collider)
            if (opponent.VulnerableCollider != other)
                return;

            // Check if we're in combat and this is our target
            if (!_unitController.IsInCombat || opponent != _unitController.Opponent)
                return;

            var unit = _unitController.Self;
            // Debug.Log($"[AttackTrigger] {unit.name} attacks {opponent.name}");

            // Notify the controller about the hit
            _unitController.OnAttackHit(opponent);
        }

        private void OnTriggerStay(Collider other)
        {
            // Debug.Log($"[AttackTrigger] OnTriggerStay: {other.gameObject.name}");
        }

        private void OnTriggerExit(Collider other)
        {
            // Debug.Log($"[AttackTrigger] OnTriggerExit: {other.gameObject.name}");
        }
    }
}
