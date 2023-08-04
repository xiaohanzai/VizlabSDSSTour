using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using SDSSNamespace;
using getReal3D;

public class FindObjectAndDoStuff : MonoBehaviour
{
    private PlayerInputs m_playerInputs;

    // the wand tip
    public Transform wandTip;

    // text to write on canvas
    public TextMeshProUGUI textMeshPro;

    // database file name
    public string fileName;
    // database objects
    private List<ObjectInfo> objectList;

    // find objects inside the tip of the wand (x a factor of 2 or 5)
    private float maxDistanceThreshold;

    // the point cloud game object; need its position and scale
    public Transform targetObject;

    // used for displaying image and spectrum
    public RawImage imageDisplay;
    public RawImage spectrumDisplay;

    void Start()
    {
        m_playerInputs = GetComponent<PlayerInputs>();
        LoadData();
        maxDistanceThreshold = wandTip.lossyScale.magnitude * 2;
    }

    void Update()
    {
        if (wandTip.gameObject.activeInHierarchy)
        {
            if (m_playerInputs.WandButtonDown)
            {
                ObjectInfo objectInfo = null;
                if (objectList.Count > 0)
                {
                    objectInfo = FindClosestObject();
                }
                TxtDB.PrintObjectInfo(objectInfo, textMeshPro);
                CallSDSSAPI.CallAPI(this, objectInfo, "image", imageDisplay);
                CallSDSSAPI.CallAPI(this, objectInfo, "spectrum", spectrumDisplay);
            }
        }
        //else
        //{
        //    wandTip.gameObject.SetActive(false);
        //    textMeshPro.text = "hello world";
        //}
    }

    void LoadData()
    {
        objectList = TxtDB.LoadData(fileName);

        if (objectList.Count > 0)
        {
            foreach (ObjectInfo objectInfo in objectList)
            {
                objectInfo.position.x = objectInfo.position.x * targetObject.localScale.x + targetObject.position.x;
                objectInfo.position.y = objectInfo.position.y * targetObject.localScale.y + targetObject.position.y;
                objectInfo.position.z = objectInfo.position.z * targetObject.localScale.z + targetObject.position.z;
            }
        }
        else
        {
            textMeshPro.text = "File not found!";
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
