#define USE_MRU
//#define USE_MRUA
//#define USE_RIGIDBODY

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Animator m_Animator;

    public PlayerInput m_PlayerInput;

    private bool m_ShootPressed = false;
    public Vector2 m_MoveInput = Vector2.zero;

    public float m_Speed = 10f;
    public float m_MaxSpeed = 10.0f;
    public float m_Acceleration = 1.0f;
    public float m_Deceleration = 1.0f;

    public float m_RotationSpeed = 0f;
    public float m_MaxRotationSpeed;
    public float m_RotationAcceleration = 45f;
    public float m_RotationDamping;

    private Rigidbody m_Rigidbody;

    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();

        m_Speed = 0f;
        m_RotationSpeed = 0f;
        m_MaxSpeed = 5f;
        m_MaxRotationSpeed = 11180.0f;
        m_RotationDamping = 10.0f;
        m_RotationAcceleration = 90.0f;
        m_PlayerInput = GetComponent<PlayerInput>();
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        AnimationControl();

        Inputs();
        Movement(dt);
        Rotation(dt);

    }

    private void AnimationControl()
    {
        float speed = 0;
        if (Input.GetKey(KeyCode.W))
        {
            speed = 0.5f;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                speed = 1f;
            }
        }
        m_Animator.SetFloat("MovementSpeed", speed);
    }
    private void Inputs()
    {
        m_MoveInput = m_PlayerInput.actions["Move"].ReadValue<Vector2>();
    }

    private void Movement(float dt)
    {
        float forwardMovement = m_MoveInput.y;

#if USE_MRU
        transform.position = transform.position + transform.forward * forwardMovement * m_Speed * dt;

#elif USE_MRUA

        if(forwardMovement!=0)
        {
            m_Speed += m_Acceleration * forwardMovement * dt;
            Mathf.Clamp(m_Speed, -m_MaxSpeed, m_MaxSpeed);
        }
        else
        {
            if(Mathf.Abs(m_Speed)>0.1)
            {
                m_Speed = Mathf.Lerp(m_Speed, 0, m_Deceleration * dt);
            }
            else
            {
                m_Speed = 0;
            }
        }


        Vector3 newPosition = transform.position + transform.forward * m_Speed * dt;

        transform.position = newPosition;

#elif USE_RIGIDBODY

        Vector3 movement = transform.forward * forwardMovement * m_Speed * dt;
        m_Rigidbody.AddForce(movement);


#endif
    }

    private void Rotation(float dt)
    {
        float horizontalMovement = m_MoveInput.x;

#if USE_MRU
        float rotationY = horizontalMovement * m_RotationSpeed * dt;
        transform.Rotate(0, rotationY, 0);

#elif USE_MRUA

        if(horizontalMovement != 0)
        {
            m_RotationSpeed += horizontalMovement * m_RotationAcceleration * dt;
            Mathf.Clamp(m_RotationSpeed, -m_MaxRotationSpeed, m_MaxRotationSpeed);
        }
        else
        {
            if(Mathf.Abs(m_RotationSpeed) > 0.1f)
            {
                m_RotationSpeed = Mathf.Lerp(m_RotationSpeed, 0, m_RotationDamping * dt);
            }
            else
            {
                m_RotationSpeed = 0;
            }
        }

        float rotationY = m_RotationSpeed * dt;
        transform.Rotate(0, rotationY, 0);

#elif USE_RIGIDBODY
        float rotationY = m_RotationSpeed * horizontalMovement * dt;
        m_Rigidbody.AddTorque(0, rotationY, 0);

#endif
    }
}

