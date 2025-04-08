using UnityEngine;
using UnityEngine.InputSystem;

namespace VoxelCommand.Client
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        private Camera _mainCamera;

        [SerializeField, Tooltip("Layer for selectable units")]
        private LayerMask _unitLayerMask;

        [SerializeField, Tooltip("Layer for walkable ground")]
        private LayerMask _groundLayerMask;

        private UnitController _selectedUnitController;
        private Unit _selectedUnit;

        private void Awake()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            
            // Clear selection initially
            _selectedUnitController = null;
            _selectedUnit = null;
        }

        private void Update()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleLeftClick();
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                HandleRightClick();
            }
        }

        private void HandleLeftClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, _unitLayerMask))
            {
                // Check if the hit object has a Unit component and belongs to the Player team
                Unit unit = hit.collider.GetComponentInParent<Unit>(); // Get Unit component from parent if collider is child
                if (unit != null && unit.Team == Team.Player)
                {
                    SelectUnit(unit);
                }
                else
                {
                    DeselectUnit();
                }
            }
            else
            {
                // Clicked somewhere else, deselect
                DeselectUnit();
            }
        }

        private void HandleRightClick()
        {
            if (_selectedUnitController == null)
                return;

            Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 200f, _groundLayerMask))
            {
                _selectedUnitController.ManualMoveToPosition(hit.point);
            }
        }

        private void SelectUnit(Unit unit)
        {
            if (_selectedUnit != null && _selectedUnit != unit)
            {
                _selectedUnit.IsSelected.Value = false;
            }

            _selectedUnit = unit;
            _selectedUnitController = unit.Controller;
            _selectedUnit.IsSelected.Value = true;
        }

        private void DeselectUnit()
        {
            if (_selectedUnit != null)
            {
                _selectedUnit.IsSelected.Value = false;
            }

            _selectedUnitController = null;
            _selectedUnit = null;
        }
    }
}
