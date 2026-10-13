using System;

[Serializable]
public class JobData
{
    public struct JobDatas
    {
        public string name;            // 職業名

        // 基礎ステータスの条件
        public int reqLevel_;          // レベル
        public int reqPower_;          // 攻撃力
        public int reqMagic_;          // 魔法攻撃力

        // 技術ステータスの条件
        public int reqPharmacy_;       // 薬学
        public int reqMartialArts_;    // 武術
        public int reqMagicKnowledge_; // 魔法知識
        public int reqFaith_;          // 信仰
        public int reqArchaeology_;    // 考古学
        public int reqAstrology_;      // 占星術
    }

    //status メンバー変数
    public JobDatas status;

    // コンストラクタ（C++版と同じデフォルト引数を適用）[cite: 9, 10]
    public JobData(string n, int lv = 0, int pow = 0, int mag = 0,
                   int pha = 0, int mar = 0, int mkn = 0,
                   int fai = 0, int arc = 0, int ast = 0)
    {
        status = new JobDatas
        {
            name = n,
            reqLevel_ = lv,
            reqPower_ = pow,
            reqMagic_ = mag,
            reqPharmacy_ = pha,
            reqMartialArts_ = mar,
            reqMagicKnowledge_ = mkn,
            reqFaith_ = fai,
            reqArchaeology_ = arc,
            reqAstrology_ = ast
        };
    }

    // 職業テキスト用の構造体
    public struct JobText
    {
        public string description;
        public string skillName;
        public string skillDescription;

        public JobText(string description, string skillName = null, string skillDescription = null)
        {
            this.description = description;
            this.skillName = skillName;
            this.skillDescription = skillDescription;
        }
    }

    // C++の GetJobText を C# に移植
    public static JobText GetJobText(int index)
    {
        JobText[] jobTexts = new JobText[]
        {
        new JobText("資格を取得し魔法の使用が公に認められた一般的な魔法使い。\nこれから魔法とともに冒険の旅に出る者。"),
        new JobText("物体に魔法や術式を施して戦うことを得意とする魔法使い。\n魔道具の扱いに長けている。"),
        new JobText("武器に魔力を流し、強化された物理攻撃を得意とする。\n剣や斧から鎧など魔力を流せるものは様々である。"),
        new JobText("一般魔法使いより多くの魔法を使いこなすことが出来る。\n多様な魔法を用いて皆を導くエリート魔法使い。"),
        new JobText("神からの啓示を受け、聖なる力を使う魔法使い。\nモンスターの浄化や呪いの解呪など民を守ることが多い職業。"),
        new JobText("魔法を私利私欲に使い、邪悪を振り撒かんとする者達の総称。\n汚染・隷属など攻撃的な魔法を多く扱う。"),
        new JobText("水晶玉を通すことで、映された者が辿る運命を見通す力を持つ魔法使い。\n直後から最期まで指定した未来を知ることが出来る。"),
        new JobText("物質の変換や錬成など、この世の真理とも呼べる技術を探求し模索する研究家。", "エリクサー", "幻の素材により調合される奇跡の秘薬。\n毎ターンHPを回復する。"),
        new JobText("生来の肉体に加えて、天からの護りも加わることで突破力と堅牢さを兼ね備えた武闘派の魔法使い…？", "極聖光", "その拳は敵の弱点をさらに強く撃ち抜く。\n会心発生時の威力が上昇する。"),
        new JobText("魔導師よりもさらに魔法を極めた者にのみ与えられる称号。\nその数は世界中で数えるほどしかいない。", "カドゥケウス", "その杖からは魔力が際限なく溢れ出る。\n物理攻撃に魔力の半分を加算する。"),
        new JobText("魔を浄めることに心血を注ぎ続けた者。\n人々を救うための瞳と光も、魔族には冷酷に映るだろう。", "神秘の護り", "死をも拒絶する神の護り。\n敵の最初の攻撃を防ぐ。"),
        new JobText("死者の蘇生と魂の隷従、2つの禁忌の呪文を自在に操る恐ろしい魔法使い。\n歪な執着は倫理を超え命を支配する。", "ネクロマンス", "死者の軍勢は全てを呑み込む。\nWave開始時魔力に基づくダメージを与える。"),
        new JobText("占いは人に留まらず星の未来まで見通すことが出来る。\n次に見えるのは吉祥かはたまた凶兆か。", "未来視", "未来を見通し勝利の運命を手繰る。\n死亡するような攻撃を1度だけ耐える。"),
        new JobText("全てを求めた先にあるのはかつての師の背中。\nそこから見える景色は虹と影のどちらに満ちているのだろうか。", "無詠唱魔術", "どの記録にもない師の教え。\n魔法使用のクールタイムを解除する。")
        };

        if (index < 0 || index >= jobTexts.Length)
        {
            return new JobText("");
        }

        return jobTexts[index];
    }
}