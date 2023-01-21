using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using AndroidNativeCore;
using UnityEngine.Networking;

public class RecentPosters : MonoBehaviour
{
    public static RecentPosters Singleton;
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
    private List<string> previousPostersCollection = new List<string>();
    private List<string> currentPostersCollection = new List<string>();
    private List<GameObject> recentPostersCollection = new List<GameObject>();

    [SerializeField] private GameObject recentPosterPrefab;
    [SerializeField] private GameObject content;
    private CollectionReference colRef;
    private void Start()
    {
        FirebaseFirestore db = FirebaseFirestore.DefaultInstance;
        colRef = db.Collection("SquARe").Document("AR Poster").Collection("AllPostersData");
        StartCoroutine(RefreshDatabaseAfter());
    }
    public void RefreshRecentposters()
    {
        ClearRecentPosters();
        Debug.Log("!!!!!!!!!!!");
        Query query = colRef.WhereEqualTo("UserID", UserData.userID);
        //Debug.Log("CCCCC");
        Debug.Log("1111111111111111");

        query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            Debug.Log("DDDD");

            QuerySnapshot capitalQuerySnapshot = task.Result;
            Debug.Log("EEEEE");
            previousPostersCollection = new List<string>(currentPostersCollection);
            currentPostersCollection.Clear();
            foreach (DocumentSnapshot documentSnapshot in capitalQuerySnapshot.Documents)
            {
                Debug.Log("333333333333");

                documentSnapshot.TryGetValue("TimeStamp", out object posterTimestamp);
                documentSnapshot.TryGetValue("Text", out object posterCaption);
                documentSnapshot.TryGetValue("ImageUrl", out object posterImage);
                documentSnapshot.TryGetValue("IsDeleted", out object posterDeleted);
                Debug.Log("444444444");
                if (!(bool)posterDeleted)
                {
                    string docID = documentSnapshot.Id;
                    GameObject newrecentposter = Instantiate(recentPosterPrefab, content.transform);
                    newrecentposter.GetComponentInChildren<TextMeshProUGUI>().text = (string)posterTimestamp;
                    newrecentposter.GetComponentsInChildren<TextMeshProUGUI>()[1].text = (string)posterCaption;
                    newrecentposter.GetComponentInChildren<Button>().onClick.AddListener(delegate
                    {
                        Debug.Log("55555555555");

                        OnClickPosterDeleteAsync(docID, newrecentposter);
                    });
                    if ((string)posterImage != null && (string)posterImage != "")
                    {
                        StartCoroutine(GetTexture((string)posterImage, newrecentposter.GetComponentsInChildren<Image>()[1]));
                    }
                    recentPostersCollection.Add(newrecentposter);
                }

            }
        });
    }

    public void ClearRecentPosters()
    {
        foreach (GameObject recentPoster in recentPostersCollection)
        {
            Destroy(recentPoster);
        }
        recentPostersCollection.Clear();
    }

    private void OnClickPosterDeleteAsync(string _docID, GameObject recentPoster)
    {
        Debug.Log("8888888888888");

        AlertDialog alert = new AlertDialog();
        alert.build(AlertDialog.THEME_HOLO_DARK)
       .setTitle("Delete Poster")
       .setMessage("Do you really want to delete this poster?")
       .setIcon("alert_icon")
       .setPositiveButtion("NO", () =>
       {
           alert.dismiss();
       })
       .setNegativeButtion("Yes", () =>
       {
           DocumentReference posterRef = colRef.Document(_docID);
           Dictionary<string, object> updates = new Dictionary<string, object>
{
    { "IsDeleted", true }
};
           posterRef.UpdateAsync(updates).ContinueWithOnMainThread(task =>
           {
               if (!task.IsCanceled && !task.IsFaulted)
               {
                   recentPostersCollection.Remove(recentPoster);
                   Destroy(recentPoster);
                   AlertDialog alert = new AlertDialog();
                   alert.build(AlertDialog.THEME_HOLO_DARK)
                  .setTitle("Poster Deleted")
                  .setMessage("Poster has been deleted successfully")
                  .setIcon("alert_icon")
                  .setNeutralButtion("OK", () =>
                  {
                      alert.dismiss();
                  })
                  .show();
               }
               else
               {
                   AlertDialog alert = new AlertDialog();
                   alert.build(AlertDialog.THEME_HOLO_DARK)
                  .setTitle("Failed")
                  .setMessage("Poster can not be deleted")
                  .setIcon("alert_icon")
                  .setNeutralButtion("OK", () =>
                  {
                      alert.dismiss();
                  })
                  .show();
               }
           });

       })
       .show();
 
    
    
    
    
    }


    private IEnumerator RefreshDatabaseAfter()
    {
        yield return new WaitForSeconds(10);
        RefreshRecentposters();
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
}
