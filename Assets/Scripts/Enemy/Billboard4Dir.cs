using UnityEngine;

public sealed class Billboard4Dir : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Directional Sprites (from the ENEMY's perspective)")]
    [SerializeField] private Sprite north;
    [SerializeField] private Sprite east;
    [SerializeField] private Sprite south;
    [SerializeField] private Sprite west;

    [Header("Behavior")]
    [SerializeField] private bool faceCameraYAxisOnly = true;

    private void Awake()
    {
        if (this.targetCamera == null)
        {
            this.targetCamera = Camera.main;
        }

        if (this.spriteRenderer == null)
        {
            Debug.LogError("Billboard4Dir missing SpriteRenderer reference.");
        }
    }

    private void LateUpdate()
    {
        if (this.targetCamera == null || this.spriteRenderer == null)
        {
            return;
        }

        Vector3 toCam = this.targetCamera.transform.position - this.transform.position;
        toCam.y = 0f;

        if (toCam.sqrMagnitude < 0.0001f)
        {
            return;
        }

        if (this.faceCameraYAxisOnly)
        {
            Quaternion rot = Quaternion.LookRotation(toCam.normalized, Vector3.up);
            this.transform.rotation = rot;
        }
        else
        {
            this.transform.LookAt(this.targetCamera.transform);
        }

        this.UpdateSprite(toCam);
    }

    private void UpdateSprite(Vector3 toCamFlat)
    {
        // Determine which direction the camera is relative to the enemy (flat XZ)
        Vector3 d = toCamFlat.normalized;

        // Compare dot products against cardinal directions
        float n = Vector3.Dot(d, Vector3.forward);
        float e = Vector3.Dot(d, Vector3.right);
        float s = Vector3.Dot(d, Vector3.back);
        float w = Vector3.Dot(d, Vector3.left);

        float best = n;
        Sprite chosen = this.north;

        if (e > best)
        {
            best = e;
            chosen = this.east;
        }

        if (s > best)
        {
            best = s;
            chosen = this.south;
        }

        if (w > best)
        {
            best = w;
            chosen = this.west;
        }

        if (chosen != null && this.spriteRenderer.sprite != chosen)
        {
            this.spriteRenderer.sprite = chosen;
        }
    }
}
