using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIMover : MonoBehaviour
{
    private Image targetImage;

    //定数
    // --- アニメーション関連の定数 ---
    private const int STEP_DISTANCE = 4;          // 左へ動くピクセル数
    private const int TOTAL_ATTACK_FRAMES = 6;    // 攻撃を行うフレーム数
    private const int DAMAGE_ANIM_FRAMES = 10;    // ダメージのフレーム数

    // --- 点滅演出用の定数 ---
    private const int DAMAGE_BLINK_INTERVAL = 2;  // 点滅の切り替わり間隔（フレーム）
    private const int DAMAGE_BLINK_CYCLE = 4;     // 点滅の周期

    private int attackAnimTimer = 0;
    private int damageAnimTimer = 0;

    private RectTransform rectTransform;   //
    private Vector2 originalPosition;      //元々の位置を記憶する用
    private Color originalColor;           //元々の色を記憶する
    private bool isAttacking = false;      //攻撃を行ったか
    private bool isDamageing = false;      //攻撃を行ったか

    private void Awake()
    {
        //位置を管理するRectTransformを取得しておく
        rectTransform = GetComponent<RectTransform>();
        targetImage = GetComponent<Image>();
    }

    private void Start()
    {
        originalPosition = rectTransform.anchoredPosition;
        originalColor = targetImage.color;
    }

    private void Update()
    {
        // タイマーが動いている間は移動処理を行う
        if (attackAnimTimer > 0)
        {
            attackAnimTimer--;

            if (attackAnimTimer >= 0)
            {
                rectTransform.anchoredPosition = originalPosition + (Vector2.left * STEP_DISTANCE);
            }
        }
        else
        {
            // 演出終了：完全に元の位置にスナップする
            rectTransform.anchoredPosition = originalPosition;
        }

        //ダメージ中は色を赤くして点滅させる
        if (damageAnimTimer > 0)
        {
            damageAnimTimer--;
            if ((damageAnimTimer / DAMAGE_BLINK_INTERVAL) % DAMAGE_BLINK_CYCLE == 0)
            {
                targetImage.color = Color.red;
            }
            else
            {
                targetImage.color = originalColor;
            }
        }
        else
        {
            // 演出終了：元の色と透明度に完全に戻す
            if (targetImage != null)
            {
                targetImage.color = originalColor;
            }
        }

    }

    public void StartAttackAnimation()
    {
        Debug.Log("【デバッグ】StartAttackAnimation が呼び出されました！");

        // すでに動いていたら二重起動しないようにする
        if (isAttacking) return;

        // すでに動いている場合は二重起動しない
        if (attackAnimTimer > 0) return;

        // 現在の位置を基準位置として記憶し、タイマーをセット
        originalPosition = rectTransform.anchoredPosition;
        attackAnimTimer = TOTAL_ATTACK_FRAMES;
    }

    //ダメージのアニメーション
    public void StartDamageAnimation()
    {
        Debug.Log("【デバッグ】StartDamageAnimation が呼び出されました！");

        // すでに動いていたら二重起動しないようにする
        if (isDamageing) return;

        // すでに動いている場合は二重起動しない
        if (damageAnimTimer > 0) return;

        damageAnimTimer = DAMAGE_ANIM_FRAMES;
    }

    public bool IsAttacking()
    {
        return isAttacking;
    }
    public bool IsDamageing()
    {
        return isDamageing;
    }
}