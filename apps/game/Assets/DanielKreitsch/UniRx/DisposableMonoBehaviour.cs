using System;
using UniRx;
using UnityEngine;

namespace DanielKreitsch
{
    /// <summary>
    /// Base MonoBehaviour that implements IDisposable to properly manage resources.
    /// Use this class when you need to handle disposable resources that should be
    /// cleaned up when the GameObject is destroyed.
    /// </summary>
    public abstract class DisposableMonoBehaviour : MonoBehaviour, IDisposable
    {
        /// <summary>
        /// Collection of disposables that will be automatically disposed when this MonoBehaviour is destroyed.
        /// </summary>
        protected CompositeDisposable _disposables = new();
        
        /// <summary>
        /// Gets the CompositeDisposable for adding disposables from outside the class.
        /// </summary>
        public CompositeDisposable Disposables => _disposables;

        /// <summary>
        /// Tracks whether this object has been disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// Clears all current disposables and creates a new CompositeDisposable container.
        /// </summary>
        public virtual void ClearDisposables()
        {
            if (_disposables != null)
            {
                _disposables.Dispose();
                _disposables = new CompositeDisposable();
            }
        }

        /// <summary>
        /// Disposes all managed resources.
        /// </summary>
        public virtual void Dispose()
        {
            if (_isDisposed)
                return;

            lock (this)
            {
                if (_isDisposed)
                    return;

                _isDisposed = true;
                ClearDisposables();
                OnDispose();
            }
        }

        /// <summary>
        /// Override this method to add custom disposal logic.
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        /// Called when the MonoBehaviour is being destroyed.
        /// Automatically disposes all resources.
        /// </summary>
        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// Returns whether this object has been disposed.
        /// </summary>
        public bool IsDisposed => _isDisposed;
    }
}
