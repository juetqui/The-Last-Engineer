public class NodeChecker
{
    private TaskManager[] _taskManagers;
    private NodeType _requiredType;

    public NodeChecker(TaskManager[] taskManagers, NodeType requiredType)
    {
        _taskManagers = taskManagers;
        _requiredType = requiredType;
    }

    public bool IsNodeCorrect(NodeType receivedType)
    {
        return receivedType == _requiredType;
    }

    public void HandleNodeCorrect(NodeType receivedType)
    {
        foreach (var tm in _taskManagers) tm.AddConnection(receivedType);
    }

    public void HandleNodeIncorrect()
    {
        foreach (var tm in _taskManagers) tm.RemoveConnection(_requiredType);
    }
}
