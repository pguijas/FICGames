using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransicioner : MonoBehaviour{
    
    public void GoMainMenu(){
        SceneManager.LoadScene("MainMenu");
    }

    public void GoLevel(string level){
        SceneManager.LoadScene(level);
    }


    public void Exit(){
        Application.Quit();
    }
}
