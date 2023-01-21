using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Test1 : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<CanvasRenderer>().gameObject.AddComponent<Button>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnButtonClicked()
    {
        Debug.Log("Canvas Clicked");
    }
}
