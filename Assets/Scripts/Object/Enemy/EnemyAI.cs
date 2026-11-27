using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public float WaitTime => waitTime;
    
    public EnemyAIConfig Config => config;
    public EnemyCombat Combat => combat;
    public NavMeshAgent Agent => agent;
    public EnemyChaseState ChaseState => chaseState;
    public EnemyPatrolState PatrolState => patrolState;
    public EnemyReturnState ReturnState => returnState;
    public EnemyWaitState WaitState => waitState;
    public Transform[] patrolPoints;
    private NavMeshAgent agent;
    private StateMachine stateMachine;
    private EnemyChaseState chaseState;
    private EnemyDeadState deadState;
    private EnemyPatrolState patrolState;
    private EnemyReturnState returnState;
    private EnemyWaitState waitState;
    [SerializeField] private float waitTime = 2f;
    private int currentIndex = 0;
    private EnemyAIConfig config;
    private EnemyCombat combat;
    private CharacterHealth health;
   

    private void Awake()
    {
        stateMachine = new StateMachine();
        chaseState = new EnemyChaseState(this);
        deadState = new EnemyDeadState(this);
        patrolState = new EnemyPatrolState(this);
        waitState = new EnemyWaitState(this);
        returnState = new EnemyReturnState(this);
        config = Resources.Load<EnemyAIConfig>("GameData/Enemy/EnemyAIConfig");
        combat = GetComponent<EnemyCombat>();
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<CharacterHealth>();
    }

    public void ChangeState(IState state)
    {
        stateMachine.ChangeState(state);
    }

    private void OnPlayerStateChange(bool arg0)
    {
        //TODO 后续攻击状态也直接返回
        if (stateMachine.CurrentState == chaseState)
            ChangeState(returnState);
    }

    private void Start()
    {
        stateMachine.ChangeState(patrolState);
    }

    private void OnEnable()
    {
        PlayerMgr.Instance.OnPlayerStateChange += OnPlayerStateChange;
        health.OnDeath += Dead;
    }

    private void OnDisable()
    {
        //考虑到后面接入缓存池所以使用这个生命周期
        PlayerMgr.Instance.OnPlayerStateChange -= OnPlayerStateChange;
        health.OnDeath -= Dead;
    }

    private void Dead()
    {
        ChangeState(deadState);
    }
    private void Update()
    {
        stateMachine.Update();
    }

    public bool PlayerIsDead()
    {
        return PlayerMgr.Instance.player.Health.isDead;
    }

    public bool CanLookPlayer()
    {
        Transform playerTransform = PlayerMgr.Instance.player.transform;
        return Vector3.Distance(playerTransform.position, transform.position) < config.detectRange && !PlayerIsDead();
    }

    public bool CanAttackPlayer()
    {
        Transform playerTransform = PlayerMgr.Instance.player.transform;
        return Vector3.Distance(playerTransform.position, transform.position) < config.attackRange;
    }
    
    public void MoveToCurrentPatrolPoint()
    {
        if (patrolPoints.Length <= 0 || patrolPoints == null)
            return;
        Transform point = patrolPoints[currentIndex];
        if (point == null)
            return;
        agent.SetDestination(point.position);
    }
    public void NextPatrolIndex()
    {
        currentIndex++;
        if (currentIndex >= patrolPoints.Length)
            currentIndex = 0;
    }

    public bool HasArrived()
    {
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;
    }

    public void ResetAI(Vector3 spawnPos,Transform[] patrolPoint)
    {
        agent.Warp(spawnPos);
        this.patrolPoints = patrolPoint;
        currentIndex = 0;
        ChangeState(PatrolState);
    }
}