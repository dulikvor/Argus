using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Argus.Common.Telemetry
{
    public class ActivityScope : IDisposable
    {
        public static string Source = "Argus";
        private static readonly ActivitySource _activitySource = new ActivitySource(Source);


        private readonly Activity _activity;
        private bool _isDisposed = false;

        public async Task<TResult> Monitor<TResult>(Func<Task<TResult>> function)
        {
            try
            {
                return await function();
            }
            catch (Exception e)
            {
                OnFailure(e);
                throw;
            }
        }

        public async Task Monitor(Func<Task> function)
        {
            try
            {
                await function();
            }
            catch (Exception e)
            {
                OnFailure(e);
                throw;
            }
        }

        public void Monitor(Action function)
        {
            try
            {
                function();
            }
            catch (Exception e)
            {
                OnFailure(e);
                throw;
            }
        }

        public TResult Monitor<TResult>(Func<TResult> function)
        {
            try
            {
                return function();
            }
            catch (Exception e)
            {
                OnFailure(e);
                throw;
            }
        }

        public static ActivityScope Create(string callingType, [CallerMemberName] string callerName = "", int stackFrames = 1)
        {
            return new ActivityScope($"{callingType}.{callerName}");
        }

        public Activity Activity { get => _activity; }

        private ActivityScope(string name)
        {
            _activity = _activitySource.StartActivity(name);
            _activity.Start();
        }

        public void OnFailure(Exception e)
        {
            _activity.SetTag("exception", e.ToString());
            _activity.SetTag("exceptionMessage", e.Message);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _activity.Dispose();
            _isDisposed = true;
        }
    }
}
