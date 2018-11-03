using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class SpriteAutoSizer : MonoBehaviour
{
    private Vector2 prevSize;
    public Vector2 actualSize;

    private Sprite mySprite;
    public bool changed = true;

    // behaviours that have a sprite
    private SpriteRenderer worldRenderer;
    private Image uiImage;

	// Use this for initialization
	void Start () {

        worldRenderer = GetComponent<SpriteRenderer>();
        if (worldRenderer != null)
            mySprite = worldRenderer.sprite;

        uiImage = GetComponent<Image>();
        if (uiImage != null)
            mySprite = uiImage.sprite;

	}
	
	void LateUpdate ()
    {
        CheckForChanges();

        if (mySprite != null && changed)
        {
            transform.localScale = new Vector2(actualSize.x / mySprite.bounds.size.x, actualSize.y / mySprite.bounds.size.y);
            changed = false;
        }
    }


    void CheckForChanges()
    {
        if (worldRenderer != null)
        {
            changed = mySprite != worldRenderer.sprite;
            mySprite = worldRenderer.sprite;
        }

        if (uiImage != null)
        {
            changed = mySprite != uiImage.sprite;
            mySprite = uiImage.sprite;
        }

        if (prevSize != actualSize)
            changed = true;
    }
}
