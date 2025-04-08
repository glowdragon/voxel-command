using System.Collections.Generic;
using DanielKreitsch;
using TMPro;
using UnityEngine;

namespace VoxelCommand.Client
{
    public class UnitVisuals : DisposableMonoBehaviour
    {
        [SerializeField]
        private List<Renderer> _renderers = new();

        [SerializeField]
        private List<Material> _playerTeamMaterials = new();

        [SerializeField]
        private List<Material> _enemyTeamMaterials = new();

        [SerializeField]
        private GameObject _worldspaceCanvas;

        [SerializeField]
        private TextMeshProUGUI _nameText;

        private Unit _unit;
        private Transform _cameraTransform;

        public void Initialize(Unit unit)
        {
            _unit = unit;
            _cameraTransform = Camera.main.transform;

            ApplyTeamVisuals();
            SetupNameDisplay();
        }

        public void SetSelected(bool selected)
        {
            foreach (var renderer in _renderers)
            {
                if (renderer.TryGetComponent<QuickOutline>(out var outline))
                {
                    outline.enabled = selected;
                }
            }
        }

        private void SetupNameDisplay()
        {
            if (_worldspaceCanvas != null && _nameText != null)
            {
                _worldspaceCanvas.SetActive(true);
                _nameText.text = _unit.name;
            }
        }

        private void LateUpdate()
        {
            if (_worldspaceCanvas != null && _cameraTransform != null)
            {
                // Billboard the canvas to always face the camera
                _worldspaceCanvas.transform.LookAt(
                    _worldspaceCanvas.transform.position + _cameraTransform.rotation * Vector3.forward,
                    _cameraTransform.rotation * Vector3.up
                );
            }
        }

        private void ApplyTeamVisuals()
        {
            if (_renderers == null || _renderers.Count == 0)
                return;

            List<Material> materialsToApply = _unit.Team == Team.Player ? _playerTeamMaterials : _enemyTeamMaterials;

            // Apply team-specific materials
            for (int i = 0; i < _renderers.Count && i < materialsToApply.Count; i++)
            {
                if (_renderers[i] != null && materialsToApply[i] != null)
                {
                    _renderers[i].material = materialsToApply[i];
                }
            }
        }
    }
}
