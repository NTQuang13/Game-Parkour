using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NameTagTracer : MonoBehaviour
{
    private TextMeshPro TMP;
    //[SerializeField] private GameObject others;
    private Transform trans;
    private Vector3 offset=new Vector3 (0,180,0);

    // Start is called before the first frame update
    void Start()
    {
        
        trans = GameObject.Find("CameraPos").GetComponent<Transform>();
        //TMP= FindAnyObjectByType<TextMeshPro>();
        //TMP.text = others.name;
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(trans);
        transform.Rotate(offset);
    }
}
