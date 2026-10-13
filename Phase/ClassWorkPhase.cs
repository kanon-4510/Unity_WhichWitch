using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ClassWorkPhase : PhaseBase
{
    private enum CLASSWORK_SELECT
    {
        Pharmacy,       // 0: 薬学
        MartialArts,    // 1: 武術
        MagicKnowledge, // 2: 魔法知識
        Faith,          // 3: 信仰
        Archaeology,    // 4: 考古学
        Astrology,      // 5: 占星術
        Max
    }

    //定数
    //ゲームバランス調整用定数
    private const int SKILL_UP_BASE = 50;
    private const int SKILL_UP_RANGE = 15; // -15 〜 +15 の振れ幅

    private const int INTERVAL_PHARMACY = 7;
    private const int INTERVAL_MARTIAL = 5;
    private const int INTERVAL_MAGIC_KNOWLEDGE = 50;
    private const int INTERVAL_FAITH = 15;
    private const int INTERVAL_ARCHAEOLOGY = 8;
    private const int INTERVAL_ASTROLOGY = 5;

    [Header("参照設定")]
    [SerializeField] private PlayerStatus playerStatus;
    [SerializeField] private GameScene gameScene;

    [Header("UI表示用テキスト")]
    [SerializeField] private TMP_Text optionsText;     // 科目一覧
    [SerializeField] private TMP_Text descriptionText; // 科目の説明文
    [SerializeField] private TMP_Text bonusText;       // 効果内容 & 残り数値表示
    [SerializeField] private TMP_Text tutorialText;    // チュートリアル表示（任意）

    private CLASSWORK_SELECT selectedIndex = CLASSWORK_SELECT.Pharmacy;

    //授業内容の名前
    private readonly string[] classNames = new string[]
    {
        "薬学調合",
        "武術訓練",
        "魔法知識",
        "信仰",
        "考古学",
        "占星術"
    };

    //初期化処理
    public void Initialize(PlayerStatus playerStatus, GameScene gameScene)
    {
        this.playerStatus = playerStatus;
        this.gameScene = gameScene;

        playerStatus.InitJob(); //職業の初期化
    }

    
    public override void OnEnterPhase()
    {
        base.OnEnterPhase();

        isFinished = false;
        PhaseResult = PHASE_RESULT.NONE;
        selectedIndex = CLASSWORK_SELECT.Pharmacy;

        UpdateUI();
    }

    //更新処理
    public override void OnUpdatePhase()
    {
        if (isFinished) return;

        ProcessSelection();
        ProcessDecision();
    }

    //上下キー・数字キーでの選択
    private void ProcessSelection()
    {
        if (Keyboard.current == null) return;

        int max = (int)CLASSWORK_SELECT.Max;
        int current = (int)selectedIndex;

        //上キー又は、マウスカーソル
        if (Keyboard.current.upArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            current = (current - 1 + max) % max;
            selectedIndex = (CLASSWORK_SELECT)current;
            UpdateUI();
        }
        //下キー又は、マウスカーソル
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y < 0f)
        {
            current = (current + 1) % max;
            selectedIndex = (CLASSWORK_SELECT)current;
            UpdateUI();
        }
    }

    //決定・キャンセル処理
    private void ProcessDecision()
    {
        if (Keyboard.current == null) return;

        //Enterで受講確定
        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            //上昇量の計算：基本50 + 乱数(-15 〜 15) => 35〜65
            int rand = Random.Range(-SKILL_UP_RANGE, SKILL_UP_RANGE + 1);
            int skillUp = SKILL_UP_BASE + rand;

            if (playerStatus != null)
            {
                //ご自身の PlayerStatus のメソッド名に合わせて調整してください
                switch (selectedIndex)
                {
                    case CLASSWORK_SELECT.Pharmacy:
                        playerStatus.AddSkillPoint(SKILL_TYPE.Pharmacy, skillUp);
                        break;
                    case CLASSWORK_SELECT.MartialArts:
                        playerStatus.AddSkillPoint(SKILL_TYPE.MartialArts, skillUp);
                        break;
                    case CLASSWORK_SELECT.MagicKnowledge:
                        playerStatus.AddSkillPoint(SKILL_TYPE.MagicKnowledge, skillUp);
                        break;
                    case CLASSWORK_SELECT.Faith:
                        playerStatus.AddSkillPoint(SKILL_TYPE.Faith, skillUp);
                        break;
                    case CLASSWORK_SELECT.Archaeology:
                        playerStatus.AddSkillPoint(SKILL_TYPE.Archaeology, skillUp);
                        break;
                    case CLASSWORK_SELECT.Astrology:
                        playerStatus.AddSkillPoint(SKILL_TYPE.Astrology, skillUp);
                        break;
                }
            }

            PhaseResult = PHASE_RESULT.NEXT_TURN; //ターンを進める
            isFinished = true;
        }

        //TABキーでキャンセルして戻る
        if (Keyboard.current.tabKey.wasPressedThisFrame || Keyboard.current.escapeKey.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame)
        {
            PhaseResult = PHASE_RESULT.CANCEL; //ターンを進めずに戻る
            isFinished = true;
        }
    }

    //UI全体（選択肢・説明文・技能ボーナス）の表示更新
    private void UpdateUI()
    {
        //科目一覧の描画（選択中のものを黄色に）
        if (optionsText != null)
        {
            string str = "";
            for (int i = 0; i < classNames.Length; i++)
            {
                if (i == (int)selectedIndex)
                {
                    str += $"<color=yellow>{classNames[i]}</color>\n\n";
                }
                else
                {
                    str += $"{classNames[i]}\n\n";
                }
            }
            optionsText.text = str;
        }

        //説明文と技能ボーナスの計算＆表示
        UpdateSkillBonusUI();
    }

    private void UpdateSkillBonusUI()
    {
        string description = "";
        string effectName = "";
        int reqInterval = 0;

        int currentVal = 0;

        //PlayerStatus から現在の値を取得
        switch (selectedIndex)
        {
            case CLASSWORK_SELECT.Pharmacy:
                description = "魔法薬の調合だけでなく、魔石鑑定や魔道具の扱い方を学ぶ。\n受講すると回復魔法の効果が上昇する。";
                effectName = "治癒力";
                currentVal = playerStatus != null ? playerStatus.pharmacy : 0;
                reqInterval = INTERVAL_PHARMACY - (currentVal % INTERVAL_PHARMACY);
                break;

            case CLASSWORK_SELECT.MartialArts:
                description = "魔法一筋では生きていくことなど出来ない。基礎的な体術や武器の取回しを学ぶ。\n受講すると会心発生率が上昇する。";
                effectName = "会心率";
                currentVal = playerStatus != null ? playerStatus.martialArts : 0;
                reqInterval = INTERVAL_MARTIAL - (currentVal % INTERVAL_MARTIAL);
                break;

            case CLASSWORK_SELECT.MagicKnowledge:
                description = "魔法についての知見を深める授業。\n魔法の原理から法陣図や記述式も学ぶ。\n受講すると使える魔法が増える。";
                effectName = "使える魔法の種類";
                currentVal = playerStatus != null ? playerStatus.magicKnowledge : 0;
                reqInterval = INTERVAL_MAGIC_KNOWLEDGE - (currentVal % INTERVAL_MAGIC_KNOWLEDGE);
                break;

            case CLASSWORK_SELECT.Faith:
                description = "神が人に魔法を授けるまでの神話と、もたらされた加護と寵愛を主に学ぶ。\n受講すると守備力が上昇する。";
                effectName = "被ダメージ軽減";
                currentVal = playerStatus != null ? playerStatus.faith : 0;
                reqInterval = INTERVAL_FAITH - (currentVal % INTERVAL_FAITH);
                break;

            case CLASSWORK_SELECT.Archaeology:
                description = "古代魔術やルーンの解読、封印術の解呪など魔法がたどった歴史を学ぶ。\n受講すると獲得経験値が上昇する。";
                effectName = "獲得経験値";
                currentVal = playerStatus != null ? playerStatus.archaeology : 0;
                reqInterval = INTERVAL_ARCHAEOLOGY - (currentVal % INTERVAL_ARCHAEOLOGY);
                break;

            case CLASSWORK_SELECT.Astrology:
                description = "天文を知り、星の導きから運命力や透視などの予見について学ぶ。\n受講すると回避率が上昇する。";
                effectName = "回避率";
                currentVal = playerStatus != null ? playerStatus.astrology : 0;
                reqInterval = INTERVAL_ASTROLOGY - (currentVal % INTERVAL_ASTROLOGY);
                break;
        }

        if (descriptionText != null)
        {
            descriptionText.text = description;
        }

        if (bonusText != null)
        {
            bonusText.text = $"<color=yellow>技能ボーナス</color>\n" +
                             $"効果内容: {effectName}\n" +
                             $"<color=green>残り {reqInterval} で能力アップ!</color>";
        }
    }
}