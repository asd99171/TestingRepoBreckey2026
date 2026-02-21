using System.Reflection;
using UnityEngine;

/// <summary>
/// Binds an existing PlayerHealth-style component to HUDController.SetHealth without requiring code changes
/// to the gameplay script.
/// </summary>
public class HUDPlayerHealthBinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private HUDController hudController;
    [SerializeField] private MonoBehaviour playerHealthSource;

    [Header("Member Names on PlayerHealth")]
    [SerializeField] private string currentHealthMember = "currentHealth";
    [SerializeField] private string maxHealthMember = "maxHealth";

    [Header("Update")]
    [SerializeField] private bool updateEveryFrame = true;

    private MemberInfo currentHealthInfo;
    private MemberInfo maxHealthInfo;

    private void Awake()
    {
        if (hudController == null)
        {
            hudController = GetComponent<HUDController>();
        }

        CacheMembers();
    }

    private void Start()
    {
        ApplyHealthToHud();
    }

    private void Update()
    {
        if (!updateEveryFrame)
        {
            return;
        }

        ApplyHealthToHud();
    }

    public void SetPlayerHealthSource(MonoBehaviour source)
    {
        playerHealthSource = source;
        CacheMembers();
        ApplyHealthToHud();
    }

    public void ApplyHealthToHud()
    {
        if (hudController == null || playerHealthSource == null)
        {
            return;
        }

        if (!TryReadFloat(currentHealthInfo, out var current) || !TryReadFloat(maxHealthInfo, out var max))
        {
            return;
        }

        hudController.SetHealth(current, max);
    }

    private void CacheMembers()
    {
        currentHealthInfo = null;
        maxHealthInfo = null;

        if (playerHealthSource == null)
        {
            return;
        }

        var type = playerHealthSource.GetType();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        currentHealthInfo = (MemberInfo)type.GetField(currentHealthMember, flags) ?? type.GetProperty(currentHealthMember, flags);
        maxHealthInfo = (MemberInfo)type.GetField(maxHealthMember, flags) ?? type.GetProperty(maxHealthMember, flags);

        if (currentHealthInfo == null || maxHealthInfo == null)
        {
            Debug.LogWarning($"HUDPlayerHealthBinder could not find '{currentHealthMember}' or '{maxHealthMember}' on {type.Name}.");
        }
    }

    private bool TryReadFloat(MemberInfo memberInfo, out float value)
    {
        value = 0f;
        if (memberInfo == null || playerHealthSource == null)
        {
            return false;
        }

        switch (memberInfo)
        {
            case FieldInfo field:
                return TryConvert(field.GetValue(playerHealthSource), out value);
            case PropertyInfo property:
                return property.CanRead && TryConvert(property.GetValue(playerHealthSource), out value);
            default:
                return false;
        }
    }

    private static bool TryConvert(object source, out float value)
    {
        value = 0f;
        if (source == null)
        {
            return false;
        }

        switch (source)
        {
            case float f:
                value = f;
                return true;
            case int i:
                value = i;
                return true;
            default:
                return float.TryParse(source.ToString(), out value);
        }
    }
}
