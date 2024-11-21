using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text.RegularExpressions;
using UnityEngine.UIElements;
using System.Linq.Expressions;

public class getTransform : MonoBehaviour
{
    public GameObject network;
    // Start is called before the first frame update
    public Event GetEvent;
    public Rigidbody rb;
    void Start()
    {
        //network.GetComponent<NetworkCommunicator>().OnUpdateTransform += GetTransform_OnUpdateTransform;
        rb= GetComponent<Rigidbody>();
    }
    public string StripControlChars(string s)
    {
        return Regex.Replace(s, @"[^\x20-\x7F]", "");
    }
    private void Move(string pos)
    {
        //this.transform.position=  
        string[] parts = pos.Split(',');

        if (parts.Length == 3)
        {
            // Step 2: Parse each part to a float
            float x = float.Parse(parts[0].Trim());
            float y = float.Parse(parts[1].Trim());
            float z = float.Parse(parts[2].Trim());

            // Step 3: Create a Vector3 from the values
            Vector3 newPosition = new Vector3(x, y, z);
            //Rigidbody.position = newPosition;
            rb.position = newPosition;
            //Debug.Log($"Position updated to: {newPosition}");
        }
        else
        {
            Debug.LogError("Invalid position string format!");
        }
    }
    /*private void GetTransform_OnUpdateTransform(object sender, NetworkCommunicator.OnUpdateTransformEventArgs e ) //worked
    {
        string[] part = e.message.Split('|');
        string Name = StripControlChars(part[0]);
        if (part[0] == this.gameObject.name)
        {
            //Debug.Log($"   {this.gameObject.name} moved");
            Move(part[1]);

            // Step 4: Assign the position to this object
            // more thing to do

        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }*/
}
