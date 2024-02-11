using System.Transactions;
using Enterprise.Core.Transactions;

namespace Enterprise.Tests.Transactions
{
    internal sealed class SimpleTransactionalEntity
    {
        private readonly IInvoker invoker = TransactionalInvoker.Instance;

        private int state;

        public int State
        {
            get
            {
                return this.state;
            }
            set
            {
                if (Transaction.Current == null)
                {
                    this.state = value;
                    return;
                }

                var command = new PropertyChangeCommand(this, value);
                this.invoker.ExecuteCommand(command);
            }
        }

        private class PropertyChangeCommand : ICommand
        {
            private readonly SimpleTransactionalEntity entity;

            private readonly int originalState;

            private readonly int newState;

            public PropertyChangeCommand(
                SimpleTransactionalEntity entity,
                int newState)
            {
                this.entity = entity;
                this.originalState = entity.state;
                this.newState = newState;
            }

            public void Execute()
            {
                this.entity.state = newState;
            }

            public void UnExecute()
            {
                this.entity.state = originalState;
            }
        }
    }
}
