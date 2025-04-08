using UnityEngine;
using UnityEngine.InputSystem; // Using the new Input System

namespace VoxelCommand.Client
{
    public class PlayerInputController : MonoBehaviour
    {
        [SerializeField]
        private Camera _mainCamera;

        [SerializeField]
        private LayerMask _unitLayerMask; // Layer for selectable units (Player team)

        [SerializeField]
        private LayerMask _groundLayerMask; // Layer for walkable ground (NavMesh)

        private UnitController _selectedUnitController;
        private Unit _selectedUnit; // Keep track of the selected Unit too

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
            HandleLeftClick();
            HandleRightClick();
        }

        private void HandleLeftClick()
        {
            // Check if the left mouse button was clicked this frame
            if (Mouse.current.leftButton.wasPressedThisFrame)
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
        }

        private void HandleRightClick()
        {
            // Check if a unit is selected and the right mouse button was clicked
            if (_selectedUnitController != null && Mouse.current.rightButton.wasPressedThisFrame)
            {
                Ray ray = _mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
                // Raycast against the ground layer
                if (Physics.Raycast(ray, out RaycastHit hit, 200f, _groundLayerMask))
                {
                    // Issue a manual move command to the selected unit
                    Debug.Log($"Commanding {_selectedUnit.name} to move to {hit.point}");
                     _selectedUnitController.ManualMoveToPosition(hit.point);

                    // Optional: Deselect unit after issuing command?
                    // DeselectUnit();
                }
            }
        }

        private void SelectUnit(Unit unit)
        {
            if (unit.Controller != null)
            {
                 // Deselect previous unit
                if (_selectedUnit != null && _selectedUnit != unit)
                {
                     _selectedUnit.IsSelected = false; // Set IsSelected to false for the old unit
                    Debug.Log($"Deselected {_selectedUnit.name}");
                }

                // Select the new unit
                _selectedUnit = unit;
                _selectedUnitController = unit.Controller;
                _selectedUnit.IsSelected = true; // Set IsSelected to true for the new unit
                Debug.Log($"Selected {_selectedUnit.name}");
            }
            else {
                Debug.LogWarning($"Clicked unit {unit.name} has no UnitController.");
                DeselectUnit();
            }
        }

        private void DeselectUnit()
        {
            // Deselect previous unit
            if (_selectedUnit != null)
            {
                _selectedUnit.IsSelected = false; // Set IsSelected to false
                Debug.Log($"Deselected {_selectedUnit.name}");
            }

            _selectedUnitController = null;
            _selectedUnit = null;
        }
    }
} 