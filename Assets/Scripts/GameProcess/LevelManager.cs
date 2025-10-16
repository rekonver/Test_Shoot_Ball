using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameplayConfig config;
    public PlayerController player;

    void Awake()
    {
        if (Instances.Instance != null) Instances.Instance.Register<LevelManager>(this);
    }

    public void Fail(string reason)
    {
        Debug.Log("Level failed: " + reason);
        // TODO: UI, restart
    }

    public void Success()
    {
        Debug.Log("Level success");
        // TODO: UI, next level
    }

    void OnDestroy()
    {
        if (Instances.Instance != null) Instances.Instance.Unregister<LevelManager>();
    }
}
