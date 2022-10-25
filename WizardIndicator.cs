using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WizardIndicator : MonoBehaviour
{
    [SerializeField] GameObject description;

    public void OnHover() {
        description.SetActive(true);
    }

    public void OnExit() {
        description.SetActive(false);
    }
}
