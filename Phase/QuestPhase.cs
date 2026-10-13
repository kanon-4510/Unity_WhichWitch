using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public enum QUEST_LOCATION
{
    PLAINS,     // 平原
    FOREST,     // 魔法の森
    SHRINE,     // 岩山の道場
    CONTINENT,  // 魔大陸
    CATHEDRAL,  // 壊れた聖堂
    RUINS,      // 古代遺跡
    HILL,       // 星の丘
    EXTRA       // エクストラ
}

public class QuestPhase : PhaseBase
{
    public enum BATTLE_STEP
    {
        DIFFICULTY_SELECTION,   // 場所選択
        COMMAND_SELECTION,      // 攻撃 / 魔法 選択
        COMMAND_SUB_SELECTION,  // 魔法のカテゴリ選択
        MAGIC_SELECTION,        // 魔法選択
        DETERMINE,              // 行動順計算
        ACTION_LOOP,            // 行動実行・ダメージ処理
        STATUS_EFFECT,          // ターンの終わりの状態異常処理
        RESULT                  // 遠征クリア結果表示
    }

    public enum COMMAND
    {
        ATTACK,
        MAGIC,
        MAX
    }

    public class ActionUnit
    {
        public string name;         // 名前
        public int speed;           // 速さ
        public bool isPlayer;       // プレイヤーか
        public int command;         // 攻撃か魔法か
        public string skillName;    // 攻撃名
        public MAGIC_TYPE magicType;// 魔法の種類
    }

    [System.Serializable]
    public struct LocationBackground
    {
        public QUEST_LOCATION location;
        public Sprite backgroundSprite;
    }

    [Header("背景設定")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Sprite defaultBgSprite;
    [SerializeField] private List<LocationBackground> locationBackgrounds;

    [SerializeField] private Image boardImage;
    [SerializeField] private Sprite boardSprite;
    [SerializeField] private List<LocationBackground> boardList;

    [Header("UI参照")]
    [SerializeField] private GameObject questUIPanel;
    [SerializeField] private TextMeshProUGUI difficultyText;
    [SerializeField] private TextMeshProUGUI battleMessageText;
    [SerializeField] private TextMeshProUGUI commandMenuText;
    [SerializeField] private TextMeshProUGUI magicDescText;
    [SerializeField] private TextMeshProUGUI NowWaveText;

    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private Slider enemyHp;
    [SerializeField] private TextMeshProUGUI enemyName;

    [Header("表示オブジェクト")]
    [SerializeField] private GameObject playerStatusBer;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private EnemyView enemyView;

    // 外部参照
    private GameScene gameScene;
    private PlayerStatus playerStatus;
    SoundManager soundManager = SoundManager.Instance;

    // 内部変数
    private BATTLE_STEP battleStep = BATTLE_STEP.DIFFICULTY_SELECTION;
    private COMMAND command = COMMAND.ATTACK;
    private STATUS_EFFECT enemyStatusEffect = STATUS_EFFECT.NONE;

    //定数
    //クエスト・ターン関連の定数
    private const int EXTRA_STAGE_START_TURN = 17;
    private const int GAME_CLEAR_TURN = 21;
    private const int MAX_WAVES_NORMAL = 3;
    private const int MAX_WAVES_HELL= 5;

    //戦闘計算用の定数
    private const float NORMAL_CRIT_MULTIPLIER = 2.0f;
    private const float BOOSTED_CRIT_MULTIPLIER = 6.0f;
    private const int CRITICAL_DIVISOR = 5;      // クリティカル率計算用
    private const int EVASION_DIVISOR = 5;       // 回避率計算用
    private const int MAX_EVASION_CHANCE = 90;   // 回避率の上限 (%)
    private const int POISON_DIVISOR = 16;       // 毒ダメージの割合計算用


    //演出・遅延用定数
    private const float GAMEOVER_DELAY = 0.5f;     // ゲームオーバー画面遷移までの遅延

    //クエストクリア
    private const int REWARD_BASE_VALUE = 15;
    private const int REWARD_RANDOM_RANGE = 21;
    private const int REWARD_OFFSET = 10;

    //状態異常
    private const int PERCENT_MAX = 100;
    private const int FREEZE_FAIL_CHANCE = 60;
    private const int FLASH_MISS_CHANCE = 50;
    private const int PLAYER_EFFECT_MAX = 4; //プレイヤーの呪いと沈黙カウント最大値
    private const int ENEMY_CURS_MAX = 6; // 敵の呪いカウント最大値
    private const int EXTRA_EXP = 450; //エクストラステージの経験値

    private const int LOCATION_MENU_DISPLAY_COUNT = 4;
    private const int COMMAND_MENU_DISPLAY_COUNT = 2;
    private const int MAGIC_MENU_DISPLAY_COUNT = 3;

    private int statusTurns = 4;    //沈黙の回復ターン
    private int enemyCurs = 6;      // 敵の呪いカウント
    private int poisonCnt = 0;

    // 選択されたクエストの場所
    private QUEST_LOCATION location;
    private List<string> locationMenu = new List<string>();
    private List<QUEST_LOCATION> selectableLocations = new List<QUEST_LOCATION>();
    private int difficultyCursor = 0;

    //魔法関係
    private bool wasMagicUsedLastTurn = false;  //魔法が使えるか
    private bool magicUsedThisTurn = false;     //限在ターンに魔法を使ったか
    private List<MagicData> availableMagics = new List<MagicData>();
    private List<string> magicTypeMessages = new List<string>();
    private MagicData selectedMagic;

    //敵関係
    private Enemy activeEnemy = null;
    private int battleTurn = 1;
    private int currentWave = 1;
    private bool isHellQuest = false;

    //コマンド関係
    private List<ActionUnit> actionOrder = new List<ActionUnit>();
    private int currentActionIdx = 0;
    private int subMenuCursor = 0;
    private int magicMenuCursor = 0;
    private List<string> subActionMessages = new List<string>();
    private string battleMessage = "";

    private bool isGameOver = false;

    public void Initialize(PlayerStatus playerStatus, GameScene gameScene, bool isHellQuest = false)
    {
        this.playerStatus = playerStatus;
        this.gameScene = gameScene;
        this.isHellQuest = isHellQuest;

        playerStatus.InitJob();

        locationMenu = new List<string> { "平原", "魔法の森", "岩山の道場", "魔大陸", "壊れた聖堂", "古代遺跡", "星の丘" };
        selectableLocations = new List<QUEST_LOCATION> {
            QUEST_LOCATION.PLAINS, QUEST_LOCATION.FOREST, QUEST_LOCATION.SHRINE,
            QUEST_LOCATION.CONTINENT, QUEST_LOCATION.CATHEDRAL, QUEST_LOCATION.RUINS, QUEST_LOCATION.HILL
        };

        if (gameScene.GetTurn() >= EXTRA_STAGE_START_TURN && gameScene.GetTurn() < GAME_CLEAR_TURN)
        {
            locationMenu.Add("エクストラ");
            selectableLocations.Add(QUEST_LOCATION.EXTRA);
        }
    }

    public override void OnEnterPhase()
    {
        base.OnEnterPhase();

        //クエスト用BGM
        if (gameScene.questBgm != null) soundManager.PlayBGM(gameScene.questBgm);

        //クエスト開始時の処理
        playerStatusBer.SetActive(false);
        playerObject.SetActive(false);
        enemyView.gameObject.SetActive(false);

        if (questUIPanel != null) questUIPanel.SetActive(true);

        battleStep = BATTLE_STEP.DIFFICULTY_SELECTION;
        playerStatus.statusEffect = STATUS_EFFECT.NONE;
        enemyStatusEffect = STATUS_EFFECT.NONE;
        difficultyCursor = 0;
    }

    public override void OnUpdatePhase()
    {
        base.OnUpdatePhase();

        //ゲームオーバーなら処理しない
        if (isGameOver) return;

        ManageTurn();

        playerStatus?.Update();

        CheckPlayerIsAlive();

        UpdateUI();
    }

    private void ManageTurn()
    {
        switch (battleStep)
        {
            case BATTLE_STEP.DIFFICULTY_SELECTION:
                playerStatus.hp = playerStatus.GetMaxHp();
               // playerStatus.UpdateHpBar();
                ProcessDifficulty();
                break;
            case BATTLE_STEP.COMMAND_SELECTION:
                ProcessPlayerAction();
                break;
            case BATTLE_STEP.COMMAND_SUB_SELECTION:
                ProcessPlayerSubAction();
                break;
            case BATTLE_STEP.MAGIC_SELECTION:
                MagicSelection();
                break;
            case BATTLE_STEP.DETERMINE:
                DetermineActionOrder();
                break;
            case BATTLE_STEP.ACTION_LOOP:
                ProcessActionLoop();
                break;
            case BATTLE_STEP.STATUS_EFFECT:
                ProcessStatusEffect();
                break;
            case BATTLE_STEP.RESULT:
                DisplayResult();
                break;
        }
    }


    private void ProcessDifficulty()
    {
        int prevCursor = difficultyCursor;
        HandleCursorInput(ref difficultyCursor, locationMenu.Count);

        QUEST_LOCATION currentLoc = QUEST_LOCATION.FOREST;

        //カーソル位置が変わった時だけ画像を切り替える
        if (prevCursor != difficultyCursor)
        {
            if (locationMenu[difficultyCursor] == "エクストラ")
            {
                currentLoc = QUEST_LOCATION.EXTRA;
            }
            else if (difficultyCursor < selectableLocations.Count)
            {
                currentLoc = selectableLocations[difficultyCursor];
            }
            ChangeBackground(currentLoc); // メソッド名を変更
        }

        if (IsEnterPressed())
        {
            if (locationMenu[difficultyCursor] == "エクストラ")
            {
                isHellQuest = true;
                gameScene.isExtrastage = true;
                location = QUEST_LOCATION.EXTRA;
                SpawnRushEnemy(0);
            }
            else
            {
                isHellQuest = false;
                location = selectableLocations[difficultyCursor];
                SpawnEnemyForTurn();
            }

            //クエスト決定時に背景を変更
            UpdateBackground(location);
            battleStep = BATTLE_STEP.COMMAND_SELECTION;

            //決定SE
            soundManager.PlayDecision();
        }
        else if (IsCancelPressed())
        {
            PhaseResult = PHASE_RESULT.CANCEL;
            isFinished = true; 
            if (gameScene.defaultBgm != null) soundManager.PlayBGM(gameScene.defaultBgm);
        }
    }

    //ステージ選択の画像を切り替える
    private void ChangeBackground(QUEST_LOCATION Cousr)
    {
        // ボード（ステージ説明）画像の更新
        if (boardImage != null)
        {
            Sprite targetBoard = boardSprite; // デフォルトのスプライト
            foreach (var bl in boardList)
            {
                if (bl.location == Cousr)
                {
                    targetBoard = bl.backgroundSprite;
                    break;
                }
            }
            boardImage.sprite = targetBoard;
        }
    }

    //クエスト決定時の背景を切り替える
    private void UpdateBackground(QUEST_LOCATION targetLocation)
    {
        if (bgImage == null) return;

        foreach (var item in locationBackgrounds)
        {
            if (item.location == targetLocation)
            {
                bgImage.sprite = item.backgroundSprite;
                return;
            }
        }
    }

    // プレイヤーの行動選択処理
    private void ProcessPlayerAction()
    {
        int cmdIdx = (int)command;
        HandleCursorInput(ref cmdIdx, (int)COMMAND.MAX);
        command = (COMMAND)cmdIdx;

        //ネクロマンサーのユニークスキルを発動
        if (playerStatus.hasStartDamage && !playerStatus.isStartDamageUsed)
        {
            playerStatus.isStartDamageUsed = true;
            activeEnemy?.Damage(playerStatus.MagicAttack());
            battleMessage = "【死者の力】魔法攻撃を放った！";
        }

        if (IsEnterPressed())
        {
            if (command == COMMAND.MAGIC && playerStatus.statusEffect == STATUS_EFFECT.SILENCE)
            {
                battleMessage = "<color=#FF0000>魔法を唱えられない</color>";
                return;
            }
            if (command == COMMAND.MAGIC && wasMagicUsedLastTurn && !playerStatus.hasMagicUnlock)
            {
                battleMessage = "<color=#FF0000>魔法詠唱中...</color>";
                return;
            }

            magicUsedThisTurn = (command == COMMAND.MAGIC);
            subActionMessages.Clear();
            subMenuCursor = 0;
            battleMessage = "";

            if (command == COMMAND.ATTACK)
            {
                battleStep = BATTLE_STEP.DETERMINE;
                return;
            }
            else if (command == COMMAND.MAGIC)
            {
                subActionMessages = new List<string> { "攻撃", "回復", "状態異常付与" };
                battleStep = BATTLE_STEP.COMMAND_SUB_SELECTION;
            }
        }
    }

    // プレイヤーのサブアクション選択処理（魔法カテゴリ選択）
    private void ProcessPlayerSubAction()
    {
        if (subActionMessages.Count == 0) return;
        HandleCursorInput(ref subMenuCursor, subActionMessages.Count);

        if (IsEnterPressed())
        {
            magicMenuCursor = 0;
            magicTypeMessages.Clear();
            availableMagics.Clear();

            MAGIC_TYPE selectedCategory = (MAGIC_TYPE)subMenuCursor;

            foreach (var magic in MagicDatabase.LIST)
            {
                if (magic.type == selectedCategory)
                {
                    availableMagics.Add(magic);
                    if (playerStatus.magicKnowledge >= magic.reqMagicKnowledge)
                    {
                        magicTypeMessages.Add(magic.name);
                    }
                    else
                    {
                        magicTypeMessages.Add($"???（知識:{magic.reqMagicKnowledge}）");
                    }
                }
            }

            battleStep = BATTLE_STEP.MAGIC_SELECTION;
        }
        else if (IsCancelPressed())
        {
            battleStep = BATTLE_STEP.COMMAND_SELECTION;
        }
    }

    // プレイヤーの魔法選択処理
    private void MagicSelection()
    {
        if (magicTypeMessages.Count == 0) return;
        HandleCursorInput(ref magicMenuCursor, magicTypeMessages.Count);

        if (IsEnterPressed())
        {
            var chosenMagic = availableMagics[magicMenuCursor];
            if (playerStatus.magicKnowledge >= chosenMagic.reqMagicKnowledge)
            {
                battleMessage = "";
                selectedMagic = chosenMagic;
                battleStep = BATTLE_STEP.DETERMINE;
            }
            else
            {
                battleMessage = "<color=#FF0000>魔法知識が足りない！</color>";
            }
        }
        else if (IsCancelPressed())
        {
            battleStep = BATTLE_STEP.COMMAND_SUB_SELECTION;
        }
    }

    // 行動順を決定する処理
    private void DetermineActionOrder()
    {
        actionOrder.Clear();

        var playerUnit = new ActionUnit
        {
            name = playerStatus.GetName(),
            speed = playerStatus.GetSpeed(),
            isPlayer = true,
            command = (int)command
        };

        if (command == COMMAND.ATTACK) playerUnit.skillName = "単体攻撃";
        else if (command == COMMAND.MAGIC) playerUnit.magicType = (MAGIC_TYPE)subMenuCursor;

        actionOrder.Add(playerUnit);

        if (activeEnemy != null && !activeEnemy.IsDead())
        {
            var (skillIndex, skillName) = activeEnemy.DecideAction();
            actionOrder.Add(new ActionUnit
            {
                name = activeEnemy.name,
                speed = activeEnemy.speed,
                isPlayer = false,
                command = skillIndex,
                skillName = skillName
            });
        }

        actionOrder.Sort((a, b) => b.speed.CompareTo(a.speed));

        currentActionIdx = 0;
        battleStep = BATTLE_STEP.ACTION_LOOP;
    }

    // 行動ループの処理
    private void ProcessActionLoop()
    {
        if (currentActionIdx >= actionOrder.Count)
        {
            battleStep = BATTLE_STEP.STATUS_EFFECT;
            battleTurn++;
            currentActionIdx = 0;
            battleMessage = "";
            return;
        }

        var unit = actionOrder[currentActionIdx];

        //行動がプレイヤーじゃなく敵が死亡していたら行動を次に進める
        if (!unit.isPlayer && (activeEnemy == null || activeEnemy.IsDead()))
        {
            currentActionIdx++;
            return;
        }

        if (string.IsNullOrEmpty(battleMessage))
        {
            // 凍結状態の判定（プレイヤー・敵共通）
            STATUS_EFFECT currentEffect = unit.isPlayer ? playerStatus.statusEffect : enemyStatusEffect;
            if (currentEffect == STATUS_EFFECT.FREEZE)
            {
                if (Random.Range(0, PERCENT_MAX) < FREEZE_FAIL_CHANCE)
                {
                    battleMessage = $"{unit.name}は凍りついて動けない！";
                    if (IsEnterPressed())
                    {
                        battleMessage = "";
                        currentActionIdx++;
                    }
                    return;
                }
                else
                {
                    if (unit.isPlayer) playerStatus.statusEffect = STATUS_EFFECT.NONE;
                    else enemyStatusEffect = STATUS_EFFECT.NONE;

                    battleMessage = $"{unit.name}の氷が溶けた！\n";
                }
            }

            //
            if (string.IsNullOrEmpty(battleMessage))
            {
                if (unit.isPlayer) ExecutePlayerAttack(unit);
                else ExecuteEnemyAttack(unit);

                //アニメーションの更新
                if (activeEnemy != null && activeEnemy.IsDead())
                {
                    EnemyView enemyViewObj = Object.FindAnyObjectByType<EnemyView>();
                    if (enemyViewObj != null) enemyViewObj.ChangeAnim(EnemyAnim.DEAD);
                }
            }
        }

        if (IsEnterPressed() &&
            enemyView.IsAnimFinish())
        {
            //ここで敵を倒したときの文を表示する
            if (activeEnemy != null && activeEnemy.IsDead() && !battleMessage.Contains("を倒した！"))
            {
                battleMessage = $"{activeEnemy.GetName()}を倒した！";
                return; // ここで一度処理を止め、次のEnterで次に進めるようにする
            }

            battleMessage = "";
            if (activeEnemy != null && activeEnemy.IsDead())
            {
                CheckEnemyDeath();
                return;
            }
            currentActionIdx++;
        }
    }

    // プレイヤーの攻撃処理
    private void ExecutePlayerAttack(ActionUnit unit)
    {
        // 閃光状態の判定（プレイヤー）
        if (playerStatus.statusEffect == STATUS_EFFECT.FLASH && selectedMagic.type != MAGIC_TYPE.HEAL && Random.Range(0, PERCENT_MAX) < FLASH_MISS_CHANCE)
        {
            battleMessage = $"{unit.name}の攻撃！\n閃光で目が眩んで攻撃が外れた！";
            return;
        }

        if (command == COMMAND.ATTACK)
        {
            battleMessage = $"{unit.name}の攻撃！";
            int attackPow = playerStatus.Attack();

            if (playerStatus.hasMagicToAttack)
            {
                attackPow += playerStatus.MagicAttack() / COMMAND_MENU_DISPLAY_COUNT;
                battleMessage += "\n【カドゥケウス】攻撃の威力が上がった！";
            }
            int criticalChance = playerStatus.martialArts / CRITICAL_DIVISOR;
            if (Random.Range(0, PERCENT_MAX) < criticalChance)
            {
                battleMessage += "\nクリティカルヒット！";
                float critMultiplier = playerStatus.hasCritBoost ? BOOSTED_CRIT_MULTIPLIER : NORMAL_CRIT_MULTIPLIER;
                //クリティカル時のダメージ計算
                if (playerStatus.hasCritBoost)
                {
                    battleMessage += "\n【極聖光】会心倍率が上がった！";
                }
                activeEnemy.Damage((int)(attackPow * critMultiplier));
            }
            else if (activeEnemy != null)
            {
                activeEnemy.Damage(attackPow);
            }

            //敵のダメージアニメーションを再生
            EnemyView enemyViewObj = Object.FindAnyObjectByType<EnemyView>();
            if (enemyViewObj != null) enemyViewObj.ChangeAnim(EnemyAnim.HURT);
        }
        else if (command == COMMAND.MAGIC)
        {
            battleMessage = $"{unit.name}の{selectedMagic.name}！";

            //魔法の威力計算とダメージ処理
            if (selectedMagic.powerMultiplier > 0 && (selectedMagic.type == MAGIC_TYPE.ATTACK || selectedMagic.type == MAGIC_TYPE.DEBUFF))
            {
                int magicDamage = (int)(playerStatus.MagicAttack() * selectedMagic.powerMultiplier);
                activeEnemy.Damage(magicDamage);
            }

            switch (selectedMagic.type)
            {
                case MAGIC_TYPE.HEAL:
                    if (selectedMagic.isDrain)
                    {
                        int drainDamage = (int)(playerStatus.MagicAttack() * selectedMagic.powerMultiplier);
                        activeEnemy.Damage(drainDamage);
                        playerStatus.Heal(drainDamage);
                        battleMessage += "\n敵から体力を奪った！";
                    }
                    else if (selectedMagic.powerMultiplier > 0.0f)
                    {
                        int healAmount = (int)(playerStatus.MagicAttack() * selectedMagic.powerMultiplier);
                        playerStatus.Heal(healAmount);
                        battleMessage += $"\n体力が{healAmount}回復した！";
                    }
                    if (selectedMagic.curesStatus)
                    {
                        playerStatus.CureAilment();
                        battleMessage += "\n状態異常が回復した！";
                    }
                    break;
                case MAGIC_TYPE.DEBUFF:
                    if (selectedMagic.ailment != STATUS_EFFECT.NONE && Random.Range(0, PERCENT_MAX) < selectedMagic.ailmentChance)
                    {
                        enemyStatusEffect = selectedMagic.ailment;
                        if (selectedMagic.ailment == STATUS_EFFECT.CURSE)
                        {
                            enemyCurs = ENEMY_CURS_MAX; // 敵に呪いを付与した際にカウントをリセット
                        }
                        battleMessage += "\n敵に状態異常を与えた！";
                    }
                    break;
            }
        }
    }

    // 敵の攻撃処理
    private void ExecuteEnemyAttack(ActionUnit unit)
    {
        //凍結状態の判定（敵）
        if (enemyStatusEffect == STATUS_EFFECT.FREEZE && Random.Range(0, PERCENT_MAX) < FREEZE_FAIL_CHANCE)
        {
            battleMessage = $"{unit.name}は凍りついて動けない！";
            return;
        }
        else if (enemyStatusEffect == STATUS_EFFECT.FREEZE)
        {
            enemyStatusEffect = STATUS_EFFECT.NONE;
            battleMessage = $"{unit.name}は氷が溶けた動けるようになった！\n";
        }

        // 閃光状態の判定（敵）
        if (enemyStatusEffect == STATUS_EFFECT.FLASH && Random.Range(0, PERCENT_MAX) < FLASH_MISS_CHANCE)
        {
            battleMessage = $"{unit.name}の{unit.skillName}！\n目が眩んで攻撃が外れた！";
            return;
        }

        battleMessage = $"{unit.name}の{unit.skillName}！";

        // 敵のアニメーションを変更
        EnemyView enemyViewObj = Object.FindAnyObjectByType<EnemyView>();
        if (enemyViewObj != null)
        {
            if (unit.command == 0) enemyViewObj.ChangeAnim(EnemyAnim.ACT1);
            else if (unit.command == 1) enemyViewObj.ChangeAnim(EnemyAnim.ACT2);
            else enemyViewObj.ChangeAnim(EnemyAnim.ACT3);
        }

        // 敵の攻撃力を取得
        int damage = 0;
        activeEnemy.ResetGuard(); // 敵のガードをリセット

        //敵の回復
        if (unit.skillName == "大地の恵み" || unit.skillName == "電力チャージ"
           || unit.skillName == "自己再生")
        {
            damage = activeEnemy.GetPower(unit.command);
            activeEnemy.Heal(damage);
            battleMessage += $"\n{unit.name}は体力が{damage}回復した！";
        }
        //ボスの遡行
        else if (unit.skillName == "遡行")
        {
            damage = activeEnemy.GetPower(unit.command);
            activeEnemy.Heal(damage);
            enemyStatusEffect = STATUS_EFFECT.NONE;
            enemyCurs = ENEMY_CURS_MAX; // 呪いカウントをリセット
            battleMessage += $"\n{unit.name}の肉体が巻き戻る！";
        }
        else if (unit.skillName == "まもる" || unit.skillName == "守る"
             || unit.skillName == "守りの構え" || unit.skillName == "受流しの構え")
        {
            //Power分ダメージ軽減
            damage = activeEnemy.GetPower(unit.command);
            activeEnemy.SetGuard(damage);
            battleMessage += $"\n{unit.name}は身構えている";
        }
        else
        {
            // 回避判定
            damage = activeEnemy.GetPower(unit.command);
            int evasionChance = Mathf.Min(playerStatus.astrology / EVASION_DIVISOR, MAX_EVASION_CHANCE);
            if (Random.Range(0, PERCENT_MAX) < evasionChance)
            {
                battleMessage += "\n攻撃を回避した！";
            }
            else if (!activeEnemy.IsDead())
            {
                // 初回攻撃無効化の判定
                if (playerStatus.hasFirstHitNull && !playerStatus.isFirstHitUsed)
                {
                    playerStatus.isFirstHitUsed = true; // 初回攻撃無効化を使用済みにする
                    battleMessage += "\n【神秘の護り】攻撃を防いだ！";
                    return;
                }

                // プレイヤーに状態異常の付与判定
                else if (unit.skillName == "蛇にらみ" || unit.skillName == "石化の魔眼"
                   || unit.skillName == "金縛り" || unit.skillName == "発狂")
                {
                    //プレイヤーを凍結状態にする
                    damage = activeEnemy.GetPower(unit.command);
                    playerStatus.Damage(damage);
                    if (playerStatus.statusEffect == STATUS_EFFECT.NONE)
                    {
                        playerStatus.statusEffect = STATUS_EFFECT.FREEZE;
                        battleMessage += "\n" + playerStatus.GetName() + "は凍りついた！";
                    }
                }
                else if (unit.skillName == "放熱" || unit.skillName == "ふきつなかぜ"
                    || unit.skillName == "破魔空間" || unit.skillName == "火炎放射" || unit.skillName == "沈黙の呪い")
                {
                    //プレイヤーを沈黙状態にする
                    damage = activeEnemy.GetPower(unit.command);
                    playerStatus.Damage(damage);
                    if (playerStatus.statusEffect == STATUS_EFFECT.NONE)
                    {
                        playerStatus.statusEffect = STATUS_EFFECT.SILENCE;
                        battleMessage += "\n" + playerStatus.GetName() + "は沈黙になった！";
                    }
                }
                else if (unit.skillName == "どくのや" || unit.skillName == "毒の粉"
                    || unit.skillName == "毒牙" || unit.skillName == "かみつく" || unit.skillName == "毒パンチ")
                {
                    //プレイヤーを毒状態にする
                    damage = activeEnemy.GetPower(unit.command);
                    playerStatus.Damage(damage);
                    if (playerStatus.statusEffect == STATUS_EFFECT.NONE)
                    {
                        playerStatus.statusEffect = STATUS_EFFECT.POISON;
                        battleMessage += "\n" + playerStatus.GetName() + "は毒状態になった！";
                    }
                }
                else if (unit.skillName == "呪い" || unit.skillName == "呪われた包丁"
                    || unit.skillName == "血槍" || unit.skillName == "鬼火" || unit.skillName == "切断")
                {
                    //プレイヤーを呪い状態にする
                    damage = activeEnemy.GetPower(unit.command);
                    playerStatus.Damage(damage);
                    if (playerStatus.statusEffect == STATUS_EFFECT.NONE)
                    {
                        playerStatus.statusEffect = STATUS_EFFECT.CURSE;
                        battleMessage += "\n" + playerStatus.GetName() + "は呪われた！";
                    }
                }
                else if (unit.skillName == "ばくはつ" || unit.skillName == "電撃斬"
                    || unit.skillName == "雷連斬" || unit.skillName == "エレキビーム" || unit.skillName == "斬撃"
                    || unit.skillName == "魔の威光")
                {
                    //プレイヤーを閃光状態にする
                    damage = activeEnemy.GetPower(unit.command);
                    playerStatus.Damage(damage);
                    if (playerStatus.statusEffect == STATUS_EFFECT.NONE)
                    {
                        playerStatus.statusEffect = STATUS_EFFECT.FLASH;
                        battleMessage += "\n" + playerStatus.GetName() + "は目がくらんだ！";
                    }
                }
                else
                {
                    //上記のどれにも当てはまらない場合は通常攻撃として処理
                    switch (unit.command)
                    {
                        case 0:
                            damage = activeEnemy.GetPower(unit.command);
                            playerStatus.Damage(damage);
                            break;
                        case 1:
                            damage = activeEnemy.GetPower(unit.command);
                            playerStatus.Damage(damage);
                            break;
                        case 2:
                            damage = activeEnemy.GetPower(unit.command);
                            playerStatus.Damage(damage);
                            break;
                        default:
                            damage = activeEnemy.GetPower(unit.command);
                            playerStatus.Damage(damage);
                            break;
                    }
                }
            }
            if (playerStatus.hasGuts && !playerStatus.isGutsUsed)
            {
                playerStatus.isGutsUsed = true;
                battleMessage += $"\n【未来氏】{playerStatus.GetName()}は死から逃れた";
            }
        }

    }

    // ターン終了時の状態異常処理
    private void ProcessStatusEffect()
    {
        if (string.IsNullOrEmpty(battleMessage))
        {
            List<string> turnEndMessages = new List<string>();

            //自動回復（エリクサー）
            if (playerStatus.hasAutoRegen && playerStatus.hp > 0 && playerStatus.hp < playerStatus.GetMaxHp())
            {
                playerStatus.HealRegeneration();
                turnEndMessages.Add($"【エリクサー】{playerStatus.GetName()}の傷が塞がっていく！");
            }

            //プレイヤーの毒ダメージ
            if (playerStatus.statusEffect == STATUS_EFFECT.POISON)
            {
                int poisonDamage = (playerStatus.GetMaxHp() * poisonCnt) / POISON_DIVISOR;
                playerStatus.Damage(poisonDamage);
                turnEndMessages.Add(playerStatus.GetName() + "毒のダメージを受けた！");
            }

            //敵の毒ダメージ
            if (enemyStatusEffect == STATUS_EFFECT.POISON && activeEnemy != null && !activeEnemy.IsDead())
            {
                int enemyPoisonDmg = Mathf.Max(1, activeEnemy.GetMaxHp() / POISON_DIVISOR);
                activeEnemy.Damage(enemyPoisonDmg);
                turnEndMessages.Add(activeEnemy.GetName() + $"<color=#00CC00>毒</color>により {enemyPoisonDmg} ダメージを受けた！");
            }

            if (activeEnemy != null && activeEnemy.IsDead())
            {
                EnemyView enemyViewObj = Object.FindAnyObjectByType<EnemyView>();
                if (enemyViewObj != null) enemyViewObj.ChangeAnim(EnemyAnim.DEAD);
                turnEndMessages.Add($"{activeEnemy.GetName()}を倒した！");
            }

            //プレイヤーの呪い処理
            if (playerStatus.statusEffect == STATUS_EFFECT.CURSE && playerStatus.hp > 0)
            {
                statusTurns--;
                if (statusTurns <= 0)
                {
                    playerStatus.UpdateEffectTimers();
                    playerStatus.statusEffect = STATUS_EFFECT.NONE;
                    statusTurns = PLAYER_EFFECT_MAX;
                    turnEndMessages.Add("呪いが発動した！");
                }
                else
                {
                    turnEndMessages.Add($"呪い発動まで あと <color=#CC0000>{statusTurns}</color> ターン");
                }
            }

            //敵の呪い処理
            if (enemyStatusEffect == STATUS_EFFECT.CURSE && activeEnemy != null && !activeEnemy.IsDead())
            {
                enemyCurs--;
                if (enemyCurs <= 0)
                {
                    if (activeEnemy.GetCurrentHp() > 1)
                    {
                        turnEndMessages.Add(activeEnemy.GetName() + "に呪いが発動た！");
                        activeEnemy.Damage(activeEnemy.GetCurrentHp() - 1);
                    }

                    enemyStatusEffect = STATUS_EFFECT.NONE;
                    enemyCurs = ENEMY_CURS_MAX;
                }
                else
                {
                    turnEndMessages.Add($"敵の呪い発動まで あと {enemyCurs} ターン");
                }
            }

            //プレイヤーの沈黙解除処理
            if (playerStatus.statusEffect == STATUS_EFFECT.SILENCE)
            {
                statusTurns--;
                if (statusTurns <= 0)
                {
                    playerStatus.statusEffect = STATUS_EFFECT.NONE;
                    statusTurns = PLAYER_EFFECT_MAX;
                    turnEndMessages.Add("沈黙が解けた");
                }
            }

            if (turnEndMessages.Count > 0)
            {
                battleMessage = string.Join("\n", turnEndMessages);
            }
            else
            {
                NextTurnCommandSelection();
                return;
            }
        }

        if (IsEnterPressed())
        {
            battleMessage = "";

            //ターン終了時効果によって敵が死亡していた場合、次のウェーブやリザルトへ進む
            if (activeEnemy != null && activeEnemy.IsDead())
            {
                CheckEnemyDeath();
                return;
            }
            NextTurnCommandSelection();
        }
    }

    // 次のターンのコマンド選択に進む処理
    private void NextTurnCommandSelection()
    {
        battleMessage = "";
        wasMagicUsedLastTurn = magicUsedThisTurn;
        currentActionIdx = 0;
        battleStep = BATTLE_STEP.COMMAND_SELECTION;
    }

    //プレイヤーが死亡したか
    private void CheckPlayerIsAlive()
    {
        if (playerStatus.IsDead() && !isGameOver)
        {
            if (IsEnterPressed())
            {
                isGameOver = true;
                battleMessage = "プレイヤーは力尽きた...";
                UpdateUI();
                Invoke("LoadGameOverScene", GAMEOVER_DELAY);
                return;
            }
        }
    }


    // 敵が死亡した場合の処理
    private void CheckEnemyDeath()
    {
        //21ターン目に魔王を倒したらゲームクリアに遷移
        if (gameScene.GetTurn() >= GAME_CLEAR_TURN
            && activeEnemy.name == "魔王"
            && activeEnemy.IsDead())
        {
            UpdateUI();
            LoadGameClearScene();
            return;
        }


        if (!isHellQuest) playerStatus.GetExp(activeEnemy.GetExp());

        int maxWaves = isHellQuest ? MAX_WAVES_HELL : MAX_WAVES_NORMAL;

        if (currentWave < maxWaves)
        {
            currentWave++;

            if (isHellQuest)
            {
                SpawnRushEnemy(currentWave - 1);
            }
            else
            {
                SpawnEnemyForTurn();
            }

            actionOrder.Clear();
            currentActionIdx = 0;
            battleMessage = "";
            enemyStatusEffect = STATUS_EFFECT.NONE;
        }
        else
        {
            CompleteQuest();
        }
    }

    // クエスト完了時の処理
    private void CompleteQuest()
    {
        //全Waveクリア 経験値と場所ボーナスを付与
        int rand = REWARD_BASE_VALUE + (Random.Range(0, REWARD_RANDOM_RANGE) - REWARD_OFFSET); //5～25の乱数
        int statusBonus = 0;
        string rewardMsg = "";

        //場所ごとの報酬処理
        switch (location) //現在の場所を保持している変数に変更してください
        {
            case QUEST_LOCATION.PLAINS:
                playerStatus.GetExp(rand);
                rewardMsg = $"追加で経験値を{rand}獲得した";
                break;
            case QUEST_LOCATION.FOREST:
                statusBonus = playerStatus.AddSkillPoint(SKILL_TYPE.Pharmacy, rand);
                rewardMsg = $"追加で薬学を{statusBonus}獲得した";
                break;
            case QUEST_LOCATION.SHRINE:
                statusBonus = playerStatus.AddSkillPoint(SKILL_TYPE.MartialArts, rand);
                rewardMsg = $"追加で武術を{statusBonus}獲得した";
                break;
            case QUEST_LOCATION.CONTINENT:
                statusBonus = playerStatus.AddSkillPoint(SKILL_TYPE.MagicKnowledge, rand);
                rewardMsg = $"追加で魔法知識を{statusBonus}獲得した";
                break;
            case QUEST_LOCATION.CATHEDRAL:
                statusBonus = playerStatus.AddSkillPoint(SKILL_TYPE.Faith, rand);
                rewardMsg = $"追加で信仰を{statusBonus}獲得した";
                break;
            case QUEST_LOCATION.RUINS:
                statusBonus = playerStatus.AddSkillPoint(SKILL_TYPE.Archaeology, rand);
                rewardMsg = $"追加で考古学を{statusBonus}獲得した";
                break;
            case QUEST_LOCATION.HILL:
                statusBonus = playerStatus.AddSkillPoint(SKILL_TYPE.Astrology, rand);
                rewardMsg = $"追加で占星術を{statusBonus}獲得した";
                break;
        }

        // 激ムズクエスト（ヘルクエスト）のボーナス
        if (isHellQuest)
        {
            playerStatus.GetExp(EXTRA_EXP);
        }

        // メッセージと状態更新
        battleMessage = rewardMsg;
        wasMagicUsedLastTurn = magicUsedThisTurn;
        battleStep = BATTLE_STEP.RESULT;

        // 状態異常リセット（紐づけ解消のためplayerStatus側を操作）
        if (playerStatus != null) playerStatus.statusEffect = STATUS_EFFECT.NONE;
        enemyStatusEffect = STATUS_EFFECT.NONE;
    }

    // クエスト結果表示後の処理
    private void DisplayResult()
    {
        if (IsEnterPressed())
        {
            ResetQuestState();
            PhaseResult = PHASE_RESULT.NEXT_TURN;
            isFinished = true;
            playerStatus.FullHeal();

           //元のBGMに戻す
            if (gameScene.defaultBgm != null) soundManager.PlayBGM(gameScene.defaultBgm);
        }
    }


    // 敵をスポーンさせる処理
    private void SpawnEnemyForTurn()
    {
        activeEnemy = EnemySpawner.SpawnEnemyByTurn(gameScene.GetTurn());

        if (enemyView != null && activeEnemy != null)
        {
            // 敵の表示を有効化
            enemyView.gameObject.SetActive(true);

            enemyView.InitEnemyImages(activeEnemy.folderPath);
            playerStatus.isStartDamageUsed = false;
        }

        if (enemyHp != null && activeEnemy != null)
        {
            enemyHp.gameObject.SetActive(true);
            enemyName.gameObject.SetActive(true);
            enemyHp.maxValue = activeEnemy.GetMaxHp();
            enemyHp.value = activeEnemy.GetCurrentHp();
        }
    }

    // 激ムズクエスト（ヘルクエスト）用の敵をスポーンさせる処理
    private void SpawnRushEnemy(int waveIndex)
    {
        activeEnemy = EnemySpawner.SpawnRushEnemy(waveIndex);

        if (enemyView != null && activeEnemy != null)
        {
            //敵の表示を有効化
            enemyView.gameObject.SetActive(true);

            enemyView.InitEnemyImages(activeEnemy.folderPath);
            playerStatus.isStartDamageUsed = false;
        }

        if (enemyHp != null && enemyName != null && activeEnemy != null)
        {
            enemyHp.gameObject.SetActive(true);
            enemyName.gameObject.SetActive(true);
            enemyHp.maxValue = activeEnemy.GetMaxHp();
            enemyHp.value = activeEnemy.GetCurrentHp();
        }
    }

    // キーボード入力の判定
    private bool IsEnterPressed()
    {
        return Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame;
    }
    // キーボード入力の判定
    private bool IsCancelPressed()
    {
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame;
    }

    //キーボード入力によるカーソル操作の処理
    private void HandleCursorInput(ref int cursor, int maxCount)
    {
        if (Keyboard.current == null || maxCount <= 0) return;

        if (Keyboard.current.upArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            cursor = (cursor - 1 + maxCount) % maxCount;
        }
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y < 0f)
        {
            cursor = (cursor + 1) % maxCount;
        }
    }

    // 魔法の説明文を取得する処理
    private string GetMagicDescription(int magicId)
    {
        switch (magicId)
        {
            case 1: return "火を放ち敵を攻撃する魔法。\n威力は小さい。";
            case 2: return "炎で敵を攻撃する魔法。\n威力は中くらい。";
            case 3: return "灼熱で敵を焼き尽くす魔法。\n威力は大きい。";
            case 4: return "目の前の敵を影ごと消し去る魔法。\n威力は絶大。";
            case 5: return "敵の生命力を奪い、\n自身の体力を回復する魔法。\n効果はまあまあ。";
            case 6: return "自分の体力を回復する魔法。\n効果はちょこっと。";
            case 7: return "自分の体力を回復する魔法。\n効果はなかなか。";
            case 8: return "自分の体力を回復する魔法。\n効果はけっこう。";
            case 9: return "自分の状態異常を治療する魔法。\n回復効果はない。";
            case 10: return "自分の体力を回復し、\nさらに状態も治す魔法。\n効果はぼちぼち。";
            case 11: return "毒の針で突き刺す魔法。\n確率で毒状態にする。\n威力と確率は低い。";
            case 12: return "毒の槍で貫く魔法。\n確率で毒状態にする。\n威力と確率は普通。";
            case 13: return "毒の巨戟で穿つ魔法。\n確率で毒状態にする。\n威力と確率は高い。";
            case 14: return "毒液を浴びせる魔法。\n必ず毒状態にする。\n威力はない。";
            case 15: return "冷たい風を吹かせる魔法。\n確率で凍結状態にする。\n威力と確率は低い。";
            case 16: return "凍える嵐を起こす魔法。\n確率で凍結状態にする。\n威力と確率は普通。";
            case 17: return "氷の爆風を作り出す魔法。\n確率で凍結状態にする。\n威力と確率は高い。";
            case 18: return "凍らせる魔法。\n必ず凍結状態にする。\n威力はない。";
            case 19: return "光る球をぶつける魔法。\n確率で閃光状態にする。\n威力と確率は低い。";
            case 20: return "輝く光線で攻撃する魔法。\n確率で閃光状態にする。\n威力と確率は普通。";
            case 21: return "煌めく雨を降らせる魔法。\n確率で閃光状態にする。\n威力と確率は高い。";
            case 22: return "閃光を発生させる魔法。\n必ず閃光状態にする。\n威力はない。";
            case 23: return "闇の爪で攻撃する魔法。\n超低確率で呪い状態にする。\n威力は普通。";
            case 24: return "命を刈り取る剣を呼び出す魔法。\n低確率で呪い状態にする。\n威力は高い。";
            default: return "説明文がありません。";
        }
    }

    //UIの更新処理
    private void UpdateUI()
    {
        bool isDifficultySelection = (battleStep == BATTLE_STEP.DIFFICULTY_SELECTION);

        if (difficultyText != null)
        {
            if (battleStep == BATTLE_STEP.DIFFICULTY_SELECTION)
            {
                if(boardImage != null)
                {
                    boardImage.gameObject.SetActive(true);
                }

                difficultyText.text = GetMenuString(locationMenu, difficultyCursor, LOCATION_MENU_DISPLAY_COUNT, false);
            }
            else
            {
                boardImage.gameObject.SetActive(false);
                difficultyText.text = "";
            }
        }

        if (playerNameText != null && playerStatus != null)
        {
            playerNameText.text = $"{playerStatus.GetName()}\n (HP: {playerStatus.hp}/{playerStatus.GetMaxHp()})";
        }

        if (activeEnemy != null)
        {
            if (!activeEnemy.IsDead())
            {
                enemyHp.value = activeEnemy.GetCurrentHp();
                enemyName.text = activeEnemy.GetName();
                enemyName.text += $"\n (HP: {activeEnemy.GetCurrentHp()}/{activeEnemy.GetMaxHp()})";
            }
            else
            {
                enemyHp.gameObject.SetActive(false);
                enemyName.gameObject.SetActive(false);
            }
        }

        if (playerObject != null && !playerStatus.IsDead()) playerObject.SetActive(!isDifficultySelection);
        if (enemyView != null) enemyView.gameObject.SetActive(!isDifficultySelection);
        playerStatusBer.SetActive(!isDifficultySelection);

        if (battleMessageText != null) battleMessageText.text = battleMessage;

        if (NowWaveText != null && gameScene.GetTurn() != GAME_CLEAR_TURN)
        {
            NowWaveText.gameObject.SetActive(!isDifficultySelection);
            NowWaveText.text = $"<color=#DF0000>【Wave {currentWave} / {MAX_WAVES_NORMAL}】</color>";
        }

        string descString = "";

        if (commandMenuText != null)
        {
            switch (battleStep)
            {
                case BATTLE_STEP.DIFFICULTY_SELECTION:
                    commandMenuText.text = "";
                    break;
                case BATTLE_STEP.COMMAND_SELECTION:
                    commandMenuText.text = GetMenuString(new List<string> { "攻撃", "魔法" }, (int)command, COMMAND_MENU_DISPLAY_COUNT, false);
                    break;
                case BATTLE_STEP.COMMAND_SUB_SELECTION:
                    commandMenuText.text = GetMenuString(subActionMessages, subMenuCursor, subActionMessages.Count, false);
                    break;
                case BATTLE_STEP.MAGIC_SELECTION:
                    {
                        bool shouldScroll = (magicTypeMessages.Count >= MAGIC_MENU_DISPLAY_COUNT);
                        commandMenuText.text = GetMenuString(magicTypeMessages, magicMenuCursor, MAGIC_MENU_DISPLAY_COUNT, shouldScroll);

                        if (magicMenuCursor >= 0 && magicMenuCursor < availableMagics.Count)
                        {
                            var hoverMagic = availableMagics[magicMenuCursor];
                            descString = "【魔法の効果】\n";

                            if (playerStatus.magicKnowledge >= hoverMagic.reqMagicKnowledge)
                            {
                                descString += GetMagicDescription(hoverMagic.id);
                            }
                            else
                            {
                                descString += "詳細不明";
                            }
                        }
                    }
                    break;
                default:
                    commandMenuText.text = "";
                    break;
            }
        }

        if (magicDescText != null)
        {
            magicDescText.text = descString;
        }
    }

    // メニューの文字列を取得する処理
    private string GetMenuString(List<string> items, int cursor, int maxDisplayCount = MAGIC_MENU_DISPLAY_COUNT, bool enableScroll = true)
    {
        string result = "";
        if (items.Count == 0) return result;

        int startIndex = 0;
        int endIndex = items.Count;

        if (enableScroll && items.Count > maxDisplayCount)
        {
            startIndex = cursor - maxDisplayCount / COMMAND_MENU_DISPLAY_COUNT;
            if (startIndex < 0) startIndex = 0;
            if (startIndex > items.Count - maxDisplayCount)
            {
                startIndex = items.Count - maxDisplayCount;
            }
            endIndex = Mathf.Min(startIndex + maxDisplayCount, items.Count);
        }

        int displayedCount = 0;
        for (int i = startIndex; i < endIndex; i++)
        {
            if (i == cursor)
            {
                result += $"<color=#FFE600>{items[i]}</color>\n";
            }
            else
            {
                result += $"<color=#FFFFFF>{items[i]}</color>\n";
            }
            displayedCount++;
        }

        for (int i = displayedCount; i < maxDisplayCount; i++)
        {
            result += "\n";
        }

        return result;
    }

    // クエストの状態をリセットする処理
    public void ResetQuestState()
    {
        battleStep = BATTLE_STEP.DIFFICULTY_SELECTION;
        difficultyCursor = 0;
        command = COMMAND.ATTACK;
        magicUsedThisTurn = false;
        wasMagicUsedLastTurn = false;
        subMenuCursor = 0;
        magicMenuCursor = 0;
        currentWave = 1;

        if (bgImage != null && defaultBgSprite != null)
        {
            bgImage.sprite = defaultBgSprite;
        }

        battleMessage = "";
    }

    // ゲームオーバーシーンをロードする処理
    private void LoadGameOverScene()
    {
        GameSceneManager.LoadScene(GameSceneManager.SCENE_GAMEOVER);
        soundManager.StopSound();
    }
    // ゲームクリアをロードする処理
    private void LoadGameClearScene()
    {
        GameSceneManager.LoadScene(GameSceneManager.SCENE_CLEAR);
        soundManager.StopSound();
    }
}