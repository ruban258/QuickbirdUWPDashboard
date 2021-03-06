﻿namespace Quickbird.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using JetBrains.Annotations;
    using Util;

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<TaskCompletionSource<object>> _resumeAction;
        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly Action<TaskCompletionSource<object>> _suspendAction;

        protected List<DispatcherTimer> DispatcherTimers = new List<DispatcherTimer>();

        public ViewModelBase()
        {
            _resumeAction = BaseResume;
            _suspendAction = BaseSuspend;
            BroadcasterService.Instance.Suspending.Subscribe(_suspendAction);
            BroadcasterService.Instance.Resuming.Subscribe(_resumeAction);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>Clean up timers messenger things etc.</summary>
        public abstract void Kill();

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void BaseResume(TaskCompletionSource<object> completer)
        {
            Util.LoggingService.LogInfo("resumuing timers", Windows.Foundation.Diagnostics.LoggingLevel.Verbose);
            foreach (var dispatcherTimer in DispatcherTimers)
            {
                dispatcherTimer.Start();
            }
            completer.SetResult(null);
        }

        private void BaseSuspend(TaskCompletionSource<object> completer)
        {
            Util.LoggingService.LogInfo("suspending timers", Windows.Foundation.Diagnostics.LoggingLevel.Verbose);
            foreach (var dispatcherTimer in DispatcherTimers)
            {
                dispatcherTimer.Stop();
            }
            completer.SetResult(null);
        }
    }
}
