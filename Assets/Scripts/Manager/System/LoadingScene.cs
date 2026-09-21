using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScene : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image LoadingBarFill;

    public void LoadScene(int sceneId)
    {
        StartCoroutine(LoadSceneAsync(sceneId));
    }

    IEnumerator LoadSceneAsync(int sceneId)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneId);
        loadingScreen.SetActive(true);
        while (!operation.isDone)
        {
            float progressValue = Mathf.Clamp01(operation.progress / 0.09f);
            LoadingBarFill.fillAmount = progressValue;
            yield return null;
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  HOUSEFLOW INTEGRATION (Prefab Loading)
    // ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Loads a HOUSEFLOW puzzle level (prefab) without changing the Unity Scene.
    /// Uses the same loading UI to provide a smooth transition.
    /// </summary>
    public void LoadPuzzleLevel(HouseFlow.Level.LevelData levelData, HouseFlow.Level.LevelFlowController flowController)
    {
        StartCoroutine(LoadPuzzleLevelAsync(levelData, flowController));
    }

    IEnumerator LoadPuzzleLevelAsync(HouseFlow.Level.LevelData levelData, HouseFlow.Level.LevelFlowController flowController)
    {
        loadingScreen.SetActive(true);
        LoadingBarFill.fillAmount = 0f;

        // Animate the loading bar quickly to give the player a transition screen
        // (Since prefabs instantiate instantly, we fake a tiny load time)
        float t = 0;
        float fakeLoadDuration = 0.5f;

        while (t < fakeLoadDuration)
        {
            t += Time.deltaTime;
            LoadingBarFill.fillAmount = t / fakeLoadDuration;
            yield return null;
        }

        // Swap the level prefabs instantly behind the loading screen
        flowController.StartLevel(levelData);

        // Hide screen
        loadingScreen.SetActive(false);
    }
}