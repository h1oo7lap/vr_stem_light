using UnityEngine;

public class EnvironmentStarter : MonoBehaviour
{
    public Light sun;
    public Material skyboxMaterial;
    public GameObject menuBoard;
    public GameObject plane;
    public GameObject hhtiScene;
    public GameObject startButton;

    private void Start()
    {
        // 1. Chỉnh môi trường về màu đen tuyệt đối
        RenderSettings.skybox = null; // Bỏ skybox
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = Color.black; // Chỉnh ánh sáng môi trường thành màu đen

        // 2. Tắt ánh sáng mặt trời
        if (sun != null)
            sun.intensity = 0f;

        // 3. Ẩn các GameObject
        if (menuBoard != null)
            menuBoard.SetActive(false);
        if (hhtiScene != null)
            hhtiScene.SetActive(false);

        // 4. Ẩn Plane nhưng giữ va chạm
        if (plane != null && plane.GetComponent<MeshRenderer>() != null)
        {
            plane.GetComponent<MeshRenderer>().enabled = false;
        }
    }

    public void StartEnvironment()
    {
        // 1. Bật lại bầu trời và ánh sáng môi trường
        if (skyboxMaterial != null)
        {
            RenderSettings.skybox = skyboxMaterial;
            // Trả lại chế độ ánh sáng theo Skybox
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        }

        // 2. Bật ánh sáng mặt trời
        if (sun != null)
            sun.intensity = 1f;

        // 3. Hiện các đối tượng và Plane
        if (menuBoard != null)
            menuBoard.SetActive(true);
        if (hhtiScene != null)
            hhtiScene.SetActive(true);
        if (plane != null && plane.GetComponent<MeshRenderer>() != null)
        {
            plane.GetComponent<MeshRenderer>().enabled = true;
        }

        // 4. Cập nhật lại toàn bộ ánh sáng cảnh vật
        DynamicGI.UpdateEnvironment();

        // 5. Ẩn nút Start
        if (startButton != null)
            startButton.SetActive(false);
    }
}
