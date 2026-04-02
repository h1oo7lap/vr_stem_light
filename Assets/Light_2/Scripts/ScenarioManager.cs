using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ScenarioManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text menuBoardText;

    [Header("Dependencies")]
    public LightPhysics mainLaser;
    public Visualization visualization;
    public RefractiveMaterial targetWaterTank;
    public LightPhysics dispersionPrismLaser;

    [Header("Audio")]
    public AudioSource scenarioAudioSource;
    public AudioClip[] stepClips;

    [Header("Images")]
    public MeshRenderer image;
    public Texture[] stepTextures;

    [Header("Dynamic Objects")]
    public GameObject indexButtonPanel;
    public GameObject prismObject;

    [Header("Events")]
    public UnityEvent onStepCompleted;
    public UnityEvent onAllComplete;

    [Header("Runtime State")]
    [Range(0, 9)]
    public int currentStep = 0;

    private Coroutine autoAdvanceCoroutine;
    private bool isWaitingToAdvance = false;

    void Start()
    {
        currentStep = 0;
        UpdateInstructions();
    }

    void Update()
    {
        CheckAutoProgress();
    }

    void AdvanceStep()
    {
        if (currentStep < 9)
        {
            currentStep++;
            onStepCompleted?.Invoke();
            UpdateInstructions();

            if (currentStep == 9)
            {
                onAllComplete?.Invoke();
            }
        }
    }

    // Kiểm tra hoàn thành THỬ THÁCH (Thao tác tay)
    void CheckAutoProgress()
    {
        // Chỉ kiểm tra khi KHÔNG trong trạng thái chờ nhảy bước tự động
        if (isWaitingToAdvance)
            return;

        if (currentStep == 3) // THỬ THÁCH 1: Khúc xạ
        {
            if (
                visualization != null
                && visualization.currentAngleR >= 35f
                && visualization.currentAngleR <= 40f
            )
            {
                AdvanceStepWithDelay(1.5f);
            }
        }
        else if (currentStep == 4) // THỬ THÁCH 2: TIR
        {
            if (mainLaser != null && targetWaterTank != null)
            {
                if (
                    mainLaser.environmentRefractiveIndex > targetWaterTank.refractiveIndex
                    && visualization.isTIR
                )
                {
                    AdvanceStepWithDelay(2f);
                }
            }
        }
        else if (currentStep == 8) // THỰC HÀNH: Prism
        {
            if (dispersionPrismLaser != null && dispersionPrismLaser.isHittingPrism)
            {
                AdvanceStepWithDelay(2f);
            }
        }
    }

    void AdvanceStepWithDelay(float delay)
    {
        if (!isWaitingToAdvance)
        {
            isWaitingToAdvance = true;
            if (autoAdvanceCoroutine != null)
                StopCoroutine(autoAdvanceCoroutine);
            StartCoroutine(WaitAndAdvance(delay));
        }
    }

    IEnumerator WaitAndAdvance(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceStep();
    }

    public void UpdateInstructions()
    {
        if (menuBoardText == null)
            return;
        isWaitingToAdvance = false;

        // Quản lý Ẩn/Hiện vật thể
        ManageObjectVisibility();

        // Cập nhật nội dung bảng chữ
        UpdateTextContent();

        // LOGIC MỚI: Đợi Audio xong mới chuyển bước
        HandleAudioAndSequence();
    }

    private void HandleAudioAndSequence()
    {
        if (scenarioAudioSource == null || stepClips == null || currentStep >= stepClips.Length)
            return;

        AudioClip currentClip = stepClips[currentStep];

        if (currentClip != null)
        {
            // 1. Phát âm thanh
            scenarioAudioSource.Stop();
            scenarioAudioSource.clip = currentClip;
            scenarioAudioSource.Play();

            // 2. Kiểm tra xem bước này có phải là bước tự động chuyển (Lý thuyết) không?
            // Các bước Lý thuyết: 0, 1, 2, 5, 6, 7, 9
            // Các bước Thử thách: 3, 4, 8 (Sẽ đợi CheckAutoProgress)
            bool isTheoryStep = (currentStep != 3 && currentStep != 4 && currentStep != 8);

            if (isTheoryStep)
            {
                if (autoAdvanceCoroutine != null)
                    StopCoroutine(autoAdvanceCoroutine);
                // Đợi hết chiều dài Audio + 1 giây nghỉ rồi mới AdvanceStep
                autoAdvanceCoroutine = StartCoroutine(
                    AutoAdvanceAfterAudio(currentClip.length + 1.0f)
                );
            }
        }
    }

    IEnumerator AutoAdvanceAfterAudio(float duration)
    {
        isWaitingToAdvance = true;
        yield return new WaitForSeconds(duration);
        AdvanceStep();
    }

    void UpdateTextContent()
    {
        switch (currentStep)
        {
            case 0:
                menuBoardText.text =
                    "<b>XIN CHÀO!</b>\nChào mừng bạn đến với phòng thí nghiệm vật lý ánh sáng thực tế ảo (VR).";
                break;
            case 1:
                menuBoardText.text =
                    "<b>ĐẶT VẤN ĐỀ:</b>\nBạn có bao giờ nhìn thấy một chiếc đũa hoặc một vật thể đi vào trong nước bị \"gãy\" hoặc cong đi chưa?\n\nTại sao ánh sáng lại không đi thẳng như bình thường?";
                break;
            case 2:
                menuBoardText.text =
                    "<b>GIẢI THÍCH:</b>\nĐó là do hiện tượng <b>Khúc Xạ Ánh Sáng</b>.\n\nKhi ánh sáng đi từ môi trường này sang môi trường khác (ví dụ: không khí → nước), tốc độ ánh sáng thay đổi làm tia sáng bị bẻ cong.\n\nĐịnh lý này được miêu tả bằng Định luật Snell.";
                break;
            case 3:
                menuBoardText.text =
                    "<b>THỬ THÁCH 1: TÌM GÓC KHÚC XẠ</b>\nHãy dùng tay xoay đèn Laser chiếu vào Bể nước sao cho bắt được <b>góc khúc xạ r</b> ở nửa dưới mặt nước dao động trong khoảng <b>35° - 40°</b>.\n\n<i>Hệ thống tự động quét tia sáng của bạn...</i>";
                break;
            case 4:
                menuBoardText.text =
                    "<b>THỬ THÁCH 2: PHẢN XẠ TOÀN PHẦN</b>\n Hãy <b>Bấm Nút</b> để bơm hóa chất, biến đổi <b>Chiết suất của Môi trường bên ngoài</b> sao cho đặc hơn cả khối Nước (n Môi trường > n Nước).\n\n Nhờ Môi trường giờ đây đặc hơn Nước, hãy thử chiếu Laser chéo góc từ ngoài vào bể để quan sát ánh sáng bị phản dội ngược lại hoàn toàn (TIR)!";
                break;
            case 5:
                menuBoardText.text =
                    "<b>GIẢI THÍCH PHẢN XẠ TOÀN PHẦN:</b>\nTuyệt vời! Bạn vừa tự tay tạo ra hiện tượng <b>Phản xạ Toàn phần</b>.\nÁnh sáng bị dội ngược lại như một tấm gương soi do nó muốn đi từ môi trường chiết suất Cao (Khí) sang Thấp (Nước), nhưng lại va đập với ranh giới ở góc quá lớn khiến tia sáng bị dội ngược trở lại.\n(Đây là nguyên lý của cáp quang truyền Internet).";
                break;
            case 6:
                menuBoardText.text =
                    "<b>ĐẶT VẤN ĐỀ:</b>\nLại một câu hỏi nữa!\n\nBạn có bao giờ nhìn thấy Cầu vồng lấp lánh xuất hiện sau cơn mưa rào?\nHoặc ánh sáng trắng khi đi qua những con suốt kim cương lăng kính lại tách thành 7 màu lấp lánh?";
                break;
            case 7:
                menuBoardText.text =
                    "<b>GIẢI THÍCH TÁN SẮC:</b>\nHiện tượng này gọi là <b>Tán sắc ánh sáng</b>.\nÁnh sáng trắng thực chất gồm 7 dải màu gộp lại (Đỏ -> Tím).\nMỗi màu có tần số và mức độ bẻ cong khác nhau: Tím bẻ cong nhiều nhất, Đỏ bẻ cong ít nhất.\n Kết quả: Chúng tách rời nhau ra khỏi dòng tụ tạo thành cầu vồng vạn hoa.";
                break;
            case 8:
                menuBoardText.text =
                    "<b>THỰC HÀNH TÁN SẮC:</b>\n Nhiệm vụ: Hãy cầm đèn Laser và chiếu thẳng vào khối <b>Lăng Kính (Prism)</b>.\n\nQuan sát xem làm thế nào ánh sáng trắng tĩnh lặng bị khối thủy tinh tách cấu trúc thành 7 dải màu cầu vồng rực rỡ nhé!";
                break;
            case 9:
                menuBoardText.text =
                    "<b>KẾT THÚC BÀI HỌC!</b>\nTuyệt vời! Hoàn hảo!\nBạn đã tự tay thực hành xong bộ thí nghiệm Khúc Xạ, Phản Xạ Toàn Phẩn (TIR) và Tán Sắc Lăng Hình.\nCảm ơn bạn đã trải nghiệm VRStem như một nguyên lý khoa học thực thụ!";
                break;
        }
    }

    void ManageObjectVisibility()
    {
        bool showWater = (currentStep >= 0 && currentStep <= 5);
        bool showButtons = (currentStep == 4 || currentStep == 5);
        bool showPrism = (currentStep >= 6);
        bool showImage = (
            currentStep == 1
            || currentStep == 2
            || currentStep == 3
            // || currentStep == 4
            || currentStep == 5
            // || currentStep == 6
            || currentStep == 7
            || currentStep == 8
        );

        if (targetWaterTank != null)
            targetWaterTank.gameObject.SetActive(showWater);
        if (indexButtonPanel != null)
            indexButtonPanel.gameObject.SetActive(showButtons);
        if (prismObject != null)
            prismObject.gameObject.SetActive(showPrism);
        if (image != null)
        {
            image.gameObject.SetActive(showImage);
            image.material.SetTexture("_BaseMap", stepTextures[currentStep]);
        }

        // Reset chiết suất môi trường nếu cần
        if (mainLaser != null && (currentStep <= 3 || currentStep >= 6))
            mainLaser.environmentRefractiveIndex = 1.0f;
    }
}
