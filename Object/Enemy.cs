using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Enemy : MonoBehaviour
{
    private const int SKILL_VALUE = 3;

    //基本ステータス
    public string enemyName;
    public string folderPath;
    public int maxHp;
    public int currentHp;
    public int[] power = new int[SKILL_VALUE];
    public int speed;
    public int exp;
    public string[] skills = new string[SKILL_VALUE];

    //敵のHP
    [SerializeField] private Slider enemyHp;

    //戦闘用ステータス
    public int guard = 0;

    //コンストラクタ
    public void Init(string enemyName, string path , int hp, int p1, int p2, int p3, int speed, int exp, string skill1, string skill2, string skill3)
    {
        gameObject.name = enemyName; // ここで安全に名前を設定できる
        this.enemyName = enemyName;
        this.folderPath = path; 
        this.maxHp = hp;
        this.currentHp = hp;
        this.power = new int[] {p1,p2,p3 };
        this.speed = speed;
        this.exp = exp;
        this.skills = new string[] { skill1, skill2, skill3 };

        // 敵オブジェクトに付いている EnemyView を取得して、画像を初期化する
        EnemyView enemyView = GetComponent<EnemyView>();
        if (enemyView != null)
        {
            // シーン上の "EnemyImage" という名前のオブジェクトを自動で探してセットする
            GameObject uiImageObj = GameObject.Find("EnemyImage");
            if (uiImageObj != null)
            {
                Image targetImage = uiImageObj.GetComponent<Image>();
            }
            enemyView.InitEnemyImages(path);
        }
    }

    //行動決定
    public (int skillIndex, string skillName) DecideAction()
    {
        int chosenIndex = Random.Range(0, skills.Length); // 0, 1, 2 のいずれか
        return (chosenIndex, skills[chosenIndex]);
    }

    //敵の最大HPをもらう
    public int GetMaxHp()
    {
        return maxHp;
    }

    //現在のHPをもらう
    public int GetCurrentHp()
    {
        return currentHp;
    }

    //防御力をセットする
    public void SetGuard(int power)
    {
        guard = power;
    }

    //防御力をリセットする
    public void ResetGuard() 
    {
        guard = 0; 
    }

    //ダメージ処理
    public void Damage(int damage)
    {
        int netDamage = Mathf.Max(0, damage - guard);
        currentHp = Mathf.Max(0, currentHp - netDamage);
    }

    //回復処理
    public void Heal(int amount)
    {
        currentHp = Mathf.Min(maxHp, currentHp + amount);
    }

    //敵の死亡判定
    public bool IsDead()
    {
        return currentHp <= 0;
    }

    //ステータスを取得するためのゲッター
    public string GetName() => name;
    public int GetExp() => exp;

    // インデックスを指定して取得する
    public int GetPower(int index)
    {
        if (index >= 0 && index < power.Length)
        {
            return power[index];
        }
        return 0; // エラー時のデフォルト値など
    }
}