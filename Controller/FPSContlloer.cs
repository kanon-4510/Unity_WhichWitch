using UnityEngine;

public class FrameRateLimiter : MonoBehaviour
{

    private const int FRAMAE_RATE = 60; //FPSの値

    private void Awake()
    {
        //VSync（垂直同期）を無効化する（これがないと設定が無視されます）
        QualitySettings.vSyncCount = 0;

        //目標フレームレートを 60 に固定する
        Application.targetFrameRate = FRAMAE_RATE;
    }
}