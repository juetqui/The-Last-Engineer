using System.Collections.Generic;
using System;
using UnityEngine;

public class DarkRoomTest : MonoBehaviour
{
    [SerializeField] GameObject PreviousRoom;
    [SerializeField] GameObject CurrentRoom;
    private void OnTriggerEnter(Collider other)
    {
        PreviousRoom.SetActive(true);
        CurrentRoom.SetActive(false);
    }
   
}
