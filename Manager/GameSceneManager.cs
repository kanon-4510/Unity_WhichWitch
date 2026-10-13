using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSceneManager : MonoBehaviour
{
    //シーン名の定数   
    public const string SCENE_TITLE = "TitleScene";
    public const string SCENE_GAME = "GameScene";
    public const string SCENE_GAMEOVER = "OverScene";
    public const string SCENE_CLEAR = "ClearScene";

    [Header("フェード設定")]
    [SerializeField] private CanvasGroup fadeCanvasGroup; // フェード用パネルのCanvasGroupを割り当てる
    [SerializeField] private float fadeDuration = 1.0f;   // フェードにかかる時間（秒）

    //シングルトン
    private static GameSceneManager instance;

    private void Awake()
    {
        // シーンを跨いで破棄されないようにする（シングルトン構成）
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // シーンロード時のイベントに登録
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        // イベントの解除
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    // シーンが切り替わった時に自動で呼ばれる
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 新しいシーンにフェード用パネル（CanvasGroup）があるか自動で探し直す
        FindFadeCanvasGroup();
    }

    // フェード用パネルをシーン内から探す処理
    private void FindFadeCanvasGroup()
    {
        if (fadeCanvasGroup == null)
        {
            // シーン内から "FadePanel" などの名前、または CanvasGroup を持つオブジェクトを探す
            // ※もしシーン内のフェードパネルに特定の名前がある場合は、ここに探す処理を書けます
            CanvasGroup foundGroup = FindAnyObjectByType<CanvasGroup>();
            if (foundGroup != null)
            {
                fadeCanvasGroup = foundGroup;
            }
        }
    }

    //シーンをロードするメソッド
    public static void LoadScene(string sceneName)
    {
        if (instance != null)
        {
            instance.StartCoroutine(instance.FadeAndLoadScene(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
    //シーンをリロードするメソッド
    public static void ReloadCurrentScene()
    {
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        UnityEngine.SceneManagement.SceneManager.LoadScene(currentSceneName);
    }

    // フェードを挟んでシーンを切り替えるコルーチン
    private IEnumerator FadeAndLoadScene(string sceneName)
    {
        //フェード中に
        //キーボードやコントローラーの操作を無効する
        UnityEngine.EventSystems.EventSystem.current.enabled = false;

        //フェードアウト（画面を黒くする）
        yield return StartCoroutine(Fade(fadeDuration));

        //シーンを非同期で読み込む
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        //フェードイン（画面を明るくする）
        yield return StartCoroutine(Fade(0.0f));

        //キーボードやコントローラーの操作を有効にする
        UnityEngine.EventSystems.EventSystem.current.enabled = true;
    }

    //Alpha値を滑らかに変化させるコルーチン
    private IEnumerator Fade(float targetAlpha)
    {
        if (fadeCanvasGroup == null) yield break;

        //パネルを操作できるように有効化
        fadeCanvasGroup.gameObject.SetActive(true);

        float startAlpha = fadeCanvasGroup.alpha;
        float time = 0f;

        //タイムフェードの時間より大きくなるまでループ
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }
}
