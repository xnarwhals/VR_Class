using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;


public class AnimateHandOnInput : MonoBehaviour
{
    public InputActionProperty pinchAction;
    public InputActionProperty gripAction;
    public Animator handAnim;

    public void Update()
    {
        float triggerValue = pinchAction.action.ReadValue<float>();
        handAnim.SetFloat("Trigger", triggerValue);

        float gripValue = gripAction.action.ReadValue<float>();
        handAnim.SetFloat("Grip", gripValue);
    }

}
