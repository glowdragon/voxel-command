using DanielKreitsch;
using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    /// <summary>
    /// Controls time scale for fast-forwarding gameplay
    /// </summary>
    public class FastForwardController : DisposableMonoBehaviour
    {
        [SerializeField]
        private float _fastForwardTimeScale = 10f;

        [SerializeField]
        private KeyCode _fastForwardKey = KeyCode.Space;

        private ReactiveProperty<bool> _isFastForwarding = new(false);
        public IReadOnlyReactiveProperty<bool> IsFastForwarding => _isFastForwarding;

        private float _normalTimeScale = 1f;

        private void Start()
        {
            _isFastForwarding.Subscribe(isFastForwarding =>
            {
                if (isFastForwarding)
                {
                    _normalTimeScale = Time.timeScale;
                    Time.timeScale = _fastForwardTimeScale;
                }
                else
                {
                    Time.timeScale = _normalTimeScale;
                }
            });
        }

        private void Update()
        {
            if (Input.GetKeyDown(_fastForwardKey))
            {
                _isFastForwarding.Value = true;
            }
            else if (Input.GetKeyUp(_fastForwardKey))
            {
                _isFastForwarding.Value = false;
            }
        }

        private void OnDisable()
        {
            _isFastForwarding.Value = false;
        }
    }
}
