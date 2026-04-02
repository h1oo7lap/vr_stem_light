using TMPro;
using UnityEngine;

public class Visualization : MonoBehaviour
{
    [Header("Line Renderers (Kéo thả vào đây)")]
    public LineRenderer normalLine;
    public LineRenderer arcIRenderer;
    public LineRenderer arcRRenderer;

    [Header("Text Labels (Kéo TextMeshPro vào)")]
    public TMP_Text textI;
    public TMP_Text textR;

    [Header("Kích thước đồ hoạ")]
    public float normalLength = 1.5f;
    public float arcRadius = 0.5f;

    [Header("Runtime Data (Không cần sửa)")]
    public float currentAngleI = 0f;
    public float currentAngleR = 0f;
    public bool isTIR = false;

    public void UpdateVisuals(
        Vector3 hitPoint,
        Vector3 incomingDir,
        Vector3 normal,
        Vector3 refractDir,
        bool tirHappened = false
    )
    {
        isTIR = tirHappened;
        // Vector tia tới (chiều đi từ điểm cắt ngược lên nguồn sáng)
        Vector3 reverseIncoming = -incomingDir.normalized;
        // Vector tia khúc xạ (chiều đi xuyên xuống môi trường)
        Vector3 refracted = refractDir.normalized;
        // Pháp tuyến mặt phẳng (nửa trên)
        Vector3 n = normal.normalized;
        // Trục pháp tuyến (nửa dưới)
        Vector3 reverseN = -n;

        // 1. Vẽ đường nét đứt Pháp Tuyến (N - N')
        if (normalLine != null)
        {
            normalLine.positionCount = 2;
            normalLine.SetPosition(0, hitPoint + n * normalLength);
            normalLine.SetPosition(1, hitPoint + reverseN * normalLength);
        }

        // 2. Tính toán và vẽ vòng cung Góc Tới (i)
        currentAngleI = Vector3.Angle(reverseIncoming, n);
        if (arcIRenderer != null)
        {
            DrawArc(arcIRenderer, hitPoint, n, reverseIncoming, arcRadius);
        }

        if (textI != null)
        {
            textI.transform.position =
                hitPoint + Vector3.Slerp(n, reverseIncoming, 0.5f).normalized * (arcRadius + 0.15f);
            textI.text = $"i = {currentAngleI:F1}°";

            // Xoay mặt chữ hướng về Camera người dùng VR
            if (Camera.main != null)
            {
                textI.transform.LookAt(Camera.main.transform);
                textI.transform.Rotate(0, 180, 0);
            }

            // Ép cấu hình TMP để không bị tàng hình do tràn viền (Clipping)
            textI.enableWordWrapping = false;
            textI.overflowMode = TextOverflowModes.Overflow;
            textI.alignment = TextAlignmentOptions.Center;
        }

        // 3. Tính toán và vẽ vòng cung Góc Khúc Xạ (r) / Góc Phản Xạ
        currentAngleR = Vector3.Angle(refracted, reverseN);
        if (arcRRenderer != null)
        {
            DrawArc(arcRRenderer, hitPoint, reverseN, refracted, arcRadius);
        }

        if (textR != null)
        {
            textR.transform.position =
                hitPoint
                + Vector3.Slerp(reverseN, refracted, 0.5f).normalized * (arcRadius + 0.15f);
            textR.text = isTIR ? $"r' = {currentAngleR:F1}° (TIR)" : $"r = {currentAngleR:F1}°";

            if (Camera.main != null)
            {
                textR.transform.LookAt(Camera.main.transform);
                textR.transform.Rotate(0, 180, 0);
            }

            // Ép cấu hình
            textR.enableWordWrapping = false;
            textR.overflowMode = TextOverflowModes.Overflow;
            textR.alignment = TextAlignmentOptions.Center;
        }
    }

    private void DrawArc(
        LineRenderer lr,
        Vector3 center,
        Vector3 fromDir,
        Vector3 toDir,
        float radius
    )
    {
        int segments = 20;
        lr.positionCount = segments + 1;
        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            // Dùng Slerp để điểm uốn cong tròn đều quanh điểm cắt (center)
            Vector3 dir = Vector3.Slerp(fromDir, toDir, t).normalized;
            lr.SetPosition(i, center + dir * radius);
        }
    }

    public void HideVisuals()
    {
        currentAngleI = 0f;
        currentAngleR = 0f;
        isTIR = false;

        if (normalLine != null)
            normalLine.positionCount = 0;
        if (arcIRenderer != null)
            arcIRenderer.positionCount = 0;
        if (arcRRenderer != null)
            arcRRenderer.positionCount = 0;
        if (textI != null)
            textI.text = "";
        if (textR != null)
            textR.text = "";
    }
}
