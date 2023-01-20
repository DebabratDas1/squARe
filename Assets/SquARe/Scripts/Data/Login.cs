using Google;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Firebase.Extensions;

public class Login : MonoBehaviour
{
    [SerializeField] private string webClientId = "<your client id here>";

    private GoogleSignInConfiguration configuration;

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    Firebase.Auth.FirebaseAuth auth;
    public Firebase.Auth.FirebaseUser user = null;

    /*public bool isLoggedIn
    {

        get
        {
            return isLoggedIn;
        }
        set
        {
            Debug.Log("JJJJJJJJJJJJJJ");
            isLoggedIn = value;
            if (isLoggedIn)
            {
                loginScreen.SetActive(false);
                profileScreen.SetActive(true);
                Debug.Log("CCCCCCCCCCCCC");
            }
        }
    }*/


    [SerializeField] private TextMeshProUGUI userNameTxt;
    [SerializeField] private Image userProfilePic;
    private string ImageUrl = "";


    [SerializeField] private TextMeshProUGUI statusText;

    // Defer the configuration creation until Awake so the web Client ID
    // Can be set via the property inspector in the Editor.
    void Awake()
    {
        /*Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });*/
        configuration = new GoogleSignInConfiguration
        {
            WebClientId = webClientId,
            RequestIdToken = true
        };
    }

    void Start()
    {
        UIManager.Singleton.CurrentUIState = UIStates.Login;
        InitFirebase();
    }

    void InitFirebase()
    {


        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);

    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {

        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
                SetProfileData();

            }
        }
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    public void GoogleSignInClick()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        if (task.IsFaulted)
        {
            Debug.Log("Fault");
        }
        else if (task.IsCanceled)
        {
            Debug.Log("Login Cancel");
        }
        else
        {
            Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.Log("SignInwithCredentialAsync was cancelled");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.Log("SignInwithCredentialAsync was Errored");
                    return;
                }
                Debug.Log("SignedIn");
                SetProfileData();
                
                Debug.Log("SignedIn :::::"+auth.CurrentUser.UserId);

            });
        }
       /* if (task.IsFaulted)
        {
            using (IEnumerator<System.Exception> enumerator =
                    task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                            (GoogleSignIn.SignInException)enumerator.Current;
                    AddStatusText("Got Error: " + error.Status + " " + error.Message);
                }
                else
                {
                    AddStatusText("Got Unexpected Exception?!?" + task.Exception);
                }
            }
        }
        else if (task.IsCanceled)
        {
            AddStatusText("Canceled");
        }
        else
        {
            //Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

            //Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            Debug.Log("AAAAAAAAAAA");
            user = auth.CurrentUser;
            Debug.Log("NNNNNNNNNNNNN");
            //Debug.Log("User : " + user.UserId.ToString());
            //user.UserId
            Debug.Log("PPPPPPPPPPPPP");

            SetProfileData();
            Debug.Log("ZZZZZZZZZZZZZZZZ");

        }*/
    }

    private void SetProfileData()
    {
        Debug.Log("BBBBBBBBBBBB");
        UIManager.Singleton.CurrentUIState = UIStates.Home;
        UserData.userID = auth.CurrentUser.UserId;
        UserData.userName = auth.CurrentUser.DisplayName;
        UserData.user = auth.CurrentUser;


        userNameTxt.text = "Hii, "+user.DisplayName+"\nWelcome to SquARe!";
        

        UIManager.Singleton.CurrentUIState = UIStates.Home;
        Debug.Log("EEEEEEEEEEEEE");
        StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl.ToString())));
        Debug.Log("FFFFFFFFFFFF");
        Debug.Log("SignedIn :::::+++" + auth.CurrentUser.UserId);
    }

    private string CheckImageUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }
        return ImageUrl;
    }



    IEnumerator LoadImage(string imageUrl)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result==UnityWebRequest.Result.ConnectionError)
            {
                AddStatusText("Failed To Load profile image");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                UserData.userPic = texture;
                userProfilePic.sprite = Sprite.Create(texture,
                    new Rect(0, 0, texture.width, texture.height),
                    new Vector2(0, 0));
            }
        }



    }

    private List<string> messages = new List<string>();
    public void AddStatusText(string text)
    {
        if (messages.Count == 5)
        {
            messages.RemoveAt(0);
        }
        messages.Add(text);
        string txt = "";
        foreach (string s in messages)
        {
            txt += "\n" + s;
        }
        statusText.text = txt;
    }
}
