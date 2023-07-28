using Assets.ShootyMood.Scripts.Managers;
using ShootyMood.Scripts.Config.Wave;
using ShootyMood.Scripts.ShootyGameEvents;
using System;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static UnityEngine.GraphicsBuffer;

namespace Assets.ShootyMood.Scripts.UIScripts
{
    public class UIScore : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private GameObject effectPostion;
        [SerializeField] private GameObject uiParticle;
        [Inject] private WavesConfig wavesConfig;
        [Inject] private SignalBus signalBus;

        //private Vector3 uiParticleStartingScale;

        private int score = 0;

        private void OnDisable()
        {
            if (uiParticle != null)
            {
                uiParticle.gameObject.SetActive(false);
            }
        }

        public void Initialize()
        {
            signalBus.Subscribe<PlayerKilled>(OnPlayerKilled);
            signalBus.Subscribe<EnemyKilled>(OnEnemyKilled);
            signalBus.Subscribe<GameStarted>(OnGameStarted);
        }

        public void Dispose()
        {
            signalBus.TryUnsubscribe<PlayerKilled>(OnPlayerKilled);
            signalBus.TryUnsubscribe<EnemyKilled>(OnEnemyKilled);
            signalBus.TryUnsubscribe<GameStarted>(OnGameStarted);
        }

        private void OnEnemyKilled(EnemyKilled evt)
        {
            if (evt.isFriendly)
            {
                score -= wavesConfig.ScoreAddition * 2;
            }
            else
            {
                score += wavesConfig.ScoreAddition;
            }

            score = score < 0 ? 0 : score;
            scoreText.text = score.ToString();

            if (score % 10 == 0)
            {
                InstantiateParticle();
            }
        }

        private void InstantiateParticle()
        {
            var uiPart = Instantiate(uiParticle);
            var starPosition = Camera.main.ScreenToWorldPoint(effectPostion.transform.position);
            uiPart.transform.position = new Vector3(starPosition.x, starPosition.y, 0f);
        }

        private void OnGameStarted(GameStarted evt)
        {
            score = 0;
            scoreText.text = score.ToString();
        }

        private void OnPlayerKilled(PlayerKilled evt)
        {
            SaveManager.SaveScore(score);
        }
        
    }
}
