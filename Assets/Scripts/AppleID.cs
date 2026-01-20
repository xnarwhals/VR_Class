using UnityEngine;

public enum AppleColor { Red, Blue, Green, Yellow }

public class AppleID : MonoBehaviour
{
    [SerializeField] private AppleColor color;
    public AppleColor Color => color;

    private bool consumed;

    public bool TryConsume()
    {
        if (consumed)
            return false;

        consumed = true;
        return true;
    }
}
