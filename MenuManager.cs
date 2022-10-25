using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    [SerializeField] Button[] levels;
    [SerializeField] GameObject wonBanner;

    private async void Start() {
        levelManager.progressManager.endofShow = false;
        SetupButtons();
        levelManager.audioManager.TransitionMusic(MusicType.Main);
        if(levelManager.progressManager.gameCompleted) {
            wonBanner.SetActive(true);
        }
        if (!levelManager.progressManager.firstTimeAtMenu) {
            await Task.Delay(1000);
            levelManager.circleTransition.OpenBlackScreen();
        } else {
            levelManager.progressManager.firstTimeAtMenu = false;
        }
    }

    private void SetupButtons() {
        for (int i = 0; i < levels.Length; i++) {
            if (i == 0) continue;
            if (levelManager.progressManager.levelsStared[i]) {
                levels[i].interactable = true;
            } else {
                levels[i].interactable = false;
            }
        }
    }

    public async void LoadLevel(int levelIndex) {
        levelManager.audioManager.PlaySound("Click");
        levelManager.progressManager.levelsStared[levelIndex] = false;
        levelManager.circleTransition.CloseBlackScreen();
        await Task.Delay(1000);
        switch (levelIndex) {
            case 0:
                if (levelManager.progressManager.levelsStared[0]) {
                    SceneManager.LoadScene(1);
                } else {
                    SceneManager.LoadScene(4);
                }
                break;
            case 1:
                SceneManager.LoadScene(2);
                break;
            case 2:
                SceneManager.LoadScene(3);
                break;

        }
    }

    public void QuitGame() {
        Application.Quit();
    }
}
