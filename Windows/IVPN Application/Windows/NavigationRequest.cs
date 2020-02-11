using System;
using System.Windows.Controls;

namespace IVPN.Windows
{
    public class NavigationRequest
    {
         public NavigationRequest(Frame frame, NavigationAnimation animation, Action onComplete)
        {
            Frame = frame;
            Animation = animation;
            OnComplete = onComplete;
        }

        public Frame Frame { get; private set; }

        public NavigationAnimation Animation { get; private set; }

        public Action OnComplete { get; private set; }

    }
}
