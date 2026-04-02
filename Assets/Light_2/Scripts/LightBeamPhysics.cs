using System.Collections.Generic;
using UnityEngine;

public class LightBeamPhysics : MonoBehaviour
{
    [Header("Main Beam Setup")]
    public LineRenderer mainLineRenderer;
    public Transform startPoint;

    [Header("Settings")]
    public int maxInteractions = 10;
    public float maxDistance = 50f;

    [Header("Dispersion Material (URP)")]
    [Tooltip("Material URP/Unlit, Transparent + Additive, Base Color trắng")]
    public Material dispersionBeamMaterial;

    [Header("Dispersion Strength")]
    public float dispersionStrength = 0.05f; // Giảm xuống để dải màu ko bị xòe quá gắt gây gãy khúc

    [Header("Volumetric Effect (Hiệu ứng ánh sáng)")]
    [Tooltip("Cường độ sáng chói (HDR)")]
    public float glowIntensity = 3.5f;

    [Tooltip("Dùng lớp LineRenderer phụ để làm quầng sáng (Halo) bao quanh tia sáng")]
    public bool useVolumetricHalo = true;
    public float haloWidthMultiplier = 4.0f;

    [Range(0.01f, 1f)]
    public float haloAlpha = 0.15f;

    [Header("Visual Aids & Environment")]
    [Tooltip("Gắn script OpticVisualAids vào đây để hiển thị số đo góc")]
    public OpticVisualAids opticVisualAids;

    [Tooltip(
        "Chiết suất của Không gian môi trường bên ngoài (Mặc định Không khí n=1.0). Tăng con số này lên vượt qua vật thể để tạo Phản Xạ Toàn Phần."
    )]
    [Range(1.0f, 3.0f)]
    public float environmentRefractiveIndex = 1.0f;

    [HideInInspector]
    public bool isDispersionHittingTarget = false; // Trạng thái đập trúng tường

    [HideInInspector]
    public bool isHittingPrism = false; // Trạng thái bắn tia phân tách lăng kính

    // Chiết suất thực tế của thủy tinh Crown Glass (gần nhau để chúng đi túm tụm cùng 1 đường nhỏ)
    private readonly float[] refractiveIndices =
    {
        1.510f, // Red
        1.513f, // Orange
        1.516f, // Yellow
        1.519f, // Green
        1.522f, // Blue
        1.525f, // Indigo
        1.530f, // Violet
    };

    private readonly Color[] rainbowColors = new Color[]
    {
        new Color(1f, 0f, 0f, 1f), // Red (Bẻ cong ít nhất -> Nằm ở trên cùng)
        new Color(1f, 0.5f, 0f, 1f), // Orange
        new Color(1f, 1f, 0f, 1f), // Yellow
        new Color(0f, 1f, 0f, 1f), // Green
        new Color(0f, 0f, 1f, 1f), // Blue
        new Color(0.3f, 0f, 1f, 1f), // Indigo
        new Color(0.8f, 0f, 1f, 1f), // Violet (Bẻ cong gập nhiều nhất -> Nằm ở dưới cùng)
    };

    private List<LineRenderer> dispersionLines = new List<LineRenderer>();
    private List<LineRenderer> dispersionHalos = new List<LineRenderer>();
    private LineRenderer mainHaloRenderer;
    private Material beamMaterial;

    void Awake()
    {
        // Tắt va chạm mặt trong để tia sáng không bị gấp khúc lần thứ 2 khi xuyên qua khối nước
        Physics.queriesHitBackfaces = false;

        if (dispersionBeamMaterial != null)
        {
            beamMaterial = dispersionBeamMaterial;
        }
        else
        {
            Shader unlit = Shader.Find("Universal Render Pipeline/Unlit");
            if (unlit != null)
            {
                beamMaterial = new Material(unlit);
                Debug.LogWarning("Fallback URP/Unlit used - assign material thủ công!");
            }
            else
            {
                Debug.LogError("Không tìm thấy URP/Unlit shader!");
                return;
            }
        }

        // Tạo Halo cho Main Beam (Tia chính)
        if (useVolumetricHalo && mainLineRenderer != null)
        {
            GameObject mainHaloObj = new GameObject("MainBeam_Halo");
            mainHaloObj.transform.SetParent(mainLineRenderer.transform);
            mainHaloObj.hideFlags = HideFlags.HideInHierarchy;

            mainHaloRenderer = mainHaloObj.AddComponent<LineRenderer>();
            mainHaloRenderer.material = beamMaterial;
            mainHaloRenderer.numCapVertices = 8;
            mainHaloRenderer.numCornerVertices = 8;
            mainHaloRenderer.enabled = false;
        }

        for (int i = 0; i < 7; i++)
        {
            GameObject go = new GameObject("DispersionBeam_" + i);
            go.transform.SetParent(transform);
            go.hideFlags = HideFlags.HideInHierarchy;

            LineRenderer lr = go.AddComponent<LineRenderer>();
            lr.positionCount = 0;
            lr.startWidth = lr.endWidth = 0.035f;
            lr.material = beamMaterial;
            lr.numCapVertices = 8;
            lr.numCornerVertices = 8;
            lr.enabled = false;

            // Chỉnh HDR để kích hoạt thẻ phát sáng
            Color hdrColor = rainbowColors[i] * glowIntensity;
            hdrColor.a = 1f;
            lr.startColor = hdrColor;
            lr.endColor = hdrColor;
            dispersionLines.Add(lr);

            if (useVolumetricHalo)
            {
                GameObject hGo = new GameObject("DispersionHalo_" + i);
                hGo.transform.SetParent(transform);
                hGo.hideFlags = HideFlags.HideInHierarchy;

                LineRenderer hLr = hGo.AddComponent<LineRenderer>();
                hLr.positionCount = 0;
                hLr.startWidth = hLr.endWidth = 0.035f * haloWidthMultiplier;
                hLr.material = beamMaterial;
                hLr.numCapVertices = 8;
                hLr.numCornerVertices = 8;
                hLr.enabled = false;

                Color haloColor = rainbowColors[i];
                haloColor.a = haloAlpha;
                hLr.startColor = haloColor;
                hLr.endColor = haloColor;
                dispersionHalos.Add(hLr);
            }
        }
    }

    void Update()
    {
        if (startPoint == null || mainLineRenderer == null)
            return;

        CastBeam();
    }

    void CastBeam()
    {
        List<Vector3> mainPoints = new List<Vector3>();
        Ray ray = new Ray(startPoint.position, startPoint.forward);
        mainPoints.Add(ray.origin);

        bool showVisualAidsThisFrame = false;
        bool hitPrismThisFrame = false;
        isDispersionHittingTarget = false; // Reset trạng thái Tán sắc chạm bảng mỗi khung hình
        int bounceCount = 0;

        while (bounceCount < maxInteractions)
        {
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
            {
                mainPoints.Add(hit.point);

                Vector3 incomingDir = ray.direction;
                Vector3 normal = hit.normal;

                if (hit.collider.CompareTag("Prism"))
                {
                    hitPrismThisFrame = true;

                    // 1. Phóng tia sáng Trắng đi khúc xạ xuyên qua lõi Lăng Kính (Chưa tẽ màu)
                    Vector3 internalDir = Refract(
                        incomingDir,
                        normal,
                        environmentRefractiveIndex,
                        1.52f
                    ).normalized;

                    // 2. Dùng tia phụ (đảo ngược) bắn vào vỏ Lăng kính từ bên ngoài để tìm ĐIỂM THOÁT (Exit Point)
                    Ray prismReverseRay = new Ray(
                        hit.point + internalDir * maxDistance,
                        -internalDir
                    );
                    if (
                        hit.collider.Raycast(
                            prismReverseRay,
                            out RaycastHit prismExitHit,
                            maxDistance * 2f
                        )
                    )
                    {
                        // Nối dài tia Trắng (Main Ray) từ mặt Vào đến đúng nốt mặt Thoát
                        mainPoints.Add(prismExitHit.point);

                        // Lấy Pháp tuyến (Normal) hướng tụ vào bên trong lõi Lăng Kính
                        Vector3 exitNormal = -prismExitHit.normal;

                        // 3. Tại đúng Điểm Thoát, kích nổ 7 tia màu Tán Sắc bắn xòe ra ngoài không khí
                        SpawnDispersionRaysAtExit(prismExitHit.point, internalDir, exitNormal);
                    }

                    break; // Cắt đứt hoàn toàn tia Trắng tại điểm Thoát, nhường sân khấu vinh quang cho 7 tia Tán sắc
                }
                else if (hit.collider.CompareTag("Mirror"))
                {
                    Vector3 reflectDir = Vector3.Reflect(incomingDir, normal);
                    ray = new Ray(hit.point + reflectDir * 0.001f, reflectDir);
                }
                else if (hit.collider.CompareTag("Water"))
                {
                    RefractiveMaterial rm = hit.collider.GetComponent<RefractiveMaterial>();
                    float n_object =
                        rm != null
                            ? rm.refractiveIndex
                            : (hit.collider.CompareTag("Glass") ? 1.5f : 1.33f);

                    // Xác định chiều chiết suất đi từ Không Gian Ngoài (Môi Trường) -> Vật thể
                    float n1 = environmentRefractiveIndex;
                    float n2 = n_object;

                    // 1. Chỉ thực hiện Khúc xạ duy nhất một lần tại mặt chạm đầu tiên
                    Vector3 refractDir = Refract(incomingDir, normal, n1, n2);

                    // Kiểm tra xem tia có bị dội ngược lại hoàn toàn không (TIR)
                    // Tia khúc xạ xuyên qua mặt phẳng sẽ có Dot(refractDir, normal) < 0
                    // Nếu dội lại (Phản xạ toàn phần), nó sẽ cùng chiều với normal mặt bích: Dot > 0
                    bool tirHappened = Vector3.Dot(refractDir, normal) > 0.001f;

                    // Gọi Visual Aids vẽ góc khúc xạ chuẩn theo mặt VÀO
                    if (bounceCount == 0 && opticVisualAids != null)
                    {
                        opticVisualAids.UpdateVisuals(
                            hit.point,
                            incomingDir,
                            normal,
                            refractDir,
                            tirHappened
                        );
                        showVisualAidsThisFrame = true;
                    }

                    // Nếu xảy ra TIR, tia sáng dội lại bên trong môi trường, ta ghi nhận nó là Mirror bounce để nó vẽ tia bật lại
                    ray = new Ray(hit.point + refractDir * 0.001f, refractDir);
                }
                else
                {
                    break;
                }
            }
            else
            {
                mainPoints.Add(ray.GetPoint(maxDistance));
                break;
            }

            bounceCount++;
        }

        if (!hitPrismThisFrame)
        {
            DisableDispersionRays();
        }

        if (!showVisualAidsThisFrame && opticVisualAids != null)
        {
            opticVisualAids.HideVisuals();
        }

        mainLineRenderer.positionCount = mainPoints.Count;
        mainLineRenderer.SetPositions(mainPoints.ToArray());

        // Tăng sáng (HDR Glow) cho tia chính
        Color mainHdr = Color.white * glowIntensity;
        mainHdr.a = 1f; // Giữ nguyên độ đặc
        mainLineRenderer.startColor = mainHdr;
        mainLineRenderer.endColor = mainHdr;

        if (useVolumetricHalo && mainHaloRenderer != null)
        {
            mainHaloRenderer.positionCount = mainPoints.Count;
            mainHaloRenderer.SetPositions(mainPoints.ToArray());
            mainHaloRenderer.startWidth = mainLineRenderer.startWidth * haloWidthMultiplier;
            mainHaloRenderer.endWidth = mainLineRenderer.endWidth * haloWidthMultiplier;

            Color haloC = Color.white;
            haloC.a = haloAlpha;
            mainHaloRenderer.startColor = haloC;
            mainHaloRenderer.endColor = haloC;
            mainHaloRenderer.enabled = true;
        }

        // Báo cáo thành tích ném tia vào Lăng kính cho Thầy giáo ScenarioManager
        isHittingPrism = hitPrismThisFrame;
    }

    void SpawnDispersionRaysAtExit(Vector3 exitPoint, Vector3 internalDir, Vector3 exitNormal)
    {
        // Tính trục Xòe Quạt bằng phép chiếu vuông góc giữa Tia Tới và Pháp Tuyến Thoát
        Vector3 dispersionAxis = Vector3.Cross(internalDir, exitNormal).normalized;
        if (dispersionAxis.sqrMagnitude < 0.001f)
            dispersionAxis = Vector3.up;

        for (int i = 0; i < 7; i++)
        {
            float n = refractiveIndices[i];

            // Tính tia màu thoát ra khỏi lăng kính (Từ môi trường thủy tinh (n) dội ra Không khí (1.0))
            Vector3 exitDir = Refract(internalDir, exitNormal, n, 1.0f).normalized;

            // Kích hoạt Xòe Góc Quạt nhẹ nhàng để dải màu tách bạch mà vẫn giữ được hướng chính của tia
            float offsetFactor = (i - 3f) / 3f;
            Quaternion rot = Quaternion.AngleAxis(
                -offsetFactor * dispersionStrength * 90f, // Giảm từ 180 xuống 90 để tia đi thẳng hơn
                dispersionAxis
            );
            exitDir = rot * exitDir;

            List<Vector3> points = new List<Vector3> { exitPoint };
            Ray colorRay = new Ray(exitPoint + exitDir * 0.001f, exitDir);

            int colorBounce = 0;

            while (colorBounce < maxInteractions)
            {
                if (Physics.Raycast(colorRay, out RaycastHit colorHit, maxDistance))
                {
                    points.Add(colorHit.point);

                    Vector3 currentDir = colorRay.direction;
                    Vector3 hitNormal = colorHit.normal;

                    if (colorHit.collider.CompareTag("Mirror"))
                    {
                        Vector3 reflect = Vector3.Reflect(currentDir, hitNormal);
                        colorRay = new Ray(colorHit.point + reflect * 0.001f, reflect);
                    }
                    else if (
                        colorHit.collider.CompareTag("Screen")
                        || colorHit.collider.name.ToLower().Contains("screen")
                    )
                    {
                        isDispersionHittingTarget = true;
                        break; // Stop raycasting for this color ray after hitting the screen
                    }
                    else if (
                        colorHit.collider.CompareTag("Prism")
                        || colorHit.collider.CompareTag("Water")
                    )
                    {
                        RefractiveMaterial rm =
                            colorHit.collider.GetComponent<RefractiveMaterial>();
                        float baseN = 1.52f; // Mặc định Prism
                        if (colorHit.collider.CompareTag("Glass"))
                            baseN = 1.5f;
                        else if (colorHit.collider.CompareTag("Water"))
                            baseN = 1.33f;

                        if (rm != null)
                            baseN = rm.refractiveIndex;

                        float colorSpecificN = baseN * (n / 1.52f);

                        // 1. Entry refraction
                        Vector3 enterRefractDir = Refract(
                            currentDir,
                            hitNormal,
                            1.0f,
                            colorSpecificN
                        );

                        // 2. Backward raycast trick for material exit
                        Ray revRay = new Ray(
                            colorHit.point + enterRefractDir * maxDistance,
                            -enterRefractDir
                        );
                        if (
                            colorHit.collider.Raycast(
                                revRay,
                                out RaycastHit exHit,
                                maxDistance * 2f
                            )
                        )
                        {
                            points.Add(exHit.point);
                            Vector3 exNormal = -exHit.normal;
                            Vector3 exDir = Refract(
                                enterRefractDir,
                                exNormal,
                                colorSpecificN,
                                1.0f
                            );
                            colorRay = new Ray(exHit.point + exDir * 0.001f, exDir);
                            colorBounce++;
                        }
                        else
                        {
                            colorRay = new Ray(
                                colorHit.point + enterRefractDir * 0.001f,
                                enterRefractDir
                            );
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    points.Add(colorRay.GetPoint(maxDistance));
                    break;
                }

                colorBounce++;
            }

            LineRenderer lr = dispersionLines[i];
            lr.positionCount = points.Count;
            lr.SetPositions(points.ToArray());
            lr.enabled = true;

            if (useVolumetricHalo && dispersionHalos.Count > i)
            {
                LineRenderer hLr = dispersionHalos[i];
                hLr.positionCount = points.Count;
                hLr.SetPositions(points.ToArray());
                hLr.enabled = true;
            }
        }
    }

    void DisableDispersionRays()
    {
        foreach (var lr in dispersionLines)
        {
            lr.enabled = false;
            lr.positionCount = 0;
        }
        foreach (var hLr in dispersionHalos)
        {
            hLr.enabled = false;
            hLr.positionCount = 0;
        }
        if (mainHaloRenderer != null)
        {
            mainHaloRenderer.enabled = false; // Tạm tắt halo chính nếu main tắt (dù không bao giờ tắt main)
        }
    }

    Vector3 Refract(Vector3 incident, Vector3 normal, float n1, float n2)
    {
        float cosi = -Vector3.Dot(incident, normal);
        float eta = n1 / n2;
        float sinT2 = eta * eta * (1f - cosi * cosi);

        if (sinT2 >= 1f)
        {
            return Vector3.Reflect(incident, normal);
        }

        float cosT = Mathf.Sqrt(1f - sinT2);
        return eta * incident + (eta * cosi - cosT) * normal;
    }

    void OnDestroy()
    {
        if (dispersionBeamMaterial == null && beamMaterial != null)
        {
            Destroy(beamMaterial);
        }
    }
}
