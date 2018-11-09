using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using RGRPG.Core;
using TMPro;

// <summary>
//  Controls the HUD to display when an action occurs
namespace RGRPG.Controllers
{

    public class AnimationHUDController : MonoBehaviour
    {
        //Prefabs
        public GameObject healEvent;
        //Also need attack, defend, etc event
        //and animations for each

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

        public void executeAction(ICharacterAction action)
        {
            
            //If it's a heal action...
            if (action.GetType() == typeof(HealAction))
            {

                
               List<Character> targets =  action.GetTargets();
                foreach (Character c in targets)
                {
                    GameObject currentAction = Instantiate(healEvent, this.transform);
                    //Add particle effects
                }
                //Tell to spawn over right character
            }
                
        }
    }
}