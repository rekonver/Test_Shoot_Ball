using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
public class LevelManager : MonoBehaviour
{
    public GameplayConfig config;
    bool restartScheduled = false;

    void Awake()
    {
        if (Instances.Instance != null) Instances.Instance.Register<LevelManager>(this);
    }

    public void Fail()
    {
        if (!restartScheduled)
            StartCoroutine(RestartAfterDelay(1f));
    }

    public void Success()
    {
        if (!restartScheduled)
            StartCoroutine(RestartAfterDelay(1f));
    }

    IEnumerator RestartAfterDelay(float delay)
    {
        restartScheduled = true;
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    void OnDestroy()
    {
        if (Instances.Instance != null) Instances.Instance.Unregister<LevelManager>();
    }
}
