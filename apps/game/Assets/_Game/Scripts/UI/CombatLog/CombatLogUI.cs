using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

namespace VoxelCommand.Client
{
    /// <summary>
    /// UI component that displays combat log entries in a scrollable list
    /// </summary>
    public class CombatLogUI : MonoBehaviour
    {
        [Inject]
        private CombatLogManager _combatLogManager;

        [Header("UI References")]
        [SerializeField] 
        private ScrollRect _scrollRect;
        
        [SerializeField] 
        private RectTransform _contentParent;
        
        [SerializeField] 
        private TextMeshProUGUI _logEntryPrefab;
        
        [SerializeField]
        private Button _clearButton;

        [Header("Settings")]
        [SerializeField] 
        private bool _autoScroll = true;

        private List<TextMeshProUGUI> _entryTextComponents = new List<TextMeshProUGUI>();
        private bool _isScrolling = false;

        private void Start()
        {
            // Set up auto-scrolling detection
            _scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            
            // Set up clear button
            if (_clearButton != null)
            {
                _clearButton.onClick.AddListener(() => _combatLogManager.ClearLog());
            }

            // Subscribe to log changes
            _combatLogManager.LogEntries
                .ObserveAdd()
                .Subscribe(e => AddEntryToUI(e.Value))
                .AddTo(this);

            _combatLogManager.LogEntries
                .ObserveReset()
                .Subscribe(_ => ClearUI())
                .AddTo(this);

            // Initialize UI with existing entries
            foreach (var entry in _combatLogManager.LogEntries)
            {
                AddEntryToUI(entry);
            }
        }

        private void OnScrollValueChanged(Vector2 pos)
        {
            // Detect manual scrolling to disable auto-scroll
            if (_scrollRect.velocity.magnitude > 0.01f)
            {
                _isScrolling = true;
            }
            
            // Re-enable auto-scroll when user scrolls back to bottom
            if (_isScrolling && pos.y < 0.01f)
            {
                _isScrolling = false;
            }
        }

        /// <summary>
        /// Adds a combat log entry to the UI display
        /// </summary>
        private void AddEntryToUI(CombatLogEntry entry)
        {
            // Instantiate and configure text element
            TextMeshProUGUI textComponent = Instantiate(_logEntryPrefab, _contentParent);
            textComponent.text = entry.GetFormattedText();
            textComponent.color = entry.Color;
            
            _entryTextComponents.Add(textComponent);

            // Scroll to bottom if auto-scroll is enabled and user isn't manually scrolling
            if (_autoScroll && !_isScrolling)
            {
                Canvas.ForceUpdateCanvases();
                _scrollRect.normalizedPosition = new Vector2(0, 0);
            }
        }

        /// <summary>
        /// Clears all log entries from the UI
        /// </summary>
        private void ClearUI()
        {
            foreach (var textComponent in _entryTextComponents)
            {
                Destroy(textComponent.gameObject);
            }
            
            _entryTextComponents.Clear();
        }
    }
} 