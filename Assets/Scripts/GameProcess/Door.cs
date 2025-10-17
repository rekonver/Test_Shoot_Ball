using UnityEngine;


public class Door : MonoBehaviour
{
    [SerializeField] private Animator animator;
    private bool opened = false;
    
    public void Open()
    {
        if (opened) return;
        opened = true;
        if (animator) animator.SetTrigger("Open");
    }
}