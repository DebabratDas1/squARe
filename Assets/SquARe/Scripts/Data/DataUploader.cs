using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;
using Google.XR.ARCoreExtensions;
using System.Threading.Tasks;
using Firebase.Storage;
using System;
using System.IO;
using System.Collections;

public class DataUploader : MonoBehaviour
{
    private FirebaseStorage storage;
    string lastUploadedFileURL = null;
    private CollectionReference colRef;

    public static DataUploader Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }


    private void Start()
    {
         FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        colRef = db.Collection("SquARe").Document("AR Poster").Collection("AllPostersData");
        
        /* colRef.AddAsync(user).ContinueWithOnMainThread(task =>
         {
             Debug.Log("Added data to the alovelace document in the users collection.");
         });*/


        //AREarthManager.Convert(Pose)
    }


    [SerializeField] private AREarthManager earthManager;
    public void UploadPoster(Poster _poster)
    {
        GeospatialPose posterGeoPose = GetPosterGeoPose(_poster.transform);
        Vector3 posterScale = _poster.transform.lossyScale;
        Debug.Log("Poster Image Path : " + _poster.posterImagePath);
        if (_poster.posterImagePath != null)
        {
            _poster.posterImageUrl = UploadPosterImage(_poster.posterImagePath);
        }
        Debug.Log("PosterImageURL : " + _poster.posterImageUrl);
        Dictionary<string, object> poster = new Dictionary<string, object>
{
        { "UserID", UserData.userID },
        { "Lat", posterGeoPose.Latitude },
        { "Lon", posterGeoPose.Longitude },
        { "Alt", posterGeoPose.Altitude },
        { "ScaleX", posterScale.x },
        { "ScaleY", posterScale.y },
        { "ScaleZ", posterScale.z },
        { "IsPrivate", _poster.isPosterPrivate },
        { "IsDeleted", false },
        { "Text", _poster.posterText },
        { "ImageUrl", _poster.posterImageUrl },
        { "TimeStamp",DateTime.Now.ToString() },
};
        Debug.Log("Poster Uploading...Poster Privacy : " + _poster.isPosterPrivate);

        colRef.AddAsync(poster).ContinueWithOnMainThread(task =>
        {
            Debug.Log("Added data to the AllPostersData colection in the Square/ARPoster collection.");
            _poster.PosterCurrentState = PosterState.Placed;
        });

        _poster.PosterCurrentState = PosterState.Placed;
    }


    private GeospatialPose GetPosterGeoPose(Transform _go)
    {
        Pose posterPose = new Pose(
           _go.position,
           _go.rotation
           );
        GeospatialPose _posterGeoPose = earthManager.Convert(posterPose);
        return _posterGeoPose;
    }



    public string UploadPosterImage(string imagePath)
    {
        Debug.Log("!!!!!!!!!!!!!!!!!!");
        storage = FirebaseStorage.DefaultInstance;
        StorageReference storageRef = storage.GetReferenceFromUrl(
            "gs://square-9f7d0.appspot.com/SquARe/ARPosterImages"
            );
        Debug.Log("111111111111111");

        string newBackstageItemID = System.Guid.NewGuid().ToString();
        Debug.Log("2222222222222222222");

        Debug.Log("New Image Name in Storage :  " + DateTime.Now);
        StorageReference imgref = storageRef.Child(UserData.userID + "/" + DateTime.Now.ToString() + ".jpg");


        Debug.Log("33333333333333333");
        string local_file = Path.Combine(Application.persistentDataPath, imagePath);
        string local_file_uri = string.Format("{0}://{1}", Uri.UriSchemeFile, local_file);


        //byte[] imageBytes = textureToUpload.GetRawTextureData();

        Debug.Log("444444444444444444");

        // Upload the file to the path "images/rivers.jpg"
        imgref.PutFileAsync(local_file_uri)
            .ContinueWithOnMainThread((Task<StorageMetadata> task) => {

                Debug.Log("555555555555555555555555");

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.Log("6666666666666666666666");

                    Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                else
                {
                    Debug.Log("7777777777777777777");

                    // Metadata contains file metadata such as size, content-type, and md5hash.
                    // StorageMetadata metadata = task.Result;
                    // string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                    // Debug.Log("md5 hash = " + md5Hash);
                    lastUploadedFileURL = imgref.GetDownloadUrlAsync().Result.AbsoluteUri;
                    Debug.Log("Download URL : " + lastUploadedFileURL);


                }
            });
        Debug.Log("88888888888888888888888888");
        return lastUploadedFileURL;
    }











    public void UploadPoster1(Poster _poster)
    {
        GeospatialPose posterGeoPose = GetPosterGeoPose(_poster.transform);
        Vector3 posterScale = _poster.transform.lossyScale;
        Debug.Log("Poster Image Path : " + _poster.posterImagePath);
        if (_poster.posterImagePath != null)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!");
            storage = FirebaseStorage.DefaultInstance;
            StorageReference storageRef = storage.GetReferenceFromUrl(
                "gs://square-9f7d0.appspot.com/SquARe/ARPosterImages"
                );
            Debug.Log("111111111111111");

            string newBackstageItemID = System.Guid.NewGuid().ToString();
            Debug.Log("2222222222222222222");

            Debug.Log("New Image Name in Storage :  " + DateTime.Now);
            StorageReference imgref = storageRef.Child(UserData.userID + "/" + DateTime.Now.ToString() + ".jpg");


            Debug.Log("33333333333333333");
            string local_file = Path.Combine(Application.persistentDataPath, _poster.posterImagePath);
            string local_file_uri = string.Format("{0}://{1}", Uri.UriSchemeFile, local_file);


            //byte[] imageBytes = textureToUpload.GetRawTextureData();

            Debug.Log("444444444444444444");

            // Upload the file to the path "images/rivers.jpg"
            
            imgref.PutFileAsync(local_file_uri)
                .ContinueWithOnMainThread((Task<StorageMetadata> task) => {

                    Debug.Log("555555555555555555555555");
                    if (task.IsFaulted || task.IsCanceled)
                    {
                        Debug.Log("6666666666666666666666");

                        Debug.Log(task.Exception.ToString());
                    // Uh-oh, an error occurred!
                }
                    else
                    {
                        Debug.Log("7777777777777777777");

                    // Metadata contains file metadata such as size, content-type, and md5hash.
                    // StorageMetadata metadata = task.Result;
                    // string md5Hash = metadata.Md5Hash;
                    Debug.Log("Finished uploading...");
                        Debug.Log("Finished uploading..."+ task.Result.Path);
                        // Debug.Log("md5 hash = " + md5Hash);
                        //lastUploadedFileURL = imgref.GetDownloadUrlAsync().Result.AbsoluteUri;
                        imgref.GetDownloadUrlAsync().ContinueWithOnMainThread(grt =>
                        {
                            Debug.Log("Download URL : " + grt.Result.AbsoluteUri);
                            lastUploadedFileURL = grt.Result.AbsoluteUri;
                            _poster.posterImageUrl = lastUploadedFileURL;



                            Debug.Log("Download URL : " + lastUploadedFileURL);



                            Debug.Log("PosterImageURL : " + _poster.posterImageUrl);
                            Dictionary<string, object> poster = new Dictionary<string, object>
{
        { "UserID", UserData.userID },
        { "Lat", posterGeoPose.Latitude },
        { "Lon", posterGeoPose.Longitude },
        { "Alt", posterGeoPose.Altitude },
        { "ScaleX", posterScale.x },
        { "ScaleY", posterScale.y },
        { "ScaleZ", posterScale.z },
        { "IsPrivate", _poster.isPosterPrivate },
        { "IsDeleted", false },
        { "Text", _poster.posterText },
        { "ImageUrl", _poster.posterImageUrl },
        { "TimeStamp",DateTime.Now.ToString() },
};

                            Debug.Log("Poster Uploading...Poster Privacy : " + _poster.isPosterPrivate);

                            colRef.AddAsync(poster).ContinueWithOnMainThread(task =>
                            {
                                Debug.Log("Added data to the AllPostersData colection in the Square/ARPoster collection.");
                                _poster.PosterCurrentState = PosterState.Placed;
                            });

                            _poster.PosterCurrentState = PosterState.Placed;
                            UIManager.Singleton.fuelingPanel.SetActive(false);


                        });
                        

                        


                    }
                });
            Debug.Log("88888888888888888888888888");
            //return lastUploadedFileURL;
        }
        else
        {
            Debug.Log("PosterImageURL : " + _poster.posterImageUrl);
            Dictionary<string, object> poster = new Dictionary<string, object>
{
        { "UserID", UserData.userID },
        { "Lat", posterGeoPose.Latitude },
        { "Lon", posterGeoPose.Longitude },
        { "Alt", posterGeoPose.Altitude },
        { "ScaleX", posterScale.x },
        { "ScaleY", posterScale.y },
        { "ScaleZ", posterScale.z },
        { "IsPrivate", _poster.isPosterPrivate },
        { "IsDeleted", false },
        { "Text", _poster.posterText },
        { "ImageUrl", _poster.posterImageUrl },
        { "TimeStamp",DateTime.Now.ToString() },
};
            Debug.Log("Poster Uploading...Poster Privacy : " + _poster.isPosterPrivate);

            colRef.AddAsync(poster).ContinueWithOnMainThread(task =>
            {
                Debug.Log("Added data to the AllPostersData colection in the Square/ARPoster collection.");
                _poster.PosterCurrentState = PosterState.Placed;
                UIManager.Singleton.fuelingPanel.SetActive(false);
            });
        }
        

        //_poster.PosterCurrentState = PosterState.Placed;
        //yield return null;

    }


    public IEnumerator UploadPoster2(Poster _poster)
    {
        GeospatialPose posterGeoPose = GetPosterGeoPose(_poster.transform);
        Vector3 posterScale = _poster.transform.lossyScale;
        Debug.Log("Poster Image Path : " + _poster.posterImagePath);
        if (_poster.posterImagePath != null)
        {
            Debug.Log("!!!!!!!!!!!!!!!!!!");
            storage = FirebaseStorage.DefaultInstance;
            StorageReference storageRef = storage.GetReferenceFromUrl(
                "gs://square-9f7d0.appspot.com/SquARe/ARPosterImages"
                );
            Debug.Log("111111111111111");

            string newBackstageItemID = System.Guid.NewGuid().ToString();
            Debug.Log("2222222222222222222");

            Debug.Log("New Image Name in Storage :  " + DateTime.Now);
            StorageReference imgref = storageRef.Child(UserData.userID + "/" + DateTime.Now.ToString() + ".jpg");


            Debug.Log("33333333333333333");
            string local_file = Path.Combine(Application.persistentDataPath, _poster.posterImagePath);
            string local_file_uri = string.Format("{0}://{1}", Uri.UriSchemeFile, local_file);


            //byte[] imageBytes = textureToUpload.GetRawTextureData();

            Debug.Log("444444444444444444");

            // Upload the file to the path "images/rivers.jpg"

            var abc = imgref.PutFileAsync(local_file_uri);
            
            
                

                    Debug.Log("555555555555555555555555");
                    if (abc.IsFaulted || abc.IsCanceled)
                    {
                        Debug.Log("6666666666666666666666");

                        Debug.Log(abc.Exception.ToString());
                        // Uh-oh, an error occurred!
                    }
                    else
                    {
                        Debug.Log("7777777777777777777");

                        // Metadata contains file metadata such as size, content-type, and md5hash.
                        // StorageMetadata metadata = task.Result;
                        // string md5Hash = metadata.Md5Hash;
                        Debug.Log("Finished uploading...");
                        // Debug.Log("md5 hash = " + md5Hash);
                        lastUploadedFileURL = imgref.GetDownloadUrlAsync().Result.AbsoluteUri;
                        Debug.Log("Download URL : " + lastUploadedFileURL);
                        _poster.posterImageUrl = lastUploadedFileURL;


                    }
            Debug.Log("88888888888888888888888888");
            yield return new WaitUntil(() => abc.IsCompleted);
            Debug.Log("9999999999999999999999");
            //return lastUploadedFileURL;
        }
        Debug.Log("PosterImageURL : " + _poster.posterImageUrl);
        Dictionary<string, object> poster = new Dictionary<string, object>
{
        { "UserID", UserData.userID },
        { "Lat", posterGeoPose.Latitude },
        { "Lon", posterGeoPose.Longitude },
        { "Alt", posterGeoPose.Altitude },
        { "ScaleX", posterScale.x },
        { "ScaleY", posterScale.y },
        { "ScaleZ", posterScale.z },
        { "IsPrivate", _poster.isPosterPrivate },
        { "IsDeleted", false },
        { "Text", _poster.posterText },
        { "ImageUrl", _poster.posterImageUrl },
        { "TimeStamp",DateTime.Now.ToString() },
};
        Debug.Log("Poster Uploading...Poster Privacy : " + _poster.isPosterPrivate);

        colRef.AddAsync(poster).ContinueWithOnMainThread(task =>
        {
            Debug.Log("Added data to the AllPostersData colection in the Square/ARPoster collection.");
            _poster.PosterCurrentState = PosterState.Placed;
        });

        _poster.PosterCurrentState = PosterState.Placed;
        //yield return null;

    }

}
