using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poster : MonoBehaviour
{
    [SerializeField] private PosterState posterCurrentState;


    public string posterText = null;
    public string posterImageUrl = null;
    public string posterImagePath = null;
    public Texture2D posterImage = null;

    public PosterState PosterCurrentState
    {
        get
        {
            return posterCurrentState;
        }
        set
        {
            posterCurrentState = value;
            
            ActivateConfirmPlaceButton();


        }
    }

    private void ActivateConfirmPlaceButton()
    {
        if (posterCurrentState == PosterState.Positioning)
        {
            UIManager.Singleton.placePosterBtn.gameObject.SetActive(true);
        }
        else
        {
            UIManager.Singleton.placePosterBtn.gameObject.SetActive(false);

        }
    }

    private void Awake()
    {
        PosterCurrentState = PosterState.NotPlaced;
    }


    private void Update()
    {
        //transform.rotation = Quaternion.LookRotation(transform.position - Camera.main.transform.position);
        /*var lookPos =-Camera.main.transform.position + transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 1);*/
        
            // Get the camera's position
            Vector3 cameraPosition = Camera.main.transform.position;

            // Get the current rotation of the game object
            Vector3 currentRotation = transform.rotation.eulerAngles;

            // Make the game object look at the camera
            transform.LookAt(cameraPosition);

            // Set the game object's y rotation to the current y rotation
            transform.rotation = Quaternion.Euler(currentRotation.x, Camera.main.transform.rotation.eulerAngles.y, currentRotation.z);
        


    }





}


public enum PosterState
{
    NotPlaced,
    Positioning,
    Placing,
    Placed
}
