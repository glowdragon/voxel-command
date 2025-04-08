using SingularityGroup.HotReload;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class HotReloadHandler
{
    [InvokeOnHotReload]
    public static void OnHotReload()
    {
        if (Application.isPlaying)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
