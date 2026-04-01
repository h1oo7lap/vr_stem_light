using UnityEngine;

public class RefractiveMaterial : MonoBehaviour
{
    [Tooltip("Chiết suất của vật liệu (1.0: Không khí, 1.33: Nước, 1.5: Thủy tinh, 2.42: Kim cương)")]
    [Range(1f, 3f)]
    public float refractiveIndex = 1.33f;
}
