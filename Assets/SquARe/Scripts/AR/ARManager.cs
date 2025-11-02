using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARManager : MonoBehaviour
{
    public static ARManager Singleton;
    private void Awake()
    {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }
        LoaderUtility.Deinitialize();
    }


    [SerializeField] private ARSession arSession;



    public void EnableAR(bool val=true)
    {
        //arSession.SetActive(val);
    }

    private bool isARSessionEnabled = false;
    public void ChangeARSession()
    {
        if (arSession.gameObject.activeInHierarchy)
        {
            isARSessionEnabled = true;
        }
        else
        {
            isARSessionEnabled = false;

        }
        isARSessionEnabled = !isARSessionEnabled;
        arSession.gameObject.SetActive(isARSessionEnabled);
        Debug.Log("isARSessionEnabled" + isARSessionEnabled);
    }
}
