using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RGRPG.Core;

namespace RGRPG.Controllers
{
    public class SceneBGController : MonoBehaviour
    {

        Image bg;


        // Use this for initialization
        void Start() {
            bg = GetComponent<Image>();
        }

        // Update is called once per frame
        void Update() {

            bg.enabled = GameController.instance.IsInCombat;

            bg.sprite = SpriteManager.getSprite(SpriteManager.AssetType.COMBAT_BACKGROUND, GameController.instance.CurrentSceneType);

        }
    }
}