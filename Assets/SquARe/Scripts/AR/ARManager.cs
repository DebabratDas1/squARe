using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    }


    [SerializeField] private GameObject arSession;


    public void EnableAR(bool val=true)
    {
        arSession.SetActive(val);
    }
}
