using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using TMPro;

public class ScenarioManager : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Kéo TextMeshPro UI hiển thị bảng chỉ dẫn vào đây")]
    public TMP_Text menuBoardText;

    [Header("Dependencies")]
    [Tooltip("Kéo Cây Laser chiếu Khúc Xạ vào đây để hệ thống theo dõi chiết suất môi trường")]
    public LightBeamPhysics mainLaser;
    [Tooltip("Kéo object chứa Script OpticVisualAids của cây Laser vào đây")]
    public OpticVisualAids opticVisualAids;
    [Tooltip("Kéo Bể Nước (Water Tank) vào đây để check bài thay đổi chiết suất")]
    public RefractiveMaterial targetWaterTank;
    [Tooltip("Kéo cây Laser dùng cho Lăng Kính vào (để check cầu vồng đập vào bảng)")]
    public LightBeamPhysics dispersionPrismLaser;

    [Header("Audio (Lồng tiếng / Thông báo)")]
    [Tooltip("Kéo AudioSource dùng để phát âm thanh vào đây")]
    public AudioSource scenarioAudioSource;
    [Tooltip("Danh sách 10 file âm thanh tương ứng với 10 Case trong switch(currentStep)")]
    public AudioClip[] stepClips;


    [Header("Dynamic Objects (Tự động Hiện/Ẩn)")]
    [Tooltip("Lắp Nút bấm đổi chiết suất (hoặc Canvas chứa nút) vào đây để nó tàng hình ở Vòng 1")]
    public GameObject indexButtonPanel;
    [Tooltip("Lắp Bản thể Lăng Kính (Prism) vào đâyt")]
    public GameObject prismObject;

    [Header("Events")]
    public UnityEvent onStepCompleted;
    public UnityEvent onAllComplete;

    [Header("Runtime State")]
    [Range(0, 9)]
    public int currentStep = 0;

    // Thời gian chờ cho mỗi bước (tính bằng giây). 
    // Các bước bằng 0 là các bước Thử thách (cần bắt thao tác tay của người học thay vì chờ giờ)
    // 0: Chào -> 1: Hỏi khúc xạ -> 2: Giải thích khúc xạ -> 3: Task Khúc xạ -> 4: Task Phản xạ toàn phần (TIR) -> 5: Giải thích TIR -> 6: Hỏi cầu vồng -> 7: Giải thích tán sắc -> 8: Task Tán sắc -> 9: Kết thúc
    private float[] stepDurations = { 7f, 7f, 8f, 0f, 0f, 9f, 6f, 9f, 0f, 0f };

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

    // Tự động kiểm tra điều kiện hoàn thành thử thách
    void CheckAutoProgress()
    {
        if (currentStep == 3) // THỬ THÁCH 1 – KHÚC XẠ: góc 35-40
        {
            if (opticVisualAids != null && !opticVisualAids.isTIR)
            {
                if (opticVisualAids.currentAngleR >= 35f && opticVisualAids.currentAngleR <= 40f)
                {
                    AdvanceStepWithDelay(2f); // Delay 2s để học sinh có thời gian "Ngắm" lại thành quả
                }
            }
        }
        else if (currentStep == 4) // THAY ĐỔI CHIẾT SUẤT MÔI TRƯỜNG + THỬ NGHIỆM TIR
        {
            // Kiểm tra xem đã tăng n Môi Trường > n Nước (tạo điều kiện ngược TIR)
            if (mainLaser != null && targetWaterTank != null)
            {
                if (mainLaser.environmentRefractiveIndex > targetWaterTank.refractiveIndex)
                {
                    if (opticVisualAids != null && opticVisualAids.isTIR)
                    {
                        AdvanceStepWithDelay(3f);
                    }
                }
            }
        }
        else if (currentStep == 8) // THỰC HÀNH PRISM
        {
            // Bài Lăng kính: Chỉ cần bắt được tia chớp Lăng kính phân tách 7 màu là ăn điểm
            if (dispersionPrismLaser != null && dispersionPrismLaser.isHittingPrism)
            {
                AdvanceStepWithDelay(3f);
            }
        }
    }

    void AdvanceStepWithDelay(float delay)
    {
        if (!isWaitingToAdvance)
        {
            isWaitingToAdvance = true;
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
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
        if (menuBoardText == null) return;
        isWaitingToAdvance = false; // Reset cờ chờ khi nhảy bài

        // Tự động phát âm thanh mỗi khi chuyển bước (nếu có)
        PlayStepAudio();

        // Quản lý Ẩn/Hiện vật thể theo từng Màn
        ManageObjectVisibility();


        switch (currentStep)
        {
            case 0:
                menuBoardText.text = "<b>XIN CHÀO!</b>\nChào mừng bạn đến với phòng thí nghiệm vật lý ánh sáng thực tế ảo (VR).\n\n<i>(Hệ thống tự động tiếp tục sau vài giây...)</i>";
                break;
            case 1:
                menuBoardText.text = "<b>ĐẶT VẤN ĐỀ:</b>\nBạn có bao giờ nhìn thấy một chiếc đũa hoặc một vật thể đi vào trong nước bị \"gãy\" hoặc cong đi chưa?\n\nTại sao ánh sáng lại không đi thẳng như bình thường?";
                break;
            case 2:
                menuBoardText.text = "<b>GIẢI THÍCH:</b>\nĐó là do hiện tượng <b>Khúc Xạ Ánh Sáng</b>.\n\nKhi ánh sáng đi từ môi trường này sang môi trường khác (ví dụ: không khí → nước), tốc độ ánh sáng thay đổi làm tia sáng bị bẻ cong.\n\nĐịnh lý này được miêu tả bằng Định luật Snell.";
                break;
            case 3:
                menuBoardText.text = "<b>THỬ THÁCH 1: TÌM GÓC KHÚC XẠ</b>\nHãy dùng tay xoay đèn Laser chiếu vào Bể nước sao cho bắt được <b>góc khúc xạ r</b> ở nửa dưới mặt nước dao động trong khoảng <b>35° - 40°</b>.\n\n<i>Hệ thống tự động quét tia sáng của bạn...</i>";
                break;
            case 4:
                menuBoardText.text = "<b>THỬ THÁCH 2: PHẢN XẠ TOÀN PHẦN</b>\n Hãy <b>Bấm Nút</b> để bơm hóa chất, biến đổi <b>Chiết suất của Môi trường bên ngoài</b> sao cho đặc hơn cả khối Nước (n Môi trường > n Nước).\n\n Nhờ Môi trường giờ đây đặc hơn Nước, hãy thử chiếu Laser chéo góc từ ngoài vào bể để quan sát ánh sáng bị phản dội ngược lại hoàn toàn (TIR)!";
                break;
            case 5:
                menuBoardText.text = "<b>GIẢI THÍCH PHẢN XẠ TOÀN PHẦN:</b>\nTuyệt vời! Bạn vừa tự tay tạo ra hiện tượng <b>Phản xạ Toàn phần</b>.\nÁnh sáng bị dội ngược lại như một tấm gương soi do nó muốn đi từ môi trường chiết suất Cao (Khí) sang Thấp (Nước), nhưng lại va đập với ranh giới ở góc quá lớn khiến tia sáng bị dội ngược trở lại.\n(Đây là nguyên lý của cáp quang truyền Internet).";
                break;
            case 6:
                menuBoardText.text = "<b>ĐẶT VẤN ĐỀ:</b>\nLại một câu hỏi nữa!\n\nBạn có bao giờ nhìn thấy Cầu vồng lấp lánh xuất hiện sau cơn mưa rào?\nHoặc ánh sáng trắng khi đi qua những con suốt kim cương lăng kính lại tách thành 7 màu lấp lánh?";
                break;
            case 7:
                menuBoardText.text = "<b>GIẢI THÍCH TÁN SẮC:</b>\nHiện tượng này gọi là <b>Tán sắc ánh sáng</b>.\nÁnh sáng trắng thực chất gồm 7 dải màu gộp lại (Đỏ -> Tím).\nMỗi màu có tần số và mức độ bẻ cong khác nhau: Tím bẻ cong nhiều nhất, Đỏ bẻ cong ít nhất.\n Kết quả: Chúng tách rời nhau ra khỏi dòng tụ tạo thành cầu vồng vạn hoa.";
                break;
            case 8:
                menuBoardText.text = "<b>THỰC HÀNH TÁN SẮC:</b>\n Nhiệm vụ: Hãy cầm đèn Laser và chiếu thẳng vào khối <b>Lăng Kính (Prism)</b>.\n\nQuan sát xem làm thế nào ánh sáng trắng tĩnh lặng bị khối thủy tinh tách cấu trúc thành 7 dải màu cầu vồng rực rỡ nhé!";
                break;
            case 9:
                menuBoardText.text = "<b>KẾT THÚC BÀI HỌC!</b>\nTuyệt vời! Hoàn hảo!\nBạn đã tự tay thực hành xong bộ thí nghiệm Khúc Xạ, Phản Xạ Toàn Phẩn (TIR) và Tán Sắc Lăng Hình.\nCảm ơn bạn đã trải nghiệm VRStem như một nguyên lý khoa học thực thụ!";
                break;
        }

        // Tự động đếm giờ để chuyển bước nếu là Lý thuyết
        if (stepDurations[currentStep] > 0)
        {
            if (autoAdvanceCoroutine != null) StopCoroutine(autoAdvanceCoroutine);
            autoAdvanceCoroutine = StartCoroutine(AutoAdvanceRoutine(stepDurations[currentStep]));
        }
    }

    IEnumerator AutoAdvanceRoutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        AdvanceStep();
    }

    void ManageObjectVisibility()
    {
        bool showWater = false;
        bool showButtons = false;
        bool showPrism = false;

        // Từ Bước 0 đến 3 (Học Khúc Xạ): Chỉ hiện Bể nước + Laser Khúc Xạ
        if (currentStep >= 0 && currentStep <= 3)
        {
            showWater = true;
            if (mainLaser != null) mainLaser.environmentRefractiveIndex = 1.0f; // Luôn đảm bảo n=1 lúc Học
        }
        // Bước 4, 5 (Phản Xạ Toàn Phần): Hiện Bể nước + Hiện Nút bấm hóa chất
        else if (currentStep == 4 || currentStep == 5)
        {
            showWater = true;
            showButtons = true; // Hiện Nút bấm n=2
        }
        // Bước 6 đến 9 (Tán Sắc Cầu Vồng): Ẩn Nước, Ẩn Nút, Hiện Prism + Laser Prism, Reset n=1
        else if (currentStep >= 6 && currentStep <= 9)
        {
            showPrism = true;
            if (mainLaser != null) mainLaser.environmentRefractiveIndex = 1.0f; // Xả môi trường về n=1
        }

        // Bật/Tắt các vật thể trên màn hình VR (Nước, Nút bấm, Lăng kính)
        if (targetWaterTank != null) targetWaterTank.gameObject.SetActive(showWater);
        if (indexButtonPanel != null) indexButtonPanel.gameObject.SetActive(showButtons);
        if (prismObject != null) prismObject.gameObject.SetActive(showPrism);
    }

    // Hàm phụ trợ phát âm thanh cho từng bước
    private void PlayStepAudio()
    {
        if (scenarioAudioSource != null && stepClips != null && currentStep < stepClips.Length)
        {
            // Dừng âm thanh đang phát (nếu có) để tránh chồng chéo
            scenarioAudioSource.Stop();

            // Phát âm thanh của bước hiện tại nếu file đó tồn tại
            if (stepClips[currentStep] != null)
            {
                scenarioAudioSource.clip = stepClips[currentStep];
                scenarioAudioSource.Play();
            }
        }
    }
}

