using DG.Tweening;
using ShootyMood.Scripts.Handlers;
using ShootyMood.Scripts.Handlers.Base;
using ShootyMood.Scripts.Models.Base;
using ShootyMood.Scripts.ShootyGameEvents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Zenject;

namespace Assets.ShootyMood.Scripts.Handlers.Enemy
{
    public class EnemyEscapeHandler : MonoBehaviour
    {
        [SerializeField] private EnemyModel characterModel;
        [SerializeField] private GameObject escapeParticle;

        [Inject] private SignalBus signalBus;

        //
        private float dissapearTimer = 0;


        void FixedUpdate()
        {
            if (characterModel.IsDead || characterModel.IsDying)
            {
                return;
            }

            if(dissapearTimer >= characterModel.EscapeTime)
            {
                characterModel.IsDead = true;
                var position = gameObject.transform.position;

                if (escapeParticle != null)
                {
                    Instantiate(escapeParticle, position, Quaternion.identity);
                }

                signalBus.Fire(new EnemyEscaped());
                Destroy(gameObject);
            }

            dissapearTimer += Time.deltaTime;
        }
    }
}
