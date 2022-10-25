using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class CutSceneController : MonoBehaviour
{
    [SerializeField] LevelManager levelManager;
    [SerializeField] GameObject[] openingSlides;
    [SerializeField] GameObject[] closingSlides;
    [SerializeField] DialogueTrigger openingDialogue;
    [SerializeField] DialogueTrigger closingDialogue;

    GameObject[] slides;
    int currentSlide = 0;

    bool cutscenedone = false;

    private async void Start() {
        levelManager.audioManager.TransitionMusic(MusicType.Dialogue);
        levelManager.progressManager.currentLevel = 4;
        levelManager.progressManager.leftCutscene = false;
        await Task.Delay(1000);
        levelManager.circleTransition.OpenBlackScreen();
        await Task.Delay(500);
        if (levelManager.progressManager.endofShow) {
            slides = closingSlides;
            closingDialogue.TriggerDialogue();
        } else {
            slides = openingSlides;
            openingDialogue.TriggerDialogue();
        }
        slides[0].SetActive(true);
    }

    private void Update() {
        if (cutscenedone) return;
        if (Input.GetKeyDown(KeyCode.Space) && levelManager.dialogueManager.inConversation) {
            NextSlide();
        } 
    }

    public void CutSceneOver() {
        levelManager.NextLevel();
    }

    private void NextSlide() {
        GameObject prevSlide = slides[currentSlide];
        LeanTween.value(gameObject, 1f, 0f, .2f).setOnUpdate((float val) => {
            prevSlide.GetComponent<CanvasGroup>().alpha = val;
        }).setOnComplete(()=> prevSlide.SetActive(false));
        currentSlide++;
        if(currentSlide >= slides.Length) {
            cutscenedone = true;
            return;
        }
        slides[currentSlide].SetActive(true);
        //LeanTween.value(gameObject, 0f, 1f, .2f).setOnUpdate((float val) => {
        //    slides[currentSlide].GetComponent<CanvasGroup>().alpha = val;
        //});
    }
}
