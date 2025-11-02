using Google;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Firebase.Extensions;
using AndroidNativeCore;
using Google.Impl;
using Firebase.Auth;


public class Login : MonoBehaviour
{
    [SerializeField] private string webClientId = "65968097671-duts45t9uhprqhp9vuvavdoclp0bv1m8.apps.googleusercontent.com";

    private GoogleSignInConfiguration configuration;

    Firebase.DependencyStatus dependencyStatus = Firebase.DependencyStatus.UnavailableOther;
    Firebase.Auth.FirebaseAuth auth;
    public Firebase.Auth.FirebaseUser user = null;
    private bool isGoogleSignInInitialized = false;

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
            RequestIdToken = true,
            RequestEmail=true,
         
        };
    }

    void Start()
    {
        UIManager.Singleton.CurrentUIState = UIStates.Login;
        InitFirebase();
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
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
                auth = null;
                UIManager.Singleton.CurrentUIState = UIStates.Login;

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
    }

    public void GoogleSignInClick()
    {
        GoogleSignIn.Configuration = configuration;
        GoogleSignIn.Configuration.UseGameSignIn = false;
        GoogleSignIn.Configuration.RequestIdToken = true;
        GoogleSignIn.Configuration.RequestEmail = true;
        GoogleSignIn.DefaultInstance.EnableDebugLogging(true);

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(OnAuthenticationFinished);
    }

    private void GoogleLogin()
    {

        Debug.Log("Inside GoogleLogin ooo");
        if (!isGoogleSignInInitialized)
        {
            Debug.Log("Inside GoogleLogin isGoogleSignInInitialized  false");

            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = webClientId,
                RequestEmail = true
            };

            Debug.Log("Google Configuration Set  1111");

            isGoogleSignInInitialized = true;
            Debug.Log(" isGoogleSignInInitialized  true");

        }

        Debug.Log(" outside of IF isGoogleSignInInitialized");


        /*GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = webClientId
        };
        Debug.Log("Google Configuration Set  2222");


        GoogleSignIn.Configuration.RequestEmail = true;

        Debug.Log("Google SIGN IN will be called");*/


        Task<GoogleSignInUser> signIn = GoogleSignIn.DefaultInstance.SignIn();

        Debug.Log("Google SIGN IN called");


        TaskCompletionSource<FirebaseUser> signInCompleted = new TaskCompletionSource<FirebaseUser>();

        Debug.Log(" TaskCompletionSource FirebaseUser declared");


        signIn.ContinueWith(task =>
        {
            Debug.Log(" SignIn ContinueWith");

            if (task.IsCanceled)
            {
                Debug.Log("Login ++++++++++++ Canceled");
                signInCompleted.SetCanceled();
            }
            else if (task.IsFaulted)
            {
                Debug.Log("Login ++++++++++++ Faulted");
                signInCompleted.SetException(task.Exception);

                Debug.Log("Failed : " + task.Exception);
            }
            else
            {
                Debug.Log("Login Inside else block");
                Credential credential = GoogleAuthProvider.GetCredential(task.Result.IdToken, null);

                Debug.Log("Credential declared ");
                    auth.SignInWithCredentialAsync(credential).ContinueWith(authTask =>
                    {
                        Debug.Log("Inside SignInWithCredentialAsync ContinueWith ");

                        if (authTask.IsCanceled)
                        {
                            Debug.Log("Inside authTask iscancelled");
                            signInCompleted.SetCanceled();
                        }
                        else if (authTask.IsFaulted)
                        {
                            Debug.Log("Inside authTask isfaulted");

                            signInCompleted.SetException(authTask.Exception);
                            Debug.Log("Faulted in auth " + authTask.Exception);
                        }
                        else
                        {
                            Debug.Log("Inside authTask else block");

                            signInCompleted.SetResult(((Task<FirebaseUser>)authTask).Result);
                            Debug.Log("Success");

                            user = auth.CurrentUser;

                            Debug.Log("User has been set");
                            SetProfileData();

                            Debug.Log("SetProfileData called");
                        }
                    });
            }
        });
    }

    public void OnClickSignOut()
    {
        AlertDialog alert = new AlertDialog();
        alert.build(AlertDialog.THEME_HOLO_DARK)
       .setTitle("Sign Out")
       .setMessage("Do you really want to Sign Out?")
       .setIcon("alert_icon")
       .setNegativeButtion("Yes", () => {
           //auth.SignOut();
           GoogleSignIn.DefaultInstance.SignOut();
           GoogleSignIn.DefaultInstance.Disconnect();
           alert.dismiss();
           
           Debug.Log("Sign out...ABC");
           //UserData.userID = "";
           UIManager.Singleton.CurrentUIState = UIStates.Login;
           Debug.Log("Sign out...DEF"+auth.CurrentUser.ToString());
           RecentPosters.Singleton.ClearRecentPosters();
       })
       .setPositiveButtion("No", () => { alert.dismiss();})
       //.setNeutralButtion("OK", () => { Debug.Log("Negitive btn clicked"); alert.dismiss(); })
       .show();
        //AddStatusText("Calling SignOut");
        //GoogleSignIn.DefaultInstance.SignOut();
    }

    internal void OnAuthenticationFinished(Task<GoogleSignInUser> task)
    {
        Debug.Log("Inside OnAuthenticationFinished");
        if (task.IsFaulted)
        {
            Debug.Log("Fault "+task.Status.ToString());

            using (System.Collections.Generic.IEnumerator<System.Exception> enumerator =
               task.Exception.InnerExceptions.GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    GoogleSignIn.SignInException error =
                                (GoogleSignIn.SignInException)enumerator.Current;

                    // CRITICAL: Log the error Status code
                    Debug.LogError("Google Sign-In FAILURE! Status Code: " + error.Status +
                                   " | Message: " + error.Message);
                }
            }

        }
        else if (task.IsCanceled)
        {
            Debug.Log("Login Cancel");
        }
        else
        {
            Debug.Log("Inside OnAuthenticationFinished else");

            Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(task.Result.IdToken, null);
            Debug.Log("Inside OnAuthenticationFinished After Authentication credential");

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
            {
                Debug.Log("Inside SignInWithCredentialAsync ");

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
                UserData.isDataSet = true;
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
