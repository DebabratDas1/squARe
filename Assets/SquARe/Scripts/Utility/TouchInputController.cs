using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInputController
{
    Touch touch1 = Input.GetTouch(0);
    Touch touch2 = Input.GetTouch(1);

    private void Update()
    {
        
    }



    public bool WillChangeVerticalPos()
    {
        return true;

    }

}
