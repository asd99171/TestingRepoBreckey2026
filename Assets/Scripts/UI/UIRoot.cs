using UnityEngine;
using UnityEngine.UI;

public class UIRoot : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameStateManager gameStateManager;

    [Header("Panels")]
    [SerializeField] private GameObject panelStart;
    [SerializeField] private GameObject panelPause;
    [SerializeField] private GameObject panelDead;
    [SerializeField] private GameObject panelEnd;
    [SerializeField] private GameObject panelHud;

    [Header("Start Buttons")]
    [SerializeField] private Button btnNewGame;
    [SerializeField] private Button btnQuit;

    [Header("Pause Buttons")]
    [SerializeField] private Button btnResume;
    [SerializeField] private Button btnMainMenuPause;
    [SerializeField] private Button btnQuitPause;

    [Header("Dead Buttons")]
    [SerializeField] private Button btnRetry;
    [SerializeField] private Button btnMainMenuDead;

    [Header("End Buttons")]
    [SerializeField] private Button btnMainMenuEnd;
    [SerializeField] private Button btnNewGameEnd;

    private void Awake()
    {
        if (gameStateManager == null)
        {
            gameStateManager = GameStateManager.Instance;
        }

        BindButtons();
    }

    private void OnEnable()
    {
        if (gameStateManager != null)
        {
            gameStateManager.StateChanged += OnStateChanged;
            RefreshPanels(gameStateManager.CurrentState);
        }
    }

    private void OnDisable()
    {
        if (gameStateManager != null)
        {
            gameStateManager.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(GameState state, bool _)
    {
        RefreshPanels(state);
    }

    private void BindButtons()
    {
        Bind(btnNewGame, () => gameStateManager?.StartNewGame());
        Bind(btnQuit, () => gameStateManager?.QuitGame());

        Bind(btnResume, () => gameStateManager?.ResumeGame());
        Bind(btnMainMenuPause, () => gameStateManager?.GoToMainMenu());
        Bind(btnQuitPause, () => gameStateManager?.QuitGame());

        Bind(btnRetry, () => gameStateManager?.RetryGame());
        Bind(btnMainMenuDead, () => gameStateManager?.GoToMainMenu());

        Bind(btnMainMenuEnd, () => gameStateManager?.GoToMainMenu());
        Bind(btnNewGameEnd, () => gameStateManager?.StartNewGame());
    }

    private static void Bind(Button button, UnityEngine.Events.UnityAction action)
    {
        if (button == null)
        {
            return;
        }

        button.onClick.RemoveListener(action);
        button.onClick.AddListener(action);
    }

    private void RefreshPanels(GameState state)
    {
        SetPanel(panelStart, state == GameState.Start);
        SetPanel(panelPause, state == GameState.Paused);
        SetPanel(panelDead, state == GameState.Dead);
        SetPanel(panelEnd, state == GameState.End);

        var playing = state == GameState.Playing;
        SetPanel(panelHud, playing);
    }

    private static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null)
        {
            panel.SetActive(active);
        }
    }
}
