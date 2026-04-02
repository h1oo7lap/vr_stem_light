using TMPro;
using UnityEngine;

public class EnvironmentIndexManager : MonoBehaviour
{
    [Header("Liên kết Nguồn (Target)")]
    public LightPhysics mainLaser;

    [Header("Giao diện hiển thị (UI)")]
    public TMP_Text valueText;

    void Start()
    {
        if (mainLaser != null)
        {
            UpdateText(mainLaser.environmentRefractiveIndex);
        }
    }

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
