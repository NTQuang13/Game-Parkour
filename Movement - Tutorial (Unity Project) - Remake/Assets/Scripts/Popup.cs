using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public TMP_Text popUpNameText;
    public TMP_Text popUpDescription;
    public void SetPopUpName(string text)
    {
        popUpNameText.text = text;
    }
    public void SetPopUpDescription(string text) 
    {  
        popUpDescription.text = text;
    }
}
