using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Toggle : MonoBehaviour
{
    public bool isButtonStatePrivate = false;

    [SerializeField] private Sprite publicStateSprite, privateStateSprite;
    private Image buttonImage;
    [SerializeField] private TextMeshProUGUI buttonStatusText;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnToggle);
        buttonImage = GetComponent<Image>();
    }
    private void OnToggle()
    {
        isButtonStatePrivate = !isButtonStatePrivate;
        if (isButtonStatePrivate)
        {
            buttonImage.sprite = privateStateSprite;
            buttonStatusText.text = "Private";
        }
        else
        {
            buttonImage.sprite = publicStateSprite;
            buttonStatusText.text = "Public";

        }
        Debug.Log("Is Private : " + isButtonStatePrivate);
    }

}
