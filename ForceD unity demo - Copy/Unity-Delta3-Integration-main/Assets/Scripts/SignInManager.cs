using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class SignInManager : MonoBehaviour
{
    public TMP_InputField personalIDField;
    public TMP_Text statusText;
    public Button signInButton;

    private string[] validIDs = { "1675920", "1234567", "0123456" };
    public GameObject gameplayRoot;


    private void Start()
    {
        signInButton.onClick.AddListener(OnSignInButtonClicked);
        //gameplayRoot.SetActive(false);
    }

    public void OnSignInButtonClicked()
    {
        string personalID = personalIDField.text;

        // Your authentication logic here...

        if (ArrayContains(validIDs, personalID))
        {
            statusText.text = "Sign-in successful!";
            gameplayRoot.SetActive(true);
            LoadGameplayScene();
        }
        else
        {
            statusText.text = "Invalid credentials. Please try again.";
        }
    }
    private bool ArrayContains(string[] array, string value)
    {
        foreach (string item in array)
        {
            if (item == value)
            {
                return true;
            }
        }
        return false;
    }

    private void LoadGameplayScene()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
