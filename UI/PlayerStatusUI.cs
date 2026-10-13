using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerStatusUI : MonoBehaviour
{
    //ステータスの表示方法
    public enum DisplayMode
    {
        FULL_STATUS,    //メインメニューで表示
        QUEST_STATUS,   //クエストで表示
    }

    [Header("表示モード選択")]
    [SerializeField] private DisplayMode displayMode = DisplayMode.FULL_STATUS;

    [Header("参照")]
    [SerializeField] private PlayerStatus playerStatus;

    [Header("GameScene表示テキスト")]
    [SerializeField] private TMP_Text allStatusText;

    [Header("ステータス表示テキスト")]
    [SerializeField] private TMP_Text statusText;       //基礎ステータス
    [SerializeField] private TMP_Text skillText;        //技能
    [SerializeField] private TMP_Text jobToEffectText;  //ジョブと現在の状態異常

    //定数
    //技能ボーナス計算用の係数
    private const int PHARMACY_DIVISOR = 10;     // 薬学の換算係数
    private const int MARTIAL_ARTS_DIVISOR = 5;   // 武術の換算係数
    private const int FAITH_DIVISOR = 15;         // 信仰の換算係数
    private const int ARCHAEOLOGY_DIVISOR = 10;   // 考古学の換算係数
    private const int ASTROLOGY_DIVISOR = 5;      // 占星術の換算係数

    private void Start()
    {
        if (playerStatus == null)
        {
            playerStatus = FindFirstObjectByType<PlayerStatus>();
        }
    }

    private void Update()
    {
        if (playerStatus == null) return;

        // 職業ボーナス値の取得
        int hpBonus = playerStatus.GetJobBonus().hp;
        int powerBonus = playerStatus.GetJobBonus().power;
        int magicBonus = playerStatus.GetJobBonus().magic;
        int speedBonus = playerStatus.GetJobBonus().speed;

        // ボーナスがある場合のみカッコと緑色で文字列化する
        string hpEffect = (hpBonus > 0) ? $"<color=#00FF00>({hpBonus})</color>" : "";
        string powerEffect = (powerBonus > 0) ? $"<color=#00FF00>({powerBonus})</color>" : "";
        string magicEffect = (magicBonus > 0) ? $"<color=#00FF00>({magicBonus})</color>" : "";
        string speedEffect = (speedBonus > 0) ? $"<color=#00FF00>({speedBonus})</color>" : "";

        // 各技能の追加効果
        int healBonus = playerStatus.pharmacy / PHARMACY_DIVISOR;      // 薬学：回復効果
        int critBonus = playerStatus.martialArts / MARTIAL_ARTS_DIVISOR;   // 武術：会心発生率
        int defBonus = playerStatus.faith / FAITH_DIVISOR;         // 信仰：軽減量
        int expBonus = playerStatus.archaeology / ARCHAEOLOGY_DIVISOR;   // 考古：経験値
        int luckBonus = playerStatus.astrology / ASTROLOGY_DIVISOR;      // 占星：回避率

        // ボーナス表記の作成
        string pharmacyEffect = (healBonus > 0) ?   $"<color=#00FF00>(治癒力+{healBonus})</color>" : "";
        string martialEffect = (critBonus > 0) ?    $"<color=#00FF00>(会心率+{critBonus})</color>" : "";
        string faithEffect = (defBonus > 0) ?       $"<color=#00FF00>(軽減量+{defBonus})</color>" : "";
        string archEffect = (expBonus > 0) ?        $"<color=#00FF00>(経験値+{expBonus})</color>" : "";
        string astroEffect = (luckBonus > 0) ?      $"<color=#00FF00>(回避率+{luckBonus})</color>" : "";

        if (displayMode == DisplayMode.FULL_STATUS)
        {
            // 【GameScene用】これまで通りの1つにまとめた表示
            if (allStatusText != null)
            {
                // プレイヤーの情報を表示する
                string display = "";
                display += $"名前：{playerStatus.playerName}\n";
                display += $"職業：{playerStatus.job}\n";
                display += $"レベル：{playerStatus.level}\n";
                display += $"体力：{playerStatus.GetMaxHp()}{hpEffect}\n";
                display += $"筋力：{playerStatus.GetAttack()}{powerEffect}\n";
                display += $"魔力：{playerStatus.GetMagic()}{magicEffect}\n";
                display += $"速力：{playerStatus.GetSpeed()}{speedEffect}\n";
                display += "\n";
                display += $"薬学：{playerStatus.pharmacy}{pharmacyEffect}\n";
                display += $"武術：{playerStatus.martialArts}{martialEffect}\n";
                display += $"魔法：{playerStatus.magicKnowledge}\n";
                display += $"信仰：{playerStatus.faith}{faithEffect}\n";
                display += $"考古：{playerStatus.archaeology}{archEffect}\n";
                display += $"占星：{playerStatus.astrology}{astroEffect}";

                allStatusText.text = display;
            }
        }
        else
        {
            //レベルと基礎ステータス
            if (statusText != null)
            {
                string baseDisplay = "";
                baseDisplay += $"レベル　{playerStatus.level}\n";
                baseDisplay += $"　筋力　{playerStatus.GetAttack()}\n";
                baseDisplay += $"　魔力　{playerStatus.GetMagic()}\n";
                baseDisplay += $"　速力　{playerStatus.GetSpeed()}";

                statusText.text = baseDisplay;
            }

            //技能
            if (skillText != null)
            {
                string skillDisplay = "";
                skillDisplay += $"治癒力：+　  <pos=110>{healBonus}<pos=130>\n";
                skillDisplay += $"会心率：　  <pos=110>{critBonus} % <pos=130>\n";
                skillDisplay += $"魔法学：　  <pos=110>{playerStatus.magicKnowledge}<pos=130>\n";
                skillDisplay += $"軽減量：　  <pos=110>{defBonus} <pos=130>\n";
                skillDisplay += $"経験値：+　  <pos=110>{expBonus} <pos=130>\n";
                skillDisplay += $"回避率：　  <pos=110>{luckBonus} % <pos=130>\n";

                skillText.text = skillDisplay;
            }

            //職業と現在の状態異常
            if (jobToEffectText != null)
            { 
                string jobDisplay = $"職業：{playerStatus.job}\n";


                string ailmentName = "なし";
                string colorHex = "#FFFFFF";

                //QuestPhase側のstatusEffect または playerStatus.statusEffect を参照
                STATUS_EFFECT currentEffect = playerStatus.statusEffect != STATUS_EFFECT.NONE ? playerStatus.statusEffect : playerStatus.statusEffect;

                if (currentEffect != STATUS_EFFECT.NONE)
                {
                    switch (currentEffect)
                    {
                        case STATUS_EFFECT.POISON:
                            ailmentName = "毒";
                            colorHex = "#00CC00"; //緑
                            break;
                        case STATUS_EFFECT.FREEZE:
                            ailmentName = "凍結";
                            colorHex = "#00CCCC"; //水色
                            break;
                        case STATUS_EFFECT.SILENCE:
                            ailmentName = "沈黙";
                            colorHex = "#DD0000"; //灰色
                            break;
                        case STATUS_EFFECT.CURSE:
                            ailmentName = "呪い";
                            colorHex = "#CC00CC"; //赤系
                            break;
                        case STATUS_EFFECT.FLASH:
                            ailmentName = "閃光";
                            colorHex = "#FFFF00"; //黄色
                            break;
                    }
                }

                string ailmentDisplay = $"状態：<color={colorHex}>{ailmentName}</color>";
                jobToEffectText.text = jobDisplay + ailmentDisplay;
            }
        }
    }
}