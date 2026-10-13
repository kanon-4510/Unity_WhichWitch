using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using static JobData;
using static PhaseBase;
using static PlayerStatus;

public class JobChangePhase : PhaseBase
{
    [Header("目次（リスト用 UI）")]
    [SerializeField] private TMP_Text leftListText;     // 目次：左ページ
    [SerializeField] private TMP_Text rightListText;    // 目次：右ページ

    [Header("詳細表示用 UI")]
    [SerializeField] private TMP_Text leftDetailText;   // 詳細：左ページ（職業名・説明）
    [SerializeField] private TMP_Text leftDetailSkillText;// 詳細：左ページ（ユニークスキル）
    [SerializeField] private TMP_Text rightDetailText;  // 詳細：右ページ（必要条件）
    [SerializeField] private TMP_Text rightBonusText;  // 詳細：右ページ（ボーナス）
    [SerializeField] private TMP_Text WarnigText;       // 警告テキスト

    //外部参照
    private PlayerStatus playerStatus;

    //定数
    private const int WARNING_COUNT = 120;
    private const int ITEMS_PER_PAGE = 7; // 1ページ（左右の片側）あたりの表示最大数
    private const int MASTER_JOB_INDEX = 13; // 全技能条件などの特殊ジョブインデックス
    
    private const string POS_LABEL = "<pos=80>";
    private const string POS_COLON = "<pos=120>";
    private const string POS_LAVEL_COLON = "<pos=120>";
    private const string POS_VALUE = "<pos=140>";

    //変数
    private int selectedIndex_ = 0;
    private int timer = 0;

    //フラグ
    private bool isShowingDetails_ = false; //詳細画面の表示、非表示
    private bool isWarning = false; //詳細画面の表示、非表示


    //初期化処理
    public void Initialize(PlayerStatus playerStatus, GameScene gameScene)
    {
        this.playerStatus = playerStatus;

        selectedIndex_ = 0;
        isShowingDetails_ = false;

        playerStatus.InitJob();
        UpdateUI();
    }

    //更新処理
    public override void OnUpdatePhase()
    {
        if (isFinished) return;
        if (Keyboard.current == null) return;

        if (!isShowingDetails_)
        {
            ProcessJobListSelection();
        }
        else
        {
            ProcessDetailsListSelection();
        }

        if (isWarning)
        {
            timer++;
            if (timer > WARNING_COUNT)
            {
                isWarning = false;
                WarnigText.gameObject.SetActive(false);
                timer = 0;
            }
        }

        UpdateUI();
    }

    //職業リストの選択処理
    private void ProcessJobListSelection()
    {
        var keyboard = Keyboard.current;
        var jobList = playerStatus.GetJobList();
        if (jobList == null || jobList.Count == 0) return;

        int count = jobList.Count;

        //上キー又は、マウスカーソル
        if (Keyboard.current.upArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            selectedIndex_ = (selectedIndex_ - 1 + count) % count;
        }
        //下キー又は、マウスカーソル
        else if (Keyboard.current.downArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y < 0f)
        {
            selectedIndex_ = (selectedIndex_ + 1) % count;
        }
        //右キー
        else if (keyboard.rightArrowKey.wasPressedThisFrame)
        {
            selectedIndex_ = (selectedIndex_ + ITEMS_PER_PAGE) % count;
        }
        //左キー
        else if (keyboard.leftArrowKey.wasPressedThisFrame)
        {
            selectedIndex_ = (selectedIndex_ - ITEMS_PER_PAGE + count) % count;
        }
        //Enter又は、マウス左ボタン
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            isShowingDetails_ = true;
        }
        //TAB、ESC又は、マウス右ボタン
        else if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame)
        {
            PhaseResult = PHASE_RESULT.CANCEL;
            isFinished = true;
        }
    }

    //詳細リストの選択処理
    private void ProcessDetailsListSelection()
    {
        var keyboard = Keyboard.current;
        var jobList = playerStatus.GetJobList();
        if (jobList == null || jobList.Count == 0) return;

        int count = jobList.Count;

        if (keyboard.rightArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y > 0f)
        {
            selectedIndex_ = (selectedIndex_ + 1) % count;
        }
        else if (keyboard.leftArrowKey.wasPressedThisFrame
            || Mouse.current.scroll.ReadValue().y < 0f)
        {
            selectedIndex_ = (selectedIndex_ - 1 + count) % count;
        }
        else if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame
            || Mouse.current.leftButton.wasPressedThisFrame)
        {
            var selectedJob = jobList[selectedIndex_];
            // 職業変更の条件を満たしているか確認し、変更可能なら変更する
            //現在のジョブと選択中のジョブが一緒なら変更しない
            if (playerStatus.GetJobName() == selectedJob.status.name)
            {
                // すでに同じ職業なので変更しない
                if(WarnigText != null)
                {
                    isWarning = true;
                    WarnigText.gameObject.SetActive(true);
                }
                return;
            }
            else if (playerStatus.GetJobName() != selectedJob.status.name && playerStatus.JobCheck(selectedJob))
            {
                playerStatus.SetJob(selectedJob.status.name);
                PhaseResult = PHASE_RESULT.NEXT_TURN;
                isFinished = true;
                isShowingDetails_ = false;
            }
        }
        else if (keyboard.tabKey.wasPressedThisFrame || keyboard.escapeKey.wasPressedThisFrame
            || Mouse.current.rightButton.wasPressedThisFrame)
        {
            isShowingDetails_ = false;
        }
    }

    // UIの更新処理
    private void UpdateUI()
    {
        var jobList = playerStatus.GetJobList();
        if (jobList == null || jobList.Count == 0) return;

        if (!isShowingDetails_)
        {
            // --------------------------------------------------
            // 【目次モード】目次を表示、詳細は非表示
            // --------------------------------------------------
            if (leftListText != null) leftListText.gameObject.SetActive(true);
            if (rightListText != null) rightListText.gameObject.SetActive(true);

            if (leftDetailText != null) leftDetailText.gameObject.SetActive(false);
            if (leftDetailSkillText != null) leftDetailSkillText.gameObject.SetActive(false);
            if (rightDetailText != null) rightDetailText.gameObject.SetActive(false);
            if (rightBonusText != null) rightBonusText.gameObject.SetActive(false);
            if (WarnigText != null) WarnigText.gameObject.SetActive(false);

            // 現在の職業名を取得
            string currentJobName = playerStatus.GetJobName();

            // 左ページ（0〜6）
            string leftStr = "";
            for (int i = 0; i < Mathf.Min(ITEMS_PER_PAGE, jobList.Count); i++)
            {
                var jobData = jobList[i];
                string cursor = (i == selectedIndex_) ? "→ " : "    ";
                string jobName = jobData.status.name;

                // ンの色を決定
                string colorName = "#FFFFFF"; // デフォルト（選択可能）

                if (currentJobName == jobName)
                {
                    colorName = "#00FF00"; // 現在の職業は黄色
                }
                else if (!playerStatus.JobCheck(jobData)) // ★ i番目のjobDataで判定する
                {
                    colorName = "#808080";   // 満たしていない職業は灰色
                }

                //1行につき1回だけ追加する
                leftStr += $"{cursor}<color={colorName}>{jobName}</color>\n";
            }
            if (leftListText != null) leftListText.text = leftStr;

            //右ページ（7〜13）
            string rightStr = "";
            for (int i = ITEMS_PER_PAGE; i < jobList.Count; i++)
            {
                var jobData = jobList[i];
                string cursor = (i == selectedIndex_) ? "→ " : "    ";
                string jobName = jobData.status.name;

                //色を決定
                string colorName = "#FFFFFF"; //デフォルト（選択可能）

                if (currentJobName == jobName)
                {
                    colorName = "#00FF00"; //現在の職業は黄色
                }
                else if (!playerStatus.JobCheck(jobData)) //i番目のjobDataで判定する
                {
                    colorName = "#808080";   //満たしていない職業は灰色
                }

                //1行につき1回だけ追加する
                rightStr += $"{cursor}<color={colorName}>{jobName}</color>\n";
            }
            if (rightListText != null) rightListText.text = rightStr;
        }
        else
        {
            //詳細モード
            if (leftListText != null) leftListText.gameObject.SetActive(false);
            if (rightListText != null) rightListText.gameObject.SetActive(false);

            if (leftDetailText != null) leftDetailText.gameObject.SetActive(true);
            if (leftDetailSkillText != null) leftDetailSkillText.gameObject.SetActive(true);
            if (rightDetailText != null) rightDetailText.gameObject.SetActive(true);
            if (rightBonusText != null) rightBonusText.gameObject.SetActive(true);

            var jobData = jobList[selectedIndex_];
            var job = jobData.status;

            JobText jobTextData = JobData.GetJobText(selectedIndex_);

            if (leftDetailText != null)
            {
                string leftContent = $"<b><align=center>{job.name}</align></b>\n\n";
                leftContent += jobTextData.description;
                leftDetailText.text = leftContent;
            }

            if (leftDetailSkillText != null)
            {
                string leftSkillContent = "";
                if (!string.IsNullOrEmpty(jobTextData.skillName))
                {
                    leftSkillContent += $"<b><align=center>クラススキル</align></b>\n";
                    leftSkillContent += $"<b><align=center>【{jobTextData.skillName}】</align></b>\n";
                    leftSkillContent += $"\n{jobTextData.skillDescription}\n";
                }
                leftDetailSkillText.text = leftSkillContent;
            }

            if (rightDetailText != null)
            {
                string rightContent = "<b><align=center>必要レベルとステータス</align></b>\n\n";

                if (job.reqLevel_ > 0) rightContent += $"    レベル{POS_LAVEL_COLON}：{POS_VALUE} {job.reqLevel_}\n";
                if (selectedIndex_ == MASTER_JOB_INDEX)
                {
                    rightContent += $"    全技能{POS_COLON}：{POS_VALUE} 100\n";
                }
                else
                {
                    if (job.reqMagicKnowledge_ > 0) rightContent += $"     魔法学{POS_COLON}：{POS_VALUE} {job.reqMagicKnowledge_}\n";
                    if (job.reqPower_ > 0) rightContent += $"     攻撃力{POS_COLON}：{POS_VALUE} {job.reqPower_}\n";
                    if (job.reqMagic_ > 0) rightContent += $"       魔力{POS_COLON}：{POS_VALUE} {job.reqMagic_}\n";
                    if (job.reqPharmacy_ > 0) rightContent += $"       薬学{POS_COLON}：{POS_VALUE} {job.reqPharmacy_}\n";
                    if (job.reqMartialArts_ > 0) rightContent += $"       武術{POS_COLON}：{POS_VALUE} {job.reqMartialArts_}\n";
                    if (job.reqFaith_ > 0) rightContent += $"       信仰{POS_COLON}：{POS_VALUE} {job.reqFaith_}\n";
                    if (job.reqArchaeology_ > 0) rightContent += $"     考古学{POS_COLON}：{POS_VALUE} {job.reqArchaeology_}\n";
                    if (job.reqAstrology_ > 0) rightContent += $"     占星術{POS_COLON}：{POS_VALUE} {job.reqAstrology_}\n";
                }
                rightDetailText.text = rightContent;
            }

            if (rightBonusText != null)
            {
                string BonusContent = "<b><align=center>職業ボーナス</align></b>\n\n";
                JOB_BONUS bonus = playerStatus.GetJobBonus(job.name);

                if (bonus.hp > 0) BonusContent += $" {POS_LABEL}HP{POS_COLON}：{POS_VALUE}+{bonus.hp}\n";
                if (bonus.power > 0) BonusContent += $" {POS_LABEL}POW{POS_COLON}：{POS_VALUE}+{bonus.power}\n";
                if (bonus.magic > 0) BonusContent += $" {POS_LABEL}MAG{POS_COLON}：{POS_VALUE}+{bonus.magic}\n";
                if (bonus.speed > 0) BonusContent += $" {POS_LABEL}SPD{POS_COLON}：{POS_VALUE}+{bonus.speed}\n";

                rightBonusText.text = BonusContent;
            }

            if (WarnigText != null == isWarning)
            {
                WarnigText.text = "<color=#FF0000>現在の職業と同じ職業は選択できません</color>";
            }
        }
    }
}