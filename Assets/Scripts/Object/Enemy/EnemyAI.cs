using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    private NavMeshAgent agent;
    [SerializeField] private float waitTime = 2f;
    private int currentIndex = 0;
    private bool isWaiting = false;
    private float waitUntil;
    private EnemyAIConfig config;
    private float playerDistance;
    private float currentWay;
    private bool isFollowUp = false;
    private Vector3 startPos;
    private bool isReturning;
    private EnemyCombat combat;
    private CharacterHealth health;
    private void Awake()
    {
        config = Resources.Load<EnemyAIConfig>("GameData/Enemy/EnemyAIConfig");
        combat = GetComponent<EnemyCombat>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<CharacterHealth>();
    }

    private void Start()
    {
        UpdatePatrolPoint();
    }

    private void Update()
    {
        if (health.isDead)
        {
            agent.isStopped = true;
            return;
        }
        CheckPlayer();
        if (isWaiting)
        {
            if (Time.time > waitUntil)
            {
                isWaiting = false;
                agent.isStopped = false;
            }
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance && !isFollowUp&&!agent.pathPending)
        {
            isReturning = false;
            isWaiting = true;
            agent.isStopped = true;
            waitUntil = Time.time + waitTime;
            currentIndex++;
            if (currentIndex >= patrolPoints.Length)
            {
                currentIndex = 0;
            }
            UpdatePatrolPoint();
        }
    }

    public void CheckPlayer()
    {
        if (isReturning)
            return;
        playerDistance = Vector3.Distance(transform.position,PlayerMgr.Instance.player.transform.position);
        if (isFollowUp)
        {
            agent.SetDestination(PlayerMgr.Instance.player.transform.position);
            currentWay = Vector3.Distance(transform.position, startPos);
            print(currentWay);
            if (playerDistance > config.loseRange
                ||
                currentWay > config.maxRange)
            {
                LoseTarget();
            }
        }
        if (playerDistance < config.detectRange && !isFollowUp)
        {
            if (isReturning)
                return;
            isReturning = false;
            isFollowUp = true;
            isWaiting = false;
            agent.isStopped = false;
            startPos = transform.position;
            agent.SetDestination(PlayerMgr.Instance.player.transform.position);
            print("开始追击");
        }

        if (playerDistance < config.attackRange)
        {
            combat.TryAttack();
        }
    }

    public void LoseTarget()
    {
        isFollowUp = false;
        isReturning = true;
        UpdatePatrolPoint();
    }
    public void UpdatePatrolPoint()
    {
        if (patrolPoints.Length <= 0|| patrolPoints==null)
            return;
        Transform point = patrolPoints[currentIndex];
        if (point == null)
            return;
        agent.SetDestination(point.position);
    }
}
