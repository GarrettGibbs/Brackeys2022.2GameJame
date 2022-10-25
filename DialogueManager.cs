using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public DialogueTrigger startOfLevel;
    public DialogueTrigger restartLevel;
    public DialogueTrigger endOfLevel;
    [SerializeField] LevelManager levelManager;

    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text dialogueText;
    [SerializeField] GameObject dialogueBox;
    [SerializeField] Image portraitImage;
    //[SerializeField] Sprite[] portraits;
    private Queue<string> sentences;
    private Dialogue[] dialogues;
    private int dialogueIndex = 0;

    public bool inConversation = false;
    public float timeSinceEndOfConversation = 0;

    public UnityEvent dialogueDone;

    void Awake()
    {
        sentences = new Queue<string>();
    }

    private void Update() {
        if(Input.GetKeyDown(KeyCode.Space) && inConversation) {
            DisplayNextSentence();
        } else if (!inConversation) {
            timeSinceEndOfConversation += Time.deltaTime;
        }
    }

    public void StartDialogue(Dialogue[] d) {
        dialogues = d;
        inConversation = true;
        LeanTween.moveLocalY(dialogueBox, -285, .6f);
        sentences.Clear();
        dialogueIndex = 0;

        NextDialogue();

        DisplayNextSentence();
    }

    private void NextDialogue() {
        levelManager.audioManager.PlayScribble();
        nameText.text = dialogues[dialogueIndex].name;
        //Sprite s = Array.Find(portraits, s => s.name == dialogues[dialogueIndex].name);
        Sprite s = Resources.Load<Sprite>($"Sprites/Portraits/{dialogues[dialogueIndex].name}");
        if (s == null) {
            Debug.LogWarning($"Portrait: {dialogues[dialogueIndex].name} not found");
            portraitImage.gameObject.SetActive(false);
        } else {
            portraitImage.gameObject.SetActive(true);
            portraitImage.sprite = s;
        }

        foreach (string sentence in dialogues[dialogueIndex].sentences) {
            sentences.Enqueue(sentence);
        }
    }
    
    public void DisplayNextSentence() {
        if(sentences.Count == 0 && dialogueIndex < dialogues.Length-1) {
            dialogueIndex++;
            NextDialogue();
        } else if (sentences.Count == 0) {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence) {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray()) {
            dialogueText.text += letter;
            yield return null;
        }
    }

    private void EndDialogue() {
        timeSinceEndOfConversation = 0f;
        Time.timeScale = 1f;
        LeanTween.moveLocalY(dialogueBox, -1000, .6f);
        inConversation = false;
        dialogueDone.Invoke();
    }
}
