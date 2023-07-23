using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using DG.Tweening;
using ShootyMood.Scripts.Config.Wave;
using ShootyMood.Scripts.Managers;
using ShootyMood.Scripts.Models;
using ShootyMood.Scripts.ShootyGameEvents;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace ShootyMood.Scripts.Handlers
{
    public class GameController : MonoBehaviour, IInitializable, IDisposable, IFixedTickable
    {
        [SerializeField] private EnemyModel enemyPrefab;
        [SerializeField] private EnemyModel friendlyPrefab;

        [SerializeField] private GameObject spawnParticle;

        [Inject] private SignalBus signalBus;
        [Inject] private PositionSpawnService positionSpawnService;
        [Inject] private DiContainer diContainer;
        [Inject] private WavesConfig wavesConfig;
        [Inject] private SpawnPointConfig spConfig;
        [Inject] private PlayerHitHandler playerHealthHandler;
        
        //
        private int waveIndex = 0;
        private int currentWaveSpawnCount = 0;
        private int iterationSpawnCount = 1;
        private float timer;
        private float spawnDuration = 1f;
        private float mobAttackDurationDegradeRatio = 0f;
        private float mobEscapeDurationDegradeRatio = 0f;

        private float friendlySpawnRatio = 0f;
        private float timeAdditionOnDeath = 0f;

        public void Initialize()
        {
            signalBus.Subscribe<PlayerKilled>(OnPlayerKilled);
        }
        
        public void Dispose()
        {
            signalBus.TryUnsubscribe<PlayerKilled>(OnPlayerKilled);
        }

        private void OnPlayerKilled(PlayerKilled evt)
        {
            waveIndex = 0;
            timer = 0;
            OrganizeCurrentWave(GetCurrentWave());
            playerHealthHandler.ResetHealth();
            positionSpawnService.ResetSpawns();
        }

        public void FixedTick()
        {
            if (GameStateManager.Instance.GetState() != GameStateManager.GameState.PLAY)
            {
                return;
            }
            
            timer += Time.deltaTime;
            if (timer >= spawnDuration)
            {
                for(int i = 0; i < iterationSpawnCount; i++)
                    SpawnOnPositions();
                
                timer = 0f;
            }
        }

        private SingleWaveConfig GetCurrentWave()
        {
            SingleWaveConfig result = null;
            if (wavesConfig.waveConfigs.Count > waveIndex && wavesConfig.waveConfigs[waveIndex] != null)
            {
                result = wavesConfig.waveConfigs[waveIndex];
            }

            return result;
        }

        private void OrganizeCurrentWave(SingleWaveConfig currentWave)
        {
            if (currentWave == null)
                return;

            spawnDuration = currentWave.spawnDuration;
            mobAttackDurationDegradeRatio = currentWave.mobAttackDurationDecreaseAmount;
            mobEscapeDurationDegradeRatio = currentWave.mobEscapeDurationDecreaseAmount;
            friendlySpawnRatio = currentWave.friendlyRatio;
            iterationSpawnCount = currentWave.iterationSpawnCount;
            timeAdditionOnDeath = currentWave.timeAdditionOnDeath;


            if (currentWaveSpawnCount > currentWave.waveSpawnCount)
            {
                waveIndex++;
                currentWaveSpawnCount = 0;
            }
        }

        private void SpawnOnPositions()
        {
            List<EnemySpawnPosition> availablePositions = positionSpawnService.GetAvailablePositions();
            if (availablePositions == null || availablePositions.Count == 0)
            {
                return;
            }
            
            availablePositions = availablePositions.OrderBy(x => Random.Range(-10.0f, 10.0f)).ToList();
            var sp = availablePositions[0];

            bool isSpawnFriendly = false;
            float ran = Random.Range(0f, 1f);
            if(ran <= friendlySpawnRatio)
            {
                isSpawnFriendly = true;
            }

            EnemyModel newSpawn = null;
            float timeAddition = timeAdditionOnDeath;
            if (!isSpawnFriendly) 
            {
                newSpawn = diContainer.InstantiatePrefabForComponent<EnemyModel>(enemyPrefab);
            }
            else
            {
                newSpawn = diContainer.InstantiatePrefabForComponent<EnemyModel>(friendlyPrefab);
                newSpawn.isFriendly = true;
                timeAddition *= -1;
            }
             
            newSpawn.AttackDelayDecrementRatio = mobAttackDurationDegradeRatio;
            newSpawn.EscapeDelayDecrementRatio = mobEscapeDurationDegradeRatio;
            newSpawn.TimeAddition = timeAddition;
            newSpawn.gameObject.SetActive(false);
            
            float posDefX = Random.Range((-1 * spConfig.SpawnPosDeviation), spConfig.SpawnPosDeviation);
            float posDefY = Random.Range((-1 * spConfig.SpawnPosDeviation), spConfig.SpawnPosDeviation);
            
            var spawnPos = new Vector3((sp.X + posDefX), (sp.Y + posDefY));
            InstSpawnParticle(spawnParticle, spawnPos);

            newSpawn.transform.position = spawnPos;
            positionSpawnService.SpawnOnPos(ref sp, newSpawn);
            currentWaveSpawnCount++;

            signalBus.Fire(new EnemySpawned());
            //
            OrganizeCurrentWave(GetCurrentWave());
        }

        private void InstSpawnParticle(GameObject origSpawnParticle, Vector3 pos)
        {
            if(origSpawnParticle == null)
            {
                return;
            }

            var particlePos = new Vector3(pos.x, pos.y, pos.z - 2);
            var spawnParticleObject = Instantiate(origSpawnParticle, particlePos, Quaternion.identity);
            var origSpawnParticleScale = spawnParticleObject.transform.localScale;
            spawnParticleObject.transform.localScale = Vector3.zero;
            
            spawnParticleObject.transform.DOScale(origSpawnParticleScale, spConfig.SpawnDuration - .1f);
            Destroy(spawnParticleObject, spConfig.SpawnDuration);
        }
    }
}