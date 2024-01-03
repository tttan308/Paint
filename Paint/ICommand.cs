namespace Paint
{
    public interface ICommand
    {
        void Execute();
        void Unexecute();
    }
}