using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace VoxelCommand.Client
{
    public class UnitHudElement : MonoBehaviour
    {
        [SerializeField]
        private Image _borderImage;

        [SerializeField]
        private Image _backgroundImage;

        [SerializeField]
        private Image _avatarBackgroundImage;
        [SerializeField]
        private Image _avatarImage;

        [SerializeField]
        private Image _healthBorderImage;

        [SerializeField]
        private Image _healthBackgroundImage;

        [SerializeField]
        private Image _healthFillImage;

        [SerializeField]
        private Image _manaBorderImage;

        [SerializeField]
        private Image _manaBackgroundImage;

        [SerializeField]
        private Image _manaFillImage;

        [SerializeField]
        private TextMeshProUGUI _nameText;

        [SerializeField]
        private TextMeshProUGUI _levelText;

        private float _initialHealthBarWidth = 0;
        private float _initialManaBarWidth = 0;

        public void SetBorderColors(Color borderColor)
        {
            _borderImage.color = borderColor;
            _healthBorderImage.color = borderColor;
            _manaBorderImage.color = borderColor;
        }

        public void SetBackgroundColors(Color backgroundColor, Color darkBackgroundColor)
        {
            _backgroundImage.color = backgroundColor;
            _avatarBackgroundImage.color = darkBackgroundColor;
        }

        public void SetAvatar(Sprite avatar)
        {
            _avatarImage.sprite = avatar;
        }

        public void SetHealthColor(Color backgroundColor, Color fillColor)
        {
            _healthBackgroundImage.color = backgroundColor;
            _healthFillImage.color = fillColor;
        }

        public void SetManaColor(Color backgroundColor)
        {
            _manaBackgroundImage.color = backgroundColor;
        }

        public void SetName(string name)
        {
            _nameText.text = name;
        }

        public void SetLevel(int level)
        {
            _levelText.text = level.ToString();
        }

        public void SetHealth(float currentHealth, float maxHealth)
        {
            var rectTransform = _healthFillImage.rectTransform;
            if (rectTransform == null)
                return;

            // Store initial width if not already cached
            if (_initialHealthBarWidth <= 0)
            {
                _initialHealthBarWidth = rectTransform.sizeDelta.x;
            }

            // Handle potential division by zero or invalid maxHealth
            if (maxHealth <= 0)
            {
                maxHealth = 1; // Avoid division by zero
                currentHealth = 0; // Treat health as 0 if maxHealth is invalid
            }

            float healthRatio = currentHealth / maxHealth;
            float newWidth = _initialHealthBarWidth * healthRatio;

            // Update the width of the RectTransform
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = Mathf.Max(0, newWidth); // Ensure width doesn't go negative
            rectTransform.sizeDelta = sizeDelta;
        }

        public void SetMana(float currentMana, float maxMana)
        {
            if (_manaFillImage == null)
                return;

            var rectTransform = _manaFillImage.rectTransform;
            if (rectTransform == null)
                return;

            // Store initial width if not already cached
            if (_initialManaBarWidth <= 0)
            {
                _initialManaBarWidth = rectTransform.sizeDelta.x;
            }

            // Handle potential division by zero or invalid maxMana
            if (maxMana <= 0)
            {
                maxMana = 1; // Avoid division by zero
                currentMana = 0; // Treat mana as 0 if maxMana is invalid
            }

            float manaRatio = currentMana / maxMana;
            float newWidth = _initialManaBarWidth * manaRatio;

            // Update the width of the RectTransform
            Vector2 sizeDelta = rectTransform.sizeDelta;
            sizeDelta.x = Mathf.Max(0, newWidth); // Ensure width doesn't go negative
            rectTransform.sizeDelta = sizeDelta;
        }
    }
}
