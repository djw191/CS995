using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuUIManager : MonoBehaviour
{
    private Button _quitButton;
    private Button _startButton;

    private UIDocument _uiDocument;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        _uiDocument = GetComponent<UIDocument>();
        _startButton = _uiDocument.rootVisualElement.Q<Button>("StartButton");
        _quitButton = _uiDocument.rootVisualElement.Q<Button>("QuitButton");
        _startButton.clicked += StartButtonOnclicked;
        _quitButton.clicked += Application.Quit;
    }

    private void OnDestroy()
    {
        _startButton.clicked -= StartButtonOnclicked;
        _quitButton.clicked -= Application.Quit;
    }

    private void StartButtonOnclicked()
    {
        SceneManager.LoadScene("Main");
    }
}