using UnityEngine;
using UnityEngine.Rendering;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("オーディオソース")]
    [SerializeField] private AudioSource bgmAudioSource;
    [SerializeField] private AudioSource seAudioSource;

    [Header("各種SEクリップ")]
    [SerializeField] private AudioClip decisionClip; // 決定音
    [SerializeField] private AudioClip attackClip;   // 攻撃音
    [SerializeField] private AudioClip damageClip;   // ダメージ音

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //シーンをまたいでも消さない
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // BGMを切り替えるメソッド
    public void PlayBGM(AudioClip clip)
    {
        if (bgmAudioSource == null || clip == null)
        {
            return;
        }
        if (bgmAudioSource.clip == clip)
        {
            return; // すでに同じ曲が流れているなら何もしない
        }

        bgmAudioSource.clip = clip;
        bgmAudioSource.loop = true;
        bgmAudioSource.Play();
    }


    // 決定音を鳴らすメソッド
    public void PlaySE(AudioClip clip)
    {
        if (clip != null)
        {
            // PlayOneShotを使うことで、音が重なっても途切れない
            seAudioSource.PlayOneShot(clip);
        }
    }

    //決定SE
    public void PlayDecision()
    {
        PlaySE(decisionClip);
    }
    //攻撃SE
    public void PlayAttack()
    {
        PlaySE(attackClip);
    }
    //ダメージSE
    public void PlayDamage()
    {
        PlaySE(damageClip);
    }

    //再生中のサウンドを止める
    public void StopSound()
    {
        if (bgmAudioSource != null)
        {
            bgmAudioSource.Stop();
        }
    }
}