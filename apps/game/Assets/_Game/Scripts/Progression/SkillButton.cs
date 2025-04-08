using System;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

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

        private Subject<UniRx.Unit> _onClicked = new();
        public IObservable<UniRx.Unit> OnClicked => _onClicked;

        private StatType _statType;

        public void Initialize(StatType statType)
        {
            _statType = statType;
            _button.OnClickAsObservable().Subscribe(_onClicked).AddTo(this);
        }

        public void SetInfo(Unit unit)
        {
            int currentRank = unit.State.GetStatRank(_statType);
            int newRank = currentRank + 1;

            // Set the stat info showing current and upgraded values
            string statName = _statType.ToString();
            float currentValue = CalculateStatValue(unit.Config, _statType, currentRank);
            float newValue = CalculateStatValue(unit.Config, _statType, newRank);

            _text.text = $"{statName}\n\n{currentRank} → {newRank}\n\n{currentValue:F1} → {newValue:F1}";
        }

        private float CalculateStatValue(UnitConfig config, StatType statType, int rank)
        {
            switch (statType)
            {
                case StatType.Health:
                    return _unitStatsCalculator.CalculateMaxHealth(config, rank);
                case StatType.Damage:
                    return _unitStatsCalculator.CalculateDamageOutput(config, rank);
                case StatType.Defense:
                    return _unitStatsCalculator.CalculateIncomingDamageReduction(config, rank);
                case StatType.Speed:
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
    }

    public enum StatType
    {
        Health,
        Damage,
        Defense,
        Speed,
    }
}
