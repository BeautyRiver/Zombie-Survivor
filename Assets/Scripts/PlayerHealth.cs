using UnityEngine;
using UnityEngine.UI; // UI 관련 코드

// 플레이어 캐릭터의 생명체로서의 동작을 담당
public class PlayerHealth : LivingEntity {
    public Slider healthSlider; // 체력을 표시할 UI 슬라이더

    public AudioClip deathClip; // 사망 소리
    public AudioClip hitClip; // 피격 소리
    public AudioClip itemPickupClip; // 아이템 습득 소리

    private AudioSource playerAudioPlayer; // 플레이어 소리 재생기
    private Animator playerAnimator; // 플레이어의 애니메이터

    private PlayerMovement playerMovement; // 플레이어 움직임 컴포넌트
    private PlayerShooter playerShooter; // 플레이어 슈터 컴포넌트

    private void Awake() {
        // 사용할 컴포넌트를 가져오기
        playerAnimator = GetComponent<Animator>();
        playerAudioPlayer = GetComponent<AudioSource>();
        playerMovement = GetComponent<PlayerMovement>();
        playerShooter = GetComponent<PlayerShooter>();
    }

    protected override void OnEnable() {
        // LivingEntity의 OnEnable() 실행 (상태 초기화)
        base.OnEnable();
        // 체력 슬라이더 활성화
        healthSlider.gameObject.SetActive(true);
        // 체력 슬라이더의 최대값을 기본 체력값으로 변경
        healthSlider.maxValue = startingHealth;
        // 체력 슬라이더의 값을 현재 체력값으로 변경
        healthSlider.value = health;
        // 플레이어 조작을 받는 컴포넌트들 활성화
        playerMovement.enabled = true;
        playerShooter.enabled = true; ;
    }

    // 체력 회복
    public override void RestoreHealth(float newHealth) {
        // LivingEntity의 RestoreHealth() 실행 (체력 증가)
        base.RestoreHealth(newHealth);
        // 체력 갱신
        healthSlider.value = health;
    }

    // 데미지 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitDirection) {
        if (!dead)
        {
            // 사망하지않은경우에만효과음을재생
            playerAudioPlayer.PlayOneShot(hitClip);
        }
        // LivingEntity의OnDamage() 실행(데미지적용)
        base.OnDamage(damage, hitPoint, hitDirection);
        // 갱신된체력을체력슬라이더에반영
        healthSlider.value = health;
    }

    // 사망 처리
    public override void Die() {
        // LivingEntity의Die() 실행(사망적용)
        base.Die();
        // 체력슬라이더비활성화
        healthSlider.gameObject.SetActive(false);
        // 사망음재생
        playerAudioPlayer.PlayOneShot(deathClip);
        // 애니메이터의Die 트리거를발동시켜사망애니메이션재생
        playerAnimator.SetTrigger("Die");
        // 플레이어조작을받는컴포넌트들비활성화
        playerMovement.enabled = false;
        playerShooter.enabled = false;
    }

    private void OnTriggerEnter(Collider other) {
        // 아이템과충돌한경우해당아이템을사용하는처리
        // 사망하지않은경우에만아이템사용가능
        if (!dead)
        {
            // 충돌한상대방으로부터Item 컴포넌트를가져오기시도
            IItem item = other.GetComponent<IItem>();
            // 충돌한상대방으로부터Item 컴포넌트가가져오는데성공했다면
            if (item != null)
            {
                // Use 메서드를실행하여아이템사용
                item.Use(gameObject);
                // 아이템습득소리재생
                playerAudioPlayer.PlayOneShot(itemPickupClip);
            }
        }
    }
}