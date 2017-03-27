using System;
using System.Timers;
using de.webducer.net.Detector.Base;
using Detector.Example.Events;
using Prism.Events;

namespace Detector.Example.Detectors
{
    public class DisconnectedDetector<T> : DetectorBase<T>
    {
        private static readonly TimeSpan _DEFAULT_TIMEOUT_TO_FIRST_EVENT = TimeSpan.FromSeconds(1);
        private readonly ConnectStateEvent _connectEvent;
        private readonly Timer _timer;

        public DisconnectedDetector(ConnectStateEvent connectEvent, ResultState positiveState = ResultState.Success,
            T positiveResult = default(T))
            : base(positiveState, positiveResult)
        {
            if (connectEvent == null) throw new ArgumentNullException(nameof(connectEvent));

            _connectEvent = connectEvent;

            // Timer for min wait time for the first event
            _timer = new Timer(_DEFAULT_TIMEOUT_TO_FIRST_EVENT.TotalMilliseconds)
            {
                AutoReset = false
            };
            _timer.Elapsed += OnFirstEventTimeOut;
        }

        protected override void OnStart()
        {
            // Register event processing on start
            _connectEvent.Subscribe(OnConnectionStateFired, ThreadOption.BackgroundThread);
            _timer.Start();
        }

        protected override void OnCleanup()
        {
            // Unregistre event procession on cleanup
            _connectEvent.Unsubscribe(OnConnectionStateFired);

            // Release timer resources
            _timer.Elapsed -= OnFirstEventTimeOut;
            _timer.Stop();
            _timer.Close();
        }

        private void OnConnectionStateFired(ConnectState state)
        {
            // Event received => no timer needed
            _timer.Elapsed -= OnFirstEventTimeOut;
            _timer.Stop();

            // Check if result already set
            if (TcsResult.IsResultSet)
            {
                // Unregister event processing
                _connectEvent.Unsubscribe(OnConnectionStateFired);
                return;
            }

            // Check for correct state
            if (state != ConnectState.Disconnected) return;

            // Process correct state
            // Unsubscribe event processing
            _connectEvent.Unsubscribe(OnConnectionStateFired);

            // Create result
            var result = new TaskResult<T>(PositiveState, PositiveResult);

            // Set result
            TcsResult.CheckAndSetResult(result);
        }

        private void OnFirstEventTimeOut(object sender, ElapsedEventArgs e)
        {
            // No events received => Disconnected
            _timer.Elapsed -= OnFirstEventTimeOut;
            _connectEvent.Publish(ConnectState.Disconnected);
        }
    }
}