using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, ������̼� �ý��� ���� �ڵ� ��������

// ���� AI ����
public class Zombie : LivingEntity
{
    public LayerMask whatIsTarget; // ���� ��� ���̾�

    protected LivingEntity targetEntity; // ���� ���
    protected NavMeshAgent navMeshAgent; // ��� ��� AI ������Ʈ

    public ParticleSystem hitEffect; // �ǰ� �� ����� ��ƼŬ ȿ��
    public AudioClip deathSound; // ��� �� ����� �Ҹ�
    public AudioClip hitSound; // �ǰ� �� ����� �Ҹ�

    protected Animator zombieAnimator; // �ִϸ����� ������Ʈ
    protected AudioSource zombieAudioPlayer; // ����� �ҽ� ������Ʈ
    public Renderer zombieBodyRenderer; // ������ ������Ʈ

    public float damage = 20f; // ���ݷ�

    // ������ ����� �����ϴ��� �˷��ִ� ������Ƽ
    protected bool hasTarget
    {
        get
        {
            // ������ ����� �����ϰ�, ����� ������� �ʾҴٸ� true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // �׷��� �ʴٸ� false
            return false;
        }
    }

    private void Awake()
    {
        // ���� ������Ʈ�κ��� ����� ������Ʈ���� ��������
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioPlayer = GetComponent<AudioSource>();

    }

    // ���� AI�� �ʱ� ������ �����ϴ� �¾� �޼���
    public virtual void Setup(ZombieData zombieData)
    {
        // ü�� ����
        startingHealth = zombieData.health;
        health = zombieData.health;
        // ���ݷ� ����
        damage = zombieData.damage;
        // ����޽� ������Ʈ�� �̵� �ӵ� ����
        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = zombieData.speed;
    }

    private void Start()
    {
        // ���� ������Ʈ Ȱ��ȭ�� ���ÿ� AI�� ���� ��ƾ ����
        StartCoroutine(UpdatePath());
    }

    protected virtual void Update()
    {
        // ���� ����� ���� ���ο� ���� �ٸ� �ִϸ��̼� ���
        zombieAnimator.SetBool("HasTarget", hasTarget);
    }

    // �ֱ������� ������ ����� ��ġ�� ã�� ��� ����
    private IEnumerator UpdatePath()
    {
        // ��� �ִ� ���� ���� ����
        while (!dead)
        {
            if (hasTarget)
            {
                // �����������: ��θ������ϰ�AI �̵����������
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(targetEntity.transform.position);
            }
            else
            {
                // ���� ��� ���� : AI �̵� ����
                navMeshAgent.isStopped = true;
                // 20 ������ �������� ���� ������ ���� �׷�����, ���� ��ġ�� ��� �ݶ��̴��� ������
                // ��, whatIsTarget ���̾ ���� �ݶ��̴��� ���������� ���͸�
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);
                // ��� �ݶ��̴����� ��ȸ�ϸ鼭, ����ִ� LivingEntity ã��
                for (int i = 0; i < colliders.Length; i++)
                {
                    // �ݶ��̴��κ��� LivingEntity ������Ʈ ��������
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();
                    // LivingEntity ������Ʈ�� �����ϸ�, �ش� LivingEntity�� ����ִٸ�,
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        // ���� ����� �ش� LivingEntity�� ����
                        targetEntity = livingEntity;
                        // for�� ���� ��� ����
                        break;
                    }
                }
            }
            // 0.25�� �ֱ�� ó�� �ݺ�
            yield return new WaitForSeconds(0.25f);
        }
    }

    // �������� �Ծ��� �� ������ ó��
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        // ���� ������� ���� ��쿡�� �ǰ� ȿ�� ���
        if (!dead)
        {
            // ���� ���� ������ �������� ��ƼŬ ȿ���� ���
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();
            // �ǰ� ȿ���� ���
            zombieAudioPlayer.PlayOneShot(hitSound);
        }
        // LivingEntity�� OnDamage()�� �����Ͽ� ������ ����
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // ��� ó��
    public override void Die()
    {
        // LivingEntity��Die()�������Ͽ��⺻���ó������
        base.Die();
        // �ٸ�AI�������������ʵ����ڽ��Ǹ���ݶ��̴�������Ȱ��ȭ
        Collider[] zombieColliders = GetComponents<Collider>();
        for (int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }
        // AI �����������ϰ���޽�������Ʈ����Ȱ��ȭ
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;
        // ����ִϸ��̼����
        zombieAnimator.SetTrigger("Die");
        // ���ȿ�������
        zombieAudioPlayer.PlayOneShot(deathSound);
    }

    // ����׿����� �� �信 ���� ������ �׷���
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 20f);
    }

}