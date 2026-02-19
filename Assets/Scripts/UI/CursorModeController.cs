using UnityEngine;

public class CursorModeController : MonoBehaviour
{
    [SerializeField] private GameStateManager gameStateManager;

    private bool hasUserGrantedGameplayLock;

    private void Awake()
    {
        if (gameStateManager == null)
        {
            gameStateManager = GameStateManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (gameStateManager != null)
        {
            gameStateManager.StateChanged += OnStateChanged;
            ApplyForState(gameStateManager.CurrentState, false);
        }
    }

    private void OnDisable()
    {
        if (gameStateManager != null)
        {
            gameStateManager.StateChanged -= OnStateChanged;
        }
    }

    private void OnStateChanged(GameState state, bool allowCursorLock)
    {
        ApplyForState(state, allowCursorLock);
    }

    private void ApplyForState(GameState state, bool allowCursorLock)
    {
        if (allowCursorLock)
        {
            hasUserGrantedGameplayLock = true;
        }

        var shouldLock = state == GameState.Playing && hasUserGrantedGameplayLock;

        Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !shouldLock;
    }
}
