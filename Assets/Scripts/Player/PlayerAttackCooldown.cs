using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public sealed class PlayerAttackCooldown : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridMap gridMap;
    [SerializeField] private PlayerGridStepper stepper;
    [SerializeField] private GridOccupancyIndex occupancyIndex;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float cooldownSeconds = 0.55f;

    [Header("Range (Tiles)")]
    [Tooltip("Minimum tiles away the target must be. For 1-tile-gap melee, set to 2.")]
    [SerializeField] private int minRangeTiles = 2;

    [Tooltip("Maximum tiles away the target can be. For melee, set to 2. For ranged later, increase.")]
    [SerializeField] private int maxRangeTiles = 2;

    [Tooltip("If true, all tiles between attacker and target must be clear of blockers.")]
    [SerializeField] private bool requireClearLine = true;

    [Tooltip("If true, intermediate tiles must not be occupied (recommended for melee).")]
    [SerializeField] private bool requireEmptyIntermediateTiles = true;

    [Header("Input")]
    [SerializeField] private InputActionReference attackAction;

    [Header("Feedback")]
    [SerializeField] private UnityEvent onAttack;
    [SerializeField] private UnityEvent onAttackInvalid;

    private float cooldownRemaining;

    private void Awake()
    {
        if (this.gridMap == null)
        {
            this.gridMap = FindFirstObjectByType<GridMap>();
        }

        if (this.stepper == null)
        {
            this.stepper = GetComponent<PlayerGridStepper>();
        }

        if (this.occupancyIndex == null)
        {
            this.occupancyIndex = FindFirstObjectByType<GridOccupancyIndex>();
        }
    }

    private void OnEnable()
    {
        if (this.attackAction != null && this.attackAction.action != null)
        {
            this.attackAction.action.Enable();
            this.attackAction.action.performed += this.OnAttackPerformed;
        }
    }

    private void OnDisable()
    {
        if (this.attackAction != null && this.attackAction.action != null)
        {
            this.attackAction.action.performed -= this.OnAttackPerformed;
            this.attackAction.action.Disable();
        }
    }

    private void Update()
    {
        if (this.cooldownRemaining > 0f)
        {
            this.cooldownRemaining -= Time.deltaTime;
            if (this.cooldownRemaining < 0f)
            {
                this.cooldownRemaining = 0f;
            }
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        this.TryAttack();
    }

    public void TryAttack()
    {
        if (this.stepper != null && this.stepper.IsBusy)
        {
            this.FireInvalid();
            return;
        }

        if (this.cooldownRemaining > 0f)
        {
            this.FireInvalid();
            return;
        }

        if (this.gridMap == null || this.stepper == null || this.occupancyIndex == null)
        {
            this.FireInvalid();
            return;
        }

        if (this.minRangeTiles < 1 || this.maxRangeTiles < this.minRangeTiles)
        {
            Debug.LogError("Invalid min/max range tiles on PlayerAttackCooldown.");
            this.FireInvalid();
            return;
        }

        Vector2Int originCell = this.gridMap.WorldToCell(this.gridMap.SnapToCellCenter(this.transform.position));
        Vector3 fwd = GridDirectionUtil.ToWorldVector(this.stepper.Facing);

        Vector2Int step;
        if (fwd == Vector3.forward)
        {
            step = new Vector2Int(0, 1);
        }
        else if (fwd == Vector3.right)
        {
            step = new Vector2Int(1, 0);
        }
        else if (fwd == Vector3.back)
        {
            step = new Vector2Int(0, -1);
        }
        else
        {
            step = new Vector2Int(-1, 0);
        }

        // Find the closest valid target within [minRangeTiles..maxRangeTiles] in facing direction.
        GridOccupant target = null;
        Vector2Int targetCell = originCell;

        for (int r = 1; r <= this.maxRangeTiles; r++)
        {
            Vector2Int c = originCell + step * r;

            // If line must be clear, reject if something blocks between previous cell and this cell.
            if (this.requireClearLine)
            {
                Vector2Int prev = originCell + step * (r - 1);
                Vector3 prevWorld = this.gridMap.CellToWorldCenter(prev);
                if (!this.gridMap.CanEnterCell(prevWorld, c))
                {
                    // Line is blocked here; nothing beyond is reachable in a straight line.
                    break;
                }
            }
            else
            {
                // Still require floor so we don't target into void.
                if (!this.gridMap.HasFloor(c))
                {
                    break;
                }
            }

            // For melee, the intermediate gap tile(s) should be empty.
            if (this.requireEmptyIntermediateTiles && r < this.minRangeTiles)
            {
                GridOccupant occBetween = this.occupancyIndex.GetAtCell(c);
                if (occBetween != null)
                {
                    // Something is in the way; no point continuing further in this line.
                    break;
                }
            }

            if (r < this.minRangeTiles)
            {
                continue;
            }

            GridOccupant occ = this.occupancyIndex.GetAtCell(c);
            if (occ != null)
            {
                target = occ;
                targetCell = c;
                break; // closest target
            }
        }

        if (target == null)
        {
            this.FireInvalid();
            return;
        }

        EnemyHealth hp = target.GetComponent<EnemyHealth>();
        if (hp == null)
        {
            this.FireInvalid();
            return;
        }

        hp.TakeDamage(this.damage);
        Debug.Log("Player attacks for " + this.damage + " damage.");
        this.cooldownRemaining = this.cooldownSeconds;

        if (this.onAttack != null)
        {
            this.onAttack.Invoke();
        }
    }

    private void FireInvalid()
    {
        if (this.onAttackInvalid != null)
        {
            this.onAttackInvalid.Invoke();
        }
    }
}
