namespace Enterprise.Core.Transactions
{
    public interface IInvoker
    {
        void ExecuteCommand(ICommand command);
    }
}
