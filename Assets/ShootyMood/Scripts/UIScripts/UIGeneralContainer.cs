using System;
using Assets.ShootyMood.Scripts.UIScripts;
using ShootyMood.Scripts.Managers;
using ShootyMood.Scripts.ShootyGameEvents;
using UnityEngine;
using Zenject;

namespace ShootyMood.Scripts.UIScripts
{
    public class UIGeneralContainer : MonoBehaviour, IInitializable, IDisposable
    {
        //[SerializeField] private UITimer uiTimer;
        [SerializeField] private UITimerBackwards uiTimer;
        //[SerializeField] private PlayerHealthBar playerHealthBar;
        [SerializeField] private MainMenu mainMenu;

        [Inject] private SignalBus signalBus;
        
        public void Initialize()
        {
            SetToMenuState();
            signalBus.Subscribe<PlayerKilled>(OnPLayerKilled);
            signalBus.Subscribe<PlayButtonClickEvent>(OnPlayButtonClick);
        }

        private void SetToMenuState()
        {
            uiTimer.gameObject.SetActive(false);
            //playerHealthBar.gameObject.SetActive(false);
            
            mainMenu.gameObject.SetActive(true);
            
            GameStateManager.Instance.SetState(GameStateManager.GameState.MENU);
        }

        private void SetToPlayState()
        {
            uiTimer.gameObject.SetActive(true);
            //playerHealthBar.gameObject.SetActive(true);
            
            mainMenu.gameObject.SetActive(false);
            
            signalBus.Fire(new GameStarted());
            GameStateManager.Instance.SetState(GameStateManager.GameState.PLAY);
        }

        private void OnPLayerKilled(PlayerKilled evy)
        {
            SetToMenuState();
        }

        private void OnPlayButtonClick()
        {
            SetToPlayState();
        }

        public void Dispose()
        {
            signalBus.TryUnsubscribe<PlayerKilled>(OnPLayerKilled);
            signalBus.TryUnsubscribe<PlayButtonClickEvent>(OnPlayButtonClick);
        }
    }
}