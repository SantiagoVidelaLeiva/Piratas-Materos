using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject bubblePanel;  
    [SerializeField] private TMP_Text tutorialText;   

    private Dictionary<int, string> tutorialDictionary;
    private int currentStep = 0;

    void Awake()
    {
        tutorialDictionary = new Dictionary<int, string>()
        {
            { 0, "Puse este script para saludar a los profes" },
            { 1, "Pongan lo que quieran " },
            { 2, "Si lo quieren usar obvio" },
            { 3, "Para saltar una linea usen alt+92 y despues la n" },
            { 4, "¡Saludos a todos! :)" }
        };

        ShowStep(0);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            NextStep();
        }
    }
    public void ShowStep(int step)
    {
        if (tutorialDictionary.ContainsKey(step))
        {
            bubblePanel.SetActive(true);
            tutorialText.text = tutorialDictionary[step];
            currentStep = step;
        }
    }

    public void NextStep()
    {
        currentStep++;
        if (tutorialDictionary.ContainsKey(currentStep))
            ShowStep(currentStep);
        else
            bubblePanel.SetActive(false); 
    }
}