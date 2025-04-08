using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using PrimeTween;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Helper class for handling skill button interactions
    /// </summary>
    public class SkillButton : MonoBehaviour
    {
        [Inject]
        private IUnitStatsCalculator _unitStatsCalculator;

        [SerializeField]
        private Button _button;

        [SerializeField]
        private TextMeshProUGUI _text;
        
        [SerializeField]
        private Image _buttonImage;
        
        [SerializeField]
        private float _highlightDuration = 0.3f;
        
        [SerializeField]
        private float _fadeOutDuration = 0.5f;
        
        [SerializeField]
        private Color _highlightColor = Color.green;

        private Subject<UniRx.Unit> _onClicked = new();
        public IObservable<UniRx.Unit> OnClicked => _onClicked;

        private SkillType _statType;
        private RectTransform _rectTransform;
        private Color _originalColor;

        public void Initialize(SkillType statType)
        {
            _statType = statType;
            _button.OnClickAsObservable().Subscribe(_onClicked).AddTo(this);
            _rectTransform = GetComponent<RectTransform>();
            
            if (_buttonImage != null)
            {
                _originalColor = _buttonImage.color;
            }
            else
            {
                _buttonImage = GetComponent<Image>();
                if (_buttonImage != null)
                {
                    _originalColor = _buttonImage.color;
                }
            }
        }

        public void SetInfo(Unit unit)
        {
            int currentRank = unit.State.GetSkillLevel(_statType);
            int newRank = currentRank + 1;

            // Set the stat info showing current and upgraded values
            string statName = _statType.ToString();
            float currentValue = CalculateStatValue(unit.Config, _statType, currentRank);
            float newValue = CalculateStatValue(unit.Config, _statType, newRank);

            _text.text = $"{statName}\n\n{currentRank} → {newRank}\n\n{currentValue:F1} → {newValue:F1}";
        }

        private float CalculateStatValue(UnitConfig config, SkillType statType, int rank)
        {
            switch (statType)
            {
                case SkillType.Health:
                    return _unitStatsCalculator.CalculateMaxHealth(config, rank);
                case SkillType.Strength:
                    return _unitStatsCalculator.CalculateDamageOutput(config, rank);
                case SkillType.Defense:
                    return _unitStatsCalculator.CalculateIncomingDamageReduction(config, rank);
                case SkillType.Speed:
                    return _unitStatsCalculator.CalculateMovementSpeed(config, rank);
                default:
                    Debug.LogError($"Unknown stat type: {statType}");
                    return 0;
            }
        }

        public void SetInteractable(bool interactable)
        {
            _button.interactable = interactable;
        }
        
        /// <summary>
        /// Highlights the button and fades it out
        /// </summary>
        public void HighlightAndFadeOut()
        {
            if (_buttonImage == null) return;
            
            Sequence sequence = Sequence.Create();
            
            // First highlight with a color change
            sequence.Chain(Tween.Color(target: _buttonImage, endValue: _highlightColor, duration: _highlightDuration));
            
            // Then fade out
            sequence.Chain(Tween.Alpha(target: _buttonImage, endValue: 0f, duration: _fadeOutDuration));
            
            // Disable button
            SetInteractable(false);
        }
        
        /// <summary>
        /// Makes the button fall down
        /// </summary>
        public void FallDown(float delay = 0f, float duration = 0.5f)
        {
            if (_rectTransform == null) return;
            
            Vector2 startPosition = _rectTransform.anchoredPosition;
            Vector2 endPosition = new Vector2(startPosition.x, startPosition.y - 300f); // Fall down by 300 units
            
            Tween.Position(
                target: _rectTransform, 
                endValue: endPosition, 
                duration: duration,
                ease: Ease.InCubic,
                startDelay: delay
            );
            
            // Fade out while falling
            if (_buttonImage != null)
            {
                Tween.Alpha(target: _buttonImage, endValue: 0f, duration: duration, startDelay: delay);
            }
            
            // Disable button
            SetInteractable(false);
        }
        
        /// <summary>
        /// Resets the button visual state
        /// </summary>
        public void ResetVisuals()
        {
            if (_buttonImage != null)
            {
                _buttonImage.color = _originalColor;
            }
            
            if (_rectTransform != null)
            {
                Vector2 position = _rectTransform.anchoredPosition;
                _rectTransform.anchoredPosition = new Vector2(position.x, 0);
            }
            
            SetInteractable(true);
        }
    }
}
