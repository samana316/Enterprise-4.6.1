using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Core.Common.Threading.Tasks;
using Enterprise.Core.Utilities;

namespace Enterprise.Core.Common.ServiceModel
{
    public static class CommunicationObjectExtensions
    {
        public static Task OpenAsync(
            this ICommunicationObject resource)
        {
            return resource.OpenAsync(CancellationToken.None);
        }

        public static Task OpenAsync(
            this ICommunicationObject resource,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(resource, "resource");

            return Task.Factory.FromAsync(
                resource.BeginOpen, TaskHelpers.EndInvoke, null);
        }

        public static void CloseOrAbort(
            this ICommunicationObject resource)
        {
            try
            {
                if (ReferenceEquals(resource, null))
                {
                    return;
                }

                if (resource.State == CommunicationState.Opened)
                {
                    resource.Close();
                }
                else
                {
                    resource.Abort();
                }
            }
            catch (CommunicationException)
            {
                resource.Abort();
            }
        }

        public static Task CloseOrAbortAsync(
            this ICommunicationObject resource)
        {
            return resource.CloseOrAbortAsync(CancellationToken.None);
        }

        public static async Task CloseOrAbortAsync(
            this ICommunicationObject resource,
            CancellationToken cancellationToken)
        {
            try
            {
                if (ReferenceEquals(resource, null))
                {
                    return;
                }

                if (resource.State == CommunicationState.Opened)
                {
                    await resource.CloseAsync(cancellationToken);
                }
                else
                {
                    resource.Abort();
                }
            }
            catch (CommunicationException)
            {
                resource.Abort();
            }
        }

        private static Task CloseAsync(
            this ICommunicationObject resource,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Check.NotNull(resource, "resource");

            return Task.Factory.FromAsync(
                resource.BeginClose, TaskHelpers.EndInvoke, null);
        }
    }
}
