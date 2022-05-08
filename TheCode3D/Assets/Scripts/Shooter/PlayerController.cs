using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour {

    [Header ("References")]
    public Camera playerCamera;
    [Header ("Physics")]
    public float gravityScale = -40f;
    [Header ("Player Settings")]
    public float walkSpeed = 5f;
    public float runSpeed = 20f;
    public float maxHealth = 100f;
    public float health = 100f;
    public float rotationSensibility = 1000f;
    public float jumpHeight = 1.9f;

    private bool run = false;
    private float cameraVerticalAngle;
    Vector3 moveInput = Vector3.zero;
    Vector2 rotationInput = Vector3.zero;
    CharacterController characterController;

    private void Awake() {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        characterController = GetComponent<CharacterController>();
    }

    public void SetMaxHealth(){
        health = maxHealth;
        EventManager.instance.UpdateLifeEvent.Invoke(health);
    }

    private void Update() {
        Look();
        Move();
    }

    private void Move() {
        if (characterController.isGrounded) {
            moveInput = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
            moveInput = Vector3.ClampMagnitude(moveInput, 1f);
            if (run)
                moveInput = transform.TransformDirection(moveInput) * runSpeed;
            else
                moveInput = transform.TransformDirection(moveInput) * walkSpeed;
            if (Input.GetButton("Jump"))
                moveInput.y = Mathf.Sqrt(jumpHeight * -2f * gravityScale);
        }
        moveInput.y += gravityScale * Time.deltaTime;
	    characterController.Move(moveInput * Time.deltaTime);
    }

    private void Look() {
        rotationInput.x = Input.GetAxis("Mouse X") * rotationSensibility * Time.deltaTime;
        rotationInput.y = Input.GetAxis("Mouse Y") * rotationSensibility * Time.deltaTime;
        cameraVerticalAngle = cameraVerticalAngle + rotationInput.y;
        cameraVerticalAngle = Mathf.Clamp(cameraVerticalAngle, -70f, 70f);
        transform.Rotate(Vector3.up * rotationInput.x);
        playerCamera.transform.localRotation = Quaternion.Euler(-cameraVerticalAngle, 0f, 0f);
    }

    public void Sprint() {
        run = true;
    }

    public void StopSprint() {
        run = false;
    }

    public void TakeDamage(float damage) {
        if (health - damage <= 0)
            health = 0;
        else 
            health -= damage;
        EventManager.instance.UpdateLifeEvent.Invoke(health);

    }

}
