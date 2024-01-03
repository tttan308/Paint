namespace ICommand
{
    public interface ICommand
    {
        void Execute();
        void Unexecute();
    }
}