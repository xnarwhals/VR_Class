using UnityEngine;

public class Magazine : MonoBehaviour
{
    [SerializeField] private string magazineId = "Glock";
    [SerializeField] private bool isLoaded;
    [SerializeField] private int maxAmmo = 6;
    [SerializeField] private int currentAmmo = 6;

    public string MagazineId => magazineId;
    public bool IsLoaded => isLoaded;
    public int CurrentAmmo => currentAmmo;
    public int MaxAmmo => maxAmmo;

    public void SetLoaded(bool loaded)
    {
        isLoaded = loaded;
    }

    public bool HasAmmo()
    {
        return currentAmmo > 0;
    }

    public bool TryConsumeRound()
    {
        if (currentAmmo <= 0)
        {
            return false;
        }

        currentAmmo -= 1;
        return true;
    }

    public void SetAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(amount, 0, maxAmmo);
    }
}
