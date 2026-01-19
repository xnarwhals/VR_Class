using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public enum DuckColor { Red, Blue, Green, Yellow }

public class DuckID : MonoBehaviour
{
    public DuckColor color;     
    [SerializeField] private Animator animator;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
    }


    // Optional: cache XRGrabInteractable and colliders
    // [SerializeField] private Behaviour[] disableWhenLocked; 
    [SerializeField] private Collider[] colliders;

    public void SetLocked(bool locked)
    {
        // if (disableWhenLocked != null)
        // {
        //     foreach (var behaviour in disableWhenLocked)
        //     {
        //         if (behaviour != null)
        //             behaviour.enabled = !locked;
        //     }
        // }

        if (colliders != null)
        {
            foreach (var col in colliders)
            {
                if (col != null)
                    col.enabled = !locked;
            }
        }
    }

    public void PlayDance()
    {
        if (animator != null)
        {
            animator.SetTrigger("Dance");
        }
    }
}
