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

    void Awake() //Para saltar una linea usen alt+92 y despues la n
    {
        tutorialDictionary = new Dictionary<int, string>()
        {
            { 0, "Estos son los controles del juego, presiona espacio para continuar" },
            { 1, "ASWD son el movimiento del personaje pero tambien puedes usar las flechas" },
            { 2, "CTRL para agacharse, Shift para correr y usa el mouse para mover la camara" },
            { 3, "Presiona C  para cambiar de modo y en el otro modo usa las flechas para cambiar de camara" },
            { 4, "Prueba usar la tecla E detras de este enemigo, ¡Buena Suerte y Saludos! :)" }
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