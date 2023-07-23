using System;
using ShootyMood.Scripts.ShootyGameEvents;
using UnityEngine;
using Zenject;

namespace ShootyMood.Scripts.Handlers
{
    public class AudioService : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource killAudioSource;
        [SerializeField] private AudioSource enemyAttackAudioSource;
        
        [SerializeField] private AudioClip enemySpawnClip;
        [SerializeField] private AudioClip enemyKilledClip;
        [SerializeField] private AudioClip friendlyKilled;
        [SerializeField] private AudioClip enemyAttackAudioClip;
        
        [Inject] private SignalBus signalBus;
        
        public void Initialize()
        {
            signalBus.Subscribe<EnemyKilled>(OnEnemyKilled);
            signalBus.Subscribe<EnemySpawned>(OnEnemySpawned);
            signalBus.Subscribe<PlayerGotDamage>(OnPlayerGotDamage);
        }
        
        public void Dispose()
        {
            signalBus.TryUnsubscribe<EnemyKilled>(OnEnemyKilled);
            signalBus.TryUnsubscribe<EnemySpawned>(OnEnemySpawned);
            signalBus.TryUnsubscribe<PlayerGotDamage>(OnPlayerGotDamage);
        }

        private void OnEnemyKilled(EnemyKilled evt)
        {
            if (evt.isFriendly)
            {
                killAudioSource.clip = friendlyKilled;
                killAudioSource.time = .54f;
            }
            else
            {
                killAudioSource.clip = enemyKilledClip;
                killAudioSource.time = .45f;
            }
            
            killAudioSource.Play();
        }

        private void OnEnemySpawned(EnemySpawned evt)
        {
            audioSource.clip = enemySpawnClip; 
            audioSource.Play();
        }

        private void OnPlayerGotDamage(PlayerGotDamage pg)
        {
            enemyAttackAudioSource.clip = enemyAttackAudioClip; 
            enemyAttackAudioSource.Play();
        }
    }
}