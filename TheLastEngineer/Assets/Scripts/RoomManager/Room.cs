using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private NodeType _roomType;

    public NodeType RoomType {  get { return _roomType; } }

    public void Enable(bool enable)
    {
        gameObject.SetActive(enable);
    }
}
