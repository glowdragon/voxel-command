using System.Collections.Generic;
using DanielKreitsch;
using TMPro;
using UnityEngine;

namespace VoxelCommand.Client
{
    public class UnitVisuals : DisposableMonoBehaviour
    {
        [SerializeField]
        private UnitAnimationController _animationController;
        public UnitAnimationController Animations => _animationController;

        [SerializeField]
        private GameObject _canvas;

        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private List<Renderer> _renderers = new();

        [SerializeField]
        private List<Material> _playerTeamMaterials = new();

        [SerializeField]
        private List<Material> _enemyTeamMaterials = new();

        private Unit _unit;
        private Transform _cameraTransform;

        public string DebugText { get; set; }

        public void Initialize(Unit unit)
        {
            _unit = unit;
            _cameraTransform = Camera.main.transform;
            _animationController.Initialize(_unit);

            ApplyTeamVisuals();
            SetupNameDisplay();
        }

        public void SetOutlineActive(bool active)
        {
            if (_renderers.Count == 0)
            {
                Debug.LogError("[UnitVisuals] _renderers is empty");
                return;
            }

            foreach (var renderer in _renderers)
            {
                if (renderer.TryGetComponent<QuickOutline>(out var outline))
                {
                    outline.enabled = active;
                }
            }
        }

        private void ApplyTeamVisuals()
        {
            if (_renderers.Count == 0)
            {
                Debug.LogError("[UnitVisuals] _renderers is empty");
                return;
            }

            var materialsToApply = _unit.Team == Team.Player ? _playerTeamMaterials : _enemyTeamMaterials;

            for (int i = 0; i < Mathf.Min(_renderers.Count, materialsToApply.Count); i++)
            {
                if (_renderers[i] != null && materialsToApply[i] != null)
                {
                    _renderers[i].material = materialsToApply[i];
                }
            }
        }

        private void SetupNameDisplay()
        {
            if (_canvas == null || _nameText == null)
            {
                Debug.LogError("[UnitVisuals] _canvas or _nameText is not assigned");
                return;
            }

            _canvas.SetActive(true);
            _nameText.text = _unit.name;
        }

        private void LateUpdate()
        {
            if (_canvas == null || _cameraTransform == null)
            {
                Debug.LogError("[UnitVisuals] _canvas or _cameraTransform is not assigned");
                return;
            }

            // Billboard the canvas to always face the camera
            _canvas.transform.LookAt(
                _canvas.transform.position + _cameraTransform.rotation * Vector3.forward,
                _cameraTransform.rotation * Vector3.up
            );

            _nameText.text = $"<b>{_unit.name}</b>\n{DebugText}";
        }
    }
}
