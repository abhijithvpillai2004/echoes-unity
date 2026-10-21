public interface ICommand
{
    void Execute(Unit unit);
    string GetLabel();
}