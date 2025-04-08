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
        [SerializeField] private Image _backgroundImage;
        
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
            
            if (_backgroundImage != null)
            {
                _backgroundImage.gameObject.SetActive(false);
                _backgroundImage.color = new Color(_backgroundImage.color.r, _backgroundImage.color.g, _backgroundImage.color.b, 0f);
            }
            
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
                    // Apply the skill upgrade
                    onSkillSelected(_currentUnit);
                    _currentUnit.State.AvailableStatPoints.Value--;
                    
                    // Highlight and fade out the clicked button
                    skillButton.HighlightAndFadeOut();
                    
                    // Make other buttons fall down with increasing delays
                    float delayIncrement = 0.1f;
                    float currentDelay = 0f;
                    foreach (var otherButton in _skillButtons)
                    {
                        if (otherButton != skillButton)
                        {
                            currentDelay += delayIncrement;
                            otherButton.FallDown(currentDelay);
                        }
                    }
                    
                    // Hide the menu after all animations are done (after a delay)
                    if (_currentUnit.State.AvailableStatPoints.Value <= 0)
                    {
                        // Longest duration will be fadeOutDuration + the last button's delay
                        float totalDelay = currentDelay + 0.2f; // Add a bit extra for fade out duration
                        Tween.Delay(totalDelay).OnComplete(Hide);
                    }
                    else
                    {
                        // Wait for animations to finish before resetting other buttons
                        float totalDelay = currentDelay + 0.7f;
                        Tween.Delay(totalDelay).OnComplete(() => {
                            foreach (var otherButton in _skillButtons)
                            {
                                if (otherButton != skillButton)
                                {
                                    otherButton.ResetVisuals();
                                    otherButton.SetInfo(_currentUnit);
                                }
                            }
                        });
                    }
                }
            });
            
            _skillButtons.Add(skillButton);
        }

        /// <summary>
        /// Fades in the background image
        /// </summary>
        public void FadeInBackground()
        {
            if (_backgroundImage != null)
            {
                _backgroundImage.gameObject.SetActive(true);
                Tween.Alpha(target: _backgroundImage, endValue: 0.7f, duration: _animationDuration);
            }
        }

        /// <summary>
        /// Fades out the background image
        /// </summary>
        public void FadeOutBackground()
        {
            if (_backgroundImage != null)
            {
                Tween.Alpha(target: _backgroundImage, endValue: 0f, duration: _animationDuration)
                    .OnComplete(() => _backgroundImage.gameObject.SetActive(false));
            }
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
            
            // Get the CanvasGroup component or add it if not present
            var canvasGroup = _rootPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _rootPanel.AddComponent<CanvasGroup>();
            }
            
            // Start with zero alpha
            canvasGroup.alpha = 0f;
            
            // Animate fade in
            Tween.Alpha(target: canvasGroup, endValue: 1f, duration: _animationDuration);
            
            // Update button states
            foreach (var button in _skillButtons)
            {
                // Reset any previous animations
                button.ResetVisuals();
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
            
            // Get the CanvasGroup component or add it if not present
            var canvasGroup = _rootPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = _rootPanel.AddComponent<CanvasGroup>();
            }
            
            // Make sure it's fully visible at start
            canvasGroup.alpha = 1f;
            
            // Animate fade out
            Tween.Alpha(target: canvasGroup, endValue: 0f, duration: _animationDuration)
                .OnComplete(() => 
                {
                    _rootPanel.SetActive(false);
                    // Reset scale to ensure it's ready for next time
                    _rootPanel.GetComponent<RectTransform>().localScale = Vector3.one;
                    // Trigger the OnMenuClosed event with the unit that was shown
                    OnMenuClosed.OnNext(currentUnit);
                });
            
            _currentUnit = null;
        }
    }
} 