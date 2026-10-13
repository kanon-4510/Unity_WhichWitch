using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class EventPhase : PhaseBase
{
    private enum ROUTE_SELECT
    {
        BREAKTHROUGH,
        SALVATION,
        TRUTH,
        MAX
    }

    [Header("参照設定")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private GameScene gameScene;

    [Header("イベントUI表示用")]
    [SerializeField] private TMP_Text questionText; // イベントの文章（例: 魔法を学ぶ理由は？）
    [SerializeField] private TMP_Text optionsText;  // 選択肢一覧テキスト

    [Header("制限時間設定")]
    [SerializeField] private float timeLimit = 25f; // 制限時間（初期値: 25秒）

    //変数
    private float currentTimer;
    private int selectedIndex = 0;

    //ルートを配列に入れる
    private readonly string[] routeNames = new string[]
    {
        "仇を討つための強大な力を得るため",
        "魔王から世界を守り平和をもたらすため",
        "師の背を追い偉大な魔法使いになるため"
    };

    //初期化処理
    public void Initialize(PlayerStatus playerStatus, GameScene gameScene)
    {
        PhaseResult = PHASE_RESULT.CANCEL;
        isFinished = false;

        this.playerStatus = playerStatus;

        playerStatus.InitJob(); // 職業の初期化
    }
    public override void OnEnterPhase()
    {
        base.OnEnterPhase();

        //フェーズ開始時に終了フラグを確実に降ろす
        isFinished = false;

        selectedIndex = 0;

        currentTimer = timeLimit;

        if (questionText != null)
        {
            questionText.text = "「何のために、魔法を学ぶの?」";
        }

        UpdateOptionsDisplay();
    }

    //更新処理
    public override void OnUpdatePhase()
    {
        // 時間切れになったら「無欲」ルート確定！
        if (currentTimer <= 0f)
        {
            DecideRouteByTimeout();
            return;
        }

        currentTimer -= Time.deltaTime;

        if (Keyboard.current == null) return;

        //上キー又は、マウスカーソル
        if (Keyboard.current.upArrowKey.wasPressedThisFrame || Keyboard.current.wKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            selectedIndex = (selectedIndex - 1 + routeNames.Length) % routeNames.Length;
            UpdateOptionsDisplay();
        }
        //下キー又は、マウスカーソル
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame
                || Mouse.current.scroll.ReadValue().y < 0f)
        {
            selectedIndex = (selectedIndex + 1) % routeNames.Length;
            UpdateOptionsDisplay();
        }

        // Enterで決定
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            DecideRouteByPlayer();
        }
    }

    // ルート決定処理
    private void DecideRouteByPlayer()
    {
        if (playerStatus != null)
        {
            ROUTE_SELECT routeSelect = (ROUTE_SELECT)selectedIndex;

            PLAYER_ROUTE chosenRoute = routeSelect switch
            {
                ROUTE_SELECT.BREAKTHROUGH => PLAYER_ROUTE.Breakthrough,
                ROUTE_SELECT.SALVATION => PLAYER_ROUTE.Salvation,
                ROUTE_SELECT.TRUTH => PLAYER_ROUTE.Truth,
                _ => PLAYER_ROUTE.None
            };

            playerStatus.CurrentRoute = chosenRoute;
        }

        PhaseResult = PHASE_RESULT.CANCEL;
        isFinished = true;
    }

    //時間切れで自動決定された場合（無欲）
    private void DecideRouteByTimeout()
    {
        if (playerStatus != null)
        {
            playerStatus.CurrentRoute = PLAYER_ROUTE.Selfless; //無欲を強制設定
        }

        PhaseResult = PHASE_RESULT.CANCEL;
        isFinished = true;
    }

    //選択肢の表示更新
    private void UpdateOptionsDisplay()
    {
        if (optionsText == null) return;

        string str = "";
        for (int i = 0; i < routeNames.Length; i++)
        {
            if (i == selectedIndex)
            {
                str += $"<color=#FFE600>{routeNames[i]}</color>\n";
            }
            else
            {
                str += $"<color=#FFFFFF>{routeNames[i]}</color>\n";
            }
        }
        optionsText.text = str;
    }
}