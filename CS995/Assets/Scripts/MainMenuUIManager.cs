using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIManager : MonoBehaviour
{
    private Button _quitButton;
    private Button _startButton;
    private Button _creditsButton;

    private UIDocument _uiDocument;
    private Button _backButton;
    private VisualElement _menu;
    private VisualElement _credits;
    private Button _showTutorial;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _uiDocument = GetComponent<UIDocument>();
        _startButton = _uiDocument.rootVisualElement.Q<Button>("StartButton");
        _quitButton = _uiDocument.rootVisualElement.Q<Button>("QuitButton");
        _creditsButton = _uiDocument.rootVisualElement.Q<Button>("CreditsButton");
        _backButton = _uiDocument.rootVisualElement.Q<Button>("BackButton");
        _menu = _uiDocument.rootVisualElement.Q<VisualElement>("Menu");
        _credits = _uiDocument.rootVisualElement.Q<VisualElement>("Credits");
        _showTutorial = _uiDocument.rootVisualElement.Q<Button>("ShowTutorialButton");

        _startButton.clicked += StartButtonOnclicked;
        _quitButton.clicked += Application.Quit;
        _showTutorial.clicked += ShowTutorialOnClicked;

        _creditsButton.clicked += CreditsButtonOnclicked;
        _backButton.clicked += BackButtonOnClicked;
        _credits.style.visibility = Visibility.Hidden;
        StartCoroutine(UIManager.FocusElement(_startButton));
    }

    private void ShowTutorialOnClicked()
    {
        PlayerPrefs.SetInt("Tutorial", 0);
    }

    private void BackButtonOnClicked()
    {
        _menu.style.visibility = Visibility.Visible;
        _credits.style.visibility = Visibility.Hidden;
        StartCoroutine(UIManager.FocusElement(_creditsButton));
    }

    private void OnDestroy()
    {
        _startButton.clicked -= StartButtonOnclicked;
        _quitButton.clicked -= Application.Quit;
        _showTutorial.clicked -= ShowTutorialOnClicked;
    }

    private void StartButtonOnclicked()
    {
        SceneManager.LoadScene("Main");
    }

    private void CreditsButtonOnclicked()
    {
        _menu.style.visibility = Visibility.Hidden;
        _credits.style.visibility = Visibility.Visible;
        StartCoroutine(UIManager.FocusElement(_backButton));
    }
}