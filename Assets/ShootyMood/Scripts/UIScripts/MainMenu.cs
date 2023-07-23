using ShootyMood.Scripts.ShootyGameEvents;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ShootyMood.Scripts.UIScripts
{
    public class MainMenu : MonoBehaviour
    {
        [Inject] private SignalBus signalBus; 
        
        public void PlayButtonClick()
        {
            signalBus.Fire(new PlayButtonClickEvent());
        }

        public void QuitApplication()
        {
            Application.Quit();
        }
    }
}