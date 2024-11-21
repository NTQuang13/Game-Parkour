using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPrefab;
    public GameObject canvasObject;
    public void CreatePopUp(string name, string description)
    {
        GameObject createdPopUpObject=Instantiate(popupPrefab,canvasObject.transform);
        createdPopUpObject.GetComponent<Popup>().SetPopUpDescription(description);
        createdPopUpObject.GetComponent<Popup>().SetPopUpName(name);

    }
    public void MovePopUp(GameObject createdPopUpObject)

    {
        //createdPopUpObject.GetComponent<RectTransform>().DO= Vector3.zero;

    }

}
