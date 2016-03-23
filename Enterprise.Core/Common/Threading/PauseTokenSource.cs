using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Common.Threading
{
    public sealed class PauseTokenSource
    {
        internal static readonly Task s_completedTask = Task.FromResult(true);

        private volatile TaskCompletionSource<bool> m_paused;

        public bool IsPaused
        {
            get { return m_paused != null; }
            set
            {
                if (value)
                {
                    Interlocked.CompareExchange(
                        ref m_paused, new TaskCompletionSource<bool>(), null);
                }
                else
                {
                    while (true)
                    {
                        var tcs = m_paused;
                        if (tcs == null) return;
                        if (Interlocked.CompareExchange(ref m_paused, null, tcs) == tcs)
                        {
                            tcs.SetResult(true);
                            break;
                        }
                    }
                }
            }
        }

        public PauseToken Token
        {
            get { return new PauseToken(this); }
        }

        internal Task WaitWhilePausedAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var cur = m_paused;
            return cur != null ? cur.Task : s_completedTask;
        }
    }
}
