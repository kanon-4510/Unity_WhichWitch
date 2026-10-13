using UnityEngine;

public static class EnemySpawner
{
    //定数
    //敵の出現ターンに関する定数
    private const int BOSS_TURN = 21;
    private const int HIGH_TIER_TURN = 17;
    private const int MID_TIER_TURN = 13;
    private const int LOW_TIER_TURN = 9;
    private const int BEGINNER_TURN = 5;

    //各ターン帯の敵のバリエーション数
    private const int HIGH_TIER_ENEMY_COUNT = 5;
    private const int LOW_TIER_ENEMY_COUNT = 4;


    private static Enemy CreateAndInitEnemy(string enemyName, string path, int hp, int p1, int p2, int p3, int speed, int exp, string skill1, string skill2, string skill3)
    {
        // Resources フォルダからプレハブをロード
        GameObject prefab = Resources.Load<GameObject>(path);
        if (prefab == null)
        {
            GameObject emptyObj = new GameObject(enemyName);
            Enemy fallbackEnemy = emptyObj.AddComponent<Enemy>();
            fallbackEnemy.Init(enemyName, path, hp, p1, p2, p3, speed, exp, skill1, skill2, skill3);
            return fallbackEnemy;
        }
        // プレハブをインスタンス化
        GameObject enemyObj = Object.Instantiate(prefab);
        Enemy enemy = enemyObj.GetComponent<Enemy>();
        if (enemy == null)
        {
            enemy = enemyObj.AddComponent<Enemy>();
        }

        // 敵のステータスを初期化
        enemy.Init(enemyName, path, hp, p1, p2, p3, speed, exp, skill1, skill2, skill3);
        return enemy;
    }

    public static Enemy SpawnEnemyByTurn(int turn)
    {
        if (turn >= BOSS_TURN)
        {
            return CreateAndInitEnemy("魔王", "Enemy/Boss", 999, 43, 37, 30, 50, 0, "こうげき", "魔の威光", "遡行");
        }
        else if (turn >= HIGH_TIER_TURN)
        {
            int type = Random.Range(0, HIGH_TIER_ENEMY_COUNT);
            switch (type)
            {
                case 0: return CreateAndInitEnemy("バレットマキナ", "Enemy/Robot/Bulet", 150, 9, 30, 18, 39, 5, "こうげき", "砲撃", "電力チャージ");
                case 1: return CreateAndInitEnemy("レイス", "Enemy/Ghost/Ghost_Onryo", 132, 17, 28, 10, 34, 2, "こうげき", "呪われた包丁", "金縛り");
                case 2: return CreateAndInitEnemy("悪の雷魔術師", "Enemy/Caster/Lightning", 142, 19, 23, 25, 25, 3, "こうげき", "電撃斬", "エレキビーム");
                case 3: return CreateAndInitEnemy("夜の騎士", "Enemy/Knight/Night", 178, 16, 29, 18, 26, 4, "こうげき", "切断", "守りの構え");
                default: return CreateAndInitEnemy("ダークゴルゴン", "Enemy/Gorgon/Dark", 189, 21, 32, 8, 27, 5, "こうげき", "薙ぎ払い", "蛇にらみ");
            }
        }
        else if (turn >= MID_TIER_TURN)
        {
            int type = Random.Range(0, HIGH_TIER_ENEMY_COUNT);
            switch (type)
            {
                case 0: return CreateAndInitEnemy("下級タウロス", "Enemy/Minotaur/Lesser", 127, 15, 15, 10, 20, 9, "こうげき", "こうげき", "自己再生");
                case 1: return CreateAndInitEnemy("ヴァンパイア", "Enemy/Vampire", 98, 10, 18, 5, 22, 8, "こうげき", "血槍", "沈黙の呪い");
                case 2: return CreateAndInitEnemy("カラス天狗", "Enemy/Tengu/Karasu", 95, 9, 20, 3, 25, 6, "こうげき", "居合切り", "破魔空間");
                case 3: return CreateAndInitEnemy("悪の炎魔術師", "Enemy/Caster/Fire", 80, 13, 15, 19, 16, 7, "こうげき", "ひのたま", "火炎放射");
                default: return CreateAndInitEnemy("ゴルゴン", "Enemy/Gorgon/Nomal", 103, 10, 16, 22, 24, 10, "こうげき", "かみつく", "薙ぎ払い");
            }
        }
        else if (turn >= LOW_TIER_TURN)
        {
            int type = Random.Range(0, HIGH_TIER_ENEMY_COUNT);
            switch (type)
            {
                case 0: return CreateAndInitEnemy("溶岩スライム", "Enemy/Slime/Red", 70, 6, 9, 9, 18, 14, "こうげき", "溶岩パンチ", "溶岩パンチ");
                case 1: return CreateAndInitEnemy("ゴースト", "Enemy/Ghost/Ghost_Yurei", 62, 8, 0, 5, 17, 11, "こうげき", "呪い", "発狂");
                case 2: return CreateAndInitEnemy("朝の騎士", "Enemy/Knight/Morning", 65, 7, 13, 10, 19, 13, "こうげき", "斬撃", "守る");
                case 3: return CreateAndInitEnemy("マジックボム", "Enemy/Spirit", 43, 8, 10, 12, 20, 12, "こうげき", "ばくはつ", "放熱");
                default: return CreateAndInitEnemy("トレント", "Enemy/Plent", 56, 8, 3, 13, 16, 14, "こうげき", "毒の粉", "大地の恵み");
            }
        }
        else if (turn >= BEGINNER_TURN)
        {
            int type = Random.Range(0, HIGH_TIER_ENEMY_COUNT);
            switch (type)
            {
                case 0: return CreateAndInitEnemy("毒スライム", "Enemy/Slime/Green", 35, 5, 3, 5, 13, 20, "こうげき", "毒パンチ", "こうげき");
                case 1: return CreateAndInitEnemy("ホワイトウルフ", "Enemy/Wolf/White", 32, 6, 7, 8, 12, 25, "こうげき", "ひっかく", "きりさく");
                case 2: return CreateAndInitEnemy("ブラックウルフ", "Enemy/Wolf/Black", 32, 3, 4, 5, 19, 23, "こうげき", "ひっかく", "きりさく");
                case 3: return CreateAndInitEnemy("天狗", "Enemy/Tengu/Yamabushi", 34, 3, 8, 2, 15, 18, "こうげき", "いあいぎり", "ふきつなかぜ");
                default: return CreateAndInitEnemy("剣スケルトン", "Enemy/Skelton/Warrior", 40, 3, 7, 7, 14, 19, "こうげき", "ざんげき", "まもる");
            }
        }
        else
        {
            int type = Random.Range(0, LOW_TIER_ENEMY_COUNT);
            switch (type)
            {
                case 0: return CreateAndInitEnemy("スライム", "Enemy/Slime/Blue", 26, 1, 1, 1, 7, 25, "こうげき", "こうげき", "パンチ");
                case 1: return CreateAndInitEnemy("オオカミ男", "Enemy/Wolf/Red", 21, 1, 1, 1, 30, 28, "こうげき", "こうげき", "ひっかく");
                case 2: return CreateAndInitEnemy("弓スケルトン", "Enemy/Skelton/Archer", 21, 1, 1, 1, 9, 27, "こうげき", "どくのや", "こうげき");
                default: return CreateAndInitEnemy("ゾンビ", "Enemy/Zombie", 21, 2, 2, 2, 9, 26, "こうげき", "こうげき", "こうげき");
            }
        }
    }

    public static Enemy SpawnRushEnemy(int stage)
    {
        switch (stage)
        {
            case 0: return CreateAndInitEnemy("ミノタウロス", "Enemy/Minotaur/Ark", 200, 25, 25, 20, 30, 0, "こうげき", "こうげき", "自己再生");
            case 1: return CreateAndInitEnemy("ソードマキナ", "Enemy/Robot/Sword", 130, 29, 39, 16, 35, 0, "こうげき", "雷連撃", "電力チャージ");
            case 2: return CreateAndInitEnemy("妖狐", "Enemy/Youko", 163, 25, 19, 34, 27, 0, "こうげき", "鬼火", "蒼炎");
            case 3: return CreateAndInitEnemy("落武者", "Enemy/Ghost/Ghost_Samurai", 182, 15, 28, 26, 26, 0, "こうげき", "切り裂く", "受流しの構え");
            case 4: return CreateAndInitEnemy("イビルゴルゴン", "Enemy/Gorgon/Evil", 173, 35, 28, 26, 39, 0, "こうげき", "毒牙", "石化の魔眼");
            default: return null;
        }
    }
}