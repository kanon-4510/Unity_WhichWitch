using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static GameScene;

// ゲームオーバー画面のコントローラー
public class GameOverController : MonoBehaviour
{
    [Header("UI表示用テキスト")]
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private TMP_Text selectText;

    //定数
    private const int MAX_INDEX = 2; // 選択肢の最大インデックス
    //変数
    private int selectedIndex = 0; // 選択肢のインデックス（0: リトライ, 1: タイトル）

    private void Start()
    {
        // ゲームオーバー画面が開いたときに文章を設定する
        if (messageText != null)
        {
            messageText.text = "<color=#FF0000>世界を守れなかった";
        }

        UpdateMenuDisplay();
    }

    // ゲームオーバー画面での入力処理
    private void Update()
    {
        if (Keyboard.current == null) return;

        // 上キー又は、マウスホイール
        if (Keyboard.current.upArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            selectedIndex = (selectedIndex - 1 + MAX_INDEX) % MAX_INDEX;
            UpdateMenuDisplay();
        }
        // 下キー又はマウスホイール
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

    private void UpdateMenuDisplay()
    {
        // 例として、選択中のテキストの色を変える（未選択は白、選択中は黄色）
        if (selectText != null)
        {
            string menuStr = "";
            menuStr += (selectedIndex == 0) ? "<color=#FFE600>別の道へ</color>\n" : "別の道へ\n";
            menuStr += (selectedIndex == 1) ? "<color=#FFE600> 受け入れる</color>\n" : " 受け入れる\n";

            selectText.text = menuStr;
        }

    }

    //リトライかタイトルか
    private void DecideSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                // リトライ（ゲームシーンを再読み込み）
                GameSceneManager.LoadScene(GameSceneManager.SCENE_GAME);
                break;
            case 1:
                // タイトルに戻る
                GameSceneManager.LoadScene(GameSceneManager.SCENE_TITLE);
                break;
        }
    }
}