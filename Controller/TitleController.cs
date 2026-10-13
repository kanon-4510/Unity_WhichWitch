using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;

// ゲームオーバー画面のコントローラー
public class TitleController : MonoBehaviour
{
    [Header("UI表示用テキスト")]
    [SerializeField] private TMP_Text selectText;

    //定数
    private const int MAX_INDEX = 2; // 選択肢の最大インデックス
    
    //変数
    private int selectedIndex = 0; // 選択肢のインデックス（0: リトライ, 1: タイトルへ戻る）

    //フラグ
    private bool isNextScene = false;   //遷移を行ったか

    private void Start()
    {
        UpdateMenuDisplay();
    }

    // ゲームオーバー画面での入力処理
    private void Update()
    {
        if (Keyboard.current == null) return;

        //操作を受けつけない
        if(isNextScene )return;

        // 上キー又はマウスホイール
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
            isNextScene = true;
            DecideSelection();
            SoundManager.Instance.PlayDecision();
        }
    }

    private void UpdateMenuDisplay()
    {
        //例として、選択中のテキストの色を変える（未選択は白、選択中は黄色）
        if (selectText != null)
        {
            string menuStr = "";
            menuStr += (selectedIndex == 0) ? "<color=#FFE600>ゲーム開始</color>\n" : "ゲーム開始\n";
            menuStr += (selectedIndex == 1) ? "<color=#FFE600>ゲーム終了</color>\n" : "ゲーム終了\n";

            selectText.text = menuStr;
        }
    }

    private void DecideSelection()
    {
        switch (selectedIndex)
        {
            case 0:
                //ゲームスタート
                GameSceneManager.LoadScene(GameSceneManager.SCENE_GAME);
                SoundManager.Instance.StopSound();
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