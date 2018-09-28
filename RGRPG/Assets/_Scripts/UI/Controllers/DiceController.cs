using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceController : MonoBehaviour
{
    private Button Die;
    private Text DieText;


    // Use this for initialization
    void Start()
    {
        Die = GetComponentInChildren<Button>();
        //Debug.Log(Die);
        DieText = Die.transform.GetComponentInChildren<Text>();
        
        Die.onClick.AddListener(RollDice);
        ResetDie();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ResetDie()
    {
        DieText.text = "Roll Die";
    }

    public void RollDice()
    {
        DieText.text = ((int)Random.Range(1f, 7f)).ToString();
    }
}
