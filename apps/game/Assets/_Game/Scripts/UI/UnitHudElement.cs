using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelCommand.Client
{
    public class UnitHudElement : MonoBehaviour
    {
        [SerializeField]
        private Image _healthFillImage;

        [SerializeField]
        private TextMeshProUGUI _levelText;

        [SerializeField]
        private TextMeshProUGUI _nameText;

        public Image HealthFillImage => _healthFillImage;
        public TextMeshProUGUI LevelText => _levelText;
        public TextMeshProUGUI NameText => _nameText;
    }
}
