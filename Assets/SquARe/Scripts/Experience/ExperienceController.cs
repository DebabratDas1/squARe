using AndroidNativeCore;
using Firebase.Extensions;
using Firebase.Firestore;
using Firebase.Storage;
using Google.XR.ARCoreExtensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ExperienceController : MonoBehaviour
{
    public void EnterExperienceMode()
    {
        
        ReadDatabase();
    }


    private void RefreshExperienceMode()
    {
        //ClearPreviousPosters();
        ReadDatabase();
    }

    private CollectionReference colRef;
    private List<string> previousPostersCollection = new List<string>();
    private List<string> currentPostersCollection = new List<string>();
    private List<GameObject> expPostersList = new List<GameObject>();

    private void ReadDatabase()
    {

        var earthTrackingState = earthManager.EarthTrackingState;
        Debug.Log("TrackingState" + earthTrackingState);
#if !UNITY_EDITOR
        if (earthTrackingState != TrackingState.Tracking)
        {
            AlertLocationError();
            return;
        }
#endif
        var cameraGeospatialPose = earthManager.CameraGeospatialPose;
        //Debug.Log("AAAA");

        double lat = cameraGeospatialPose.Latitude;
        double lon = cameraGeospatialPose.Longitude;
        //Debug.Log("BBBB");

        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        colRef = db.Collection("SquARe").Document("AR Poster").Collection("AllPostersData");
        Query query = colRef.WhereGreaterThanOrEqualTo("Lat", lat - 0.001)
            .WhereLessThanOrEqualTo("Lat", lon + 0.001);
        //Debug.Log("CCCCC");

        query.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            //Debug.Log("DDDD");

            QuerySnapshot capitalQuerySnapshot = task.Result;
            // Debug.Log("EEEEE");
            previousPostersCollection = new List<string>(currentPostersCollection);
            currentPostersCollection.Clear();
            foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
            {
                bool isValidLon = false, isAccessable = false, isLive = false, isPosterValid = false;

                Debug.Log(String.Format("Document data for {0} document:", documentSnapshot.Id));
                Dictionary<string, object> poster = documentSnapshot.ToDictionary();

                poster.TryGetValue("UserID", out object posterOwner);
                poster.TryGetValue("Lon", out object posterLongitude);
                poster.TryGetValue("IsDeleted", out object posterDeleted);
                poster.TryGetValue("IsPrivate", out object posterPrivate);
                
                if ((double)posterLongitude <= lon + 0.001 && (double)posterLongitude >= lon - 0.001)
                {
                    isValidLon = true;
                }

                if ((bool)posterPrivate)
                {
                    if (posterOwner.Equals(UserData.userID))
                    {
                        isAccessable = true;
                    }
                    else
                    {
                        isAccessable = false;
                    }
                }
                else
                {
                    isAccessable = true;
                }
                Debug.Log("Poster UserID : " + (string)posterOwner);
                Debug.Log("User UserID : " + UserData.userID);

                Debug.Log("Is poster Accessable : " + isAccessable);
                isLive = !(bool)posterDeleted;
                isPosterValid = isValidLon && isAccessable && isLive;
                Debug.Log("Is Poster Deleted " + (bool)posterDeleted);
                Debug.Log("Is Poster Valid to show : " + isPosterValid);
                if (isPosterValid)
                {
                    currentPostersCollection.Add(documentSnapshot.Id);
                }
                //Debug.Log(!previousPostersCollection.Contains(documentSnapshot.Id));
                if (isPosterValid && !previousPostersCollection.Contains(documentSnapshot.Id))
                {
                    




                    documentSnapshot.TryGetValue("Lat", out object posterLatitude);
                    //documentSnapshot.TryGetValue("Lon", out object posterLongitude);
                    documentSnapshot.TryGetValue("Alt", out object posterAltitude);
                    documentSnapshot.TryGetValue("ScaleX", out object posterScaleX);
                    documentSnapshot.TryGetValue("ScaleY", out object posterScaleY);
                    documentSnapshot.TryGetValue("ScaleZ", out object posterScaleZ);
                    documentSnapshot.TryGetValue("Text", out object posterCaption);
                    documentSnapshot.TryGetValue("ImageUrl", out object posterImage);


                    if (posterImage != null && !posterImage.Equals(""))
                    {

                        Debug.Log("AAAAA");

                        if (posterCaption == null || posterCaption.Equals(""))
                        {
                            // Only Image Poster
                            GameObject newPoster = GetPoster(ExpPosterType.OnlyImage);
                            if (newPoster == null)
                            {
                                AlertSomeError();
                                return;
                            }
                            
                            Debug.Log("-_________");
                            StartCoroutine(GetTexture((string)posterImage, newPoster.GetComponentsInChildren<Image>()[1]));
                            Debug.Log("File downloaded.");
                            StartCoroutine(SetPosterTransform(posterLatitude, posterLongitude, posterAltitude, posterScaleX, posterScaleY, posterScaleZ, newPoster));
                            Debug.Log("CCCCCC");
                           
                        }
                        else
                        {
                            // Both Text and Image Poster
                            GameObject newPoster = GetPoster(ExpPosterType.Both);
                            newPoster.GetComponentInChildren<TextMeshProUGUI>().text = (string)posterCaption;
                            Debug.Log("-_________");
                            StartCoroutine(GetTexture((string)posterImage, newPoster.GetComponentsInChildren<Image>()[2]));
                            Debug.Log("File downloaded.");
                            StartCoroutine(SetPosterTransform(posterLatitude, posterLongitude, posterAltitude, posterScaleX, posterScaleY, posterScaleZ, newPoster));
                            Debug.Log("CCCCCC");
                            Debug.Log("ZZZZZZZZZ");
                            
                        }
                    }

                    else
                    {
                        Debug.Log("WWWWWWWW");

                        if (posterCaption == null || posterCaption.Equals(""))
                        {
                            Debug.Log("XXXXXXXXXXXXX");
                            // Error
                            // No Valid Poster Data
                        }
                        else
                        {
                            Debug.Log("YYYYYYYYYYY");
                            // Only Text Poster
                            GameObject newPoster = GetPoster(ExpPosterType.OnlyText);
                            newPoster.GetComponentInChildren<TextMeshProUGUI>().text = (string)posterCaption;
                            Debug.Log("-_________");
                            //StartCoroutine(GetTexture((string)posterImage, newPoster.GetComponentsInChildren<Image>()[2]));
                            Debug.Log("File downloaded.");
                            StartCoroutine(SetPosterTransform(posterLatitude, posterLongitude, posterAltitude, posterScaleX, posterScaleY, posterScaleZ, newPoster));
                            Debug.Log("CCCCCC");
                            Debug.Log("ZZZZZZZZZ");
                        }
                    }
                }
            };
        });
       

    }



    FirebaseStorage storage = null;
    StorageReference storageRef = null;

    private void Start()
    {
        storage = FirebaseStorage.DefaultInstance;
        storageRef = storage.GetReferenceFromUrl(
            "gs://square-9f7d0.appspot.com/SquARe/ARPosterImages"
            );
    }


    [SerializeField] private GameObject textPoster, imagePoster, bothPoster;
    [SerializeField] private ARAnchorManager anchorManager;
    private void CreatePosterExp(Dictionary<string, object> _documentSnapshot)
    {
        _documentSnapshot.TryGetValue("Lat", out object posterLatitude);
        _documentSnapshot.TryGetValue("Lon", out object posterLongitude);
        _documentSnapshot.TryGetValue("Alt", out object posterAltitude);
        _documentSnapshot.TryGetValue("ScaleX", out object posterScaleX);
        _documentSnapshot.TryGetValue("ScaleY", out object posterScaleY);
        _documentSnapshot.TryGetValue("ScaleZ", out object posterScaleZ);
        _documentSnapshot.TryGetValue("Text", out object posterCaption);
        _documentSnapshot.TryGetValue("ImageUrl", out object posterImage);

        
        if (posterImage != null && !posterImage.Equals(""))
        {

            Debug.Log("AAAAA");

            if (posterCaption == null || posterCaption.Equals(""))
            {

                // Only Image Poster
                GameObject newPoster  = GetPoster(ExpPosterType.OnlyImage);
                /*Debug.Log("-_________");
                newPoster.GetComponent<RectTransform>().localScale = new Vector3(
            (float)posterScaleX,
            (float)posterScaleY,
            (float)posterScaleZ);
                Debug.Log("imagePoster Scaled");

                var anchor = anchorManager.AddAnchor((double)posterLatitude,
                    (double)posterLongitude,
                    (double)posterLongitude,
                    newPoster.transform.rotation);
                Debug.Log("<<<<<<");
                newPoster.transform.SetParent(anchor.transform, false);
                Debug.Log("ZZZZZ");
                Debug.Log("BBBBBB");*/

                storageRef.GetFileAsync((string)posterImage).ContinueWithOnMainThread( task => {
                if (!task.IsFaulted && !task.IsCanceled)
                {
                        Debug.Log("CCCCCC");

                        Debug.Log("File downloaded.");
                    Debug.Log(task.GetType());
                }
                    else
                    {
                        Debug.Log("DDDDDDDDD");

                        StartCoroutine(GetTexture((string)posterImage,newPoster.GetComponentsInChildren<Image>()[1]));
                        newPoster.SetActive(true);
                    }

                });
            }
            else
            {
                Debug.Log("ZZZZZZZZZ");
                // Both Text and Image Poster
            }
        }

        else
        {
            Debug.Log("WWWWWWWW");

            if (posterCaption == null || posterCaption.Equals(""))
            {
                Debug.Log("XXXXXXXXXXXXX");

                // Error
                // No Valid Poster Data


            }
            else
            {
                Debug.Log("YYYYYYYYYYY");

                // Only Text Poster
            }
        }



    }
    
    private GameObject GetPoster(ExpPosterType _posterType)
    {
        GameObject newExpPoster = null;
        if (_posterType == ExpPosterType.OnlyImage)
        {
            Debug.Log("imagePoster");
            newExpPoster = Instantiate(imagePoster);
            Debug.Log("imagePoster Instantiated");

            //newPoster.GetComponentsInChildren<Image>()[1].sprite = posterImage.sprite;
        }
        else if (_posterType == ExpPosterType.OnlyText)
        {
            Debug.Log("textPoster");

            newExpPoster = Instantiate(textPoster);
            //newPoster.GetComponentInChildren<TextMeshProUGUI>().text = posterText.text;
        }
        else if (_posterType == ExpPosterType.Both)
        {
            Debug.Log("bothPoster");
            newExpPoster = Instantiate(bothPoster);
            //newPoster.GetComponentInChildren<TextMeshProUGUI>().text = posterText.text;
            //newPoster.GetComponentsInChildren<Image>()[2].sprite = posterImage.sprite;
        }
        else
        {
            Debug.Log("Error to create Exp Poster");
        }
        Debug.Log("Posi....");
        newExpPoster.SetActive(false);
        Debug.Log("imagePoster deactivated");
        //StartCoroutine( SetPosterTransform(posterLatitude, posterLongitude, posterScaleX, posterScaleY, posterScaleZ, newExpPoster));

        if (newExpPoster != null)
        {
            newExpPoster.GetComponentInChildren<Canvas>().sortingOrder = 100;
            newExpPoster.GetComponentInChildren<Canvas>().worldCamera = Camera.main;
            Button newBtn = newExpPoster.GetComponentInChildren<Canvas>().gameObject.AddComponent<Button>();
            newBtn.onClick.AddListener(PosterClicked);
            expPostersList.Add(newExpPoster);
        }


        return newExpPoster;
    }


    public void ClearPreviousPosters()
    {
        foreach(GameObject _poster in expPostersList)
        {
            _poster.SetActive(false);
            Destroy(_poster, 5f);
        }
        expPostersList.Clear();
        previousPostersCollection.Clear();
        currentPostersCollection.Clear();
    }

    private IEnumerator SetPosterTransform(object posterLatitude, object posterLongitude, object posterAltitude, object posterScaleX, object posterScaleY, object posterScaleZ, GameObject newExpPoster)
    {
        Debug.Log("IEnumerator");
        //Debug.Log("Parse  " + (float)posterScaleX);
        Debug.Log("POSGH");
        newExpPoster.transform.localScale = new Vector3(
                    (float)(double)posterScaleX,
                    (float)(double)posterScaleY,
                    (float)(double)posterScaleZ);
        Debug.Log("imagePoster Scaled");

        var anchor = anchorManager.AddAnchor((double)posterLatitude,
            (double)posterLongitude,
            (double)posterAltitude,
            newExpPoster.transform.rotation);
        Debug.Log("<<<<<<");


        GeospatialPose newGP = new GeospatialPose();
        newGP.Latitude = (double)posterLatitude;
        newGP.Longitude = (double)posterLongitude;
        newGP.Altitude = (double)posterAltitude;
        newGP.EunRotation = newExpPoster.transform.rotation;

        var aRPose = earthManager.Convert(newGP);
        Debug.Log("ARPose " + aRPose.position);


        newExpPoster.transform.SetParent(anchor.transform, false);
        newExpPoster.transform.localPosition = Vector3.zero;

        Debug.Log("ZZZZZ  Parent Set");
        newExpPoster.SetActive(true);
        yield return null;
    }

    IEnumerator GetTexture(string _textureURL, Image _image)
    {
        Debug.Log("Unity Coroutine");
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(_textureURL))
        {
            Debug.Log("123Unity Coroutine");

            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var texture = DownloadHandlerTexture.GetContent(uwr);
                Sprite newImageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height),
        new Vector2(0.5f, 0.5f), 100);
                Debug.Log(texture);
                _image.sprite = newImageSprite;
               
            }
        }
        Debug.Log("12345Unity Coroutine");

    }

    [SerializeField] private AREarthManager earthManager;
    private void GetPosterPose(Transform _go)
    {
        
    }



    private void AlertSomeError()
    {
        //Pose pose = new Pose(currentPoster.transform.position, currentPoster.transform.rotation);
        //GeospatialPose geospatialPose = EarthManager.Convert(pose);
        AlertDialog alert = new AlertDialog();
        alert.build(AlertDialog.THEME_HOLO_DARK)
       .setTitle("Something Error")
       .setMessage("Start new Session")
       .setIcon("alert_icon")
       .setNeutralButtion("OK", () => { Debug.Log("Negitive btn clicked"); alert.dismiss(); })
       .show();
    }
    [SerializeField] private GameObject actionPanel;
    public void PosterClicked()
    {
        Debug.Log("Poster clicked");
        Toast.make("Actions Coming Soon...", Toast.LENGTH_SHORT);
        actionPanel.SetActive(true);
    }

    private void AlertLocationError()
    {
        AlertDialog alert = new AlertDialog();
        alert.build(AlertDialog.THEME_HOLO_DARK)
       .setTitle("Earth Tracking Error")
       .setMessage("Confirm your device location turned on")
       .setIcon("alert_icon")
       .setNeutralButtion("OK", () => { Debug.Log("Negitive btn clicked");
           ClearPreviousPosters();
           UIManager.Singleton.CurrentUIState = UIStates.Home;
           alert.dismiss(); })
       .show();
    }
}


public enum ExpPosterType
{
    OnlyText,
    OnlyImage,
    Both
}
