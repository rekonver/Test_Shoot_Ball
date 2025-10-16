using UnityEngine;


public class Door : MonoBehaviour
{
    public Animator animator;
    bool opened = false;


    public void Open()
    {
        Debug.Log("Open the door");
        if (opened) return;
        opened = true;
        if (animator) animator.SetTrigger("Open");
        // else simple disable collider or animate transform
        var col = GetComponent<Collider>();
        if (col) col.enabled = false;
    }
}