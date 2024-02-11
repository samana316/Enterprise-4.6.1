namespace Enterprise.Core.Transactions
{
    public interface ICommand
    {
        void Execute();

        void UnExecute();
    }
}
