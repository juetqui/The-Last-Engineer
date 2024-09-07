using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float groundDrag;

    [Header("GroundCheck")]
    public float playerHeight;
    public LayerMask whatIsGround;
    private bool grounded;

    public Transform orientation;

    private float horizontalInput, verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    [SerializeField] private LayerMask _taskObjectsLayer;
    [SerializeField] private float _taskInteractionDistance = 2f;

    public float TaskInteractionDistance { get { return _taskInteractionDistance; } }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        MyInput();
        SpeedControl();
        InitTask();

        ResetLevel();
        
        rb.drag = groundDrag;
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //limit velocity if needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    private void InitTask()
    {
        RaycastHit hit;

        Vector3 rayPos = new Vector3(transform.position.x, transform.position.y + 2, transform.position.z);

        if (Physics.Raycast(rayPos, transform.forward, out hit, _taskInteractionDistance, _taskObjectsLayer))
        {
            CowsTaskManager taskObject = hit.transform.gameObject.GetComponent<CowsTaskManager>();

            if (Input.GetKeyDown(KeyCode.E)) taskObject.StartTask();
        }
    }

    private void ResetLevel()
    {
        if (Input.GetKeyDown(KeyCode.R)) SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
