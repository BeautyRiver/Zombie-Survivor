using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[System.Serializable]
public class ZombieSpawnInfo
{
    public int count; // 스폰할 좀비 수
    public Zombie zombiePrefab; // 스폰할 좀비 프리팹 (DefaultZombie 또는 BoomerZombie)
    public ZombieData zombieData; // 적용할 좀비 데이터 (기본, 스피드, 탱커, 부머 데이터)
}


[System.Serializable]
public class StageData
{
    public ZombieSpawnInfo[] zombieSpawnInfos;    
}

[System.Serializable]
public class WaveInfo
{
    public StageData[] stages;
}
// 좀비 게임 오브젝트를 주기적으로 생성
public class ZombieSpawner : MonoBehaviour {
    public Transform[] spawnPoints; // 좀비 AI를 소환할 위치들

    private List<Zombie> zombies = new List<Zombie>(); // 생성된 좀비들을 담는 리스트

    [Header("# 게임 난이도 조절")]
    public List<WaveInfo> waveInfos; // 웨이브별 스폰 정보   
    private int currentStage = -1; // 현재 스테이지 인덱스
    private int currentWave = 0; // 현재 웨이브 인덱스

    private GameManager gm;

    private void Awake()
    {
        gm = GameManager.instance;
    }

    private void Start()
    {
        
        UIManager.instance.ShowWaveUI(currentWave + 1);
    }
    private void Update() {
        // 게임 오버 상태일때는 생성하지 않음
        if (gm.isGameover)        
            return;
        

        // 좀비를 모두 물리친 경우 다음 스폰 실행
        if (zombies.Count <= 0 && gm.canSpawnedWave)
        {
            SpawnWave();
        }

        // UI 갱신
        UpdateUI();
    }

    // 웨이브 정보를 UI로 표시
    private void UpdateUI() {
        // 현재 웨이브와 남은 적 수 표시
        UIManager.instance.UpdateStageText(currentStage, zombies.Count);
    }

    // 현재 웨이브에 맞춰 좀비들을 생성
    private void SpawnWave() {        
        currentStage++; // 스테이지 증가

        if (currentStage >= waveInfos[currentWave].stages.Length)
        {
            // 스테이지 초기화 및 웨이브 증가
            currentStage = 0;
            currentWave++;

            if (currentWave >= waveInfos.Count)
            {
                gm.GameClear();       
                return;
            }
            UIManager.instance.ShowWaveUI(currentWave + 1);
            return;
        }

        CreateZombie();
      
    }

    private void CreateZombie()
    {
        StageData currentStageData = waveInfos[currentWave].stages[currentStage];

        for (int i = 0; i < currentStageData.zombieSpawnInfos.Length; i++)
        {
            var spawnInfo = currentStageData.zombieSpawnInfos[i];

            for (int j = 0; j < spawnInfo.count; j++)
            {
                Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
                Zombie zombie = Instantiate(spawnInfo.zombiePrefab, spawnPoint.position, spawnPoint.rotation);
                
                zombie.Setup(spawnInfo.zombieData);
                zombies.Add(zombie);

                // 사망 이벤트 구독 
                zombie.onDeath += () => zombies.Remove(zombie); 
                zombie.onDeath += () => Destroy(zombie.gameObject, 10f); 
                zombie.onDeath += () => GameManager.instance.AddScore(100);
            }
        }
    }

    public void RemoveForcing(Zombie zombie)
    {
        if (zombies.Contains(zombie))
        {
            zombies.Remove(zombie);
        }
    }
}