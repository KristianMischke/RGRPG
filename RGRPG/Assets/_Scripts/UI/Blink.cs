using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Blink : MonoBehaviour
{
    public float period = 1f;
    private float theta = 0f;

    private Image image;
    private float imageOriginalAlpha = 0f;

	void Start ()
    {
        image = GetComponent<Image>();

        if(image != null)
            imageOriginalAlpha = image.color.a;	
	}
	
	void LateUpdate ()
    {
        if (image == null)
            return;

        image.color = new Color(image.color.r, image.color.g, image.color.b, imageOriginalAlpha * Mathf.Cos(theta*period));

        theta += Time.deltaTime;
	}
}
