using System.Collections.Generic;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitHudManager : DisposableMonoBehaviour
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
        private Sprite _playerAvatar;

        [SerializeField]
        private Sprite _enemyAvatar;

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
        private TeamManager _teamManager;

        private Dictionary<Unit, UnitHudElement> _unitHudElements = new Dictionary<Unit, UnitHudElement>();

        private void Start()
        {
            // Subscribe to changes in team composition instead of polling every frame
            _teamManager.Team1Units.ObserveAdd()
                .Subscribe(addEvent => AddUnitToHud(addEvent.Value, Team.Player))
                .AddTo(_disposables);

            _teamManager.Team1Units.ObserveRemove()
                .Subscribe(removeEvent => RemoveUnitFromHud(removeEvent.Value))
                .AddTo(_disposables);

            _teamManager.Team2Units.ObserveAdd()
                .Subscribe(addEvent => AddUnitToHud(addEvent.Value, Team.Enemy))
                .AddTo(_disposables);

            _teamManager.Team2Units.ObserveRemove()
                .Subscribe(removeEvent => RemoveUnitFromHud(removeEvent.Value))
                .AddTo(_disposables);

            // Initial setup of HUD elements for existing units
            foreach (var unit in _teamManager.Team1Units)
            {
                AddUnitToHud(unit, Team.Player);
            }

            foreach (var unit in _teamManager.Team2Units)
            {
                AddUnitToHud(unit, Team.Enemy);
            }
        }

        private void RemoveUnitFromHud(Unit unit)
        {
            if (_unitHudElements.TryGetValue(unit, out var hudElement))
            {
                Destroy(hudElement.gameObject);
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
            Sprite avatar = team == Team.Player ? _playerAvatar : _enemyAvatar;
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
