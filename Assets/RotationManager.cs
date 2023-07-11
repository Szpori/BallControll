using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationManager : MonoBehaviour
{
    public string scriptName = "HandlingBall"; // Name of the script you want to turn on/off
    public float preJuggDelay = 1f; // Delay in seconds
    public float jugglingTime = 3f;

    private void Start()
    {
        //InvokeRepeating("ToggleScripts", delay, delay); // Invoke the method to toggle the scripts after the specified delay
        Invoke("StartJuggling", preJuggDelay);
    }

    private void ToggleCatchingMode()
    {
        HandlingBall[] scripts = FindObjectsOfType<HandlingBall>(); // Find all MonoBehaviours in the scene

        foreach (HandlingBall script in scripts)
        {
           script.catchingMode = !script.catchingMode;
        }
    }

    private void StartJuggling()
    {
        MonoBehaviour[] scripts = FindObjectsOfType<RotationPattern>(); // Find all MonoBehaviours in the scene

        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = !script.enabled;
        }

        Invoke("ToggleCatchingMode", jugglingTime);
    }


}
