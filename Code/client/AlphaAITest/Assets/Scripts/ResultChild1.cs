using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultChild1 : MonoBehaviour
{
    [SerializeField]
    private Text TextStar, TextScore, TextName; 
    public void Init(int star, int newScore, string newName)
    {
        TextStar.text = "";
        for (int i = 0; i < star; i++)
            TextStar.text = TextStar.text + "*";

        TextScore.text = newScore.ToString();
        TextName.text = newName;
    }
}
