using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyView : MonoBehaviour
{
    [SerializeField] private Image enemyImage; // UIの画像コンポーネント

    private Dictionary<EnemyAnim, Sprite[]> animDict = new Dictionary<EnemyAnim, Sprite[]>();

    private EnemyAnim currentAnim = EnemyAnim.IDLE;
    private int currentFrame = 0;
    private float timer = 0f;
    [SerializeField] private float frameInterval = 0.1f; // コマ送りの速度

    private bool isAnimFinish = false;  //アニメーションが終わったか

    // --- 画像の自動ロード処理 ---
    public void InitEnemyImages(string enemyFolderPath)
    {
        animDict.Clear();

        LoadAnimSprites(EnemyAnim.IDLE, $"{enemyFolderPath}/Idle");
        LoadAnimSprites(EnemyAnim.ACT1, $"{enemyFolderPath}/Attack_1");
        LoadAnimSprites(EnemyAnim.ACT2, $"{enemyFolderPath}/Attack_2");
        LoadAnimSprites(EnemyAnim.ACT3, $"{enemyFolderPath}/Attack_3");
        LoadAnimSprites(EnemyAnim.HURT, $"{enemyFolderPath}/Hurt");
        LoadAnimSprites(EnemyAnim.DEAD, $"{enemyFolderPath}/Dead");

        ChangeAnim(EnemyAnim.IDLE);
    }

    private void LoadAnimSprites(EnemyAnim animType, string resourcePath)
    {
        Sprite[] sprites = Resources.LoadAll<Sprite>(resourcePath);
        if (sprites != null && sprites.Length > 0)
        {
            animDict[animType] = sprites;
        }
    }

    // --- アニメーション切り替え ---
    public void ChangeAnim(EnemyAnim newAnim)
    {
        if (!animDict.ContainsKey(newAnim)) return;

        currentAnim = newAnim;
        currentFrame = 0;
        timer = 0f;
        isAnimFinish = false;

        //画像を表示状態に戻す
        if (enemyImage != null)
        {
            enemyImage.enabled = true;
        }

        if (animDict[currentAnim].Length > 0)
        {
            enemyImage.sprite = animDict[currentAnim][0];
        }
    }

    //コマ送り処理
    private void Update()
    {
        if (!animDict.ContainsKey(currentAnim)) return;

        Sprite[] currentSprites = animDict[currentAnim];
        if (currentSprites == null || currentSprites.Length == 0) return;

        timer += Time.deltaTime;
        if (timer >= frameInterval)
        {
            timer = 0f;

            //待機アニメーションならループさせる
            if (currentAnim == EnemyAnim.IDLE)
            {
                isAnimFinish = true;
                currentFrame = (currentFrame + 1) % currentSprites.Length;
            }
            else if (currentAnim == EnemyAnim.DEAD)
            {
                if (currentFrame < currentSprites.Length - 1)
                {
                    currentFrame++;
                }
                else
                {
                    // 死亡アニメーションの最後のフレームに到達したら画像を非表示にする
                    if (enemyImage != null)
                    {
                        enemyImage.enabled = false;
                        isAnimFinish = true;
                    }
                    return;
                }
            }
            else
            {
                //攻撃やダメージのアニメーションが終わったら待機アニメーションに戻す
                currentFrame++;
                if (currentFrame >= currentSprites.Length)
                {
                    isAnimFinish = true;
                    ChangeAnim(EnemyAnim.IDLE);
                    return;
                }
                else
                {
                    isAnimFinish = false;
                }
            }

            enemyImage.sprite = currentSprites[currentFrame];
        }
    }

    //アニメ―ションが終わったかを返す
    public bool IsAnimFinish()
    {
        return isAnimFinish;
    }
}