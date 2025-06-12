using Polly;

namespace Argus.Common.StateMachine
{
    public static class StatePollyExtensions
    {
        public static async Task<T> WithRetry<T, TException>(
            this object _,
            Func<Task<T>> asyncFunc,
            int maxRetries = 3)
            where TException : Exception
        {
            var policy = Policy
                .Handle<TException>()
                .WaitAndRetryAsync(maxRetries, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

            return await policy.ExecuteAsync(async () => await asyncFunc());
        }
    }
}
