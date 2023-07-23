using DG.Tweening;
using ShootyMood.Scripts.Config.Wave;
using ShootyMood.Scripts.Managers;
using ShootyMood.Scripts.ShootyGameEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

namespace Assets.ShootyMood.Scripts.UIScripts
{
    public class UITimerBackwards : MonoBehaviour, IInitializable, IDisposable
    {
        [SerializeField] private TextMeshProUGUI timerText;
        [Inject] private SignalBus signalBus;
        [Inject] private WavesConfig wavesConfig;
        private float leftTime = 0f;
        private bool isStarted = false;

        private Color textOrigColor;

        private void Start()
        {
            textOrigColor = timerText.color;
        }

        public void Initialize()
        {
            signalBus.Subscribe<GameStarted>(OnGameStarted);
            signalBus.Subscribe<EnemyKilled>(OnEnemyKille);
        }

        public void Dispose()
        {
            signalBus.Unsubscribe<GameStarted>(OnGameStarted);
            signalBus.Unsubscribe<EnemyKilled>(OnEnemyKille);
        }

        private void Update()
        {
            if (GameStateManager.Instance.GetState() != GameStateManager.GameState.PLAY || !isStarted)
            {
                return;
            }

            UpdateTimer();
        }

        private void UpdateTimer()
        {
            float timerValue = leftTime;
            if (timerValue < 0)
            {
                timerValue = 0;
                signalBus.Fire(new PlayerKilled());
            }

            TimeSpan timeSpan = TimeSpan.FromSeconds(timerValue);
            string timeText = timeSpan.Minutes.ToString("00") + ":" + timeSpan.Seconds.ToString("00");

            timerText.text = timeText;
            leftTime -= Time.deltaTime;
        }

        private void OnGameStarted()
        {
            leftTime = wavesConfig.StartLeftTime;
            isStarted = true;
        }

        private void OnEnemyKille(EnemyKilled evt)
        {
            leftTime += evt.timeAddition;

            if(evt.isFriendly)
            {
                timerText.color = Color.red;
                timerText.DOColor(textOrigColor, 1f);
            }
        }
    }
}
