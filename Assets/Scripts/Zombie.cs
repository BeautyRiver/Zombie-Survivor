using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드 가져오기

// 좀비 AI 구현
public class Zombie : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상 레이어

    protected LivingEntity targetEntity; // 추적 대상
    protected NavMeshAgent navMeshAgent; // 경로 계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격 시 재생할 파티클 효과
    public AudioClip deathSound; // 사망 시 재생할 소리
    public AudioClip hitSound; // 피격 시 재생할 소리

    protected Animator zombieAnimator; // 애니메이터 컴포넌트
    protected AudioSource zombieAudioPlayer; // 오디오 소스 컴포넌트
    public Renderer zombieBodyRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    protected bool hasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake()
    {
        // 게임 오브젝트로부터 사용할 컴포넌트들을 가져오기
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioPlayer = GetComponent<AudioSource>();

    }

    // 좀비 AI의 초기 스펙을 결정하는 셋업 메서드
    public virtual void Setup(ZombieData zombieData)
    {
        // 체력 설정
        startingHealth = zombieData.health;
        health = zombieData.health;
        // 공격력 설정
        damage = zombieData.damage;
        // 내비메시 에이전트의 이동 속도 설정
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = zombieData.speed;
    }

    private void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    protected virtual void Update()
    {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        zombieAnimator.SetBool("HasTarget", hasTarget);
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    private IEnumerator UpdatePath()
    {
        // 살아 있는 동안 무한 루프
        while (!dead)
        {
            if (hasTarget)
            {
                // 추적대상존재: 경로를갱신하고AI 이동을계속진행
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                // 추적 대상 없음 : AI 이동 중지
                navMeshAgent.isStopped = true;
                // 20 유닛의 반지름을 가진 가상의 구를 그렸을때, 구와 겹치는 모든 콜라이더를 가져옴
                // 단, whatIsTarget 레이어를 가진 콜라이더만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);
                // 모든 콜라이더들을 순회하면서, 살아있는 LivingEntity 찾기
                for (int i = 0; i < colliders.Length; i++)
                {
                    // 콜라이더로부터 LivingEntity 컴포넌트 가져오기
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();
                    // LivingEntity 컴포넌트가 존재하며, 해당 LivingEntity가 살아있다면,
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        // 추적 대상을 해당 LivingEntity로 설정
                        targetEntity = livingEntity;
                        // for문 루프 즉시 정지
                        break;
                    }
                }
            }
            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // 아직 사망하지 않은 경우에만 피격 효과 재생
        if (!dead)
        {
            // 공격 받은 지점과 방향으로 파티클 효과를 재생
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();
            // 피격 효과음 재생
            zombieAudioPlayer.PlayOneShot(hitSound);
        }
        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의Die()를실행하여기본사망처리실행
        base.Die();
        // 다른AI들을방해하지않도록자신의모든콜라이더들을비활성화
        Collider[] zombieColliders = GetComponents<Collider>();
        for (int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }
        // AI 추적을중지하고내비메쉬컴포넌트를비활성화
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        // 사망애니메이션재생
        zombieAnimator.SetTrigger("Die");
        // 사망효과음재생
        zombieAudioPlayer.PlayOneShot(deathSound);
    }

    // 디버그용으로 씬 뷰에 추적 범위를 그려줌
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 20f);
    }

}