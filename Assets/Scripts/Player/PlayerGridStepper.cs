using System;
using UnityEngine;
using UnityEngine.Events;

public sealed class PlayerGridStepper : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridMap gridMap;
    [SerializeField] private Transform cameraPivot;

    [Header("State")]
    [SerializeField] private GridDirection facing = GridDirection.North;

    [Header("Movement")]
    [SerializeField] private float moveDuration = 0.12f;
    [SerializeField] private float turnDuration = 0.09f;

    [Header("Invalid Action Feedback Hook")]
    [SerializeField] private UnityEvent onInvalidAction;

    private bool isBusy;
    private Vector3 moveFrom;
    private Vector3 moveTo;
    private float moveT;

    private float turnFromYaw;
    private float turnToYaw;
    private float turnT;

    public GridDirection Facing
    {
        get { return this.facing; }
    }

    public bool IsBusy
    {
        get { return this.isBusy; }
    }

    private void Awake()
    {
        if (this.gridMap == null)
        {
            Debug.LogError("PlayerGridStepper missing GridMap reference.");
        }

        if (this.cameraPivot == null)
        {
            Debug.LogError("PlayerGridStepper missing cameraPivot reference.");
        }
    }

    private void Start()
    {
        Vector3 snapped = this.gridMap.SnapToCellCenter(this.transform.position);
        this.transform.position = snapped;

        this.ApplyFacingInstant(this.facing);
    }

    private void Update()
    {
        if (this.moveT > 0f)
        {
            this.UpdateMove();
        }
        else if (this.turnT > 0f)
        {
            this.UpdateTurn();
        }
    }

    public void RequestMoveForward()
    {
        this.TryMove(+1);
    }

    public void RequestMoveBackward()
    {
        this.TryMove(-1);
    }

    public void RequestTurnLeft()
    {
        this.TryTurn(true);
    }

    public void RequestTurnRight()
    {
        this.TryTurn(false);
    }

    private void TryMove(int stepSign)
    {
        if (this.isBusy)
        {
            return;
        }

        Vector3 dir = GridDirectionUtil.ToWorldVector(this.facing);
        Vector3 desired = this.transform.position + (dir * (float)stepSign * this.gridMap.CellSize);

        Vector3 snapped = this.gridMap.SnapToCellCenter(desired);
        Vector2Int targetCell = this.gridMap.WorldToCell(snapped);

        if (!this.gridMap.CanEnterCell(this.transform.position, targetCell))
        {
            this.FireInvalidAction();
            return;
        }

        this.isBusy = true;
        this.moveFrom = this.transform.position;
        this.moveTo = snapped;
        this.moveT = 0.0001f;
        this.turnT = 0f;
    }

    private void TryTurn(bool left)
    {
        if (this.isBusy)
        {
            return;
        }

        GridDirection next = left ? GridDirectionUtil.TurnLeft(this.facing) : GridDirectionUtil.TurnRight(this.facing);

        this.isBusy = true;

        this.turnFromYaw = this.cameraPivot.eulerAngles.y;
        this.turnToYaw = GridDirectionUtil.ToYaw(next);

        this.turnT = 0.0001f;
        this.moveT = 0f;

        this.facing = next;
    }

    private void UpdateMove()
    {
        float dt = Time.deltaTime;
        float dur = Mathf.Max(0.0001f, this.moveDuration);

        this.moveT += dt / dur;
        float t = Mathf.Clamp01(this.moveT);

        float eased = 1f - Mathf.Pow(1f - t, 3f);

        Vector3 pos = Vector3.Lerp(this.moveFrom, this.moveTo, eased);
        this.transform.position = pos;

        if (t >= 1f)
        {
            this.transform.position = this.moveTo;
            this.moveT = 0f;
            this.isBusy = false;
            GridOccupant occ = GetComponent<GridOccupant>();
            if (occ != null)
            {
                occ.RefreshCellFromTransform();
            }
        }
    }

    private void UpdateTurn()
    {
        float dt = Time.deltaTime;
        float dur = Mathf.Max(0.0001f, this.turnDuration);

        this.turnT += dt / dur;
        float t = Mathf.Clamp01(this.turnT);

        float eased = 1f - Mathf.Pow(1f - t, 3f);

        float yaw = Mathf.LerpAngle(this.turnFromYaw, this.turnToYaw, eased);
        Vector3 e = this.cameraPivot.eulerAngles;
        e.y = yaw;
        this.cameraPivot.eulerAngles = e;

        if (t >= 1f)
        {
            this.ApplyFacingInstant(this.facing);
            this.turnT = 0f;
            this.isBusy = false;
        }
    }

    private void ApplyFacingInstant(GridDirection dir)
    {
        Vector3 e = this.cameraPivot.eulerAngles;
        e.y = GridDirectionUtil.ToYaw(dir);
        this.cameraPivot.eulerAngles = e;
    }

    private void FireInvalidAction()
    {
        if (this.onInvalidAction != null)
        {
            this.onInvalidAction.Invoke();
        }
    }
}
