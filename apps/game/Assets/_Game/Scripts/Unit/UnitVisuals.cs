using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelCommand.Client
{
    public class UnitVisuals : DisposableComponent
    {
        [SerializeField]
        private List<Renderer> _renderers = new();

        [SerializeField]
        private List<Material> _playerTeamMaterials = new();

        [SerializeField]
        private List<Material> _enemyTeamMaterials = new();

        [SerializeField]
        private TextMeshProUGUI _levelText;

        [SerializeField]
        private Image _healthFillImage;
        
        [SerializeField]
        private GameObject _worldspaceCanvas;
        
        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private QuickOutline _outline;

        private Unit _unit;
        private Transform _cameraTransform;

        public void Initialize(Unit unit)
        {
            _unit = unit;
            _cameraTransform = Camera.main.transform;

            ApplyTeamVisuals();
            SetupNameDisplay();
            SetupSubscriptions();
        }

        public void SetSelected(bool selected)
        {
            _outline.enabled = selected;
        }

        private void SetupSubscriptions()
        {
            // We're not setting up the health and level subscriptions
            // since they'll be displayed in the HUD instead
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
                    _cameraTransform.rotation * Vector3.up);
            }
        }

        private void UpdateHealthBar()
        {
            // We're not updating the health bar since it'll be displayed in the HUD
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
