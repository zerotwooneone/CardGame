using System;
using CardGame.Utils.Abstractions.Bus;

namespace CardGame.Utils.Bus
{
    public class SubscriptionWrapper : ISubscription
    {
        private readonly IDisposable _observableSubscription;

        public SubscriptionWrapper(IDisposable observableSubscription)
        {
            if(observableSubscription == null) throw new ArgumentNullException(nameof(observableSubscription));
            _observableSubscription = observableSubscription;
        }
        public void Dispose()
        {
            _observableSubscription.Dispose();
        }
    }
}