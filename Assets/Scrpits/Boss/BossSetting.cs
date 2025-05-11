using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossSetting : MonoBehaviour
{
    public GameObject playerObject;
    void Start()
    {
        
    }


    void Update()
    {
        transform.position = new Vector3(playerObject.transform.position.x,
            transform.position.y, transform.position.z);
    }
}
