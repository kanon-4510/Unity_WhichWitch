using UnityEngine;

public abstract class PhaseBase : MonoBehaviour
{
    // C++ の PHASE_RESULT enum をそのまま完全移植
    public enum PHASE_RESULT
    {
        NONE,         // まだ結果が出ていない
        NEXT_TURN,    // 通常通り、次のターン（行動ループ）へ進む
        CANCEL,       // キャンセルされたので、コマンド選択をやり直す
        MAX
    }

    // フェーズ終了フラグと結果
    protected bool isFinished = false;
    public PHASE_RESULT PhaseResult { get; set; } = PHASE_RESULT.NONE;

    // --- フェーズのライフサイクル管理 ---

    // フェーズ開始処理（C++でのコンストラクタ/初期化の代わり）
    public virtual void OnEnterPhase()
    {
        isFinished = false;
        PhaseResult = PHASE_RESULT.NONE;
    }

    // 更新処理（C++: virtual void Update() = 0; に対応）
    public virtual void OnUpdatePhase() { }

    // フェーズが終了したかどうか（C++: virtual bool IsFinished() const = 0; に対応）
    public virtual bool IsFinished()
    {
        return isFinished;
    }
}