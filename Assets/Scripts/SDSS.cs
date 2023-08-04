using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace SDSSNamespace
{
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

    public static class TxtDB
    {
        public static List<ObjectInfo> LoadData(string fileName)
        {
            List<ObjectInfo> objectList = new List<ObjectInfo>();

            // this should give the path to the Data folder
            //string filePath = Path.Combine(Application.dataPath, "Data", fileName);
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
                    float x = float.Parse(values[4]);
                    float y = float.Parse(values[5]);
                    float z = float.Parse(values[6]);
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

            return objectList;
        }

        public static void PrintObjectInfo(ObjectInfo objectInfo, TextMeshProUGUI textMeshPro)
        {
            string outputText = string.Format("No objects nearby");
            if (objectInfo != null)
            {
                outputText = string.Format("Object found:\nRA {0:0.00}, Dec {1:0.00},\nredshift {2:0.00},\nlogMstar {3:0.0}", objectInfo.ra, objectInfo.dec, objectInfo.redshift, objectInfo.logStellarMass);
            }
            textMeshPro.text = outputText;
        }
    }

    public static class CallSDSSAPI
    {
        public static string imageUrlTemplate = "https://skyserver.sdss.org/dr16/SkyServerWS/ImgCutout/getjpeg?ra={0}&dec={1}&scale=0.4&height=512&width=512&opt=GO";
        public static string spectrumUrlTemplate = "https://skyserver.sdss.org/dr16/en/get/specById.ashx?ID={0}";


        public static void CallAPI(MonoBehaviour monoBehaviour, ObjectInfo objectInfo, string imageType, RawImage imageDisplay)
        {
            if (objectInfo == null)
            {
                // set it transparent
                Color color = imageDisplay.color;
                color.a = 0f;
                imageDisplay.color = color;
                return;
            }

            string url = "";
            if (imageType == "image")
            {
                url = string.Format(imageUrlTemplate, objectInfo.ra, objectInfo.dec);
            }
            if (imageType == "spectrum")
            {
                url = string.Format(spectrumUrlTemplate, objectInfo.specObjID);
            }
            monoBehaviour.StartCoroutine(GetImageFromAPI(url, imageDisplay));
        }

        private static IEnumerator GetImageFromAPI(string url, RawImage imageDisplay)
        {
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                yield return request.SendWebRequest();

                if (!string.IsNullOrWhiteSpace(request.error))
                {
                    Debug.Log("Error {request.responseCode} - {request.error}");
                    yield break;
                }

                // Convert the image data to Texture2D
                Texture2D texture = new Texture2D(2, 2); // You can adjust the size accordingly
                texture.LoadImage(request.downloadHandler.data);

                // Display the image on the RawImage component
                imageDisplay.texture = texture;

                // set it visible
                Color color = imageDisplay.color;
                color.a = 1f;
                imageDisplay.color = color;
            }
        }
    }
}
