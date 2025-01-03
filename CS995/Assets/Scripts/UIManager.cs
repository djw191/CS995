﻿using System.Collections;
using System.Collections.Generic;
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
    private Button _mainMenuButton;
    private VisualElement _notificationPanel;
    private Label _notificationLabel;
    private Queue<Notification> _notificationQueue = new Queue<Notification>();
    private bool _DisplayingNotification = false;
    private VisualElement _tutorialPanel;
    private Button _tutorialResumeButton;
    private Label _foodValue;
    private Label _foodMultiplierValue;
    private Label _wallValue;
    [SerializeField] private Label _wallMultiplierValue;
    private Label _attackValue;
    private Label _bonusValue;
    private Label _bonusMultiplierValue;

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

        _mainMenuButton = _pausePanel.Q<Button>("MainMenuButton");
        _mainMenuButton.clicked += OnMainMenuClicked;

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
        //TODO possibly the most manual I could've made this
        _conditionPanel = GetPanel("ConditionPanel");

        _conditionResumeButton = _conditionPanel.Q<Button>("ResumeButton");
        _conditionResumeButton.clicked += ToggleConditionPanel;
        _conditionResumeButton.Focus();

        _conditionsElement = _conditionPanel.Q<VisualElement>("ConditionsElement");

        _foodSlider = _conditionsElement.Q<Slider>("FoodSlider");
        _foodValue = _conditionsElement.Q<Label>("FoodValue");
        _foodValue.text = _boardManager.TargetFood.ToString("P0");
        _foodSlider.highValue = 1.0f;
        _foodSlider.value = _boardManager.TargetFood;
        _foodMultiplierSlider = _conditionsElement.Q<Slider>("FoodMultiplierSlider");
        _foodMultiplierValue = _conditionsElement.Q<Label>("FoodMultiplierValue");
        _foodMultiplierValue.text = _gameManager.FoodMultiplier.ToString("0.0") + " x";
        _foodMultiplierSlider.highValue = 4.0f;
        _foodMultiplierSlider.value = _gameManager.FoodMultiplier;

        _wallSlider = _conditionsElement.Q<Slider>("WallSlider");
        _wallValue = _conditionsElement.Q<Label>("WallValue");
        _wallValue.text = _boardManager.TargetWalls.ToString("P0");
        _wallSlider.highValue = 1.0f;
        _wallSlider.value = _boardManager.TargetWalls;
        _wallMultiplierSlider = _conditionsElement.Q<Slider>("WallMultiplierSlider");
        _wallMultiplierValue = _conditionsElement.Q<Label>("WallMultiplierValue");
        _wallMultiplierValue.text = _gameManager.WallMultiplier.ToString("0.0") + " x";
        _wallMultiplierSlider.highValue = 4.0f;
        _wallMultiplierSlider.value = _gameManager.WallMultiplier;

        _attackSlider = _conditionsElement.Q<SliderInt>("AttackSlider");
        _attackValue = _conditionsElement.Q<Label>("AttackValue");
        _attackValue.text = _gameManager.Player.AttackPower.ToString();
        _attackSlider.highValue = _gameManager.CurrentLevel + 9;
        _attackSlider.value = _gameManager.Player.AttackPower;

        _bonusSlider = _conditionsElement.Q<SliderInt>("BonusSlider");
        _bonusValue = _conditionsElement.Q<Label>("BonusValue");
        _bonusValue.text = _boardManager.TargetPowerups.ToString();
        _bonusSlider.highValue = (_boardManager.boardSize.x - 2) * (_boardManager.boardSize.y - 2) / 2;
        _bonusSlider.value = _boardManager.TargetPowerups;
        _bonusMultiplierSlider = _conditionsElement.Q<Slider>("BonusMultiplierSlider");
        _bonusMultiplierValue = _conditionsElement.Q<Label>("BonusMultiplierValue");
        _bonusMultiplierValue.text = _gameManager.BonusMultiplier.ToString("0.0") + " x";
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
        
        #region NotificationPanel
        _notificationPanel = GetPanel("NotificationPanel");
        _notificationLabel = _notificationPanel.Q<Label>("NotificationLabel");
        #endregion
        
        #region TutorialPanel
        _tutorialPanel = GetPanel("TutorialPanel");
        _tutorialResumeButton = _tutorialPanel.Q<Button>("ResumeButton");
        _tutorialResumeButton.clicked += ToggleTutorial;
        #endregion

        _gameManager.OnLevelComplete += SetOkayToPause;
    }

    private void Update()
    {
        if (_notificationQueue.Count > 0 && !_DisplayingNotification)
        {
            _DisplayingNotification = true;
            _notificationPanel.style.visibility = Visibility.Visible;
            Notification n = _notificationQueue.Dequeue();
            _notificationLabel.text = n.text;
            _notificationLabel.style.color = n.color;
            //TODO Play sound?
            StartCoroutine(FadeNotification(n.duration));
        }
    }

    private IEnumerator FadeNotification(float duration = 1.0f)
    {
        float elapsedTime = 0.0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float currentValue = Mathf.Lerp(1, 0, t);
            var cc = _notificationLabel.style.color.value;
            cc.a = currentValue;
            _notificationLabel.style.color = cc;
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        _notificationPanel.style.visibility = Visibility.Hidden;
        _DisplayingNotification = false;

    }

    private void OnMainMenuClicked()
    {
        TogglePauseMenu();
        SceneManager.LoadScene("MainMenu");
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
        _foodValue.text = _boardManager.TargetFood.ToString("P0");
        mult *= _boardManager.initTargetFood / _boardManager.TargetFood;
        _boardManager.TargetWalls = _wallSlider.value;
        _wallValue.text = _boardManager.TargetWalls.ToString("P0");
        mult *= _boardManager.TargetWalls / _boardManager.initTargetWalls;

        _playersInitAttackPower ??= _gameManager.Player.AttackPower;
        _gameManager.Player.AttackPower = _attackSlider.value;
        _attackValue.text = _gameManager.Player.AttackPower.ToString();
        mult *= (float)_playersInitAttackPower / _gameManager.Player.AttackPower;

        //These are always default 0, base mult on that
        _gameManager.WaypointTarget = _waypointSlider.value;
        mult *= _gameManager.WaypointTarget < 1 ? 1 : (float)_gameManager.WaypointTarget + 1;
        _boardManager.TargetPowerups = _bonusSlider.value;
        _bonusValue.text = _boardManager.TargetPowerups.ToString();
        mult *= _boardManager.TargetPowerups < 1 ? 1 : 1 / ((float)_boardManager.TargetPowerups + 1);

        _gameManager.FoodMultiplier = _foodMultiplierSlider.value;
        _foodMultiplierValue.text = _gameManager.FoodMultiplier.ToString("0.0") + " x";
        mult *= 1 / _gameManager.FoodMultiplier;
        _gameManager.WallMultiplier = _wallMultiplierSlider.value;
        _wallMultiplierValue.text = _gameManager.WallMultiplier.ToString("0.0") + " x";
        mult *= _gameManager.WallMultiplier;
        _gameManager.BonusMultiplier = _bonusMultiplierSlider.value;
        _bonusMultiplierValue.text = _gameManager.BonusMultiplier.ToString("0.0") + " x";
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
        _tutorialResumeButton.clicked -= ToggleTutorial;
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
    
    public void DisplayNotification(string message)
    {
        _notificationQueue.Enqueue(new Notification(message, Color.white, null, 1.0f));
    }
    public void DisplayNotification(string message, Color color, AudioClip audioClip = null, float duration = 1.0f)
    {
        _notificationQueue.Enqueue(new Notification(message, color, audioClip, duration));
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
        ToggleTutorial();
    }
    private void ToggleTutorial()
    {
        bool hasSeenTutorial = PlayerPrefs.GetInt("Tutorial", 0) == 1;
        if (hasSeenTutorial)
        {
            _gameManager.NewLevel();
            return;
        }
            
        
        var currentlyVisible = _tutorialPanel.style.visibility.Equals(Visibility.Visible);
        _tutorialPanel.style.visibility = currentlyVisible ? Visibility.Hidden : Visibility.Visible;
        _gameManager.TogglePause();
        //If condition panel being toggled and going invisible (resume button), start new level, only case this should be true
        StartCoroutine(FocusElement(_tutorialResumeButton));
        if (!currentlyVisible) return;
        
        PlayerPrefs.SetInt("Tutorial", 1);
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
    
    private struct Notification
    {
        public string text;
        public Color color;
        public AudioClip audioClip;
        public float duration;
        public Notification(string text, Color color, AudioClip audioClip, float duration)
        {
            this.text = text;
            this.color = color;
            this.audioClip = audioClip;
            this.duration = duration;
        }
    }
}