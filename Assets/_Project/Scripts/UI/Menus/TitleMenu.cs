using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI
{
    public class TitleMenu : MonoBehaviour
    {
        public void StartGame()
        {
            SceneManager.LoadScene(1);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}
