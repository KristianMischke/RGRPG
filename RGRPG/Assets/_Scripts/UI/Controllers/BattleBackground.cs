using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RGRPG.Core;
using UnityEngine.SceneManagement;
using RGRPG.Core.Generics;
using UnityEngine.UI;


public class BattleBackground : MonoBehaviour
{

    // Use this for initialization

	Game game;
    Image bbackground;
    public Sprite myFirstImage; //Drag your first sprite here in inspector.
    public Sprite mySecondImage; //Drag your second sprite here in inspector.

    void Start() //Lets start by getting a reference to our image component.
    {
        bbackground = GetComponent<Image>(); //Our image component is the one attached to this gameObject.
    }

    public void SetImage() //method to set our first image
    {
        
		if (game.IsInCombat){
			bbackground.sprite = myFirstImage;
		}
		else {
			bbackground.sprite = mySecondImage;
			
			}
		
    }

}