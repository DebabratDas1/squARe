using UnityEngine;
using UnityEngine.UI;

public class ImagePicker : MonoBehaviour
{
    [SerializeField] private Image image;
    

    private string imagePath = "";
    private Sprite pickedImageSprite;
    private Texture2D pickedImageTex;



   
    public void OnClickPick()
    {
        PickImage();
    }


    public void OnClickShow()
    {
        pickedImageSprite = Sprite.Create(pickedImageTex, new Rect(0, 0, pickedImageTex.width, pickedImageTex.height),
            new Vector2(0.5f, 0.5f), 100);
        image.sprite = pickedImageSprite;
        
    }


    

    private void PickImage(int maxSize=-1)
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image
                
                Texture2D texture = NativeGallery.LoadImageAtPath(path, maxSize, false);
                
                if (texture == null)
                {
                    imagePath = path;
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }
                pickedImageTex = texture;
                
            }
        });

        Debug.Log("Permission result: " + permission);
    }
}
