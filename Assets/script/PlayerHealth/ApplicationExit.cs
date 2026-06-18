using UnityEngine;

public class ApplicationExit : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("Game Quit");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}