using UnityEngine;

public class DoorAnim : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private string openParameter = "isOpen";
    [SerializeField] private AudioSource doorSound;
    

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    public void OpenDoor()
    {
        SetOpen(true);
        doorSound?.Play();
    }

    public void CloseDoor()
    {
        SetOpen(false);
        doorSound?.Play();
    }

    public void SetOpen(bool isOpen)
    {
        if (animator == null)
        {
            Debug.LogWarning($"DoorAnim on {gameObject.name} has no Animator assigned.", this);
            return;
        }

        animator.SetBool(openParameter, isOpen);
    }
}
