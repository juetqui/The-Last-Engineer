using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class RoomsTM : MonoBehaviour
{
    [SerializeField] private GenericConnectionController _connection;
    [SerializeField] private List<Room> _rooms = new List<Room>();
    
    private Dictionary<NodeType, Room> _roomsDic = new Dictionary<NodeType, Room>();

    private Room _connectedRoom = default;

    private void Awake()
    {
        _connection.OnNodeConnected += RecievedNode;

    }

    void Start()
    {
        foreach (Room room in _rooms)
        {
            if (room != null && !_roomsDic.ContainsKey(room.RoomType))
            {
                _roomsDic.Add(room.RoomType, room);
            }
        }
    }

    void Update()
    {
        
    }

    private void RecievedNode(NodeType node, bool connected)
    {
        if (!connected)
        {
            return;
        }

        UnableRooms();

        _connectedRoom = _roomsDic.Where(r => r.Key == node).FirstOrDefault().Value;
        _connectedRoom.gameObject.SetActive(true);
    }

    private void UnableRooms()
    {
        foreach (Room room in _rooms)
        {
            room.gameObject.SetActive(false);
        }
    }
}
