using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public PlayerInput m_PlayerInput;

    public Vector2 m_MoveInput = Vector2.zero;
    public bool m_IsRunning = false;
    private bool m_MeleeAttackInput = false;
    private bool m_IsAttacking = false;
    private int m_MeleeAttackCounter = 0;

    public float m_CurrentSpeed = 5.0f;
    public float m_MaxSpeedWalking = 10.0f;
    public float m_MaxSpeedRunning = 10.0f;
    public float m_Acceleration = 1.0f;
    public float m_Deceleration = 1.0f;

    public float m_CurrentRotationSpeed;
    public float m_MaxRotationSpeed;
    public float m_RotationAcceleration;
    public float m_RotationDamping;

    public Animator m_Animator;
    // Start is called before the first frame update
    void Start()
    {
        m_Animator = GetComponent<Animator>();
        m_PlayerInput = GetComponent<PlayerInput>();

        m_MaxSpeedRunning = m_MaxSpeedWalking * 2;
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;

        Inputs();
        Combat();
        Movement(dt);
        Rotation(dt);
        PlayAnimations();
    }

    private void Inputs()
    {
        m_MoveInput = m_PlayerInput.actions["Movement"].ReadValue<Vector2>();
        m_IsRunning = m_PlayerInput.actions["Running"].ReadValue<float>() > 0f;
        m_MeleeAttackInput = m_PlayerInput.actions["MeleeAttack"].WasPressedThisFrame();
    }

    private void Movement(float dt)
    {
        if (m_IsAttacking)
        {
            return;
        }

        float forwardMovement = m_MoveInput.y;

        float currentMaxSpeed = m_IsRunning && forwardMovement > 0 ? m_MaxSpeedRunning : m_MaxSpeedWalking;

        if (forwardMovement != 0)
        {
            m_CurrentSpeed += m_Acceleration * forwardMovement * dt;
            m_CurrentSpeed = Mathf.Clamp(m_CurrentSpeed, -currentMaxSpeed, currentMaxSpeed);
        }
        else
        {
            if (Mathf.Abs(m_CurrentSpeed) > 0.1f)
            {
                m_CurrentSpeed = Mathf.Lerp(m_CurrentSpeed, 0, m_Deceleration * dt);
            }
            else
            {
                m_CurrentSpeed = 0;
            }
        }

        Vector3 newPosition = transform.position + transform.forward * m_CurrentSpeed * dt;

        transform.position = newPosition;
    }

    private void Rotation(float dt)
    {
        if (m_IsAttacking)
        {
            return;
        }

        float horizontalMovement = m_MoveInput.x;
        if (horizontalMovement != 0)
        {
            m_CurrentRotationSpeed += horizontalMovement *
                m_RotationAcceleration * dt;
            m_CurrentRotationSpeed = Mathf.Clamp(m_CurrentRotationSpeed, -m_MaxRotationSpeed, m_MaxRotationSpeed);
        }
        else
        {
            if (Mathf.Abs(m_CurrentRotationSpeed) > 0.1f)
            {
                m_CurrentRotationSpeed = Mathf.Lerp(m_CurrentRotationSpeed, 0, m_RotationDamping * dt);
            }
            else
            {
                m_CurrentRotationSpeed = 0;
            }
        }
        float rotationY = m_CurrentRotationSpeed * dt;
        transform.Rotate(0, rotationY, 0);
    }

    private void PlayAnimations()
    {
        m_Animator.SetFloat("MovementSpeed", NormalizeNumbersTo01(m_CurrentSpeed,m_MaxSpeedRunning));
        
        if(m_MeleeAttackInput && !m_IsAttacking)
        {
            //First melee attack
            m_MeleeAttackCounter = 1;
            m_Animator.SetTrigger("MeleeAttack1");
        }
    }

    private void Combat()
    {
        if (m_MeleeAttackInput && m_IsAttacking) m_MeleeAttackCounter++;
        Mathf.Clamp(m_MeleeAttackCounter, 1, 3);
        m_Animator.SetBool("isAttacking", m_IsAttacking);
    }

    private void BeginMeleeAttack()
    {
        m_CurrentSpeed = 0;
        m_IsAttacking = true;
    }

    private void ChangeToSecondMelee()
    {
        if (m_MeleeAttackCounter >= 2)
        {
            m_MeleeAttackCounter = 2;
            m_Animator.SetTrigger("MeleeAttack2");
        }
    }

    private void ChangeToThirdMelee()
    {
        if (m_MeleeAttackCounter >= 3)
        {
            m_Animator.SetTrigger("MeleeAttack3");
        }
    }

    private void EndMeleeAttack()
    {
        m_IsAttacking = false;
    }

    private float NormalizeNumbersTo01(float currentNumber, float maxNumber)
    {
        if (maxNumber == 0 || currentNumber == 0) return 0;
        return currentNumber / maxNumber;
    }
}