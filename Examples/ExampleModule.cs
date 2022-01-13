using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using PrefabAPI;
using Gungeon;

namespace PrefabAPI
{
    public class ExampleModule : ETGModule
    {
        public override void Init()
        {
        }

        public override void Start()
        {
            ETGModConsole.Commands.AddUnit("spawnprefab", delegate(string[] args)
            {
                if(args.Length > 0)
                {
                    GameObject go = PrefabBuilder.builtObjects?.Find((GameObject g) => g.name == args[0]);
                    if(go == null)
                    {
                        go = PrefabBuilder.BuildObject(args[0]);
                        tk2dSprite.AddComponent(go, AmmonomiconController.ForceInstance.EncounterIconCollection, UnityEngine.Random.Range(0, AmmonomiconController.ForceInstance.EncounterIconCollection.spriteDefinitions.Length));
                    }
                    UnityEngine.Object.Instantiate(go, GameManager.Instance.PrimaryPlayer.sprite.WorldBottomLeft, Quaternion.identity);
                }
            });
            ETGModConsole.Commands.AddUnit("clone_bullet_kin", delegate (string[] args)
            {
                GameObject go = PrefabBuilder.Clone(Game.Enemies["bullet_kin"].gameObject);
                go.AddComponent<UrBadAtTheGameComponent>();
                ETGModConsole.Log("aiactor != null: " + (go.GetComponent<AIActor>() != null));
                AIActor.Spawn(go.GetComponent<AIActor>(), GameManager.Instance.PrimaryPlayer.CurrentRoom.GetRandomAvailableCellDumb(), GameManager.Instance.PrimaryPlayer.CurrentRoom, false, AIActor.AwakenAnimationType.Default, true);
            });
        }

        public override void Exit()
        {
        }
    }
}
