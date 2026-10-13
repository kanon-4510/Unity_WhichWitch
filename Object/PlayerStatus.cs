using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// ルート定義
public enum PLAYER_ROUTE
{
    None,         // 未選択（3ターン）
    Breakthrough, // 打破
    Salvation,    // 救世
    Truth,        // 真理
    Selfless      // 無欲
}

// 技能の種類
public enum SKILL_TYPE
{
    Pharmacy,       // 薬学
    MartialArts,    // 武術
    MagicKnowledge, // 魔法知識
    Faith,          // 信仰
    Archaeology,    // 考古学
    Astrology       // 占星術
}

// 追加効果の種類
public enum BONUS_TYPE
{
    ItemBonus,    // 薬学：回復効果上昇
    AttackBonus,  // 武術：会心発生率上昇
    MagicBonus,   // 魔法知識：魔法増幅
    DefenseBonus, // 信仰：ダメージ軽減量
    ExpBonus,     // 考古学：獲得経験値上昇
    LuckBonus     // 占星術：回避率上昇
}

public class PlayerStatus : MonoBehaviour
{
    // 定数
    public const int NEED_EXP = 30;         // 必要経験値
    public const int RATE_BASE = 100;       // ステータスアップ確率の基準値
    public const int SKILL_UP_RATE = 70;    // ステータスアップ確率 (70%)

    //スキルボーナスの計算用除数
    private const int PHARMACY_DIVISOR = 7;       // 薬学の効き目係数
    private const int MARTIAL_ARTS_DIVISOR = 5;   // 武術の効き目係数
    private const int FAITH_DIVISOR = 15;         // 信仰の効き目係数
    private const int ARCHAEOLOGY_DIVISOR = 8;    // 考古学の効き目係数
    private const int ASTROLOGY_DIVISOR = 5;      // 占星術の効き目係数

    private const int REGEN_DIVISOR = 10;        // リジェネの回復量計算用（最大HPの1/10）
    private const int DEFAULT_AILMENT_TURNS = 4; // 状態異常のデフォルト継続ターン

    //ルートボーナス
    private const float BONUS_ROUTE = 1.8f;     //通常ルート用
    private const float EX_BONUS_ROUTE = 1.5f;  //隠しルート用

    //職業の解禁条件定数
    //一般魔法使い
    private const int REQ_LV_MAGE = 2;
    private const int REQ_KNOW_MAGE = 10;

    //付加術師
    private const int REQ_LV_ENCHANTER = 5;
    private const int REQ_PHARM_ENCHANTER = 100;

    //魔剣士
    private const int REQ_LV_SWORDSMAN = 5;
    private const int REQ_MARTIAL_SWORDSMAN = 100;

    //魔導師
    private const int REQ_LV_ARCHMAGE = 5;
    private const int REQ_KNOW_ARCHMAGE = 130;

    //聖職者
    private const int REQ_LV_CLERIC = 5;
    private const int REQ_FAITH_CLERIC = 100;

    //呪術師
    private const int REQ_LV_SHAMAN = 5;
    private const int REQ_ARCH_SHAMAN = 100;

    //占い師
    private const int REQ_LV_FORTUNE = 5;
    private const int REQ_ASTRO_FORTUNE = 100;

    //錬金術師
    private const int REQ_LV_ALCHEMIST = 18;
    private const int REQ_PHARM_ALCHEMIST = 170;
    private const int REQ_KNOW_ALCHEMIST = 30;
    private const int REQ_ARCH_ALCHEMIST = 100;

    //聖騎士
    private const int REQ_LV_PALADIN = 18;
    private const int REQ_MARTIAL_PALADIN = 230;
    private const int REQ_FAITH_PALADIN = 70;

    //賢者
    private const int REQ_LV_SAGE = 18;
    private const int REQ_KNOW_SAGE = 200;
    private const int REQ_ARCH_SAGE = 50;
    private const int REQ_ASTRO_SAGE = 50;

    //悪魔祓い
    private const int REQ_LV_EXORCIST = 18;
    private const int REQ_PHARM_EXORCIST = 70;
    private const int REQ_MARTIAL_EXORCIST = 30;
    private const int REQ_FAITH_EXORCIST = 200;

    //死霊術師
    private const int REQ_LV_NECROMANCER = 18;
    private const int REQ_KNOW_NECROMANCER = 100;
    private const int REQ_ARCH_NECROMANCER = 200;

    //予言者
    private const int REQ_LV_PROPHET = 18;
    private const int REQ_PHARM_PROPHET = 30;
    private const int REQ_FAITH_PROPHET = 40;
    private const int REQ_ASTRO_PROPHET = 230;

    //大魔法使い
    private const int REQ_LV_GRAND = 35;
    private const int REQ_PHARM_GRAND = 100;
    private const int REQ_MARTIAL_GRAND = 100;
    private const int REQ_KNOW_GRAND = 100;
    private const int REQ_FAITH_GRAND = 100;
    private const int REQ_ARCH_GRAND = 100;
    private const int REQ_ASTRO_GRAND = 100;

    //職業ボーナス定数
    //一般魔法使い
    private const int JOB_MAGE_HP = 7;
    private const int JOB_MAGE_MAGIC = 5;
    //付加術師
    private const int JOB_ENCHANTER_HP = 14;
    private const int JOB_ENCHANTER_SPEED = 11;
    //魔剣士
    private const int JOB_SWORDSMAN_HP = 6;
    private const int JOB_SWORDSMAN_POWER = 12;
    private const int JOB_SWORDSMAN_SPEED = 7;
    //魔導士
    private const int JOB_ARCHMAGE_MAGIC = 25;
    //聖職者
    private const int JOB_CLERIC_HP = 16;
    private const int JOB_CLERIC_MAGIC = 9;
    //呪術師
    private const int JOB_SHAMAN_HP = 2;
    private const int JOB_SHAMAN_POWER = 8;
    private const int JOB_SHAMAN_MAGIC = 15;
    //占い師
    private const int JOB_FORTUNE_MAGIC = 10;
    private const int JOB_FORTUNE_SPEED = 15;
    //錬金術師
    private const int JOB_ALCHEMIST_HP = 24;
    private const int JOB_ALCHEMIST_SPEED = 21;
    //聖騎士
    private const int JOB_PALADIN_HP = 12;
    private const int JOB_PALADIN_POWER = 20;
    private const int JOB_PALADIN_SPEED = 13;
    //賢者
    private const int JOB_SAGE_MAGIC = 45;
    //悪魔祓い
    private const int JOB_EXORCIST_HP = 27;
    private const int JOB_EXORCIST_MAGIC = 18;
    //死霊術師
    private const int JOB_NECROMANCER_HP = 7;
    private const int JOB_NECROMANCER_POWER = 13;
    private const int JOB_NECROMANCER_MAGIC = 25;
    //預言者
    private const int JOB_PROPHET_MAGIC = 20;
    private const int JOB_PROPHET_SPEED = 25;
    //大魔法使い
    private const int JOB_GRAND_WIZARD_HP = 20;
    private const int JOB_GRAND_WIZARD_POWER = 10;
    private const int JOB_GRAND_WIZARD_MAGIC = 25;
    private const int JOB_GRAND_WIZARD_SPEED = 15;

    [Header("基本ステータス")]
    public string playerName = "ルピナス";
    public string job = "見習い魔女";
    public int level = 1;
    public int exp = 0;
    public int maxHp = 10;
    public int hp = 10;
    public int power = 10;
    public int magic = 10;
    public int speed = 10;
    public int heal = 2; // 基本回復量

    [Header("技能ステータス")]
    public int pharmacy = 0;       // 薬学
    public int martialArts = 0;    // 武術
    public int magicKnowledge = 0; // 魔法知識
    public int faith = 0;          // 信仰
    public int archaeology = 0;    // 考古学
    public int astrology = 0;      // 占星術

    [Header("状態管理")]
    public PLAYER_ROUTE currentRoute = PLAYER_ROUTE.None;
    public bool hasChallengedHellQuest = false;

    [Header("状態異常")]
    public STATUS_EFFECT statusEffect = STATUS_EFFECT.NONE; // 現在の状態異常
    public int ailmentTurns = 0;                               // 状態異常の残りターン数

    [Header("UI参照")]
    [SerializeField] private Slider playerHp;   // HPバー表示用
    [SerializeField] private PlayerUIMover playerUIMover; //PlayerImageを参照する

    //外部参照
    SoundManager soundManager = SoundManager.Instance;

    // ユニークスキルフラグ
    public bool hasStartDamage = false;  // 開幕固定ダメ
    public bool hasFirstHitNull = false; // 初撃無効
    public bool hasMagicUnlock = false;  // 魔法制限なし
    public bool hasCritBoost = false;    // クリティカル倍率上昇
    public bool hasMagicToAttack = false;// 攻撃時に魔力加算
    public bool hasAutoRegen = false;    // リジェネ
    public bool hasGuts = false;         // 根性（HP1で耐える）

    public bool isStartDamageUsed = false; // 開幕固定ダメを消費したか
    public bool isFirstHitUsed = false;  // 初撃無効を消費したか
    public bool isGutsUsed = false;      // 根性を消費したか

    // 職業リスト
    private List<JobData> jobList = new List<JobData>();

    // プロパティ
    public PLAYER_ROUTE CurrentRoute { get => currentRoute; set => currentRoute = value; }
    public int CurrentHp => hp;
    public int MaxHp => GetMaxHp();

    // 職業ごとのボーナス構造体
    public struct JOB_BONUS
    {
        public int hp;
        public int power;
        public int magic;
        public int speed;
    }

    private void Awake()
    {
        InitJob();
        hp = GetMaxHp();
        UpdateHpBar();
    }

    public void Init()
    {
        hp = GetMaxHp();
        currentRoute = PLAYER_ROUTE.None;

        playerHp.maxValue = GetMaxHp(); // 最大HPを設定
        playerHp.value = hp;            // 現在のHPをセット

    }

    public void Update()
    {
    }

    public string GetName()
    {
        return this.playerName;
    }

    //攻撃の処理
    public int Attack()
    {
        int powerWithJob = GetAttack();

        //playeを動かす
        if (playerUIMover != null)
        {
            playerUIMover.StartAttackAnimation();
        }

        soundManager.PlayAttack();

        return powerWithJob;
    }

    //攻撃の威力を渡す
    public int GetAttack()
    {
        int baseAtk = this.power;
        int jobBonus = GetJobBonus().power;
        int powerWithJob = baseAtk + jobBonus;

        return powerWithJob;
    }

    //魔法攻撃処理
    public int MagicAttack()
    {
        int magicWithJob = GetMagic();

        //playeを動かす
        if (playerUIMover != null)
        {
            playerUIMover.StartAttackAnimation();
        }

        soundManager.PlayAttack();

        return magicWithJob;
    }


    //魔法の威力を渡す
    public int GetMagic()
    {
        int baseMag = this.magic;
        int jobBonus = GetJobBonus().magic;
        int magicWithJob = baseMag + jobBonus;

        return SkillBonus(BONUS_TYPE.MagicBonus, magicWithJob);
    }
    // --- 回復・ダメージ処理 ---
    public void Heal(int amount)
    {
        hp += SkillBonus(BONUS_TYPE.ItemBonus, amount);
        if (hp > GetMaxHp()) hp = GetMaxHp();

        UpdateHpBar();
    }

    //リジェネ
    public void HealRegeneration()
    {
        int regen = GetMaxHp() / REGEN_DIVISOR;
        hp = Mathf.Min(hp + regen, GetMaxHp());
        UpdateHpBar();
    }

    //HPを最大回復させる
    public void FullHeal()
    {
        hp = GetMaxHp();

        UpdateHpBar();
    }

    //ダメージ処理
    public void Damage(int damageAmount)
    {
        int finalDamage = SkillBonus(BONUS_TYPE.DefenseBonus, damageAmount);
        hp -= finalDamage;

        if (playerUIMover != null)
        {
            playerUIMover.StartDamageAnimation();
        }

        if (hp <= 0)
        {
            // 食いしばり判定
            if (hasGuts && !isGutsUsed)
            {
                hp = 1;
            }
            else
            {
                hp = 0;
            }
        }

        soundManager.PlayDamage();

        UpdateHpBar();
    }

    //HPバーを更新
    public void UpdateHpBar()
    {
        if (playerHp != null)
        {
            playerHp.maxValue = GetMaxHp();
            playerHp.value = hp;
        }
    }

    public bool IsDead()
    {
        return hp <= 0;
    }


    // 状態異常を付与する
    public void InflictAilment(STATUS_EFFECT ailment, int turns = DEFAULT_AILMENT_TURNS)
    {
        statusEffect = ailment;
        ailmentTurns = turns;
    }

    // 状態異常を解除・治療する
    public void CureAilment()
    {
        if (statusEffect != STATUS_EFFECT.NONE)
        {
            Debug.Log($"<color=purple>プレイヤーの {statusEffect} が治った！</color>");
            statusEffect = STATUS_EFFECT.NONE;
            ailmentTurns = 0;
        }
    }

    // ターン経過時の状態異常カウントダウン・処理
    public void UpdateEffectTimers()
    {
        if (statusEffect == STATUS_EFFECT.NONE) return;

        // 呪いや沈黙などのターン数減少
        if (statusEffect == STATUS_EFFECT.CURSE || statusEffect == STATUS_EFFECT.SILENCE)
        {
            ailmentTurns--;
            if (ailmentTurns <= 0)
            {
                if (statusEffect == STATUS_EFFECT.CURSE)
                {
                    hp = 1;
                    UpdateHpBar();
                }
                else if (statusEffect == STATUS_EFFECT.SILENCE)
                {
                    Debug.Log("沈黙が解けた！");
                }
                CureAilment();
            }
        }
    }

    //ステータス参照
    public int GetMaxHp()
    {
        return this.maxHp + GetJobBonus().hp;
    }

    public int GetSpeed()
    {
        return this.speed + GetJobBonus().speed;
    }

    //経験値・レベルアップ
    public void GetExp(int expAmount)
    {
        int finalExp = SkillBonus(BONUS_TYPE.ExpBonus, expAmount);
        exp += finalExp;

        while (exp >= NEED_EXP)
        {
            exp -= NEED_EXP;
            LevelUp();
        }
    }

    //レベルアップの処理
    public void LevelUp()
    {
        level++;
        maxHp++;
        hp = GetMaxHp();

        // 70% の確率で各ステータス上昇
        if (Random.Range(0, RATE_BASE) < SKILL_UP_RATE) power += 1;
        if (Random.Range(0, RATE_BASE) < SKILL_UP_RATE) magic += 1;
        if (Random.Range(0, RATE_BASE) < SKILL_UP_RATE) speed += 1;

        UpdateHpBar();
    }

    // 既存のステータス変数（プロパティ）の定義...
    public int Pharmacy { get; set; }
    public int MartialArts { get; set; }
    public int MagicKnowledge { get; set; }
    public int Faith { get; set; }
    public int Archaeology { get; set; }
    public int Astrology { get; set; }

    //技能ポイント獲得（ルート倍率補正付き）
    public int AddSkillPoint(SKILL_TYPE type, int baseAmount)
    {
        float multiplier = 1.0f;

        switch (currentRoute)
        {
            case PLAYER_ROUTE.Breakthrough: // 打破：武術と考古学
                if (type == SKILL_TYPE.MartialArts || type == SKILL_TYPE.Archaeology) multiplier = BONUS_ROUTE;
                break;
            case PLAYER_ROUTE.Salvation:    // 救世：信仰と薬学
                if (type == SKILL_TYPE.Faith || type == SKILL_TYPE.Pharmacy) multiplier = BONUS_ROUTE;
                break;
            case PLAYER_ROUTE.Truth:        // 真理：魔法知識と占星術
                if (type == SKILL_TYPE.MagicKnowledge || type == SKILL_TYPE.Astrology) multiplier = BONUS_ROUTE;
                break;
            case PLAYER_ROUTE.Selfless:     // 無欲：すべてに少しボーナス
                multiplier = EX_BONUS_ROUTE;
                break;
        }

        int finalAmount = Mathf.FloorToInt(baseAmount * multiplier);

        switch (type)
        {
            case SKILL_TYPE.MartialArts: martialArts += finalAmount; break;
            case SKILL_TYPE.Archaeology: archaeology += finalAmount; break;
            case SKILL_TYPE.Faith: faith += finalAmount; break;
            case SKILL_TYPE.Pharmacy: pharmacy += finalAmount; break;
            case SKILL_TYPE.MagicKnowledge: magicKnowledge += finalAmount; break;
            case SKILL_TYPE.Astrology: astrology += finalAmount; break;
        }

        return finalAmount;
    }

    // --- 技能ボーナス計算 ---
    public int SkillBonus(BONUS_TYPE type, int baseValue)
    {
        switch (type)
        {
            case BONUS_TYPE.ItemBonus:
                return baseValue + (pharmacy / PHARMACY_DIVISOR);
            case BONUS_TYPE.AttackBonus:
                return baseValue + (martialArts / MARTIAL_ARTS_DIVISOR);
            case BONUS_TYPE.MagicBonus:
                return baseValue;
            case BONUS_TYPE.DefenseBonus:
                int finalDamage = baseValue - (faith / FAITH_DIVISOR);
                return Mathf.Max(1, finalDamage); // 最低1ダメージ
            case BONUS_TYPE.ExpBonus:
                return baseValue + (archaeology / ARCHAEOLOGY_DIVISOR);
            case BONUS_TYPE.LuckBonus:
                return baseValue + (astrology / ASTROLOGY_DIVISOR);
        }
        return baseValue;
    }

    // --- 転職チェック ---
    public bool JobCheck(JobData jobData)
    {
        if (this.level < jobData.status.reqLevel_) return false;
        if (this.power < jobData.status.reqPower_) return false;
        if (this.magic < jobData.status.reqMagic_) return false;

        if (this.pharmacy < jobData.status.reqPharmacy_) return false;
        if (this.martialArts < jobData.status.reqMartialArts_) return false;
        if (this.magicKnowledge < jobData.status.reqMagicKnowledge_) return false;
        if (this.faith < jobData.status.reqFaith_) return false;
        if (this.archaeology < jobData.status.reqArchaeology_) return false;
        if (this.astrology < jobData.status.reqAstrology_) return false;

        return true;
    }

    //職業ボーナス & ユニークスキル設定
    public JOB_BONUS GetJobBonus(string targetJob = null)
    {
        //引数が指定されていない場合は自分の現在の職業を使う
        if (string.IsNullOrEmpty(targetJob))
        {
            targetJob = this.job;
        }

        JOB_BONUS bonus = new JOB_BONUS();

        //自分の職業のステータス計算時のみフラグを更新する（UIプレビュー時の事故防止）
        bool isCurrentJob = (targetJob == this.job);

        if (isCurrentJob)
        {
            hasStartDamage = false;
            hasFirstHitNull = false;
            hasMagicUnlock = false;
            hasCritBoost = false;
            hasMagicToAttack = false;
            hasAutoRegen = false;
            hasGuts = false;
        }

        switch (targetJob)
        {
            case "一般魔法使い": bonus.hp = JOB_MAGE_HP; bonus.magic = JOB_MAGE_MAGIC; break;
            case "付加術師": bonus.hp = JOB_ENCHANTER_HP; bonus.speed = JOB_ENCHANTER_SPEED; break;
            case "魔剣士": bonus.hp = JOB_SWORDSMAN_HP; bonus.power = JOB_SWORDSMAN_POWER; bonus.speed = JOB_SWORDSMAN_SPEED; break;
            case "魔導師": bonus.magic = JOB_ARCHMAGE_MAGIC; break;
            case "聖職者": bonus.hp = JOB_CLERIC_HP; bonus.magic = JOB_CLERIC_MAGIC; break;
            case "呪術師": bonus.hp = JOB_SHAMAN_HP; bonus.power = JOB_SHAMAN_POWER; bonus.magic = JOB_SHAMAN_MAGIC; break;
            case "占い師": bonus.magic = JOB_FORTUNE_MAGIC; bonus.speed = JOB_FORTUNE_SPEED; break;

            case "錬金術師":
                bonus.hp = JOB_ALCHEMIST_HP; bonus.speed = JOB_ALCHEMIST_SPEED;
                if (isCurrentJob) hasAutoRegen = true;
                break;

            case "聖騎士":
                bonus.hp = JOB_PALADIN_HP; bonus.power = JOB_PALADIN_POWER; bonus.speed = JOB_PALADIN_SPEED;
                if (isCurrentJob) hasCritBoost = true;
                break;

            case "賢者":
                bonus.magic = JOB_SAGE_MAGIC;
                if (isCurrentJob) hasMagicToAttack = true;
                break;

            case "悪魔祓い":
                bonus.hp = JOB_EXORCIST_HP; bonus.magic = JOB_EXORCIST_MAGIC;
                if (isCurrentJob) hasFirstHitNull = true;
                break;

            case "死霊術師":
                bonus.hp = JOB_NECROMANCER_HP; bonus.power = JOB_NECROMANCER_POWER; bonus.magic = JOB_NECROMANCER_MAGIC;
                if (isCurrentJob) hasStartDamage = true;
                break;

            case "予言者":
                bonus.magic = JOB_PROPHET_MAGIC; bonus.speed = JOB_PROPHET_SPEED;
                if (isCurrentJob) hasGuts = true;
                break;

            case "大魔法使い":
                bonus.hp = JOB_GRAND_WIZARD_HP; bonus.power = JOB_GRAND_WIZARD_POWER; bonus.magic = JOB_GRAND_WIZARD_MAGIC; bonus.speed = JOB_GRAND_WIZARD_SPEED;
                if (isCurrentJob) hasMagicUnlock = true;
                break;
        }

        return bonus;
    }

    //職業初期化
    public void InitJob()
    {
        if (jobList.Count > 0) return;

        jobList.Add(new JobData("一般魔法使い", REQ_LV_MAGE, 0, 0, 0, 0, REQ_KNOW_MAGE, 0, 0, 0));
        jobList.Add(new JobData("付加術師", REQ_LV_ENCHANTER, 0, 0, REQ_PHARM_ENCHANTER, 0, 0, 0, 0, 0));
        jobList.Add(new JobData("魔剣士", REQ_LV_SWORDSMAN, 0, 0, 0, REQ_MARTIAL_SWORDSMAN, 0, 0, 0, 0));
        jobList.Add(new JobData("魔導師", REQ_LV_ARCHMAGE, 0, 0, 0, 0, REQ_KNOW_ARCHMAGE, 0, 0, 0));
        jobList.Add(new JobData("聖職者", REQ_LV_CLERIC, 0, 0, 0, 0, 0, REQ_FAITH_CLERIC, 0, 0));
        jobList.Add(new JobData("呪術師", REQ_LV_SHAMAN, 0, 0, 0, 0, 0, 0, REQ_ARCH_SHAMAN, 0));
        jobList.Add(new JobData("占い師", REQ_LV_FORTUNE, 0, 0, 0, 0, 0, 0, 0, REQ_ASTRO_FORTUNE));

        jobList.Add(new JobData("錬金術師", REQ_LV_ALCHEMIST, 0, 0, REQ_PHARM_ALCHEMIST, 0, REQ_KNOW_ALCHEMIST, 0, REQ_ARCH_ALCHEMIST, 0));
        jobList.Add(new JobData("聖騎士", REQ_LV_PALADIN, 0, 0, 0, REQ_MARTIAL_PALADIN, 0, REQ_FAITH_PALADIN, 0, 0));
        jobList.Add(new JobData("賢者", REQ_LV_SAGE, 0, 0, 0, 0, REQ_KNOW_SAGE, 0, REQ_ARCH_SAGE, REQ_ASTRO_SAGE));
        jobList.Add(new JobData("悪魔祓い", REQ_LV_EXORCIST, 0, 0, REQ_PHARM_EXORCIST, REQ_MARTIAL_EXORCIST, 0, REQ_FAITH_EXORCIST, 0, 0));
        jobList.Add(new JobData("死霊術師", REQ_LV_NECROMANCER, 0, 0, 0, 0, REQ_KNOW_NECROMANCER, 0, REQ_ARCH_NECROMANCER, 0));
        jobList.Add(new JobData("予言者", REQ_LV_PROPHET, 0, 0, REQ_PHARM_PROPHET, 0, 0, REQ_FAITH_PROPHET, 0, REQ_ASTRO_PROPHET));

        jobList.Add(new JobData("大魔法使い", REQ_LV_GRAND, 0, 0, REQ_PHARM_GRAND, REQ_MARTIAL_GRAND, REQ_KNOW_GRAND, REQ_FAITH_GRAND, REQ_ARCH_GRAND, REQ_ASTRO_GRAND));

        isStartDamageUsed = false;
        isFirstHitUsed = false;
        isGutsUsed = false;
    }

    //職業を設定するメソッド
    public void SetJob(string jobName)
    {
        job = jobName; //実際の職業変数に新しい職業名を代入する

        //転職に伴い、現在HPが新しい最大HPを超えないように調整
        hp = Mathf.Min(hp, GetMaxHp());

        UpdateHpBar();
    }
    public string GetJobName()
    {
        return job; // または現在保持している職業名のメンバ変数
    }

    //ジョブリストを渡す
    public List<JobData> GetJobList()
    {
        return jobList;
    }
}