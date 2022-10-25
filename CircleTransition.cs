using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleTransition : MonoBehaviour
{
    //https://www.youtube.com/watch?v=E_RXATRuAFQ
    public Transform player;
    [SerializeField] AudioManager audioManager;
    
    private Canvas canvas;
    private Image blackScreen;

    private Vector2 playerCanvasPos;

    public static CircleTransition instance;

    private void Awake() {
        if (instance == null) {
            instance = this;
        } else {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(this);
        canvas = GetComponent<Canvas>();
        blackScreen = GetComponentInChildren<Image>();
    }

    //private void Update() {
    //    if (Input.GetKeyDown(KeyCode.Alpha1)) {
    //        OpenBlackScreen();
    //    } else if (Input.GetKeyDown(KeyCode.Alpha2)) {
    //        CloseBlackScreen();
    //    }
    //}

    public void OpenBlackScreen() {
        blackScreen.gameObject.SetActive(true);
        DrawBlackScreen();
        //audioManager.PlaySound("TransitionIn");
        StartCoroutine(Transition(1, 0, 1.2f));
    }

    public void CloseBlackScreen() {
        blackScreen.gameObject.SetActive(true);
        DrawBlackScreen();
        //audioManager.PlaySound("TransitionOut");
        StartCoroutine(Transition(1, 1.2f, 0));
    }

    private void DrawBlackScreen() {
        var screenWidth = Screen.width;
        var screenHeight = Screen.height;
        Vector3 playerScreenPos;
        if (player != null) {
            playerScreenPos = Camera.main.WorldToScreenPoint(player.position);
        } else {
            playerScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        }
        

        var canvasRect = canvas.GetComponent<RectTransform>();
        var canvasWidth = canvasRect.rect.width;
        var canvasHeight = canvasRect.rect.height;

        playerCanvasPos = new Vector2 { x = (playerScreenPos.x / screenWidth) * canvasWidth, y = (playerScreenPos.y / screenHeight) * canvasHeight };

        var squareValue = 0f;
        if(canvasWidth > canvasHeight) {
            squareValue = canvasWidth;
            playerCanvasPos.y += (canvasWidth - canvasHeight) * 0.5f; 
        } else {
            squareValue = canvasHeight;
            playerCanvasPos.x += (canvasHeight - canvasWidth) * 0.5f;
        }

        playerCanvasPos /= squareValue;
        blackScreen.material.SetFloat("_CenterX", playerCanvasPos.x);
        blackScreen.material.SetFloat("_CenterY", playerCanvasPos.y);

        blackScreen.rectTransform.sizeDelta = new Vector2(squareValue, squareValue);
    }

    private IEnumerator Transition(float duration, float beginRadius, float endRadius) {
        var time = 0f;
        while(time <= duration) {
            time += Time.deltaTime;
            var t = time / duration;
            var radius = Mathf.Lerp(beginRadius, endRadius, t);

            blackScreen.material.SetFloat("_Radius", radius);

            yield return null;
        }
        if(endRadius != 0) {
            blackScreen.gameObject.SetActive(false);
        }
    }
}
