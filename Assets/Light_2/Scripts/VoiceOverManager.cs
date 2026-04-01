using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class VoiceOverManager : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Voice Clips")]
    public AudioClip introClip;
    public AudioClip challenge1CompleteClip;
    public AudioClip allCompleteClip;

    [Header("UI (Optional)")]
    [Tooltip("Kéo thả UI TextMeshPro vào đây nếu muốn trợ giảng hiện Phụ đề (Subtitle)")]
    public TMPro.TMP_Text subtitleText;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        PlayIntro();
    }

    public void PlayIntro()
    {
        if (introClip != null)
        {
            audioSource.PlayOneShot(introClip);
            ShowSubtitle("Chào bạn! Sau khi đã nắm vững quy luật phản xạ, chúng ta sẽ khám phá sự kỳ diệu của ánh sáng...");
        }
    }

    public void PlayChallenge1Complete()
    {
         if (challenge1CompleteClip != null)
        {
            audioSource.PlayOneShot(challenge1CompleteClip);
            ShowSubtitle("Tốt lắm! Bạn đã hoàn thành thử thách đầu tiên.");
        }
    }

    public void PlayAllComplete()
    {
        if (allCompleteClip != null)
        {
            audioSource.PlayOneShot(allCompleteClip);
            ShowSubtitle("Tuyệt vời! Bạn đã thấy ánh sáng bị bẻ cong và phân rã thành muôn màu. Đây chính là nguyên lý tạo nên cầu vồng sau cơn mưa đấy!");
        }
    }

    private void ShowSubtitle(string text)
    {
        if (subtitleText != null)
        {
            subtitleText.text = $"[Trợ giảng AI]: {text}";
        }
        else
        {
            Debug.Log("[Trợ Giảng AI]: " + text);
        }
    }
}
