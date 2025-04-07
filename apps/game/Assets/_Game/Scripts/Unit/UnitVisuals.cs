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

        private Unit _unit;

        public void Initialize(Unit unit)
        {
            Dispose();

            _unit = unit;

            ApplyTeamVisuals();
            SetupSubscriptions();
        }

        private void SetupSubscriptions()
        {
            // Level subscription
            _unit
                .State.Level.Subscribe(level =>
                {
                    if (_levelText != null)
                    {
                        _levelText.text = level.ToString();
                    }
                })
                .AddTo(_disposables);

            // Health subscription
            _unit.State.Health.Merge(_unit.State.MaxHealth).Subscribe(_ => UpdateHealthBar()).AddTo(_disposables);
        }

        private void UpdateHealthBar()
        {
            if (_healthFillImage == null)
                return;

            float health = _unit.State.Health.Value;
            float maxHealth = _unit.State.MaxHealth.Value;
            _healthFillImage.fillAmount = maxHealth > 0 ? health / maxHealth : 0;
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
