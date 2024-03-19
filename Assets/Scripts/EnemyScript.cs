using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyScript : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Wait,
        Attack
    }

    [Header("States")]
    public State m_CurrentState;

    [Header("Navigation vars")]
    public NavMeshAgent m_Agent;
    public Transform[] m_WayPoints;
    public int m_CurrentWayPoint = 0;

    [Header("Timer vars")]
    public float m_WaitingTime = 3f;
    private float m_RemaningWaitTime = 3f;

    [Header("Attack vars")]
    public float m_AttackDistance = 5.0f;
    public float m_FollowDistance = 5.0f;
    public Transform m_Player;

    //Animator
    private Animator m_Animator;

    //Rotation
    public float m_RotationSpeed = 5.0f;



    // Start is called before the first frame update
    void Start()
    {
        m_Agent = GetComponent<NavMeshAgent>();
        m_Player = GameObject.FindGameObjectWithTag("Player").transform;
        m_Animator = GetComponent<Animator>();
        TransitionToState(State.Wait);
    }

    // Update is called once per frame
    void Update()
    {
        float dt = Time.deltaTime;
        OnState(dt);
    }

    private void TransitionToState(State newState)
    {
        OnStateExit(m_CurrentState);
        m_CurrentState = newState;
        OnStateEnter(newState);
    }

    private void OnState(float dt)
    {
        switch (m_CurrentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Wait:
                Wait(dt);
                break;
            case State.Attack:
                Attack(dt);
                break;
            default:
                break;
        }
    }

    private void OnStateEnter(State thisState)
    {
        switch (thisState)
        {
            case State.Patrol:
                m_Animator.SetBool("IsFiring", false);
                m_Agent.isStopped = false;
                m_Agent.SetDestination(m_WayPoints[m_CurrentWayPoint].position);
                break;
            case State.Wait:
                m_Animator.SetBool("IsWalking", false);
                m_Animator.SetBool("IsFiring", false);
                m_RemaningWaitTime = m_WaitingTime;
                break;
            case State.Attack:
                m_Agent.isStopped = false;
                m_Agent.SetDestination(m_Player.position);
                break;
            default:
                break;
        }
    }

    private void OnStateExit(State thisState) 
    {
        switch (thisState)
        {
            case State.Patrol:
                m_CurrentWayPoint = (m_CurrentWayPoint + 1) % m_WayPoints.Length;
                break;
            case State.Wait:
                break;
            case State.Attack:
                m_Animator.SetBool("IsFiring", false);
                break;
            default:
                break;
        }
    }

    private void Patrol()
    {
        //Devuelve true si aun se esta calculando el path
        if (!m_Agent.pathPending)
        {
            m_Animator.SetBool("IsWalking", true);
            if (m_Agent.remainingDistance < 0.5f)
            {
                TransitionToState(State.Wait);
            }
        }
    }

    private void Wait(float dt)
    {
        if (m_RemaningWaitTime > 0) 
        {
            m_RemaningWaitTime -= dt;
        }
        else
        {
            TransitionToState(State.Patrol);
        }
    }

    private void Attack(float dt)
    {
        m_Agent.SetDestination(m_Player.position);
        if(m_Player!=null && !m_Agent.pathPending) 
        {
            if (m_Agent.remainingDistance <= m_AttackDistance)
            {
                RotateTowardsPlayer(dt);
                m_Animator.SetBool("IsWalking", false);
                m_Animator.SetBool("IsFiring", true);
                m_Agent.isStopped = true;
            }
            else if (m_Agent.remainingDistance > m_FollowDistance)
            {
                m_Animator.SetBool("IsWalking", true);
                m_Animator.SetBool("IsFiring", false);
                m_Agent.isStopped = false;
            }
        }
    }

    private void RotateTowardsPlayer(float dt)
    {
        Vector3 targetDirection = m_Player.position - transform.position;

        float singleStep = m_RotationSpeed * dt;

        Vector3 newDirection = Vector3.RotateTowards(transform.forward, targetDirection, singleStep, 0.0f);

        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            TransitionToState(State.Attack);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            TransitionToState(State.Wait);
        }
    }

}

