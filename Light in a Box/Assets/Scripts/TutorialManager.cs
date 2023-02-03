using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] public GameObject[] tutorialPrompts;
    public int promptIndex;

    public void UpdateDisplayedPrompt()
    {
        for (int i = 0; i < tutorialPrompts.Length; i++)
        {
            if (i == promptIndex)            
                tutorialPrompts[i].SetActive(true);
            else
                tutorialPrompts[i].SetActive(false);
        }
    }

    public void ResetTutorialIndex()
    {
        promptIndex = 0;
        tutorialPrompts[2].transform.GetChild(0).gameObject.SetActive(true);
        tutorialPrompts[3].transform.GetChild(0).gameObject.SetActive(true);
        UpdateDisplayedPrompt();
    }
}
