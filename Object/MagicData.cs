using System;
using System.Collections.Generic;

public enum MAGIC_TYPE
{
    ATTACK, // 攻撃
    HEAL,   // 回復
    DEBUFF, // 弱化
    MAX
}

public enum STATUS_EFFECT
{
    NONE,    // なし
    POISON,  // 毒
    FREEZE,  // 凍結
    FLASH,   // 閃光
    CURSE,   // 呪い
    SILENCE, // 沈黙
    MAX
}

[Serializable]
public struct MagicData
{
    public int id;                  // 魔法のID
    public string name;             // 魔法の名前
    public MAGIC_TYPE type;         // 魔法の種類
    public float powerMultiplier;   // 魔法の威力倍率
    public STATUS_EFFECT ailment;   // 状態異常の種類
    public int ailmentChance;       // 状態異常の付与確率（％）
    public int reqMagicKnowledge;   // 魔法知識の必要値
    public bool isDrain;            // ドレイン魔法かどうか
    public bool curesStatus;        // 状態異常を治す魔法かどうか

    public MagicData(int id, string name, MAGIC_TYPE type, float powerMultiplier, STATUS_EFFECT ailment, int ailmentChance, int reqMagicKnowledge, bool isDrain = false, bool curesStatus = false)
    {
        this.id = id;
        this.name = name;
        this.type = type;
        this.powerMultiplier = powerMultiplier;
        this.ailment = ailment;
        this.ailmentChance = ailmentChance;
        this.reqMagicKnowledge = reqMagicKnowledge;
        this.isDrain = isDrain;
        this.curesStatus = curesStatus;
    }
}

public static class MagicDatabase
{
    public static readonly MagicData[] LIST = new MagicData[]
    {
        // 【攻撃魔法】
        new MagicData(1, "ファイア",        MAGIC_TYPE.ATTACK, 1.5f, STATUS_EFFECT.NONE, 0, 0),
        new MagicData(2, "フレイム",        MAGIC_TYPE.ATTACK, 2.0f, STATUS_EFFECT.NONE, 0, 80),
        new MagicData(3, "プロミネンス",    MAGIC_TYPE.ATTACK, 4.0f, STATUS_EFFECT.NONE, 0, 330),
        new MagicData(4, "ビッグバン",      MAGIC_TYPE.ATTACK, 10.0f, STATUS_EFFECT.NONE, 0, 500),
        // 【回復魔法】
        new MagicData(5, "ドレイン",        MAGIC_TYPE.HEAL, 0.7f, STATUS_EFFECT.NONE, 0, 190, true, false),
        new MagicData(6, "ヒール",          MAGIC_TYPE.HEAL, 0.3f, STATUS_EFFECT.NONE, 0, 0),
        new MagicData(7, "ハイヒール",      MAGIC_TYPE.HEAL, 0.5f, STATUS_EFFECT.NONE, 0, 80),
        new MagicData(8, "メガヒール",      MAGIC_TYPE.HEAL, 1.0f, STATUS_EFFECT.NONE, 0, 350),
        new MagicData(9, "キュア",          MAGIC_TYPE.HEAL, 0.0f, STATUS_EFFECT.NONE, 0, 50, false, true),
        new MagicData(10, "レスキュー",     MAGIC_TYPE.HEAL, 0.5f, STATUS_EFFECT.NONE, 0, 240, false, true),
        // 【状態異常攻撃】
        new MagicData(11, "アシッドニードル", MAGIC_TYPE.DEBUFF, 1.0f, STATUS_EFFECT.POISON, 30, 30),
        new MagicData(12, "ヴェノムスピア",   MAGIC_TYPE.DEBUFF, 2.5f, STATUS_EFFECT.POISON, 50, 100),
        new MagicData(13, "トキシックランス", MAGIC_TYPE.DEBUFF, 4.0f, STATUS_EFFECT.POISON, 70, 290),
        new MagicData(14, "ポイズン",         MAGIC_TYPE.DEBUFF, 0.0f, STATUS_EFFECT.POISON, 100, 160),
        new MagicData(15, "コールドブレス",   MAGIC_TYPE.DEBUFF, 1.0f, STATUS_EFFECT.FREEZE, 30, 30),
        new MagicData(16, "スノーテンペスト", MAGIC_TYPE.DEBUFF, 2.5f, STATUS_EFFECT.FREEZE, 50, 100),
        new MagicData(17, "アイシクルノヴァ", MAGIC_TYPE.DEBUFF, 4.0f, STATUS_EFFECT.FREEZE, 70, 290),
        new MagicData(18, "フリーズ",         MAGIC_TYPE.DEBUFF, 0.0f, STATUS_EFFECT.FREEZE, 100, 160),
        new MagicData(19, "フォトンシュート", MAGIC_TYPE.DEBUFF, 1.0f, STATUS_EFFECT.FLASH, 30, 30),
        new MagicData(20, "プリズムレーザー", MAGIC_TYPE.DEBUFF, 2.5f, STATUS_EFFECT.FLASH, 50, 100),
        new MagicData(21, "ミラージュレイン", MAGIC_TYPE.DEBUFF, 4.0f, STATUS_EFFECT.FLASH, 70, 290),
        new MagicData(22, "フラッシュ",       MAGIC_TYPE.DEBUFF, 0.0f, STATUS_EFFECT.FLASH, 100, 160),
        new MagicData(23, "リーサルクロー",   MAGIC_TYPE.DEBUFF, 0.0f, STATUS_EFFECT.CURSE, 5, 130),
        new MagicData(24, "フェイタルソード", MAGIC_TYPE.DEBUFF, 4.5f, STATUS_EFFECT.CURSE, 10, 390)
    };
}