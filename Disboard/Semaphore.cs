using System;
using System.Threading;
using System.Threading.Tasks;

namespace Disboard
{
    sealed class Semaphore
    {
        readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        internal async Task<IDisposable> LockAsync()
        {
            await _semaphore.WaitAsync();
            return new Handler(_semaphore);
        }

        sealed class Handler : IDisposable
        {
            readonly SemaphoreSlim _semaphore;
            bool _disposed = false;

            internal Handler(SemaphoreSlim semaphore)
            {
                _semaphore = semaphore;
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    _semaphore.Release();
                    _disposed = true;
                }
            }
        }
    }
}
