using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Transactions;

namespace Enterprise.Core.Transactions
{
    internal sealed class CommandEnlistment : IEnlistmentNotification
    {
        private readonly IList<ICommand> journal = new ObservableCollection<ICommand>();

        public CommandEnlistment(
            Transaction transaction)
        {
            transaction.EnlistVolatile(this, EnlistmentOptions.None);
        }

        public void EnlistCommand(
            ICommand command)
        {
            command.Execute();
            this.journal.Add(command);
        }

        public void Commit(
            Enlistment enlistment)
        {
            this.journal.Clear();
            enlistment.Done();
        }

        public void InDoubt(
            Enlistment enlistment)
        {
            this.InternalRollback(enlistment);
        }

        public void Prepare(
            PreparingEnlistment preparingEnlistment)
        {
            try
            {
                preparingEnlistment.Prepared();
            }
            catch (Exception exception)
            {
                preparingEnlistment.ForceRollback(exception);
            }
        }

        public void Rollback(
            Enlistment enlistment)
        {
            this.InternalRollback(enlistment);
        }

        private void InternalRollback(
            Enlistment enlistment)
        {
            try
            {
                for (var i = this.journal.Count - 1; i >= 0; i--)
                {
                    this.journal[i].UnExecute();
                }
                this.journal.Clear();
            }
            finally
            {
                this.journal.Clear();
                enlistment.Done();
            }
        }
    }
}
