using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드 가져오기

// 좀비 AI 구현
public class DefaultZombie : Zombie
{
    
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점
   
    private void OnTriggerStay(Collider other)
    {
        // 자신이사망하지않았으며,
        // 최근공격시점에서timeBetAttack이상시간이지났다면공격가능
        if (!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            // 상대방으로부터LivingEntity타입을가져오기시도
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();
            // 상대방의LivingEntity가자신의추적대상이라면공격실행
            if (attackTarget != null && attackTarget == targetEntity)
            {
                // 최근공격시간을갱신
                lastAttackTime = Time.time;
                // 상대방의피격위치와피격방향을근삿값으로계산
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;
                // 공격실행
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }
        }
    }

    public override void Setup(ZombieData zombieData)
    {
        base.Setup(zombieData);

        // 렌더러가 사용중인 머테리얼의 컬러를 변경, 외형 색이 변함
        Material[] materials = zombieBodyRenderer.materials;
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].color = zombieData.skinColor;
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 20f);
    }

}