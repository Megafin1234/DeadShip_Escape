using UnityEngine;
using UnityEngine.AI;

public class CompanionAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTarget;
    [SerializeField] private PlayerCombatContext combatContext;
    [SerializeField] private CompanionShooter shooter;
    [SerializeField] private NavMeshAgent agent;

    [Header("Follow")]
    [SerializeField] private Vector3 followOffset = new Vector3(-1.2f, 0f, -0.8f);
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float teleportDistance = 12f;

    [Header("Combat")]
    [SerializeField] private float attackRange = 8f;
    [SerializeField] private float reactionDelay = 0.2f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private LayerMask lineOfSightMask = Physics.DefaultRaycastLayers;
    [SerializeField] private float lineOfSightTargetHeightOffset = 1f;
    [SerializeField] private float repositionDistanceWhenNoLOS = 1.2f;
    [SerializeField] private float noLineOfSightRepathInterval = 0.35f;
    

    

    [Header("Strafe")]
    [SerializeField] private float strafeDistance = 2f; //목표 위치까지의 거리
    [SerializeField] private float strafeSpeed = 3.8f; //움직임 속도
    [SerializeField] private float minStrafeInterval = 1.2f;//움직임 발생 체크가 발생할 최소시간
    [SerializeField] private float maxStrafeInterval = 2.0f; //최대시간
    [SerializeField] private float strafeDecisionChance = 0.8f; //움직임 발생 체크 시 실제 움직임이 발생할 확률
    [SerializeField] private float maxStrafeDuration = 0.9f; //한 번 움직이면 얼마나 움직이나- 강제 종료 조건
    [SerializeField] private float maxDistanceFromPlayerWhileStrafing = 4.5f; // 플레이어 반경 어디까지 자유롭게 움짃이나

    [Header("Recovery")]
    [SerializeField] private float stuckVelocityThreshold = 0.05f;
    [SerializeField] private float stuckCheckDelay = 1.0f;
    [SerializeField] private float stuckRecoveryDistance = 6f;
    [SerializeField] private float repathInterval = 0.2f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugLog = false;

    private Transform currentTarget;
    public Transform firePoint;
    private float reactionTimer = 0f;

    private float stuckTimer = 0f;
    private float repathTimer = 0f;

    private bool isStrafing = false;
    private float strafeTimer = 0f;
    private float nextStrafeDecisionTime = 0f;
    private Vector3 strafeTarget;
    private float defaultAgentSpeed;

    private enum CompanionState
    {
        Follow,
        Combat
    }

    private CompanionState currentState = CompanionState.Follow;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (shooter == null)
            shooter = GetComponent<CompanionShooter>();

        if (agent != null)
        {
            agent.updateRotation = false;
            defaultAgentSpeed = agent.speed;
        }
    }

    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;

        if (playerTarget != null && combatContext == null)
            combatContext = playerTarget.GetComponent<PlayerCombatContext>();
    }

    private void Update()
    {
        if (playerTarget == null || agent == null || !agent.enabled)
            return;

        if (reactionTimer > 0f)
            reactionTimer -= Time.deltaTime;

        if (repathTimer > 0f)
            repathTimer -= Time.deltaTime;

        HandleTeleportIfTooFar();

        currentTarget = combatContext != null ? combatContext.CurrentTarget : null;
        currentState = currentTarget != null ? CompanionState.Combat : CompanionState.Follow;

        switch (currentState)
        {
            case CompanionState.Combat:
                HandleCombat();
                break;

            case CompanionState.Follow:
            default:
                HandleFollow();
                break;
        }

        HandleStuckRecovery();
    }

    private void HandleFollow()
    {
        isStrafing = false;

        Vector3 desiredPos = playerTarget.position + followOffset;

        if (!TryGetNearestNavMeshPoint(desiredPos, 2f, out Vector3 navPos))
            return;

        float dist = Vector3.Distance(transform.position, navPos);

        if (dist <= stopDistance)
        {
            agent.ResetPath();
            return;
        }

        agent.speed = defaultAgentSpeed;
        agent.isStopped = false;

        if (repathTimer <= 0f)
        {
            agent.SetDestination(navPos);
            repathTimer = repathInterval;
        }

        RotateByVelocity();
    }

    private void HandleCombat()
    {
        if (currentTarget == null)
            return;

        if (isStrafing)
        {
            ExecuteStrafe();
            return;
        }

        Vector3 targetPos = currentTarget.position;
        float dist = Vector3.Distance(transform.position, targetPos);
        bool inRange = dist <= attackRange;
        bool hasLOS = HasLineOfSightToTarget();

        // 1) 사거리 밖이면 기본 접근
        if (!inRange)
        {
            agent.speed = defaultAgentSpeed;
            agent.isStopped = false;

            if (TryGetNearestNavMeshPoint(targetPos, 2f, out Vector3 navPos))
            {
                if (repathTimer <= 0f)
                {
                    agent.SetDestination(navPos);
                    repathTimer = repathInterval;
                }
            }

            RotateTowardTarget();
            return;
        }

        // 2) 사거리 안인데 LOS가 없으면,
        //    무조건 얼굴 비비듯이 직진하지 말고 약간 위치를 바꿔가며 각을 찾음
        if (!hasLOS)
        {
            agent.speed = defaultAgentSpeed;
            agent.isStopped = false;

            Vector3 toTarget = (currentTarget.position - transform.position).normalized;
            toTarget.y = 0f;

            Vector3 sideDir = Vector3.Cross(Vector3.up, toTarget).normalized;
            if (sideDir == Vector3.zero)
                sideDir = transform.right;

            // 좌/우 중 하나를 랜덤하게 골라 짧게 재배치
            if (Random.value < 0.5f)
                sideDir = -sideDir;

            Vector3 repositionCandidate = transform.position + sideDir * repositionDistanceWhenNoLOS;

            if (TryGetNearestNavMeshPoint(repositionCandidate, 1.5f, out Vector3 repositionPos))
            {
                if (repathTimer <= 0f)
                {
                    agent.SetDestination(repositionPos);
                    repathTimer = noLineOfSightRepathInterval;
                }
            }
            else
            {
                // 재배치 실패하면 타겟 쪽으로만 살짝 접근
                if (TryGetNearestNavMeshPoint(targetPos, 2f, out Vector3 fallbackPos))
                {
                    if (repathTimer <= 0f)
                    {
                        agent.SetDestination(fallbackPos);
                        repathTimer = noLineOfSightRepathInterval;
                    }
                }
            }

            RotateTowardTarget();
            return;
        }

        // 3) 사거리 안 + LOS 확보면 hold + strafe + 사격
        agent.speed = defaultAgentSpeed;
        agent.isStopped = true;
        agent.ResetPath();

        RotateTowardTarget();

        if (Time.time >= nextStrafeDecisionTime)
        {
            nextStrafeDecisionTime = Time.time + Random.Range(minStrafeInterval, maxStrafeInterval);

            if (Random.value < strafeDecisionChance)
            {
                TryStartStrafe();
            }
        }

        if (reactionTimer <= 0f && shooter != null && !shooter.IsReloading())
        {
            bool fired = shooter.TryFire();
            if (fired)
                reactionTimer = reactionDelay;
        }
    }

    private void TryStartStrafe()
    {
        if (currentTarget == null || agent == null)
            return;

        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distToPlayer > maxDistanceFromPlayerWhileStrafing)
            return;

        Vector3 toTarget = (currentTarget.position - transform.position).normalized;
        toTarget.y = 0f;

        if (toTarget == Vector3.zero)
            return;

        Vector3 sideDir = Vector3.Cross(Vector3.up, toTarget).normalized;

        if (Random.value < 0.5f)
            sideDir = -sideDir;

        Vector3 candidate = transform.position + sideDir * strafeDistance;

        if (TryGetNearestNavMeshPoint(candidate, 1.5f, out Vector3 navPos))
        {
            strafeTarget = navPos;
            isStrafing = true;
            strafeTimer = maxStrafeDuration;

            agent.speed = strafeSpeed;
            agent.isStopped = false;
            agent.SetDestination(strafeTarget);

            if (enableDebugLog)
                Debug.Log("[CompanionAI] Strafe 시작");
        }
    }

    private void ExecuteStrafe()
    {
        if (agent == null)
            return;

        strafeTimer -= Time.deltaTime;

        if (currentTarget != null)
        {
            Vector3 lookDir = currentTarget.position - transform.position;
            lookDir.y = 0f;

            if (lookDir != Vector3.zero)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRot,
                    rotationSpeed * Time.deltaTime
                );
            }
        }

        // strafing 중에 사격 
        if (reactionTimer <= 0f && shooter != null && !shooter.IsReloading() && HasLineOfSightToTarget())
        {
            bool fired = shooter.TryFire();
            if (fired)
                reactionTimer = reactionDelay;
        }

        bool arrived =
            !agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance + 0.05f;

        bool timedOut = strafeTimer <= 0f;

        bool tooFarFromPlayer =
            Vector3.Distance(transform.position, playerTarget.position) > maxDistanceFromPlayerWhileStrafing;

        if (arrived || timedOut || tooFarFromPlayer)
        {
            isStrafing = false;
            agent.speed = defaultAgentSpeed;
            agent.isStopped = true;
            agent.ResetPath();

            if (enableDebugLog)
                Debug.Log("[CompanionAI] Strafe 종료");
        }
    }

    private void HandleTeleportIfTooFar()
    {
        float dist = Vector3.Distance(transform.position, playerTarget.position);
        if (dist <= teleportDistance)
            return;

        WarpNearPlayer("TooFar");
    }

    private void HandleStuckRecovery()
    {
        if (!agent.hasPath)
        {
            stuckTimer = 0f;
            return;
        }

        bool farEnough = agent.remainingDistance > stopDistance + 0.5f;
        bool barelyMoving = agent.velocity.sqrMagnitude < (stuckVelocityThreshold * stuckVelocityThreshold);

        if (farEnough && barelyMoving)
        {
            stuckTimer += Time.deltaTime;

            if (stuckTimer >= stuckCheckDelay)
            {
                if (enableDebugLog)
                    Debug.Log("[CompanionAI] 정체 감지 -> 복구 시도");

                bool recovered = TryRecoverFromStuck();
                stuckTimer = 0f;

                if (!recovered && enableDebugLog)
                    Debug.Log("[CompanionAI] 정체 복구 실패");
            }
        }
        else
        {
            stuckTimer = 0f;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
        {
            if (enableDebugLog)
                Debug.Log("[CompanionAI] PathInvalid 감지 -> 플레이어 근처로 워프");

            WarpNearPlayer("PathInvalid");
            stuckTimer = 0f;
        }
    }

    private bool TryRecoverFromStuck()
    {
        if (currentState == CompanionState.Combat && currentTarget != null)
        {
            if (TryGetNearestNavMeshPoint(currentTarget.position, 2f, out Vector3 combatPos))
            {
                agent.ResetPath();
                agent.SetDestination(combatPos);

                if (enableDebugLog)
                    Debug.Log("[CompanionAI] 전투 목적지 재지정");
                return true;
            }
        }
        else
        {
            Vector3 desiredPos = playerTarget.position + followOffset;
            if (TryGetNearestNavMeshPoint(desiredPos, 2f, out Vector3 followPos))
            {
                agent.ResetPath();
                agent.SetDestination(followPos);

                if (enableDebugLog)
                    Debug.Log("[CompanionAI] 추종 목적지 재지정");
                return true;
            }
        }

        float distToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        if (distToPlayer > stuckRecoveryDistance)
        {
            WarpNearPlayer("StuckRecovery");
            return true;
        }

        return false;
    }

    private void WarpNearPlayer(string reason)
    {
        Vector3 desiredPos = playerTarget.position + followOffset;

        if (TryGetNearestNavMeshPoint(desiredPos, 3f, out Vector3 safePos))
        {
            agent.ResetPath();
            agent.Warp(safePos);

            repathTimer = 0f;
            stuckTimer = 0f;
            isStrafing = false;

            if (enableDebugLog)
                Debug.Log($"[CompanionAI] 플레이어 근처 워프 / reason={reason}");
        }
    }

    private void RotateTowardTarget()
    {
        if (currentTarget == null)
            return;

        Vector3 lookDir = currentTarget.position - transform.position;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }
    private void RotateByVelocity()
    {
        if (agent.velocity.sqrMagnitude <= 0.01f)
            return;

        Vector3 lookDir = agent.velocity.normalized;
        lookDir.y = 0f;

        if (lookDir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private bool TryGetNearestNavMeshPoint(Vector3 source, float maxDistance, out Vector3 result)
    {
        if (NavMesh.SamplePosition(source, out NavMeshHit hit, maxDistance, NavMesh.AllAreas))
        {
            result = hit.position;
            return true;
        }

        result = source;
        return false;
    }

    private bool HasLineOfSightToTarget()
    {
        if (currentTarget == null || firePoint == null)
            return false;

        Vector3 origin = firePoint.position;
        Vector3 target = currentTarget.position + Vector3.up * lineOfSightTargetHeightOffset;
        Vector3 direction = (target - origin).normalized;
        float distance = Vector3.Distance(origin, target);

        Debug.DrawLine(origin, target, Color.red);

        // lineOfSightMask는 "막는 장애물 레이어만" 넣는 용도
        // 즉 벽, 환경물 정도만 포함
        bool blocked = Physics.Raycast(
            origin,
            direction,
            distance,
            lineOfSightMask,
            QueryTriggerInteraction.Ignore
        );

        return !blocked;
    }
}