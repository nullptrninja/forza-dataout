using Core.Contracts;
using System;
using System.Collections.Generic;

namespace Core {
    public class Unsubscriber<T> : IUnsubscriber {
        private readonly IList<IObserver<T>> mObservers;
        private readonly IObserver<T> mBoundListener;

        public Unsubscriber(IObserver<T> listener, IList<IObserver<T>> observers) {
            mBoundListener = listener;
            mObservers = observers;
        }

        public void Dispose() {
            if (mBoundListener != null && mObservers.Contains(mBoundListener)) {
                mObservers.Remove(mBoundListener);
            }
        }
    }
}
