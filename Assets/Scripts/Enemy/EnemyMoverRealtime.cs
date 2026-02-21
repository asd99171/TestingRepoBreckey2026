using UnityEngine;
using UnityEngine.Events;
using Pathfinding;

public sealed class EnemyMoverRealtime : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridMap gridMap;
    [SerializeField] private GridOccupancyIndex occupancyIndex;
    [SerializeField] private GridOccupant selfOccupant;
    [SerializeField] private Seeker seeker;

    [Header("Target")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GridOccupant playerOccupant;

    [Header("Sight / Aggro")]
    [SerializeField] private int aggroRadiusTiles = 8;
    [SerializeField] private LayerMask sightBlockingMask;
    [SerializeField] private float sightEyeHeight = 0.9f;
    [SerializeField] private bool requireLineOfSight = true;
    [SerializeField] private float forgetAfterSeconds = 0.0f;

    [Header("Path")]
    [SerializeField] private float repathIntervalSeconds = 0.35f;

    [Header("Movement")]
    [SerializeField] private float thinkIntervalSeconds = 0.10f;
    [SerializeField] private float moveCooldownSeconds = 0.55f;
    [SerializeField] private float moveDuration = 0.12f;

    [Header("Melee Range")]
    [SerializeField] private int meleeRangeTiles = 2;
    [SerializeField] private bool requireClearLineForMelee = true;
    [SerializeField] private bool requireEmptyIntermediateForMelee = true;

    [Header("Attack")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float attackCooldownSeconds = 0.85f;

    [Header("Events")]
    [SerializeField] private UnityEvent onAggroGained;
    [SerializeField] private UnityEvent onAggroLost;
    [SerializeField] private UnityEvent onStep;
    [SerializeField] private UnityEvent onAttack;
    [SerializeField] private UnityEvent onFailedAction;

    private float thinkTimer;
    private float moveCooldownRemaining;
    private float attackCooldownRemaining;
    private float lastSeenTimer;

    private bool isAggro;

    private bool isMoving;
    private Vector3 moveFrom;
    private Vector3 moveTo;
    private float moveT;

    private float repathTimer;
    private Path currentPath;
    private int pathIndex;

    private void Awake()
    {
        if (this.gridMap == null)
        {
            this.gridMap = FindFirstObjectByType<GridMap>();
        }

        if (this.occupancyIndex == null)
        {
            this.occupancyIndex = FindFirstObjectByType<GridOccupancyIndex>();
        }

        if (this.selfOccupant == null)
        {
            this.selfOccupant = GetComponent<GridOccupant>();
        }

        if (this.seeker == null)
        {
            this.seeker = GetComponent<Seeker>();
        }

        if (this.playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                this.playerTransform = player.transform;
            }
        }

        if (this.playerTransform != null && this.playerOccupant == null)
        {
            this.playerOccupant = this.playerTransform.GetComponent<GridOccupant>();
        }
    }

    private void Start()
    {
        if (this.selfOccupant != null)
        {
            this.selfOccupant.RefreshCellFromTransform();
        }

        if (this.occupancyIndex != null && this.selfOccupant != null)
        {
            this.occupancyIndex.Register(this.selfOccupant);
        }
    }

    private void OnDestroy()
    {
        if (this.occupancyIndex != null && this.selfOccupant != null)
        {
            this.occupancyIndex.Unregister(this.selfOccupant);
        }
    }

    private void Update()
    {
        this.TickCooldowns();

        if (this.isMoving)
        {
            this.UpdateMoveAnimation();
            return;
        }

        this.thinkTimer -= Time.deltaTime;
        if (this.thinkTimer > 0f)
        {
            return;
        }

        this.thinkTimer = Mathf.Max(0.02f, this.thinkIntervalSeconds);
        this.ThinkAndAct();
    }

    private void TickCooldowns()
    {
        float dt = Time.deltaTime;

        if (this.moveCooldownRemaining > 0f)
        {
            this.moveCooldownRemaining -= dt;
            if (this.moveCooldownRemaining < 0f)
            {
                this.moveCooldownRemaining = 0f;
            }
        }

        if (this.attackCooldownRemaining > 0f)
        {
            this.attackCooldownRemaining -= dt;
            if (this.attackCooldownRemaining < 0f)
            {
                this.attackCooldownRemaining = 0f;
            }
        }

        if (this.lastSeenTimer > 0f)
        {
            this.lastSeenTimer -= dt;
            if (this.lastSeenTimer < 0f)
            {
                this.lastSeenTimer = 0f;
            }
        }

        if (this.repathTimer > 0f)
        {
            this.repathTimer -= dt;
            if (this.repathTimer < 0f)
            {
                this.repathTimer = 0f;
            }
        }
    }

    private void ThinkAndAct()
    {
        if (this.gridMap == null || this.occupancyIndex == null || this.selfOccupant == null || this.seeker == null)
        {
            return;
        }

        if (this.playerTransform == null)
        {
            return;
        }

        if (this.playerOccupant != null)
        {
            this.playerOccupant.RefreshCellFromTransform();
        }

        this.selfOccupant.RefreshCellFromTransform();

        Vector2Int myCell = this.selfOccupant.Cell;
        Vector2Int playerCell = this.GetPlayerCell();

        bool canSee = this.CanSeePlayer(myCell, playerCell);

        if (canSee)
        {
            this.SetAggro(true);
            this.lastSeenTimer = Mathf.Max(0f, this.forgetAfterSeconds);
        }
        else
        {
            if (this.forgetAfterSeconds <= 0f)
            {
                this.SetAggro(false);
            }
            else if (this.lastSeenTimer <= 0f)
            {
                this.SetAggro(false);
            }
        }

        if (!this.isAggro)
        {
            this.ClearPath();
            return;
        }

        // Hard rule: don't chase through walls.
        if (this.requireLineOfSight && !canSee)
        {
            this.ClearPath();
            return;
        }

        // Attack if in melee spacing.
        int dx = playerCell.x - myCell.x;
        int dz = playerCell.y - myCell.y;
        bool aligned = (dx == 0) || (dz == 0);
        int manhattan = Mathf.Abs(dx) + Mathf.Abs(dz);

        if (aligned && manhattan == this.meleeRangeTiles)
        {
            if (this.CanMeleeStrikeLine(myCell, playerCell))
            {
                this.TryAttackPlayer();
                return;
            }
        }

        // If too close (distance 1), back off (keep the gap).
        if (aligned && manhattan < this.meleeRangeTiles)
        {
            this.TryStepAwayFrom(playerCell, myCell);
            return;
        }

        // Ensure we have a path (repath occasionally).
        if (this.currentPath == null || this.repathTimer <= 0f)
        {
            this.repathTimer = Mathf.Max(0.05f, this.repathIntervalSeconds);
            this.RequestPathTo(playerCell);
        }

        // Step along the path.
        this.TryStepAlongPath(myCell);
    }

    private Vector2Int GetPlayerCell()
    {
        if (this.playerOccupant != null)
        {
            return this.playerOccupant.Cell;
        }

        Vector3 snapped = this.gridMap.SnapToCellCenter(this.playerTransform.position);
        return this.gridMap.WorldToCell(snapped);
    }

    private bool CanSeePlayer(Vector2Int fromCell, Vector2Int toCell)
    {
        int r = Mathf.Max(0, this.aggroRadiusTiles);

        int dx = toCell.x - fromCell.x;
        int dz = toCell.y - fromCell.y;

        int distSq = (dx * dx) + (dz * dz);
        int rSq = r * r;

        if (distSq > rSq)
        {
            return false;
        }

        if (!this.requireLineOfSight)
        {
            return true;
        }

        return this.HasPhysicsLineOfSight();
    }

    private bool HasPhysicsLineOfSight()
    {
        Vector3 from = this.transform.position + new Vector3(0f, this.sightEyeHeight, 0f);
        Vector3 to = this.playerTransform.position + new Vector3(0f, this.sightEyeHeight, 0f);

        Vector3 dir = to - from;
        float dist = dir.magnitude;

        if (dist <= 0.0001f)
        {
            return true;
        }

        dir /= dist;

        bool blocked = Physics.Raycast(from, dir, dist, this.sightBlockingMask, QueryTriggerInteraction.Ignore);
        return !blocked;
    }

    private void RequestPathTo(Vector2Int playerCell)
    {
        if (this.seeker.IsDone() == false)
        {
            return;
        }

        // Aim for a "melee position" at range 2 if possible: target cell is 2 tiles away in line.
        // For now we request a path directly to the player's cell; we will stop stepping when close enough.
        Vector3 start = this.transform.position;
        Vector3 end = this.gridMap.CellToWorldCenter(playerCell);

        this.seeker.StartPath(start, end, this.OnPathComplete); // Seeker.StartPath docs :contentReference[oaicite:4]{index=4}
    }

    private void OnPathComplete(Path p)
    {
        if (p == null)
        {
            return;
        }

        if (p.error)
        {
            this.ClearPath();
            return;
        }

        this.currentPath = p;
        this.pathIndex = 0;
    }

    private void TryStepAlongPath(Vector2Int myCell)
    {
        if (this.moveCooldownRemaining > 0f)
        {
            return;
        }

        if (this.currentPath == null || this.currentPath.vectorPath == null || this.currentPath.vectorPath.Count == 0)
        {
            if (this.onFailedAction != null)
            {
                this.onFailedAction.Invoke();
            }
            return;
        }

        // Advance pathIndex until we find a node that is not basically our current position.
        while (this.pathIndex < this.currentPath.vectorPath.Count)
        {
            Vector3 wp = this.currentPath.vectorPath[this.pathIndex];
            Vector2Int nodeCell = this.gridMap.WorldToCell(this.gridMap.SnapToCellCenter(wp));

            if (nodeCell == myCell)
            {
                this.pathIndex++;
                continue;
            }

            // We only move 1 tile per step, so nodeCell must be adjacent.
            int manhattan = Mathf.Abs(nodeCell.x - myCell.x) + Mathf.Abs(nodeCell.y - myCell.y);
            if (manhattan != 1)
            {
                // Path nodes sometimes skip due to smoothing; step toward it manually.
                Vector2Int step = new Vector2Int(
                    nodeCell.x > myCell.x ? 1 : (nodeCell.x < myCell.x ? -1 : 0),
                    nodeCell.y > myCell.y ? 1 : (nodeCell.y < myCell.y ? -1 : 0)
                );

                // Force 4-dir
                if (step.x != 0)
                {
                    step.y = 0;
                }

                nodeCell = myCell + step;
            }

            if (this.TryStep(myCell, nodeCell))
            {
                // After stepping once, stop until next cooldown.
                return;
            }

            // If blocked, abandon path; we will repath soon.
            this.ClearPath();

            if (this.onFailedAction != null)
            {
                this.onFailedAction.Invoke();
            }
            return;
        }

        this.ClearPath();
    }

    private void ClearPath()
    {
        this.currentPath = null;
        this.pathIndex = 0;
    }

    private void SetAggro(bool value)
    {
        if (this.isAggro == value)
        {
            return;
        }

        this.isAggro = value;

        if (this.isAggro)
        {
            if (this.onAggroGained != null)
            {
                this.onAggroGained.Invoke();
            }
        }
        else
        {
            if (this.onAggroLost != null)
            {
                this.onAggroLost.Invoke();
            }
        }
    }

    private void TryAttackPlayer()
    {
        if (this.attackCooldownRemaining > 0f)
        {
            return;
        }

        PlayerHealth hp = this.playerTransform.GetComponentInChildren<PlayerHealth>();
        if (hp == null)
        {
            return;
        }

        hp.TakeDamage(this.damage);
        this.attackCooldownRemaining = Mathf.Max(0.05f, this.attackCooldownSeconds);

        if (this.onAttack != null)
        {
            this.onAttack.Invoke();
        }
    }

    private bool CanMeleeStrikeLine(Vector2Int fromCell, Vector2Int toCell)
    {
        if (!this.requireClearLineForMelee && !this.requireEmptyIntermediateForMelee)
        {
            return true;
        }

        Vector2Int step = Vector2Int.zero;

        if (toCell.x == fromCell.x)
        {
            step = new Vector2Int(0, toCell.y > fromCell.y ? 1 : -1);
        }
        else if (toCell.y == fromCell.y)
        {
            step = new Vector2Int(toCell.x > fromCell.x ? 1 : -1, 0);
        }
        else
        {
            return false;
        }

        int dist = Mathf.Abs(toCell.x - fromCell.x) + Mathf.Abs(toCell.y - fromCell.y);

        for (int i = 1; i < dist; i++)
        {
            Vector2Int c = fromCell + step * i;

            if (this.requireClearLineForMelee)
            {
                Vector2Int prev = fromCell + step * (i - 1);
                Vector3 prevWorld = this.gridMap.CellToWorldCenter(prev);
                if (!this.gridMap.CanEnterCell(prevWorld, c))
                {
                    return false;
                }
            }

            if (this.requireEmptyIntermediateForMelee)
            {
                GridOccupant occ = this.occupancyIndex.GetAtCell(c);
                if (occ != null)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private void TryStepAwayFrom(Vector2Int playerCell, Vector2Int myCell)
    {
        if (this.moveCooldownRemaining > 0f)
        {
            return;
        }

        Vector2Int delta = new Vector2Int(myCell.x - playerCell.x, myCell.y - playerCell.y);

        Vector2Int stepA;
        Vector2Int stepB;

        if (Mathf.Abs(delta.x) >= Mathf.Abs(delta.y))
        {
            stepA = new Vector2Int(delta.x == 0 ? 0 : (delta.x > 0 ? 1 : -1), 0);
            stepB = new Vector2Int(0, delta.y == 0 ? 0 : (delta.y > 0 ? 1 : -1));
        }
        else
        {
            stepA = new Vector2Int(0, delta.y == 0 ? 0 : (delta.y > 0 ? 1 : -1));
            stepB = new Vector2Int(delta.x == 0 ? 0 : (delta.x > 0 ? 1 : -1), 0);
        }

        bool moved = false;

        if (stepA != Vector2Int.zero)
        {
            moved = this.TryStep(myCell, myCell + stepA);
        }

        if (!moved && stepB != Vector2Int.zero)
        {
            moved = this.TryStep(myCell, myCell + stepB);
        }

        if (!moved)
        {
            if (this.onFailedAction != null)
            {
                this.onFailedAction.Invoke();
            }
        }
    }

    private bool TryStep(Vector2Int fromCell, Vector2Int toCell)
    {
        GridOccupant occ = this.occupancyIndex.GetAtCell(toCell);
        if (occ != null && occ != this.selfOccupant)
        {
            return false;
        }

        Vector3 fromWorld = this.gridMap.CellToWorldCenter(fromCell);
        if (!this.gridMap.CanEnterCell(fromWorld, toCell))
        {
            return false;
        }

        this.isMoving = true;
        this.moveFrom = this.transform.position;
        this.moveTo = this.gridMap.CellToWorldCenter(toCell);
        this.moveT = 0.0001f;

        this.moveCooldownRemaining = Mathf.Max(0.05f, this.moveCooldownSeconds);

        this.occupancyIndex.Move(this.selfOccupant, fromCell, toCell);

        if (this.onStep != null)
        {
            this.onStep.Invoke();
        }

        return true;
    }

    private void UpdateMoveAnimation()
    {
        float dur = Mathf.Max(0.0001f, this.moveDuration);
        this.moveT += Time.deltaTime / dur;

        float t = Mathf.Clamp01(this.moveT);
        float eased = 1f - Mathf.Pow(1f - t, 3f);

        this.transform.position = Vector3.Lerp(this.moveFrom, this.moveTo, eased);

        if (t >= 1f)
        {
            this.transform.position = this.moveTo;
            this.isMoving = false;

            if (this.selfOccupant != null)
            {
                this.selfOccupant.RefreshCellFromTransform();
            }
        }
    }
}
