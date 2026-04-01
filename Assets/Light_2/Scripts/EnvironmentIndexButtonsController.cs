using UnityEngine;
using TMPro;

public class EnvironmentIndexButtonsController : MonoBehaviour
{
    [Header("Liên kết Nguồn (Target)")]
    [Tooltip("Kéo Đèn Laser Khúc Xạ vào đây")]
    public LightBeamPhysics mainLaser;

    [Header("Giao diện hiển thị (UI)")]
    [Tooltip("Kéo dòng chữ TextMeshPro vào đây để hiển thị số (Ví dụ: n=2.0)")]
    public TMP_Text valueText;

    void Start()
    {
        if (mainLaser != null)
        {
            UpdateText(mainLaser.environmentRefractiveIndex);
        }
    }

    // Gắn trực tiếp hàm này vào sự kiện OnClick() của Nút Bấm. Bấm là tự thành n=2!
    public void SetIndexTo2()
    {
        if (mainLaser != null)
        {
            mainLaser.environmentRefractiveIndex = 2.0f;
            UpdateText(2.0f);
        }
    }

    // (Tùy chọn) Gắn hàm này vào nút số 2 để đổi lại thành Không khí
    public void SetIndexTo1()
    {
        if (mainLaser != null)
        {
            mainLaser.environmentRefractiveIndex = 1.0f;
            UpdateText(1.0f);
        }
    }

    // Gắn hàm này vào 1 Nút bấm duy nhất để Đảo trạng thái (Toggle) qua lại giữa n=1.0 và n=2.0
    public void ToggleIndex()
    {
        if (mainLaser != null)
        {
            if (mainLaser.environmentRefractiveIndex <= 1.01f)
            {
                // Nếu đang là Không khí -> Bơm chất đặc n=2
                mainLaser.environmentRefractiveIndex = 2.0f;
                UpdateText(2.0f);
            }
            else
            {
                // Nếu đang đặc nghẹt -> Xả về Không khí n=1
                mainLaser.environmentRefractiveIndex = 1.0f;
                UpdateText(1.0f);
            }
        }
    }

    void UpdateText(float value)
    {
        if (valueText != null)
        {
            if (value <= 1.01f)
                valueText.text = $"Môi trường: Không khí (n = 1.00)";
            else
                valueText.text = $"Môi trường: Dung dịch đặc (n = {value:F2})";
        }
    }
}
