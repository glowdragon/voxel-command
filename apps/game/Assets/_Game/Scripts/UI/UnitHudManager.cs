using System.Collections.Generic;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitHudManager : DisposableComponent
    {
        [Header("References")]
        [SerializeField]
        private Transform _playerUnitsContainer;

        [SerializeField]
        private Transform _enemyUnitsContainer;

        [SerializeField]
        private GameObject _unitHudElementPrefab;

        [Header("Settings")]
        [SerializeField]
        private Sprite _playerAvatarMale;

        [SerializeField]
        private Sprite _playerAvatarFemale;

        [SerializeField]
        private Sprite _enemyAvatarMale;

        [SerializeField]
        private Sprite _enemyAvatarFemale;

        [SerializeField]
        private Color _playerBorderColor = Color.blue;

        [SerializeField]
        private Color _enemyBorderColor = Color.red;

        [SerializeField]
        private Color _playerBackgroundColor = Color.blue;

        [SerializeField]
        private Color _enemyBackgroundColor = Color.red;

        [SerializeField]
        private Color _playerDarkBackgroundColor = Color.black;

        [SerializeField]
        private Color _enemyDarkBackgroundColor = Color.black;

        [SerializeField]
        private Color _playerHealthColor = Color.blue;

        [SerializeField]
        private Color _enemyHealthColor = Color.red;

        [SerializeField]
        private int _maxDisplayedUnits = 10;

        [Inject]
        private BattleManager _battleManager;

        private Dictionary<Unit, UnitHudElement> _unitHudElements = new Dictionary<Unit, UnitHudElement>();

        private void Start()
        {
            // Subscribe to battle manager's units
            Observable.EveryUpdate().Subscribe(_ => UpdateHudElements()).AddTo(_disposables);
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

            // Set initial values using the setter methods
            Sprite avatar = team == Team.Player ? _playerAvatarMale : _enemyAvatarMale;
            Color healthColor = team == Team.Player ? _playerHealthColor : _enemyHealthColor;
            Color borderColor = team == Team.Player ? _playerBorderColor : _enemyBorderColor;
            Color backgroundColor = team == Team.Player ? _playerBackgroundColor : _enemyBackgroundColor;
            Color darkBackgroundColor = team == Team.Player ? _playerDarkBackgroundColor : _enemyDarkBackgroundColor;

            hudElement.SetBorderColors(borderColor);
            hudElement.SetBackgroundColors(backgroundColor, darkBackgroundColor);
            hudElement.SetAvatar(avatar);
            hudElement.SetHealthColor(darkBackgroundColor, healthColor);
            hudElement.SetManaColor(darkBackgroundColor);
            hudElement.SetName(unit.name);

            // Initial health setup
            float initialHealth = unit.State.Health.Value;
            float initialMaxHealth = unit.State.MaxHealth.Value;
            hudElement.SetHealth(initialHealth, initialMaxHealth);

            // Initial level setup
            int initialLevel = unit.State.Level.Value;
            hudElement.SetLevel(initialLevel);

            // Set up subscriptions for health and level updates
            unit.State.Health.Merge(unit.State.MaxHealth)
                .Subscribe(_ =>
                {
                    hudElement.SetHealth(unit.State.Health.Value, unit.State.MaxHealth.Value);
                })
                .AddTo(_disposables);

            unit.State.Level.Subscribe(level =>
                {
                    hudElement.SetLevel(level);
                })
                .AddTo(_disposables);

            // Store the HUD element in the dictionary
            _unitHudElements.Add(unit, hudElement);
        }
    }
}
