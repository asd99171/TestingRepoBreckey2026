using UnityEngine;

public sealed class EnemyRegisterOnStart : MonoBehaviour
{
    [SerializeField] private GridOccupancyIndex index;
    [SerializeField] private GridOccupant occupant;

    private void Awake()
    {
        if (this.index == null)
        {
            this.index = FindFirstObjectByType<GridOccupancyIndex>();
        }

        if (this.occupant == null)
        {
            this.occupant = GetComponent<GridOccupant>();
        }
    }

    private void Start()
    {
        if (this.index != null && this.occupant != null)
        {
            this.index.Register(this.occupant);
        }
    }

    private void OnDestroy()
    {
        if (this.index != null && this.occupant != null)
        {
            this.index.Unregister(this.occupant);
        }
    }
}
