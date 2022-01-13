using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PrefabAPI
{
    public class UrBadAtTheGameComponent : BraveBehaviour
    {
        public void Start()
        {
            if(GameManager.Instance.PrimaryPlayer != null)
            {
                GameManager.Instance.PrimaryPlayer.OnReceivedDamage += UrBadAtTheGame;
            }
        }

        protected override void OnDestroy()
        {
            if (GameManager.Instance.PrimaryPlayer != null)
            {
                GameManager.Instance.PrimaryPlayer.OnReceivedDamage -= UrBadAtTheGame;
            }
            base.OnDestroy();
        }

        public void UrBadAtTheGame(PlayerController p)
        {
            if (!TextBoxManager.HasTextBox(transform))
            {
                List<string> badAtTheGame = new List<string>
                {
                    "Ur so bad at the game haha",
                    "just get good lol",
                    "all you have to do is dodge the bullets, it's not that hard!",
                    "even i play better than you"
                };
                TextBoxManager.ShowTextBox(sprite.WorldTopCenter + new Vector2(0f, 0.5f), transform, 3f, BraveUtility.RandomElement(badAtTheGame), "", false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, false, false);
            }
        }
    }
}
