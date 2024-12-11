using System.Collections;
using Board;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UIDocument uiDocument;
    private VisualElement _conditionPanel;
    private Button _conditionResumeButton;
    private Label _foodLabel;
    private GameManager _gameManager;
    private Label _gameOverMessage;
    private VisualElement _gameOverPanel;
    private Button _gameOverQuit;
    private bool _okayToPause;
    private VisualElement _pausePanel;
    private Button _quitButton;
    private Button _restartButton;
    private Button _resumeButton;
    private Button _toggleImButton;
    private VisualElement _conditionsElement;
    private Slider _foodSlider;
    private Slider _foodMultiplierSlider;
    private SliderInt _waypointSlider;
    private Slider _bonusMultiplierSlider;
    private SliderInt _bonusSlider;
    private SliderInt _attackSlider;
    private Slider _wallMultiplierSlider;
    private Slider _wallSlider;
    private TextField _multiplierTextField;
    private BoardManager _boardManager;
    private VisualElement _messageBoxPanel;
    private Label _messageLabel;
    private Button _messageBoxResumeButton;
    private int? _playersInitAttackPower = null;

    private void Start()
    {
        _gameManager = GameManager.Instance;
        _boardManager = _gameManager.BoardManager;

        uiDocument = GetComponent<UIDocument>();

        _foodLabel = uiDocument.rootVisualElement.Q<Label>("FoodLabel");
        UpdateFoodLabel(_gameManager.FoodAmount);

        #region PausePanel

        _pausePanel = GetPanel("PausePanel");

        _resumeButton = _pausePanel.Q<Button>("ResumeButton");
        _resumeButton.clicked += TogglePauseMenu;

        _quitButton = _pausePanel.Q<Button>("QuitButton");
        _quitButton.clicked += Application.Quit;

        _toggleImButton = _pausePanel.Q<Button>("ToggleIMButton");
        _toggleImButton.clicked += _boardManager.ToggleMovementSpeed;
        _boardManager.ToggleMovementSpeed();

        #endregion

        #region GameOverPanel

        _gameOverPanel = GetPanel("GameOverPanel");

        _gameOverMessage = _gameOverPanel.Q<Label>("GameOverMessage");

        _restartButton = _gameOverPanel.Q<Button>("RestartButton");
        _restartButton.clicked += RestartGame;

        _gameOverQuit = _gameOverPanel.Q<Button>("QuitButton");
        _gameOverQuit.clicked += Application.Quit;

        #endregion

        #region ConditionPanel

        _conditionPanel = GetPanel("ConditionPanel");

        _conditionResumeButton = _conditionPanel.Q<Button>("ResumeButton");
        _conditionResumeButton.clicked += ToggleConditionPanel;
        _conditionResumeButton.Focus();

        _conditionsElement = _conditionPanel.Q<VisualElement>("ConditionsElement");

        _foodSlider = _conditionsElement.Q<Slider>("FoodSlider");
        _foodSlider.highValue = 1.0f;
        _foodSlider.value = _boardManager.TargetFood;
        _foodMultiplierSlider = _conditionsElement.Q<Slider>("FoodMultiplierSlider");
        _foodMultiplierSlider.highValue = 4.0f;
        _foodMultiplierSlider.value = _gameManager.FoodMultiplier;

        _wallSlider = _conditionsElement.Q<Slider>("WallSlider");
        _wallSlider.highValue = 1.0f;
        _wallSlider.value = _boardManager.TargetWalls;
        _wallMultiplierSlider = _conditionsElement.Q<Slider>("WallMultiplierSlider");
        _wallMultiplierSlider.highValue = 4.0f;
        _wallMultiplierSlider.value = _gameManager.WallMultiplier;

        _attackSlider = _conditionsElement.Q<SliderInt>("AttackSlider");
        _attackSlider.highValue = _gameManager.CurrentLevel + 9;
        _attackSlider.value = _gameManager.Player.AttackPower;

        _bonusSlider = _conditionsElement.Q<SliderInt>("BonusSlider");
        _bonusSlider.highValue = (_boardManager.boardSize.x - 2) * (_boardManager.boardSize.y - 2) / 2;
        _bonusSlider.value = _boardManager.TargetPowerups;
        _bonusMultiplierSlider = _conditionsElement.Q<Slider>("BonusMultiplierSlider");
        _bonusMultiplierSlider.value = _gameManager.BonusMultiplier;
        _bonusMultiplierSlider.highValue = 4.0f;

        _waypointSlider = _conditionsElement.Q<SliderInt>("WaypointSlider");
        _waypointSlider.highValue = _gameManager.TurnManager.TurnCount + 9;
        _waypointSlider.value = _gameManager.WaypointTarget;

        _conditionsElement.RegisterCallback<ChangeEvent<float>>(OnSliderChanged);
        _conditionsElement.RegisterCallback<ChangeEvent<int>>(OnSliderIntChanged);

        _multiplierTextField = _conditionPanel.Q<TextField>("MultiplierTextField");
        _multiplierTextField.value = "1.0x";

        ToggleConditionPanel();

        #endregion

        #region MessageBoxPanel

        _messageBoxPanel = GetPanel("MessageBoxPanel");
        _messageLabel = _messageBoxPanel.Q<Label>("MessageLabel");
        _messageBoxResumeButton = _messageBoxPanel.Q<Button>("ResumeButton");
        _messageBoxResumeButton.clicked += ToggleMessageBoxPanel;

        #endregion

        
        _gameManager.OnLevelComplete += SetOkayToPause;
    }

    private void OnSliderChanged(ChangeEvent<float> _)
    {
        UpdateValues();
    }

    private void OnSliderIntChanged(ChangeEvent<int> _)
    {
        UpdateValues();
    }

    private void UpdateValues()
    {
        float mult = 1;
        _boardManager.TargetFood = _foodSlider.value;
        mult *= _boardManager.initTargetFood / _boardManager.TargetFood;
        _boardManager.TargetWalls = _wallSlider.value;
        mult *= _boardManager.TargetWalls / _boardManager.initTargetWalls;
        
        _playersInitAttackPower ??= _gameManager.Player.AttackPower;
        _gameManager.Player.AttackPower = _attackSlider.value;
        mult *= (float)_playersInitAttackPower / _gameManager.Player.AttackPower;

        //These are always default 0, base mult on that
        _gameManager.WaypointTarget = _waypointSlider.value;
        mult *= _gameManager.WaypointTarget < 1 ? 1 : (float)_gameManager.WaypointTarget + 1;
        _boardManager.TargetPowerups = _bonusSlider.value;
        mult *= _boardManager.TargetPowerups < 1 ? 1 : 1 / ((float)_boardManager.TargetPowerups + 1);

        _gameManager.FoodMultiplier = _foodMultiplierSlider.value;
        mult *= 1 / _gameManager.FoodMultiplier;
        _gameManager.WallMultiplier = _wallMultiplierSlider.value;
        mult *= _gameManager.WallMultiplier;
        _gameManager.BonusMultiplier = _bonusMultiplierSlider.value;
        mult *= 1 / _gameManager.BonusMultiplier;

        _multiplierTextField.value = mult.ToString("n2") + "X";
        if (float.IsPositiveInfinity(mult) || float.IsNegativeInfinity(mult) || mult.Equals(0f) || float.IsNaN(mult))
            _conditionResumeButton.SetEnabled(false);
        else
            _conditionResumeButton.SetEnabled(true);
    }

    private void OnDestroy()
    {
        _resumeButton.clicked -= TogglePauseMenu;
        _quitButton.clicked -= Application.Quit;
        _toggleImButton.clicked -= _boardManager.ToggleMovementSpeed;
        _gameOverQuit.clicked -= Application.Quit;
        _conditionResumeButton.clicked -= ToggleConditionPanel;
        _messageBoxResumeButton.clicked -= ToggleMessageBoxPanel;
    }

    private void SetOkayToPause()
    {
        _okayToPause = true;
    }

    private void RestartGame()
    {
        if (!_gameManager.IsGameOver) return;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void DisplayMessageBox(string message)
    {
        _messageLabel.text = message;
        ToggleMessageBoxPanel();
        StartCoroutine(FocusElement(_messageBoxResumeButton));
    }
    public static IEnumerator FocusElement(VisualElement element)
    {
        yield return null;
        element.Focus();
    }


    private void ToggleMessageBoxPanel()
    {
        _messageBoxPanel.style.visibility = _messageBoxPanel.style.visibility.Equals(Visibility.Visible)
            ? Visibility.Hidden
            : Visibility.Visible;   
        _gameManager.TogglePause();
    }

    public void TogglePauseMenu()
    {
        if (!_okayToPause) return;

        _pausePanel.style.visibility = _pausePanel.style.visibility.Equals(Visibility.Visible)
            ? Visibility.Hidden
            : Visibility.Visible;
        StartCoroutine(FocusElement(_resumeButton));
        _gameManager.TogglePause();
    }

    public void ToggleGameOverPanel()
    {
        _okayToPause = false;
        var previousHighScore = PlayerPrefs.GetFloat("HighScore", 0);
        _gameOverPanel.style.visibility = Visibility.Visible;
        _gameOverMessage.text =
            $"Game Over\n\nYou scored: {_gameManager.Score:n2}\n\nPrevious high Score: {previousHighScore}";
        if (_gameManager.Score > previousHighScore) PlayerPrefs.SetFloat("HighScore", _gameManager.Score);
        StartCoroutine(FocusElement(_restartButton));
    }

    public void ToggleConditionPanel()
    {
        _okayToPause = false;

        var currentlyVisible = _conditionPanel.style.visibility.Equals(Visibility.Visible);
        _conditionPanel.style.visibility = currentlyVisible ? Visibility.Hidden : Visibility.Visible;
        _gameManager.TogglePause();
        //If condition panel being toggled and going invisible (resume button), start new level, only case this should be true
        StartCoroutine(FocusElement(_conditionResumeButton));
        if (!currentlyVisible) return;

        _gameManager.NewLevel();
    }

    public void UpdateFoodLabel(int newValue)
    {
        _foodLabel.text = $"Food: {newValue}";
    }

    private VisualElement GetPanel(string panelName)
    {
        var panel = uiDocument.rootVisualElement.Q<VisualElement>(panelName);
        panel.style.visibility = Visibility.Hidden;
        return panel;
    }
}