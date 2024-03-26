using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerCameraController : MonoBehaviour
{
    public bool m_ControlRotation;

    // Reference to the player to follow with the camera.
    public Transform player;

    // Ideal camera offset relative to the player.
    public Vector3 idealOffset = new Vector3(0.0f, 2.5f, -5.0f);
    // The ideal point for the camera to look at relative to the player.
    public Vector3 idealLookOffset = new Vector3(0.0f, 1.0f, 10.0f);

    // Spring constant for camera movement calculation.
    public float springK = 15.0f;
    // Damping coefficient to avoid excessive oscillations.
    public float damp = 10.0f;
    // Maximum allowed distance from the camera's ideal position.
    public float maxDistFromIdealPos = 2.0f;

    // Current velocity of the camera, used for smooth movement.
    private Vector3 vel = new Vector3(0.0f, 0.0f, 0.0f);

    private Vector2 mouseInput; // To store mouse input

    // Reference to the Input Action asset
    private PlayerInput playerInput;
    private InputAction lookAction;
    public float rotationSpeed = 5.0f; // Speed of rotation

    void Start()
    {
        // Initialize the PlayerInput component and InputAction
        playerInput = FindObjectOfType<PlayerInput>();
        lookAction = playerInput.actions["Look"]; // "Look" is the action name in your Input Actions asset
    }

    private void Update()
    {
        // Read the mouse input from the "Look" action
        mouseInput = lookAction.ReadValue<Vector2>();
    }

    // LateUpdate is called after all Update functions.
    void LateUpdate()
    {
        // Calculate the time passed since the last frame for movement calculations.
        float dt = Time.deltaTime;

        Vector3 idealPos;
        Vector3 lookAtIdealOffset;

        // Calculate the ideal camera position and look at offset based on the player's current position.
        CalcIdealCamPositions(out idealPos, out lookAtIdealOffset);

        Vector3 pos = transform.position;
        // Apply a spring-damper system to move the camera towards its ideal position smoothly and realistically.
        ApplySpringDumperSystem(idealPos, dt, ref pos);

        // Limit the camera's movement to not exceed a certain distance from its ideal position.
        LimitMovement(idealPos, ref pos);


        // Update the camera's position.
        transform.position = pos;

        // Make the camera look at the calculated point.
        transform.LookAt(lookAtIdealOffset, new Vector3(0.0f, 1.0f, 0.0f));
    }
    private float angleX = 0;
    private float angleY = 0;
    public Transform m_CameraPos;
    // Calculates the ideal positions for the camera and where it should look at based on the player's position.
    private void CalcIdealCamPositions(out Vector3 idealPos, out Vector3 idealLookPos)
    {
        if (m_ControlRotation)
        {
            mouseInput = lookAction.ReadValue<Vector2>();

            angleX += mouseInput.x * rotationSpeed * Time.deltaTime;
            angleY += mouseInput.y * rotationSpeed * Time.deltaTime;

            // Clamp angleY between -90 and 90 degrees to prevent flipping
            angleY = Mathf.Clamp(angleY, -90f, 90f);

            // Calculate new position
            Quaternion rotation = Quaternion.Euler(angleY, angleX, 0);
            Vector3 direction = rotation * Vector3.forward;
            idealPos = player.transform.position + direction * 3 + Vector3.up * 1.5f;


            Vector3 idealLookOffset = idealOffset;
            idealLookOffset.z = 0;
            idealLookPos = player.TransformPoint(idealLookOffset); 
        }
        else
        {
            // Transforms the idealOffset to world space relative to the player's position.
            idealPos = player.TransformPoint(idealOffset);
            // Transforms the idealLookOffset to world space relative to the player's position.
            idealLookPos = player.TransformPoint(this.idealLookOffset); // Adjusted to use the class variable directly
        }
    }

    // Applies a spring-damper system for smooth camera movement towards its ideal position.
    private void ApplySpringDumperSystem(Vector3 idealPos, float dt, ref Vector3 pos)
    {
        // Calculate damping velocity factor to reduce oscillation.
        float dampVelFactor = Mathf.Max(0.0f, 1.0f - damp * dt);
        vel = vel * dampVelFactor;

        // Calculate the offset and spring acceleration towards the ideal position.
        Vector3 offset = idealPos - pos;
        Vector3 springAccel = springK * offset;

        // Update velocity considering damping.
        vel += springAccel * dt;

        // Update position based on velocity.
        pos += vel * dt;
    }

    // Limits the camera's movement to ensure it doesn't stray too far from its ideal position.
    private void LimitMovement(Vector3 idealPos, ref Vector3 pos)
    {
        // Calculate direction and distance to the ideal position.
        Vector3 dirToIdealPos = idealPos - pos;
        float distToIdealPos = dirToIdealPos.magnitude;

        // If the camera is further than allowed, adjust its position.
        if (distToIdealPos > maxDistFromIdealPos)
        {
            Vector3 targetPos = idealPos + (maxDistFromIdealPos / distToIdealPos) * dirToIdealPos;
            // Suaviza el movimiento hacia el targetPos.
            pos = Vector3.Lerp(pos, targetPos, Time.deltaTime);
        }
    }
}
