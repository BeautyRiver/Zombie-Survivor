using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, ������̼� �ý��� ���� �ڵ� ��������

// ���� AI ����
public class BoomerZombie : Zombie
{    
    [Header("Boomer Propeties")]
    public GameObject model;
    public float explosionRadius = 5f;       // ���� �ݰ�
    public float explosionTriggerRange = 3f; // ������ ���۵Ǵ� �Ÿ�
    public ParticleSystem explosionEffect;       // ���� ��ƼŬ ������
    public AudioClip explosionSound;         // ���� �Ҹ�
    private bool isExploding = false; // ������ �̹� ���۵Ǿ����� Ȯ��

    public override void Setup(ZombieData zombieData)
    {
        base.Setup(zombieData);
        isExploding = false;
    }

    protected override void Update()
    {
        base.Update();
        // ���� ����� ���� ���ο� ���� �ٸ� �ִϸ��̼� ���
        zombieAnimator.SetBool("HasTarget", hasTarget);

        if (!dead && !isExploding && hasTarget)
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetEntity.transform.position);

            if (distanceToTarget <= explosionTriggerRange && !isExploding)
            {
                isExploding = true;
                navMeshAgent.speed = 0f;
                zombieAnimator.SetTrigger("Explode");
            }
        }
    }

    public void Explode()
    {
        StartCoroutine(ExplosionRoutine());
        
    }
    private IEnumerator ExplosionRoutine()
    {
        yield return null;
        if(explosionEffect != null)
        {
            explosionEffect.transform.position = transform.position;
            explosionEffect.Play();
            zombieAudioPlayer.PlayOneShot(explosionSound);
        }
        model.SetActive(false);
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius, whatIsTarget);
        if (colliders.Length > 0)
        {
            LivingEntity target = colliders[0].GetComponent<LivingEntity>();
            if (target != null && !target.dead)
            {
                float distance = Vector3.Distance(transform.position, target.transform.position);
                float damgeMultiplier = Mathf.Clamp01(1.0f - (distance / explosionRadius));
                float calculatedDamage = damage * damgeMultiplier;
                Vector3 hitPoint = colliders[0].ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - colliders[0].transform.position;

                target.OnDamage(calculatedDamage, hitPoint, hitNormal);
            }
        }
        FindAnyObjectByType<ZombieSpawner>()?.RemoveForcing(this);
        
        Destroy(gameObject, 1f);
    }
  
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 20f);
    }

}