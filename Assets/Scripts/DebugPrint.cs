using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using getReal3D;
using System.IO;

public class DebugPrint : MonoBehaviour
{
    // struct holding the rows of the database file
    [System.Serializable]
    public class ObjectInfo
    {
        public Vector3 position;
        public float ra;
        public float dec;
        public float redshift;
        public string specObjID;
        public float logStellarMass;
        public float sfr;
        public float oh;
    }

    private PlayerInputs m_playerInputs;

    // the wand and tip transforms
    public Transform Wand;
    public Transform wandTip;

    // text to write on canvas
    public TextMeshProUGUI textMeshPro;

    // database file name
    public string fileName;
    // database objects
    private List<ObjectInfo> objectList;

    // find objects inside the tip of the wand
    private float maxDistanceThreshold;

    // the point cloud game object; need its position and scale
    public Transform targetObject;

    void Start()
    {
        m_playerInputs = GetComponent<PlayerInputs>();
        LoadObjectData();
        maxDistanceThreshold = wandTip.localScale.y * Wand.localScale.y * 10; // player scale should be 1
    }

    // Update is called once per frame
    void Update()
    {
        if (Wand.gameObject.activeInHierarchy)
        {
            wandTip.gameObject.SetActive(true);
            if (m_playerInputs.WandButtonDown)
            {
                string outputText = string.Format("Here ({0},{1},{2})", wandTip.position.x, wandTip.position.y, wandTip.position.z);
                if (objectList.Count > 0)
                {
                    ObjectInfo objectInfo = FindClosestObject();
                    if (objectInfo != null)
                    {
                        outputText = string.Format("Found: (RA {0}, Dec {1}, redshift {2}, logMstar{3})", objectInfo.ra, objectInfo.dec, objectInfo.redshift, objectInfo.logStellarMass);
                        //outputText = string.Format("Found ({0},{1},{2})", wandTip.position.x, wandTip.position.y, wandTip.position.z);
                    }
                }
                textMeshPro.text = outputText;
            }
        }
        else
        {
            wandTip.gameObject.SetActive(false);
            textMeshPro.text = "hello world";
        }
     }

    void LoadObjectData()
    {
        objectList = new List<ObjectInfo>();

        // this should give the path to the Data folder
        string buildFolder = "Builds/VizlabSDSSTour_Data";
        string filePath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - buildFolder.Length), "Assets/Data", fileName);

        // Read the text file and parse object positions with additional information
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);

            foreach (string line in lines)
            {
                string[] values = line.Trim().Split(' ');

                //if (values.Length >= 6) // Assuming there are 6 columns in the file (x, y, z, name, value, ...)
                //{
                float x = float.Parse(values[4]) * targetObject.localScale.x + targetObject.position.x;
                float y = float.Parse(values[5]) * targetObject.localScale.y + targetObject.position.y;
                float z = float.Parse(values[6]) * targetObject.localScale.z + targetObject.position.z;
                Vector3 position = new Vector3(x, y, z);

                ObjectInfo objInfo = new ObjectInfo
                {
                    position = position,
                    specObjID = values[0],
                    ra = float.Parse(values[1]),
                    dec = float.Parse(values[2]),
                    redshift = float.Parse(values[3]),
                    logStellarMass = float.Parse(values[7]),
                    sfr = float.Parse(values[8]),
                    oh = float.Parse(values[9]),
                };

                objectList.Add(objInfo);
                //}
            }
        }
        else
        {
            textMeshPro.text = "File not found: " + filePath;
        }
    }

    ObjectInfo FindClosestObject()
    {
        Vector3 position = wandTip.position;

        ObjectInfo closestObjectInfo = null;
        float closestDistance = float.MaxValue;

        foreach (ObjectInfo objInfo in objectList)
        {
            float distance = Vector3.Distance(position, objInfo.position);

            if (distance < closestDistance && distance < maxDistanceThreshold)
            {
                closestDistance = distance;
                closestObjectInfo = objInfo;
            }
        }

        return closestObjectInfo;
    }
}
