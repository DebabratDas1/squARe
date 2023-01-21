using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class PosterData
{


    private static string posterText = "";
    public static string PosterText
    {
        get
        {
            return posterText;
        }
        set
        {
            posterText = value;
        }
    }


    private static Image posterImage = null;

    public static Image PosterImage
    {
        get
        {
            return posterImage;
        }
        set
        {
            posterImage = value;
            
        }
    }
    
}
