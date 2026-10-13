using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static GameScene;

// ゲームクリア画面のコントローラー
public class GameClearController : MonoBehaviour
{
    [Header("UI表示用テキスト")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text selectText;

    //定数
    private const int MAX_INDEX = 2; // 選択肢の最大インデックス
    //変数
    private int selectedIndex = 0; // 選択肢のインデックス（0: タイトル, 1: ゲーム終了）
   
    private void Start()
    {
        // ゲームクリア画面が開いたときに文章を設定する
        if (messageText != null)
        {
            messageText.text = "世界一の魔法使いになった！";
        }

        UpdateMenuDisplay();
    }

    // ゲームオーバー画面での入力処理
    private void Update()
    {
        if (Keyboard.current == null) return;

        // 上キー
        if (Keyboard.current.upArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            selectedIndex = (selectedIndex - 1 + MAX_INDEX) % MAX_INDEX;
            UpdateMenuDisplay();
        }
        // 下キー
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y < 0f)
        {
            selectedIndex = (selectedIndex + 1) % MAX_INDEX;
            UpdateMenuDisplay();
        }

        // エンターキーで決定
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            DecideSelection();
        }
    }

    //UIの更新処理
    private void UpdateMenuDisplay()
    {
        // 例として、選択中のテキストの色を変える（未選択は白、選択中は黄色）
        if (selectText != null)
        {
            string menuStr = "";
            menuStr += (selectedIndex == 0) ? "<color=#FFE600>タイトルへ</color>\n" : "タイトルへ\n";
            menuStr += (selectedIndex == 1) ? "<color=#FFE600>ゲーム終了</color>\n" : "ゲーム終了\n";

            selectText.text = menuStr;
        }
    }

    //タイトルかゲーム終了か
    private void DecideSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                //タイトルに戻る
                GameSceneManager.LoadScene(GameSceneManager.SCENE_TITLE);
                break;
            case 1:
                //ゲーム終了
                ExitGame();
                break;
        }
    }

    //ゲームを終了
    public void ExitGame()
    {
#if UNITY_EDITOR
        // Unityエディタ上でのプレイ停止
        EditorApplication.isPlaying = false;
#else
        // ビルド後のアプリケーション終了
        Application.Quit();
#endif
    }
}