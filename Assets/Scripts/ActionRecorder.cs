using System.Collections.Generic;
using UnityEngine;

public class ActionRecorder : MonoBehaviour
{
    private List<ICommand> recordedCommands = new List<ICommand>();

    public void Record(ICommand cmd)
    {
        recordedCommands.Add(cmd);
    }

    public List<ICommand> GetCommands()
    {
        return new List<ICommand>(recordedCommands);
    }

    public void Clear()
    {
        recordedCommands.Clear();
    }
}