using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitHudManager : DisposableComponent
    {
        [Header("References")]
        [SerializeField] private Transform _playerUnitsContainer;
        [SerializeField] private Transform _enemyUnitsContainer;
        [SerializeField] private GameObject _unitHudElementPrefab;

        [Header("Settings")]
        [SerializeField] private Color _playerHealthColor = Color.green;
        [SerializeField] private Color _enemyHealthColor = Color.red;
        [SerializeField] private int _maxDisplayedUnits = 10;

        [Inject] private BattleManager _battleManager;

        private Dictionary<Unit, UnitHudElement> _unitHudElements = new Dictionary<Unit, UnitHudElement>();
        
        private void Start()
        {
            // Subscribe to battle manager's units
            Observable.EveryUpdate()
                .Subscribe(_ => UpdateHudElements())
                .AddTo(_disposables);
        }
        
        private void UpdateHudElements()
        {
            // Get all units from battle manager
            var playerUnits = _battleManager.GetPlayerUnits();
            var enemyUnits = _battleManager.GetEnemyUnits();

            // Add any new player units
            foreach (var unit in playerUnits)
            {
                if (!_unitHudElements.ContainsKey(unit))
                {
                    AddUnitToHud(unit, Team.Player);
                }
            }

            // Add any new enemy units
            foreach (var unit in enemyUnits)
            {
                if (!_unitHudElements.ContainsKey(unit))
                {
                    AddUnitToHud(unit, Team.Enemy);
                }
            }

            // Remove any destroyed units
            List<Unit> unitsToRemove = new List<Unit>();
            foreach (var unitEntry in _unitHudElements)
            {
                if (unitEntry.Key == null || !playerUnits.Contains(unitEntry.Key) && !enemyUnits.Contains(unitEntry.Key))
                {
                    unitsToRemove.Add(unitEntry.Key);
                    Destroy(unitEntry.Value);
                }
            }

            foreach (var unit in unitsToRemove)
            {
                _unitHudElements.Remove(unit);
            }
        }

        private void AddUnitToHud(Unit unit, Team team)
        {
            // Choose the appropriate container based on team
            Transform container = team == Team.Player ? _playerUnitsContainer : _enemyUnitsContainer;
            
            // Check if we've reached the maximum displayed units for this team
            if (container.childCount >= _maxDisplayedUnits)
            {
                return;
            }

            // Instantiate the HUD element
            var hudElement = Instantiate(_unitHudElementPrefab, container).GetComponent<UnitHudElement>();
            hudElement.name = $"{team} Unit {unit.name} HUD";

            // Get references to the health bar and level text
            Image healthFillImage = hudElement.HealthFillImage;
            TextMeshProUGUI levelText = hudElement.LevelText;
            TextMeshProUGUI nameText = hudElement.NameText;

            // Set the appropriate color
            if (healthFillImage != null)
            {
                healthFillImage.color = team == Team.Player ? _playerHealthColor : _enemyHealthColor;
            }

            // Set the unit name
            if (nameText != null)
            {
                nameText.text = unit.name;
            }

            // Set up the subscriptions for health and level updates
            if (healthFillImage != null)
            {
                unit.State.Health.Merge(unit.State.MaxHealth)
                    .Subscribe(_ => 
                    {
                        float health = unit.State.Health.Value;
                        float maxHealth = unit.State.MaxHealth.Value;
                        healthFillImage.fillAmount = maxHealth > 0 ? health / maxHealth : 0;
                    })
                    .AddTo(_disposables);
            }

            if (levelText != null)
            {
                unit.State.Level
                    .Subscribe(level => 
                    {
                        levelText.text = level.ToString();
                    })
                    .AddTo(_disposables);
            }

            // Store the HUD element in the dictionary
            _unitHudElements.Add(unit, hudElement);
        }
    }
} 