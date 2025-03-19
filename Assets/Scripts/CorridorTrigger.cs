using System;
using UnityEngine;

public class CorridorTrigger : MonoBehaviour
{
    private ThirdPersonCamera cameraScript;

    void Start()
    {
        // 예: 태그가 "MainCamera"로 지정된 오브젝트에서 ThirdPersonCamera 스크립트를 찾는다.
        GameObject camObj = GameObject.FindWithTag("Main Camera");
        if (camObj != null)
        {
            cameraScript = camObj.GetComponent<ThirdPersonCamera>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (cameraScript != null)
            {
                cameraScript.inCorridor = true;
                Debug.Log("Corridor triggered");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (cameraScript != null)
                cameraScript.inCorridor = false;
        }
    }
}