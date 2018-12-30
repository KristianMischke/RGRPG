using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RGRPG.Core;
using TMPro;

/// <summary>
///     Controls the HUD to display when an action occurs
///  </summary>
namespace RGRPG.Controllers
{

    public class AnimationHUDController : MonoBehaviour
    {
        //Prefabs

        //Scene Obj Refs

        //Data
     
        // Use this for initialization
        void Start()
        {
            //Get animation
                                   
        }

        // Update is called once per frame
        void Update()
        {
            //play through animation
        }

        public void executeAction(Character source, ICharacterAction action)
        {
            if (action.MyInfo == null)
                return;

            List<Character> targets =  action.GetTargets();
            if (targets != null)
            {
                foreach (Character c in targets)
                {
                    Transform hudTransform = GameController.instance.GetCharacterControllerPosition(c);
                    GameObject particleEffect = Resources.Load<GameObject>(action.MyInfo.TargetsParticlePrefab);
                    if (particleEffect != null)
                    {
                        GameObject currentAction = Instantiate(particleEffect, hudTransform); //Add particle effects
                    }
                }
            }

            //spawn source particle effect if one exists
            {
                Transform hudTransform = GameController.instance.GetCharacterControllerPosition(source);
                GameObject particleEffect = Resources.Load<GameObject>(action.MyInfo.SourceParticlePrefab);
                if (particleEffect != null)
                {
                    GameObject currentAction = Instantiate(particleEffect, hudTransform); //Add particle effects
                }
            }
                
        }
    }
}