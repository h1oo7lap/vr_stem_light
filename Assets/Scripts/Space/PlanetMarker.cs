using UnityEngine;

public class PlanetMarker : MonoBehaviour
{
    public OvalRing orbit;
    public float angle;

    [Header("Scaling Settings")]
    [Tooltip("Tỷ lệ tối thiểu của Marker khi ở rất gần")]
    private float minScale = 0.05f; 
    
    [Tooltip("Hệ số nhân tỉ lệ theo khoảng cách")]
    public float scaleFactor = 0.03f;
    

    void Update()
    {
        // 1. Cập nhật vị trí trên quỹ đạo
        transform.position = orbit.GetPoint(angle);

        // Kiểm tra Singleton Instance để tránh lỗi NullReference
        if (BillboardUI.Instance == null || BillboardUI.Instance.playerCamera == null)
            return;

        Transform cam = BillboardUI.Instance.playerCamera;

        // 2. Tính toán Scale theo khoảng cách
        float dist = Vector3.Distance(transform.position, cam.position);
        
        // Tính toán kích thước mới dựa trên khoảng cách
        float calculatedScale = dist * scaleFactor;

        // Giới hạn: Nếu calculatedScale nhỏ hơn minScale thì lấy minScale
        float finalScale = Mathf.Max(minScale, calculatedScale);
        
        transform.localScale = Vector3.one * finalScale;

        // 3. Hướng về phía Camera (Billboard)
        // transform.LookAt(cam);
        // transform.Rotate(0, 180f, 0); 
    }
}