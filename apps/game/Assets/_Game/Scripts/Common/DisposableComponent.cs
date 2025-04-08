using System;
using UniRx;
using UnityEngine;

namespace VoxelCommand.Client
{
    public abstract class DisposableComponent : MonoBehaviour, IDisposable
    {
        protected CompositeDisposable _disposables = new();
        public CompositeDisposable Disposables => _disposables;

        public virtual void ClearDisposables()
        {
            _disposables.Dispose();
            _disposables = new CompositeDisposable();
        }

        public virtual void Dispose()
        {
            ClearDisposables();
            OnDispose();
        }

        protected virtual void OnDispose() { }

        protected virtual void OnDestroy()
        {
            Dispose();
        }
    }
}
