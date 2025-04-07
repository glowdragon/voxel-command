using System;
using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    public abstract class DisposableComponent : MonoBehaviour, IDisposable
    {
        protected CompositeDisposable _disposables = new();

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
}
