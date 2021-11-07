using System.Collections;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginSceneManager : MonoBehaviour
{
    private GameObject alreadyRegisteredButton;
    private GameObject emailInputField;
    private GameObject eventPopUp;
    private bool isAutomaticLogin;
    private bool isRegistering;
    private GameObject loginButton;
    private GameObject notRegisteredButton;
    private GameObject passwordInputField;
    private GameObject registerButton;
    private GameObject usernameInputField;

    private void Start()
    {
        usernameInputField = GameObject.FindWithTag("UsernameInputField");
        passwordInputField = GameObject.FindWithTag("PasswordInputField");
        emailInputField = GameObject.FindWithTag("EmailInputField");
        loginButton = GameObject.FindWithTag("LoginButton");
        registerButton = GameObject.FindWithTag("RegisterButton");
        notRegisteredButton = GameObject.FindWithTag("NotRegisteredButton");
        alreadyRegisteredButton = GameObject.FindWithTag("AlreadyRegisteredButton");
        eventPopUp = GameObject.FindWithTag("EventPopUp");

        eventPopUp.SetActive(false);

        SetActivities();

        if (PlayerPrefs.HasKey("USERNAME") && PlayerPrefs.HasKey("PASSWORD"))
        {
            isAutomaticLogin = true;
            var request = new LoginWithPlayFabRequest
            {
                Username = PlayerPrefs.GetString("USERNAME"),
                Password = PlayerPrefs.GetString("PASSWORD")
            };
            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
        }
    }

    public void OnValueChangeForInputFields()
    {
        var emailText = emailInputField.GetComponent<TMP_InputField>().text;
        var usernameText = usernameInputField.GetComponent<TMP_InputField>().text;
        var passwordText = passwordInputField.GetComponent<TMP_InputField>().text;

        if (isRegistering)
        {
            if (emailText != null
                && usernameText != null
                && passwordText != null
                && !emailText.Equals("")
                && !usernameText.Equals("")
                && !passwordText.Equals(""))
            {
                registerButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                registerButton.GetComponent<Button>().interactable = false;
            }
        }
        else
        {
            if (usernameText != null
                && passwordText != null
                && !usernameText.Equals("")
                && !passwordText.Equals(""))
            {
                loginButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                loginButton.GetComponent<Button>().interactable = false;
            }
        }
    }

    public void OnClickNotRegistered()
    {
        isRegistering = true;
        SetActivities();
    }

    public void OnClickAlreadyRegistered()
    {
        isRegistering = false;
        SetActivities();
    }

    private void SetActivities()
    {
        registerButton.SetActive(isRegistering);
        emailInputField.SetActive(isRegistering);
        alreadyRegisteredButton.SetActive(isRegistering);

        loginButton.SetActive(!isRegistering);
        notRegisteredButton.SetActive(!isRegistering);
    }

    //---------------------------- LOGIN FUNCTIONS ---------------------------------------

    public void OnClickLogin()
    {
        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
        {
            PlayFabSettings.staticSettings.TitleId = "AF053";
        }

        var usernameText = usernameInputField.GetComponent<TMP_InputField>().text;
        var passwordText = passwordInputField.GetComponent<TMP_InputField>().text;

        var request = new LoginWithPlayFabRequest
        {
            Username = usernameText,
            Password = passwordText
        };
        PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("Successful login!");

        var usernameText = usernameInputField.GetComponent<TMP_InputField>().text;
        var passwordText = passwordInputField.GetComponent<TMP_InputField>().text;

        if (!isAutomaticLogin)
        {
            PlayerPrefs.SetString("USERNAME", usernameText);
            PlayerPrefs.SetString("PASSWORD", passwordText);
        }

        isAutomaticLogin = false;

        //TODO: check result
        SceneManager.LoadScene("Start");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Login was not successful!");
        StartCoroutine(PopUpEvent(error.ErrorMessage));
        //TODO: check and manager error
        isAutomaticLogin = false;
    }

    private IEnumerator PopUpEvent(string message)
    {
        eventPopUp.SetActive(true);
        eventPopUp.GetComponentInChildren<TMP_Text>().text = message;
        yield return new WaitForSeconds(5);
        eventPopUp.SetActive(false);
    }

    //---------------------------- REGISTER FUNCTIONS ---------------------------------------

    public void OnClickRegister()
    {
        var emailText = emailInputField.GetComponent<TMP_InputField>().text;
        var usernameText = usernameInputField.GetComponent<TMP_InputField>().text;
        var passwordText = passwordInputField.GetComponent<TMP_InputField>().text;

        var registerRequest = new RegisterPlayFabUserRequest
        {
            Email = emailText,
            Username = usernameText,
            Password = passwordText
        };
        PlayFabClientAPI.RegisterPlayFabUser(registerRequest, OnRegisterSuccess, OnRegisterFailure);
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult obj)
    {
        isRegistering = false;
        SetActivities();

        //TODO: check result
        Debug.Log("Successful registering!");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        Debug.LogWarning("Registering was not successful!");
        StartCoroutine(PopUpEvent(error.ErrorMessage));
    }
}