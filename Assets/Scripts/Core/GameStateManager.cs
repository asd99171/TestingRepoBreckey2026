using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameState initialState = GameState.Start;

    public GameState CurrentState { get; private set; }

    /// <summary>
    /// bool: allow cursor lock when entering Playing.
    /// </summary>
    public event Action<GameState, bool> StateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        CurrentState = initialState;
    }

    private void Start()
    {
        RaiseStateChanged(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
        {
            return;
        }

        switch (CurrentState)
        {
            case GameState.Playing:
                ChangeState(GameState.Paused, false);
                break;
            case GameState.Paused:
                ChangeState(GameState.Playing, true);
                break;
        }
    }

    public void StartNewGame()
    {
        ChangeState(GameState.Playing, true);
    }

    public void ResumeGame()
    {
        ChangeState(GameState.Playing, true);
    }

    public void RetryGame()
    {
        Debug.Log("TODO: Retry/Respawn flow will be implemented later.");
        ChangeState(GameState.Playing, true);
    }

    public void GoToMainMenu()
    {
        ChangeState(GameState.Start, false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit requested.");
        Application.Quit();
    }

    public void DebugSetDead()
    {
        ChangeState(GameState.Dead, false);
    }

    public void DebugSetEnd()
    {
        ChangeState(GameState.End, false);
    }

    public void ChangeState(GameState newState, bool allowCursorLock)
    {
        if (CurrentState == newState)
        {
            RaiseStateChanged(allowCursorLock);
            return;
        }

        CurrentState = newState;
        RaiseStateChanged(allowCursorLock);
    }

    private void RaiseStateChanged(bool allowCursorLock)
    {
        StateChanged?.Invoke(CurrentState, allowCursorLock);
    }
}
