using System.Threading;
using System.Threading.Tasks;

namespace Enterprise.Core.Common.Threading
{
    public struct PauseToken
    {
        private readonly PauseTokenSource m_source;

        internal PauseToken(
            PauseTokenSource source)
        {
            m_source = source;
        }

        public bool IsPaused
        {
            get { return m_source != null && m_source.IsPaused; }
        }

        public Task WaitWhilePausedAsync()
        {
            return this.WaitWhilePausedAsync(CancellationToken.None);
        }

        public Task WaitWhilePausedAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return IsPaused ?
                m_source.WaitWhilePausedAsync(cancellationToken) :
                PauseTokenSource.s_completedTask;
        }
    }
}
