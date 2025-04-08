using System.Collections.Generic;
using DanielKreitsch;
using UniRx;
using UnityEngine;
using Zenject;

namespace VoxelCommand.Client
{
    public class UnitHudUI : DisposableMonoBehaviour
    {
        [Inject]
        private UnitManager _unitManager;

        [Header("References")]
        [SerializeField]
        private Transform _playerUnitsContainer;

        [SerializeField]
        private Transform _enemyUnitsContainer;

        [SerializeField]
        private GameObject _unitHudElementPrefab;

        [Header("General Settings")]
        [SerializeField]
        private int _maxDisplayedUnits = 10;

        [Header("Player Settings")]
        [SerializeField]
        private Sprite _playerAvatar;

        [SerializeField]
        private Color _playerBorderColor = Color.blue;

        [SerializeField]
        private Color _playerBackgroundColor = Color.blue;

        [SerializeField]
        private Color _playerDarkBackgroundColor = Color.black;

        [SerializeField]
        private Color _playerHealthColor = Color.blue;

        [Header("Enemy Settings")]
        [SerializeField]
        private Sprite _enemyAvatar;

        [SerializeField]
        private Color _enemyBorderColor = Color.red;

        [SerializeField]
        private Color _enemyBackgroundColor = Color.red;

        [SerializeField]
        private Color _enemyDarkBackgroundColor = Color.black;

        [SerializeField]
        private Color _enemyHealthColor = Color.red;

        private Dictionary<Unit, UnitHudElement> _unitHudElements = new();

        private void Start()
        {
            // Subscribe to changes in team composition instead of polling every frame
            _unitManager.Units.ObserveAdd().Subscribe(e => AddUnitToHud(e.Value)).AddTo(_disposables);

            _unitManager.Units.ObserveRemove().Subscribe(e => RemoveUnitFromHud(e.Value)).AddTo(_disposables);

            // Initial setup of HUD elements for existing units
            foreach (var unit in _unitManager.Units)
            {
                AddUnitToHud(unit);
            }
        }

        private void RemoveUnitFromHud(Unit unit)
        {
            if (_unitHudElements.TryGetValue(unit, out var hudElement))
            {
                if (hudElement != null)
                {
                    Destroy(hudElement.gameObject);
                }
                _unitHudElements.Remove(unit);
            }
        }

        private void AddUnitToHud(Unit unit)
        {
            Team team = unit.Team;

            // Choose the appropriate container based on team
            Transform container = unit.Team == Team.Player ? _playerUnitsContainer : _enemyUnitsContainer;

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
