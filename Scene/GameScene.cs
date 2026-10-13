using System.Drawing;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameScene : MonoBehaviour
{
    public enum PHASE_TYPE
    {
        Quest,       // 0: クエスト（戦闘）
        ClassWork,   // 1: 授業
        JobChange,   // 2: 資格試験
        Final,       // 3: 最終日
        Event        // 4: イベント
    }


    [Header("参照設定")]
    [SerializeField] private PlayerStatus playerStatus;

    [Header("UI表示用テキスト")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text routeText;
    [SerializeField] private TMP_Text phaseStateText;

    [Header("★UIパネルの参照（表示切り替え用）")]
    [SerializeField] private GameObject baseMenuPanel;      // 拠点メニュー用UI
    [SerializeField] private GameObject questUIPanel;       // クエスト用UI
    [SerializeField] private GameObject classWorkUIPanel;   // 授業用UI
    [SerializeField] private GameObject jobChangeUIPanel;   // 資格試験用UI
    [SerializeField] private GameObject eventUIPanel;       // イベント（ルート選択）用UI

    [Header("各フェーズの参照")]
    [SerializeField] private QuestPhase questPhase;
    [SerializeField] private ClassWorkPhase classWorkPhase;
    [SerializeField] private JobChangePhase jobChangePhase;
    [SerializeField] private EventPhase eventPhase;

    [Header("サウンド設定")]
    [SerializeField] public AudioClip defaultBgm;  // 元のBGM
    [SerializeField] public AudioClip questBgm;    // クエスト中のBGM

    //外部参照
    private SoundManager soundManager = SoundManager.Instance;
    private PhaseBase currentPhase = null;

    // 現在メニューで選択中の項目（初期値は Quest）
    private PHASE_TYPE selectedPhaseType = PHASE_TYPE.Quest;
    private int turn = 1;

    //定数
    private const int EVENT_TURN = 4;
    private const int MAX_TURN = 21;   //ゲームの最大ターン数
    private const int SELECT_TABLE_MAX = 3;    //フェーズの選択数

    //変数
    private int current = 0;        //選択中のフェーズ
    private int selectableMax = SELECT_TABLE_MAX;  //フェーズの選択可能数

    //フラグ
    public bool isExtrastage = false;

    private void Start()
    {
        soundManager.PlayBGM(defaultBgm);

        //初期化処理
        playerStatus.Init();

        if (questPhase != null) questPhase.Initialize(playerStatus, this);
        if (classWorkPhase != null) classWorkPhase.Initialize(playerStatus, this);
        if (jobChangePhase != null) jobChangePhase.Initialize(playerStatus, this);
        if (eventPhase != null) eventPhase.Initialize(playerStatus, this);

        UpdateMenuDisplay();
        ShowPanelForPhase(baseMenuPanel); // 初期状態は拠点メニューを表示
    }

    private void Update()
    {
        // 4ターン目 & ルート未確定なら自動でイベント発生
        if (turn == EVENT_TURN && playerStatus != null && playerStatus.CurrentRoute == PLAYER_ROUTE.None)
        {
            if (currentPhase == null)
            {
                StartPhase(eventPhase, "", eventUIPanel);
            }
        }

        //フェーズの更新処理
        if (currentPhase != null)
        {
            currentPhase.OnUpdatePhase();

            if (currentPhase.IsFinished())
            {
                EndPhase();
            }
        }
        else
        {
            // メニュー選択中の処理
            if (turn >= MAX_TURN)
            {
                //ターンが21ならクエストしか選べない
                selectableMax = 1;
                selectedPhaseType = PHASE_TYPE.Quest;
            }
            else
            {
                selectableMax = SELECT_TABLE_MAX;
            }

            ProcessInput();
            DecisionPhase();
        }
    }

    //現在のターン数を渡す
    public int  GetTurn()
    {
        return turn;
    }

    //キーボードなどの処理
    private void ProcessInput()
    {
        if (Keyboard.current == null) return;

        current = (int)selectedPhaseType;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            current = (current - 1 + selectableMax) % selectableMax;
            selectedPhaseType = (PHASE_TYPE)current;
            UpdateMenuDisplay();
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y < 0f)
        {
            current = (current + 1) % selectableMax;
            selectedPhaseType = (PHASE_TYPE)current;
            UpdateMenuDisplay();
        }
    }

    //フェーズ決定処理
    private void DecisionPhase()
    {
        // 選択中のフェーズに応じて処理を開始
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            switch (selectedPhaseType)
            {
                case PHASE_TYPE.Quest:
                    if (questPhase != null) questPhase.Initialize(playerStatus, this);
                    StartPhase(questPhase, "【クエストフェーズ】戦闘中...", questUIPanel);
                    break;
                case PHASE_TYPE.ClassWork:
                    if (classWorkPhase != null) classWorkPhase.Initialize(playerStatus, this);
                    StartPhase(classWorkPhase, "【授業フェーズ】受講中...", classWorkUIPanel);
                    break;
                case PHASE_TYPE.JobChange:
                    if (jobChangePhase != null) jobChangePhase.Initialize(playerStatus, this);
                    StartPhase(jobChangePhase, "【資格試験フェーズ】試験中...", jobChangeUIPanel);
                    break;
            }

            soundManager.PlayDecision();
        }
    }

    //選択したフェーズの開始処理
    private void StartPhase(PhaseBase phase, string phaseName, GameObject targetPanel = null)
    {
        currentPhase = phase;
        if (currentPhase != null)
        {
            currentPhase.OnEnterPhase();
        }

        if (phaseStateText != null)
        {
            phaseStateText.text = phaseName;
        }

        // 指定したパネルだけを表示する
        ShowPanelForPhase(targetPanel);
    }

    //フェーズの終了処理
    private void EndPhase()
    {
        if (currentPhase.PhaseResult != PhaseBase.PHASE_RESULT.CANCEL)
        {
            turn++;
        }

        // ターンが21以上になったら、選択可能なフェーズをクエストのみに制限
        if (turn >= MAX_TURN)
        {
            selectedPhaseType = PHASE_TYPE.Quest;
        }

        currentPhase = null;
        UpdateMenuDisplay();

        // 拠点メニューの表示に戻す
        ShowPanelForPhase(baseMenuPanel);
    }

    //パネルの表示 / 非表示を一括でコントロールするメソッド
    private void ShowPanelForPhase(GameObject activePanel)
    {
        if (baseMenuPanel != null) baseMenuPanel.SetActive(baseMenuPanel == activePanel);
        if (questUIPanel != null) questUIPanel.SetActive(questUIPanel == activePanel);
        if (classWorkUIPanel != null) classWorkUIPanel.SetActive(classWorkUIPanel == activePanel);
        if (jobChangeUIPanel != null) jobChangeUIPanel.SetActive(jobChangeUIPanel == activePanel);
        if (eventUIPanel != null) eventUIPanel.SetActive(eventUIPanel == activePanel);

        // パネル未指定の場合は拠点メニューを優先表示
        if (activePanel == null && baseMenuPanel != null)
        {
            baseMenuPanel.SetActive(true);
        }
    }


    //UI系の更新処理
    private void UpdateMenuDisplay()
    {
        if (turnText != null) turnText.text = $"現在のターン\n   {turn} / {MAX_TURN}";

        if (routeText != null && playerStatus != null)
        {
            string routeName = playerStatus.CurrentRoute switch
            {
                PLAYER_ROUTE.Breakthrough => "打破",
                PLAYER_ROUTE.Salvation => "救世",
                PLAYER_ROUTE.Truth => "真理",
                PLAYER_ROUTE.Selfless => "無欲",
                _ => "未選択"
            };
            
            routeText.text = $"現在のルート: <color=#FF0000>{routeName}</color>";
        }

        if (phaseStateText != null)
        {
            string menuStr = "";
            menuStr += (selectedPhaseType == PHASE_TYPE.Quest) ? "<color=#FFE600>クエスト</color>\n" : "クエスト\n";
            menuStr += (selectedPhaseType == PHASE_TYPE.ClassWork) ? "<color=#FFE600> 授業</color>\n" : " 授業\n";
            menuStr += (selectedPhaseType == PHASE_TYPE.JobChange) ? "<color=#FFE600>資格試験</color>\n" : "資格試験\n";

            phaseStateText.text = menuStr;
        }
    }
}