using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Transform currentPos;
    public Transform startPos;
    public Transform optionsPos;
    public Transform playPos;
    public Transform instructionsPos;
    public Transform enterDoorPos;

    public float speedFactor = 0.1f;

    void Start()
    {
        Move_To_Start();
    }

    void Update()
    {
        transform.localPosition = Vector3.Lerp(transform.localPosition, currentPos.position, speedFactor);
        transform.localRotation = Quaternion.Slerp(transform.localRotation, currentPos.rotation, speedFactor);
    }

    public void Move_To_Start()
    {
        currentPos = startPos;
    }

    public void Move_To_Options()
    {
        currentPos = optionsPos;
    }

    public void Move_To_Play()
    {
        currentPos = playPos;
    }

    public void Move_To_Instructions()
    {
        currentPos = instructionsPos;
    }

    public void Move_To_EnterDoor()
    {
        currentPos = enterDoorPos;
    }

    public void StartLevel()
    {
        SceneManager.LoadScene("Level1-TheCave");
    }

    public void Quit()
    {
        Application.Quit();
    }

    //public Transform currentPos;
    //public Transform startPos;
    //public Transform optionsPos;
    //public Transform playPos;
    //public Transform instructionsPos;
    //public Transform enterDoorPos;

    //public float speedFactor = 0.1f;

    //void Update()
    //{
    //    transform.localPosition = Vector3.Lerp(transform.localPosition, currentPos.position, speedFactor);
    //    transform.localRotation = Quaternion.Slerp(transform.localRotation, currentPos.rotation, speedFactor);
    //}

    //public void Move_To_Start()
    //{
    //    currentPos = startPos;
    //}

    //public void Move_To_Options()
    //{
    //    currentPos = optionsPos;
    //}

    //public void Move_To_Play()
    //{
    //    currentPos = playPos;
    //}

    //public void Move_To_Instructions()
    //{
    //    currentPos = instructionsPos;
    //}

    //public void Move_To_EnterDoor()
    //{
    //    currentPos = enterDoorPos;
    //}

    //public void StartLevel()
    //{
    //    SceneManager.LoadScene("Level1-TheCave");
    //}

    //public void Quit()
    //{
    //    Application.Quit();
    //}
}
