using System;
using System.Collections.Generic;
using PrimeTween;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Displays a menu for units to select skills to level up when they gain skill points.
    /// Uses PrimeTween for animations.
    /// </summary>
    public class SkillSelectionMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _rootPanel;
        [SerializeField] private Transform _skillButtonsContainer;
        [SerializeField] private Button _skillButtonPrefab;
        [SerializeField] private float _animationDuration = 0.3f;
        
        [Header("Unit Info")]
        [SerializeField] private TextMeshProUGUI _unitNameText;

        private List<SkillButton> _skillButtons = new();
        private Unit _currentUnit;
        private bool _isVisible = false;
        private List<IDisposable> _statSubscriptions = new();
        
        // Event triggered when the menu is closed
        public Subject<Unit> OnMenuClosed = new Subject<Unit>();

        [Inject]
        private UnitManager _unitManager;

        [Inject]
        private DiContainer _container;

        public void Awake()
        {
            _rootPanel.SetActive(false);
            
            // Create skill buttons
            CreateSkillButton(StatType.Health, unit => unit.State.HealthRank.Value++);
            CreateSkillButton(StatType.Damage, unit => unit.State.DamageRank.Value++);
            CreateSkillButton(StatType.Defense, unit => unit.State.DefenseRank.Value++);
            CreateSkillButton(StatType.Speed, unit => unit.State.SpeedRank.Value++);
        }

        /// <summary>
        /// Creates a button for upgrading a specific skill
        /// </summary>
        private void CreateSkillButton(StatType statType, Action<Unit> onSkillSelected)
        {
            // Use Zenject to instantiate the button to ensure dependencies are injected
            var skillButtonGameObject = _container.InstantiatePrefab(_skillButtonPrefab, _skillButtonsContainer);
            var skillButton = skillButtonGameObject.GetComponent<SkillButton>();
            skillButton.Initialize(statType);
            skillButton.OnClicked.Subscribe(_ => 
            {
                if (_currentUnit != null)
                {
                    onSkillSelected(_currentUnit);
                    _currentUnit.State.AvailableStatPoints.Value--;
                    
                    if (_currentUnit.State.AvailableStatPoints.Value <= 0)
                    {
                        Hide();
                    }
                }
            });
            
            _skillButtons.Add(skillButton);
        }

        /// <summary>
        /// Shows the skill selection menu for a specific unit
        /// </summary>
        public void Show(Unit unit)
        {
            if (unit == null || unit.State.AvailableStatPoints.Value <= 0) 
                return;
            
            _currentUnit = unit;
            _rootPanel.SetActive(true);
            _isVisible = true;
            
            // Display unit info and stats
            SetupUnitDisplay();
            
            // Animate the menu appearance
            var rectTransform = _rootPanel.GetComponent<RectTransform>();
            
            // Start with zero scale
            rectTransform.localScale = Vector3.zero;
            
            // Animate to full scale with a bounce effect
            Tween.Scale(target: rectTransform, endValue: Vector3.one, duration: _animationDuration, ease: Ease.OutBack);
            
            // Update button states
            foreach (var button in _skillButtons)
            {
                button.SetInfo(_currentUnit);
                button.SetInteractable(true);
            }
        }

        /// <summary>
        /// Sets up the unit information display and subscribes to stat changes
        /// </summary>
        private void SetupUnitDisplay()
        {
            // Clear any existing subscriptions
            foreach (var subscription in _statSubscriptions)
            {
                subscription.Dispose();
            }
            _statSubscriptions.Clear();
            
            // Set unit name
            _unitNameText.text = _currentUnit.gameObject.name;
        }

        /// <summary>
        /// Hides the skill selection menu
        /// </summary>
        public void Hide()
        {
            if (!_isVisible) return;
            
            _isVisible = false;
            
            var currentUnit = _currentUnit;
            
            // Clear subscriptions
            foreach (var subscription in _statSubscriptions)
            {
                subscription.Dispose();
            }
            _statSubscriptions.Clear();
            
            // Animate the menu disappearance
            var rectTransform = _rootPanel.GetComponent<RectTransform>();
            
            Tween.Scale(target: rectTransform, endValue: Vector3.zero, duration: _animationDuration, ease: Ease.InBack)
                .OnComplete(() => 
                {
                    _rootPanel.SetActive(false);
                    // Trigger the OnMenuClosed event with the unit that was shown
                    OnMenuClosed.OnNext(currentUnit);
                });
            
            _currentUnit = null;
        }
    }
} 