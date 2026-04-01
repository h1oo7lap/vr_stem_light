using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Hàm linh hoạt: Nhập tên bất kỳ cảnh nào vào ô String trong Inspector
    public void LoadSceneByName(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void LoadLightLab_1()
    {
        SceneManager.LoadScene("LightLab_1");
    }

    public void LoadLightLab_2()
    {
        SceneManager.LoadScene("LightLab_2");
    }

    public void LoadLightLab_0()
    {
        SceneManager.LoadScene("LightLab_0");
    }
}
