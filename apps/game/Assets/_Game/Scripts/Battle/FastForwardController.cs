using UnityEngine;
using System;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Controls game time scale for fast-forwarding gameplay
    /// </summary>
    public class FastForwardController : MonoBehaviour
    {
        [SerializeField]
        private float _fastForwardTimeScale = 10f;
        
        [SerializeField]
        private KeyCode _fastForwardKey = KeyCode.Space;
        
        private float _normalTimeScale = 1f;
        private bool _isFastForwarding = false;
        
        // Event that fires when fast forward state changes
        public event Action<bool> OnFastForwardChanged;
        
        private void Start()
        {
            // Store initial time scale
            _normalTimeScale = Time.timeScale;
        }
        
        private void Update()
        {
            // Check for fast forward key
            if (Input.GetKeyDown(_fastForwardKey))
            {
                SetFastForward(true);
            }
            else if (Input.GetKeyUp(_fastForwardKey))
            {
                SetFastForward(false);
            }
        }
        
        /// <summary>
        /// Sets the game speed to fast forward or normal
        /// </summary>
        public void SetFastForward(bool fastForward)
        {
            if (_isFastForwarding == fastForward)
                return;
                
            _isFastForwarding = fastForward;
            
            if (fastForward)
            {
                // Store current time scale and set to fast forward
                _normalTimeScale = Time.timeScale;
                Time.timeScale = _fastForwardTimeScale;
                Debug.Log($"Fast forward enabled (x{_fastForwardTimeScale})");
            }
            else
            {
                // Restore normal time scale
                Time.timeScale = _normalTimeScale;
                Debug.Log("Fast forward disabled");
            }
            
            // Notify listeners
            OnFastForwardChanged?.Invoke(fastForward);
        }
        
        /// <summary>
        /// Forces time scale back to normal
        /// </summary>
        public void ResetToNormalSpeed()
        {
            if (_isFastForwarding)
            {
                SetFastForward(false);
            }
        }
        
        /// <summary>
        /// Returns whether fast forward is currently active
        /// </summary>
        public bool IsFastForwarding => _isFastForwarding;
        
        private void OnDisable()
        {
            // Ensure time scale is reset if component is disabled
            ResetToNormalSpeed();
        }
    }
} 