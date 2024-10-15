public class NodeChecker
{
    private TaskManager _taskManager;
    private NodeType _requiredType;

    public NodeChecker(TaskManager taskManager, NodeType requiredType)
    {
        _taskManager = taskManager;
        _requiredType = requiredType;
    }

    public bool IsNodeCorrect(NodeType receivedType)
    {
        return receivedType == _requiredType;
    }

    public void HandleNodeCorrect(NodeType receivedType)
    {
        _taskManager.AddConnection(receivedType);
    }

    public void HandleNodeIncorrect(NodeType receivedType)
    {
        _taskManager.RemoveConnection(_requiredType);
    }
}
