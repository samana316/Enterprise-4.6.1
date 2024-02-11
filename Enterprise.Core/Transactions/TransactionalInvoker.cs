using System;
using System.Collections.Generic;
using System.Transactions;

namespace Enterprise.Core.Transactions
{
    public sealed class TransactionalInvoker : IInvoker
    {
        private static readonly TransactionalInvoker instance = new TransactionalInvoker();

        private readonly object sink = new object();

        [ThreadStatic]
        private IDictionary<string, CommandEnlistment> enlistments;

        private TransactionalInvoker()
        {
        }

        public static IInvoker Instance
        {
            get { return instance; }
        }

        public void ExecuteCommand(
            ICommand command)
        {
            lock (sink)
            {
                var transaction = Transaction.Current;

                if (ReferenceEquals(transaction, null))
                {
                    command.Execute();
                    return;
                }

                if (ReferenceEquals(enlistments, null))
                {
                    enlistments = new Dictionary<string, CommandEnlistment>();
                }

                CommandEnlistment enlistment;
                var key = transaction.TransactionInformation.LocalIdentifier;
                if (!enlistments.TryGetValue(key, out enlistment))
                {
                    enlistment = new CommandEnlistment(transaction);
                    enlistments.Add(key, enlistment);
                }

                enlistment.EnlistCommand(command);
            }
        }
    }
}
