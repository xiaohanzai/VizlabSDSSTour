using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WandTip : MonoBehaviour
{
    private Transform wandTransform;

    // Start is called before the first frame update
    void Start()
    {
        wandTransform = transform.parent;
    }

    // Update is called once per frame
    void Update()
    {
        if (wandTransform.gameObject.activeInHierarchy)
            this.gameObject.SetActive(true);
        else
            this.gameObject.SetActive(false);
    }
}
