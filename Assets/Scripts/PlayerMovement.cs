//#define USE_MRU
#define USE_MRUA
//#define USE_RIGIDBODY

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Animator m_Animator;
    private Rigidbody m_Rigidbody;
    public PlayerInput m_PlayerInput;

    [Header("Input variables")]
    //private bool m_ShootPressed = false;
    public Vector2 m_MoveInput = Vector2.zero;
    public bool m_ShiftPressed = false;


    [Header("Speed variables")]
    public float m_Speed = 10f;
    public float m_SpeedMultiplier = 1f;
    public float m_MaxSpeed = 10.0f;
    public float m_RotationSpeed = 0f;
    public float m_MaxRotationSpeed;
    //añadido por mi
    public float m_MaxRunningSpeed;

    [Header("Acceleration variables")]
    public float m_Acceleration = 1.0f;
    public float m_Deceleration = 1.0f;
    public float m_RotationAcceleration = 45f;
    public float m_RotationDamping;


    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();

#if USE_MRU
        m_Speed = 10f;
        m_RotationSpeed = 360f;

#elif USE_MRUA
        m_Speed = 0f;
        m_Acceleration = 5.0f;
        m_Deceleration = 5.0f;
        m_SpeedMultiplier = 1f;
        m_RotationSpeed = 0f;
        m_MaxRunningSpeed = 10f;
        m_MaxSpeed = m_MaxRunningSpeed/2;
        m_MaxRotationSpeed = 180f;
        m_RotationDamping = 10.0f;
        m_RotationAcceleration = 360f;

#elif USE_RIGIDBODY
        m_Speed = 10000f;
        m_SpeedMultiplier = 1f;
        m_RotationSpeed = 10000f;

#endif

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

#if USE_MRU || USE_RIGIDBODY

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

#elif USE_MRUA

        m_Animator.SetFloat("MovementSpeed", (m_Speed/m_MaxRunningSpeed));

#endif

    }
    private void Inputs()
    {
        m_MoveInput = m_PlayerInput.actions["Move"].ReadValue<Vector2>();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            m_ShiftPressed = true;
        }
        else
        {
            m_ShiftPressed = false;
        }
    }

    private void Movement(float dt)
    {
        float forwardMovement = m_MoveInput.y;

        m_SpeedMultiplier = m_ShiftPressed ? 2 : 1;


#if USE_MRU
        transform.position = transform.position + transform.forward * forwardMovement * m_Speed * dt;

#elif USE_MRUA

        if(forwardMovement!=0)
        {
            m_Speed += m_Acceleration * forwardMovement * dt;
            //[LO HECHO EN CLASE]
            //m_Speed = Mathf.Clamp(m_Speed, -m_MaxRunningSpeed, m_MaxRunningSpeed);

            //[MI VERSION] Si el shift esta presionado, la velocidad hace un lerp hasta la max running speed, si no, lo hace hasta la maxspeed normal
            m_Speed = m_ShiftPressed ? Mathf.Lerp(m_Speed, m_MaxRunningSpeed, m_Acceleration * dt) : Mathf.Lerp(m_Speed, m_MaxSpeed, m_Acceleration * dt);
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

        Vector3 movement = transform.forward * forwardMovement * m_Speed * m_SpeedMultiplier * dt;
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
            m_RotationSpeed = Mathf.Clamp(m_RotationSpeed, -m_MaxRotationSpeed, m_MaxRotationSpeed);
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

        //Si el jugador no presiona (A/D) el personaje no puede rotar, si los presiona, solo se bloquean la rotacion en los ejes X/Z
        if (horizontalMovement == 0)
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        else
        {
            m_Rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        float rotationY = m_RotationSpeed * horizontalMovement * dt;
        Debug.Log(rotationY);
        m_Rigidbody.AddTorque(0, rotationY, 0);

#endif
    }
}

