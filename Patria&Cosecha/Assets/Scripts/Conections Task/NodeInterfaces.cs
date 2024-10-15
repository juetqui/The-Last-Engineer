using UnityEngine;

public interface INodeEffect
{
    void PlayEffect(bool isError);
}

public interface INodeAudio
{
    void PlayClip(AudioClip audio);
}

public interface ITaskManager
{
    void AddConnection(NodeType nodeType);
    void RemoveConnection(NodeType nodeType);
}