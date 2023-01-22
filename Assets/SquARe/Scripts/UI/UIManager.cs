
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AndroidNativeCore;

public class UIManager : MonoBehaviour
{
    private UIStates currentUIState;

    public UIStates CurrentUIState
    {
        get
        {
            return currentUIState;
        }
        set
        {
            currentUIState = value;

            if (currentUIState == UIStates.Login)
            {
                ShowLoginPage();
            }
            else if(currentUIState == UIStates.Home)
            {
                ShowHomePage();
            }
            else if (currentUIState == UIStates.WritePost)
            {
                StartWritingPost();
            }
            else if (currentUIState == UIStates.PlacePosterAR)
            {
                StartPlacingAnchor();
            }
            else if (currentUIState == UIStates.Experience)
            {
                ShowExperiencePage();
            }
            /*else if (currentUIState == UIStates.PlacePosterAR)
            {
                StartPlacingAnchor();
            }*/
            else
            {
                Debug.Log("Error occurred");
            }
        }
    }


    public static UIManager Singleton;

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



    private void Start()
    {
        //CurrentUIState = 0;
    }


    [SerializeField] private GameObject writepostPanel;
    [SerializeField] private GameObject placePosterAnchorPanel;
    [SerializeField] private GameObject loginPanel, homePanel;
    [SerializeField] private GameObject experiencePanel;



    private void StartWritingPost()
    {
        loginPanel.SetActive(false);
        homePanel.SetActive(false);
        experiencePanel.SetActive(false);
        writepostPanel.SetActive(true);
        placePosterAnchorPanel.SetActive(false);
        ARManager.Singleton.EnableAR(false);
    }

    private void StartPlacingAnchor()
    {
        ARManager.Singleton.EnableAR();

        loginPanel.SetActive(false);
        homePanel.SetActive(false);
        experiencePanel.SetActive(false);
        writepostPanel.SetActive(false);
        placePosterAnchorPanel.SetActive(true);
    }


    private void ShowLoginPage()
    {
        loginPanel.SetActive(true);
        homePanel.SetActive(false);
        experiencePanel.SetActive(false);
        writepostPanel.SetActive(false);
        placePosterAnchorPanel.SetActive(false);
        ARManager.Singleton.EnableAR(false);
    }

    private void ShowHomePage()
    {
        loginPanel.SetActive(false);
        homePanel.SetActive(true);
        experiencePanel.SetActive(false);

        writepostPanel.SetActive(false);
        placePosterAnchorPanel.SetActive(false);

        ;// RecentPosters.Singleton.RefreshRecentposters();
        ARManager.Singleton.EnableAR(false);
    }

    private void ShowExperiencePage()
    {
        ARManager.Singleton.EnableAR();

        loginPanel.SetActive(false);
        homePanel.SetActive(false);
        experiencePanel.SetActive(true);

        writepostPanel.SetActive(false);
        placePosterAnchorPanel.SetActive(false);
    }

    public void OnClickUIStateButton(int _toState)
    {
        CurrentUIState = (UIStates)_toState;
    }


    [SerializeField] public TMP_InputField speedModifier;

    [SerializeField] public GameObject AddImageSubPanel;
    [SerializeField] public GameObject SelectedImageSubPanel;



    [SerializeField] public Button placePosterBtn;

    public void ComingSoonToast()
    {
        Toast.make("Coming Soon...", Toast.LENGTH_SHORT);
    }


    [SerializeField] public GameObject fuelingPanel;


    [SerializeField] private AudioSource helpAudio;
    public void PlayPositioningHelpAudio()
    {
        helpAudio.Stop();
        helpAudio.Play();
    }

    public void StopPositioningHelpAudio()
    {
        helpAudio.Stop();
    }
}



public enum UIStates
{
    Login,
    Home,
    WritePost,
    PlacePosterAR,
    Experience
}
