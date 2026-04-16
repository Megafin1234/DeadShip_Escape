using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    // 적이 가질 수 있는 이동/전투 상태
    private enum EnemyState
    {
        Patrol,    // 비전투 상태: 자기 자리 주변을 가끔 순찰
        Approach,  // 전투 상태: 플레이어에게 접근
        Hold,      // 전투 상태: 사거리 안에서 정지 후 사격/판단
        Strafe     // 전투 상태: 짧게 좌우 이동
    }

    [Header("Detection")]
    public float detectRange = 6f;

    [Header("Patrol")]
    public float patrolRadius = 2f;
    public float minPatrolInterval = 5f;
    public float maxPatrolInterval = 10f;

    [Header("Strafe")]  //전투 중 자연스럽게 밟는 스탭
    public float strafeDistance = 1.2f;  //스탭 이동거리
    public float strafeSpeed = 3.8f; //스탭 이동속도
    public float minStrafeInterval = 1.5f;   //x    x~y초 사이에 z퍼의 확률로 스탭 밟음
    public float maxStrafeInterval = 2.3f;  //y
    public float strafeDecisionChance = 0.5f;  //z
    public float maxStrafeDuration = 1.2f; //스탭의 최대 지속시간. 시간지나면 자동으로 해제
    private float strafeTimer = 0f;


    private Transform player;
    private EnemyShooter enemyShooter;
    private NavMeshAgent agent;
    private EnemyAlert enemyAlert;

    // 한 번 플레이어를 인식하면 true
    private bool hasDetectedPlayer = false;

    // 현재 상태
    private EnemyState currentState = EnemyState.Patrol;

    // 에이전트 기본 이동 속도 저장
    private float defaultMoveSpeed;

    // patrol 관련
    private Vector3 spawnPosition;
    private Vector3 patrolTarget;
    private float nextPatrolDecisionTime = 0f;

    // strafe 관련
    private Vector3 strafeTarget;
    private float nextStrafeDecisionTime = 0f;
    private bool isStrafing = false;

    //피격효과 관련
    private Renderer enemyRenderer;
    private Color originalColor;

    public float hitFlashDuration = 0.1f;
    private float hitFlashTimer = 0f;

    void Start()
    {
        // Player 참조 확보
        GameObject playerObject = GameObject.FindWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        // 필요한 컴포넌트 참조 확보
        enemyShooter = GetComponent<EnemyShooter>();
        agent = GetComponent<NavMeshAgent>();
        enemyAlert = GetComponent<EnemyAlert>();
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;
        }

        if (agent != null)
        {
            // 이동 방향으로 자동 회전하지 않게 함
            // 회전은 아래 HandleRotation()에서 직접 처리
            agent.updateRotation = false;
            defaultMoveSpeed = agent.speed;
        }

        // patrol 시작 기준 위치
        spawnPosition = transform.position;
        patrolTarget = spawnPosition;

        // 첫 patrol 목표 갱신 시점 예약
        nextPatrolDecisionTime = Time.time + Random.Range(minPatrolInterval, maxPatrolInterval);
    }

    void Update()
    {
        if (player == null || enemyShooter == null || agent == null) return;

        // 1. 감지 여부 체크
        CheckDetection();

        // 2. 현재 상황을 보고 상태 결정
        EvaluateState();

        // 3. 현재 상태 행동 실행
        ExecuteState();

        // 4. 회전 처리
        HandleRotation();

        //5. 피격처리
        HandleHitFlash();
        
    }

    void CheckDetection()
    {
        if (hasDetectedPlayer) return;

        Vector3 toPlayer = (player.position - transform.position);
        float distanceToPlayer = toPlayer.magnitude;

        Vector3 forward = transform.forward;

        // 방향 정규화
        toPlayer.Normalize();

        // 앞/뒤 판정 (dot product)
        float dot = Vector3.Dot(forward, toPlayer);

        // dot 기준
        // 1 → 완전 앞
        // 0 → 옆
        // -1 → 완전 뒤

        float effectiveDetectRange = detectRange;

        if (dot < 0f)
        {
            // 뒤쪽이면 인지 거리 감소
            effectiveDetectRange *= 0.5f; // ← 이 값으로 난이도 조절
        }

        if (distanceToPlayer <= effectiveDetectRange)
        {
            DetectPlayer();
        }
    }

    void EvaluateState()
    {
        // strafe 중이면 일단 끝날 때까지 상태 고정
        if (isStrafing)
        {
            currentState = EnemyState.Strafe;
            return;
        }

        // 아직 플레이어를 감지하지 않았다면 무조건 Patrol
        if (!hasDetectedPlayer)
        {
            currentState = EnemyState.Patrol;
            return;
        }

        // 여기서부터는 전투 상태
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float attackRange = enemyShooter.GetAttackRange();
        bool canSeePlayer = enemyShooter.CanSeePlayer();

        // 사거리 밖이면 접근
        if (distanceToPlayer > attackRange)
        {
            currentState = EnemyState.Approach;
            return;
        }

        // 사거리 안이지만 시야가 막혀 있어도 접근
        if (!canSeePlayer)
        {
            currentState = EnemyState.Approach;

            return;
        }

        // 사거리 안 + 시야 확보면 Hold
        currentState = EnemyState.Hold;

    }

    void ExecuteState()
    {
        switch (currentState)
        {
            case EnemyState.Patrol:
                ExecutePatrol();
                break;

            case EnemyState.Approach:
                ExecuteApproach();
                break;

            case EnemyState.Hold:
                ExecuteHold();
                break;

            case EnemyState.Strafe:
                ExecuteStrafe();
                break;
        }
    }

    void ExecutePatrol()
    {
        agent.speed = defaultMoveSpeed;

        // 일정 시간이 지나면 새 patrol 목적지를 다시 뽑음
        if (Time.time >= nextPatrolDecisionTime)
        {
            SetNewPatrolPoint();

            // 다음 patrol 갱신 시점도 다시 예약
            nextPatrolDecisionTime = Time.time + Random.Range(minPatrolInterval, maxPatrolInterval);
        }

        float distanceToTarget = Vector3.Distance(transform.position, patrolTarget);

        // 목표 지점까지 아직 멀면 이동
        if (distanceToTarget > 0.3f)
        {
            agent.isStopped = false;
            agent.SetDestination(patrolTarget);
        }
        else
        {
            // 도착했으면 멈춤
            agent.isStopped = true;
        }
    }

    void SetNewPatrolPoint()
    {
        // spawnPosition 기준으로 patrolRadius 안에서 새 지점 찾기
        for (int i = 0; i < 10; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRadius;
            Vector3 candidate = spawnPosition + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // NavMesh 위의 유효한 점으로 보정
            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
            {
                patrolTarget = hit.position;
                return;
            }
        }

        // 실패하면 원래 자리 유지
        patrolTarget = spawnPosition;
    }

    void ExecuteApproach()
    {
        agent.speed = defaultMoveSpeed;
        agent.isStopped = false;

        // 플레이어 위치를 계속 목적지로 설정
        // 플레이어가 도망가면 다시 따라감
        agent.SetDestination(player.position);
    }

    void ExecuteHold()
    {
        agent.speed = defaultMoveSpeed;
        agent.isStopped = true;

        // 아직 strafe 판단할 시간이 아니면 가만히 유지
        if (Time.time < nextStrafeDecisionTime)
            return;

        // 다음 strafe 판단 시점 예약
        nextStrafeDecisionTime = Time.time + Random.Range(minStrafeInterval, maxStrafeInterval);

        // 일정 확률로만 strafe 시작
        if (Random.value < strafeDecisionChance)
        {
            TryStartStrafe();
        }
    }

    void TryStartStrafe()
    {
        // 플레이어를 기준으로 좌우 방향 계산
        Vector3 toPlayer = (player.position - transform.position).normalized;
        toPlayer.y = 0f;

        Vector3 sideDir = Vector3.Cross(Vector3.up, toPlayer).normalized;

        // 왼쪽/오른쪽 랜덤 선택
        if (Random.value < 0.5f)
            sideDir = -sideDir;

        Vector3 candidate = transform.position + sideDir * strafeDistance;

        // NavMesh 위 유효한 위치인지 확인
        if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            strafeTarget = hit.position;
            isStrafing = true;
            currentState = EnemyState.Strafe;
            strafeTimer = maxStrafeDuration;

            agent.speed = strafeSpeed;
            agent.isStopped = false;
            agent.SetDestination(strafeTarget);
        }
    }

    void ExecuteStrafe()
    {
        agent.speed = strafeSpeed;
        agent.isStopped = false;

        // 시작할 때 한 번 목적지를 줬더라도, 혹시 모를 경로 꼬임 방지용으로 유지
        agent.SetDestination(strafeTarget);

        // 안전 타이머 감소
        strafeTimer -= Time.deltaTime;

        // 1) NavMeshAgent 기준 도착 판정
        bool arrived = !agent.pathPending &&
                    agent.remainingDistance <= agent.stoppingDistance + 0.05f;

        // 2) 혹시 remainingDistance가 이상하게 안 줄어드는 상황 대비용 타임아웃
        bool timedOut = strafeTimer <= 0f;

        if (arrived || timedOut)
        {
            isStrafing = false;
            agent.speed = defaultMoveSpeed;
            agent.isStopped = true;
            agent.ResetPath(); // strafe 경로 초기화
        }
    }

    void HandleRotation()
    {
        // 전투 중엔 항상 플레이어를 바라봄
        if (hasDetectedPlayer)
        {
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            }

            return;
        }

        // 비전투 중엔 이동 방향을 향하게 해서 patrol이 자연스럽게 보이게 함
        if (agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 lookDirection = agent.velocity.normalized;
            lookDirection.y = 0f;

            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 6f * Time.deltaTime);
            }
        }
    }

    void HandleHitFlash()
    {
        if (hitFlashTimer > 0f)
        {
            hitFlashTimer -= Time.deltaTime;

            if (hitFlashTimer <= 0f && enemyRenderer != null)
            {
                enemyRenderer.material.color = originalColor;
            }
        }
    }
    public void TakeHit()
    {
        DetectPlayer();
        OnHit();
    }

    public void DetectPlayer()
    {
        // 이미 인지했으면 중복 처리 안 함
        if (hasDetectedPlayer) return;

        hasDetectedPlayer = true;

        // 인지 즉시 접근 상태로 시작
        currentState = EnemyState.Approach;

        if (enemyAlert != null)
        {
            enemyAlert.ShowAlert();
        }
    }

    public bool HasDetectedPlayer()
    {
        return hasDetectedPlayer;
    }
    public void OnHit()  //피격 시 적 인지
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.material.color = Color.white;
            hitFlashTimer = hitFlashDuration;
        }
    }

    public void OnHearSound(Vector3 soundPosition, float soundRadius) //소리로 적 인지
    {
        if (hasDetectedPlayer) return;

        float distance = Vector3.Distance(transform.position, soundPosition);

        if (distance <= soundRadius) // 범위밖에서도 소리를 들었다면 플레이어 인지. 소리 인식 범위 soundRadius는 플레이어슈터에서 정의(플레이어 중심 인식 범위라)
        {
            DetectPlayer();
        }
    }
}