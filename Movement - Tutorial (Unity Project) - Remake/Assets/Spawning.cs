using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Spawning : MonoBehaviour
{

    public GameObject OtherPlayerprefab; // Assign your prefab in the Inspector
    public GameObject Player;
    public List<GameObject> spawnedObjects = new List<GameObject>();
    
    // Method to spawn the prefab
    public int SpawnPrefab(string objectName)
    {
        // Spawn the prefab at a specific position and rotation
        GameObject newObject = Instantiate(OtherPlayerprefab,this.transform.position, Quaternion.identity);

        // Assign a name to the spawned object
        newObject.name = objectName;

        // Add the spawned object to the list
        spawnedObjects.Add(newObject);
        return spawnedObjects.Count;
    }
    public int AddPlayer()
    {
        spawnedObjects.Add(Player);
        return spawnedObjects.Count;
    }
    // Example: Accessing the spawned objects later
    public void AccessSpawnedObjects()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            Debug.Log($"Object Name: {obj.name}");
        }
    }

    public bool DeleteObjectByName(string objectName)
    {
        // Find the object in the list by name
        GameObject objectToDelete = spawnedObjects.Find(obj => obj.name == objectName);

        if (objectToDelete != null)
        {
            // Remove it from the list
            spawnedObjects.Remove(objectToDelete);

            // Destroy the GameObject
            Destroy(objectToDelete);

            Debug.Log($"Object '{objectName}' deleted.");
            return true;
        }
        else
        {
            Debug.LogWarning($"Object '{objectName}' not found.");
            return false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
