using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnOrder : MonoBehaviour
{
    [SerializeField] Image face;
    [SerializeField] Image indicator;
    [SerializeField] TMP_Text healthText;
    public Sprite portrait;
    public int actionPoints = 3;
    public GameObject[] apIcons;
    public Figure figure;

    public void SetupIndicator(Figure f) {
        figure = f;
        portrait = f.portrait;
        healthText.text = figure.health.ToString();
        face.sprite = portrait;
    }

    public void ToggleIndicator(bool onOff) {
        if (onOff) {
            indicator.color = new Color(0f/255f, 255f/255f, 253f/255f);
        } else {
            indicator.color = Color.white;
        }
    }

    public void UpdateActionPoints(int change) {
        actionPoints += change;
        for (int i = 0; i < apIcons.Length; i++) {
            if(actionPoints > i) {
                apIcons[i].SetActive(true);
            } else {
                apIcons[i].SetActive(false);
            }
        }
    }

    public void SelectFigure() {
        figure.Eman.lm.audioManager.PlaySound("Click");
        if (actionPoints < 1 || !figure.Eman.playersAction || !figure.playerFigure) return;
        figure.Eman.SelectFigure(figure);
    }

    public void UpdateHealth(int newHealth) {
        healthText.text = newHealth.ToString();
    }
}
