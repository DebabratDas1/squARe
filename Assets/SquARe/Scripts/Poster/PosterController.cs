using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using Google.XR.ARCoreExtensions;
using UnityEngine.XR.ARSubsystems;
using AndroidNativeCore;

public class PosterController : MonoBehaviour
{
    [SerializeField] private GameObject posterPrefabText, posterPrefabImage, posterPrefabBoth;

    [SerializeField] private TMP_InputField posterText;
    [SerializeField] private Image posterImage;
    [SerializeField] private Button makeARPosterBtn;

    private Poster currentPoster = null;
    [SerializeField] private float posterPositioningSpeed = 0.08f;
    [SerializeField] private float posterScalingSpeed = 0.25f;


    

    [SerializeField] private ARRaycastManager raycastManager;
    [SerializeField] private float _estimateDistance = 0.5f;

    [SerializeField] private ARAnchorManager anchorManager;


    private Touch touch, touch0, touch1;
    //private Vector2 currentTouch0, prevTouch0, currentTouch1, prevTouch1;

    //private Texture2D tempTex;

    private Texture2D posterTex;
    private string posterImagePath = null, posterImageUrl = null;
    private string pickedImagePath = null;

    private void SetPosterTexture(Texture2D texture)
    {
        posterTex = texture;
        if (posterTex == null)
        {
            UIManager.Singleton.AddImageSubPanel.SetActive(true);
            UIManager.Singleton.SelectedImageSubPanel.SetActive(false);
            posterImage.sprite = null;

            if (!posterText.text.Equals(""))
            {
                makeARPosterBtn.interactable = true;
            }
            else
            {
                makeARPosterBtn.interactable = false;
            }
        }
        else
        {
            Sprite pickedImageSprite = Sprite.Create(posterTex, new Rect(0, 0, posterTex.width, posterTex.height),
        new Vector2(0.5f, 0.5f), 100);
            posterImage.sprite = pickedImageSprite;
            UIManager.Singleton.AddImageSubPanel.SetActive(false);
            UIManager.Singleton.SelectedImageSubPanel.SetActive(true);

            makeARPosterBtn.interactable = true;

        }
    }

    private void Awake()
    {
        
    }

    public void CheckMakePostButtonInteractibility()
    {
        if (posterText.text.Equals("") && posterTex == null)
        {
            makeARPosterBtn.interactable = false;
        }
        else 
        {
            makeARPosterBtn.interactable = true;

        }
    }
    /*{
        get
        {
            return posterTex;
        }
        set
        {
            posterTex = value;

            
        }
    }*/








    private void CreateNewPoster()
    {
        GameObject newPoster = SelectInitPosterTemplate();
        newPoster.SetActive(false);
        
    }

    [SerializeField] private Toggle privacyButton;
    private GameObject SelectInitPosterTemplate()
    {
        if(posterTex!=null && !posterText.text.Equals(""))
        {
            GameObject newPoster = Instantiate(posterPrefabBoth);
            newPoster.GetComponentInChildren<TextMeshProUGUI>().text = posterText.text;
            newPoster.GetComponentsInChildren<Image>()[2].sprite = posterImage.sprite;
            currentPoster = newPoster.GetComponentInChildren<Poster>();
            currentPoster.posterImagePath = pickedImagePath;
            currentPoster.posterText = posterText.text;
            currentPoster.posterImage = posterTex;
            currentPoster.isPosterPrivate = privacyButton.isButtonStatePrivate;
            Debug.Log("Button Privacy State" + privacyButton.isButtonStatePrivate);
            Debug.Log("Poster Privacy State" + currentPoster.isPosterPrivate);



            return newPoster;
        }
        else if(posterTex==null && !posterText.text.Equals(""))
        {
            GameObject newPoster = Instantiate(posterPrefabText);
            newPoster.GetComponentInChildren<TextMeshProUGUI>().text = posterText.text;
            currentPoster = newPoster.GetComponentInChildren<Poster>();
            currentPoster.posterImagePath = null;
            currentPoster.posterImage = null;
            currentPoster.posterText = posterText.text;
            currentPoster.isPosterPrivate = privacyButton.isButtonStatePrivate;

            return newPoster;
        }
        else if(posterTex != null && posterText.text.Equals(""))
        {
            GameObject newPoster = Instantiate(posterPrefabImage);
            newPoster.GetComponentsInChildren<Image>()[1].sprite = posterImage.sprite;
            currentPoster = newPoster.GetComponentInChildren<Poster>();
            currentPoster.posterImagePath = pickedImagePath;
            currentPoster.posterText = null;
            currentPoster.posterImage = posterTex;
            currentPoster.isPosterPrivate = privacyButton.isButtonStatePrivate;
            return newPoster;
        }
        else
        {
            return null;
        }

    }




    public void OnClickARPosterButton()
    {
        CreateNewPoster();
        UIManager.Singleton.CurrentUIState = UIStates.PlacePosterAR;
        

    }

    private bool IsPosterreadyToPlace()
    {
        if (currentPoster != null && currentPoster.PosterCurrentState == PosterState.NotPlaced)
        {
            instructionText.gameObject.SetActive(true);
            return true;
        }
        else
        {
            instructionText.gameObject.SetActive(false);
            
            return false;
        }
    }


    private bool IsPosterReadyToPositioning()
    {
        if (currentPoster != null && currentPoster.PosterCurrentState == PosterState.Positioning)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    [SerializeField] private TextMeshProUGUI instructionText;

    private void Update()
    {

        if (IsPosterreadyToPlace())
        {
#if UNITY_EDITOR

            if (Input.GetMouseButtonDown(0))
            {
                Vector3 worldPoint = Input.mousePosition;
                worldPoint.z = Mathf.Abs(Camera.main.transform.position.z);
                //worldPoint.z = 11f;
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(worldPoint);
                mouseWorldPosition.z = _estimateDistance;
                PlacePoster(mouseWorldPosition);
                //Instantiate(go, mouseWorldPosition, Quaternion.identity);
            }

#elif UNITY_ANDROID

            

            
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
            //Debug.Log("No touch");

                return;
            }
            //Debug.Log("Event : "+EventSystem.current.IsPointerOverGameObject(touch.fingerId));
            /*if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
            Debug.Log("Eventsystem");
                return;
            }*/

            ARRaycast raycast = raycastManager.AddRaycast(touch.position, _estimateDistance);
            if (raycast != null)
            {
            Debug.Log("BBBBBB");

                PlacePoster(raycast.transform.position);
                Debug.Log("Raycast position: "+raycast.transform.position);
                // You can instantiate a 3D object here if you haven’t set Raycast Prefab in the scene.

            }
#endif



        }




        if (IsPosterReadyToPositioning())
        {
            //float.TryParse(UIManager.Singleton.speedModifier.text, out posterPositioningSpeed);
            if (Input.touchCount > 0 && Input.touchCount < 2)
            {
                touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Moved)
                {
                    
                    currentPoster.transform.position = new Vector3(
                        currentPoster.transform.position.x + touch.deltaPosition.x * posterPositioningSpeed * Time.deltaTime,
                        currentPoster.transform.position.y,
                        currentPoster.transform.position.z + touch.deltaPosition.y * posterPositioningSpeed * Time.deltaTime);
                }
            }

            if (Input.touchCount > 1)
            {
                touch0 = Input.GetTouch(0);
                touch1 = Input.GetTouch(1);


                if (touch0.phase == TouchPhase.Moved && touch1.phase == TouchPhase.Moved)
                {
                   
                    if ((touch0.deltaPosition.y > 0 && touch1.deltaPosition.y > 0) ||
                (touch0.deltaPosition.y < 0 && touch1.deltaPosition.y < 0))
                    {
                        // Calculate the average movement of the two touches
                        float avgDeltaY = (touch0.deltaPosition.y + touch1.deltaPosition.y) / 2;

                        // Move the game object vertically based on the average movement of the two touches
                        currentPoster.transform.position += Vector3.up * avgDeltaY * posterPositioningSpeed * Time.deltaTime;
                    }


                    // Pinch to Scale Poster

                    // Calculate the distance between the two touches in this frame
                    float touchDistance = Vector2.Distance(touch0.position, touch1.position);

                    // Calculate the distance between the two touches in the previous frame
                    float prevTouchDistance = Vector2.Distance(touch0.position - touch0.deltaPosition,
                                                               touch1.position - touch1.deltaPosition);

                    // Calculate the difference in the distances between the current and previous frames
                    float deltaDistance = touchDistance - prevTouchDistance;

                    // If the distance between the touches increased, scale the game object up
                    if (deltaDistance > 0)
                    {
                        currentPoster.transform.localScale += Vector3.one * posterScalingSpeed * Time.deltaTime;
                    }
                    // If the distance between the touches decreased, scale the game object down
                    else if (deltaDistance < 0)
                    {
                        currentPoster.transform.localScale -= Vector3.one * posterScalingSpeed * Time.deltaTime;
                    }
                }




                


            }
        }


    }

    private void PlacePoster(Vector3 _pos)
    {
        currentPoster.transform.position = _pos;
        currentPoster.gameObject.SetActive(true);
        //currentPoster.transform.parent.gameObject.SetActive(true);
        currentPoster.PosterCurrentState = PosterState.Positioning;

    }


    public void OnClickSelectImage()
    {
        Texture2D texture = null;
        NativeGallery.GetImageFromGallery((path) =>
        {
            Debug.Log("Image path: " + path);
            if (path != null)
            {
                // Create Texture from selected image

                texture = NativeGallery.LoadImageAtPath(path, -1, false);

                if (texture == null)
                {
                    Debug.Log("Couldn't load texture from " + path);
                    return;
                }
                pickedImagePath = path;

                //posterTex = texture;
                SetPosterTexture(texture);

            }
        });

    }


   


    public void OnClickRemoveSelectedImage()
    {
        SetPosterTexture(null);
    }


    public void ClearPosterCreator()
    {
        SetPosterTexture(null);
        posterText.text = null;
        if (currentPoster != null)
        {
            Destroy(currentPoster.gameObject);
            currentPoster = null;
        }
    }

    public void ClearNotPlacedPoster()
    {
        if (currentPoster != null)
        {
            Destroy(currentPoster.gameObject);
            currentPoster = null;
        }
    }

    [SerializeField] private AREarthManager EarthManager;
    public void ConfirmGeoPoster()
    {
        var earthTrackingState = EarthManager.EarthTrackingState;
        Debug.Log("TrackingState" + earthTrackingState);
        if (earthTrackingState != TrackingState.Tracking)
        {
            AlertLocationError();
        }
        else
        {
            currentPoster.PosterCurrentState = PosterState.Placing;
            UIManager.Singleton.StopPositioningHelpAudio();
            UIManager.Singleton.fuelingPanel.SetActive(true);
            DataUploader.Instance.UploadPoster1(currentPoster);
            //StartCoroutine(DataUploader.Instance.UploadPoster2(currentPoster));

            Pose pose = new Pose(currentPoster.transform.position, currentPoster.transform.rotation);
            GeospatialPose geospatialPose = EarthManager.Convert(pose);
            var anchor = anchorManager.AddAnchor(geospatialPose.Latitude, geospatialPose.Longitude, geospatialPose.Altitude, geospatialPose.EunRotation);
            currentPoster.transform.SetParent(anchor.transform, false);
            currentPoster.transform.localPosition = Vector3.zero;
            Debug.Log("GeoPose : (" + geospatialPose.Latitude + "," + geospatialPose.Longitude + "," + geospatialPose.Altitude + ")");
        }
        // Get GeoCordinate from ARcoordinate
        // Upload image to firebase Storage (if any image)
        // Upload data to firestore
        // Done
        
    }

    private void AlertLocationError()
    {
        Pose pose = new Pose(currentPoster.transform.position, currentPoster.transform.rotation);
        GeospatialPose geospatialPose = EarthManager.Convert(pose);
        AlertDialog alert = new AlertDialog();
        alert.build(AlertDialog.THEME_HOLO_DARK)
       .setTitle("Earth Tracking Error")
       .setMessage("Confirm your device location turned on\n("+geospatialPose.Latitude+","+
       geospatialPose.Longitude+","+geospatialPose.Altitude+")")
       .setIcon("alert_icon")
       .setNeutralButtion("OK", () => { Debug.Log("Negitive btn clicked"); alert.dismiss(); })
       .show();
    }

    private void StorePosterDataOnNetwork()
    {

    }


    public void OnClickCreatePoster()
    {
        ClearPosterCreator();
        UIManager.Singleton.CurrentUIState = UIStates.WritePost;

    }

    public void OnClickBackToWritePoster()
    {
        if(currentPoster!=null && currentPoster.PosterCurrentState != PosterState.Placed)
        {
            AlertDialog alert = new AlertDialog();
            alert.build(AlertDialog.THEME_HOLO_DARK)
           .setTitle("Poster Not Placed")
           .setMessage("Do you really want to go back?")
           .setIcon("alert_icon")
           .setNegativeButtion("Yes", () => {
               ClearNotPlacedPoster(); 
               alert.dismiss(); 
               UIManager.Singleton.CurrentUIState = UIStates.WritePost;
           })
           .setPositiveButtion("No", () => { alert.dismiss(); return; })
           //.setNeutralButtion("OK", () => { Debug.Log("Negitive btn clicked"); alert.dismiss(); })
           .show();
           // ClearNotPlacedPoster();
        }
        if(currentPoster != null && currentPoster.PosterCurrentState == PosterState.Placed)
        {
            ClearPosterCreator();
            UIManager.Singleton.CurrentUIState = UIStates.WritePost;

        }
    }

    

    

}
