using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternManager : MonoBehaviour
{
    public static bool userControlledMode = true;
    public static int NUM_OF_COLLISON;
    public int numOfCollision;
    
    [SerializeField] bool adjustSimulationSpeed = true;
    [SerializeField] float simulationSpeed = 80f;
    float correctCycle = 2.5f;
    float handPosDiff = 0.65f;
    float handRange = 0.11f;

    [SerializeField] string patternType;
    [SerializeField] float cosScale = 3f;
    [SerializeField] float offset = 0.00f;
    public static float COS_SCALE;

    public int ballsInPattern = 3;
    [SerializeField] float patternSpeed = 5f / 3f;

    [SerializeField] Vector3 basicVector;
    [SerializeField] Vector3 basicOddPatternVector = new Vector3(0f, 7.1f, 0.47f);
    [SerializeField] Vector3 basicEvenPatternVector = new Vector3(0f, 10.7f, -0.08f);
    [SerializeField] float maxErrorZ = 0.05f;
    [SerializeField] float maxErrorY = 0.05f;
    [SerializeField] float basicHandTime = 0.7f;
    [SerializeField] float basicAirTime = 0.7f;

    [SerializeField] float basicRotYAmount = -60f;
    [SerializeField] float basicRotZAmount = -60f;

    [SerializeField] float basicROT_Y_TIME = 0.7f;
    [SerializeField] float basicROT_Z_TIME = 0.7f;

    [SerializeField] float timeScale = 0.5f;

    [SerializeField] GameObject[] balls;
    [SerializeField] int ballCount = 13;

    [SerializeField] RotationPattern leftSide;
    [SerializeField] RotationPattern rightSide;

    [SerializeField] HandlingBall leftHand;
    [SerializeField] HandlingBall rightHand;
    bool evenPattern = false;

    public bool timeScaleSpeed = false;
    [SerializeField] float speedUpTimeInterval = 1f;

    public string scriptName = "HandlingBall";
    public float preJuggDelay = 1f;
    public float jugglingTime = 3f;

    private void Awake()
    {
        if (ballsInPattern % 2 == 0)
            evenPattern = true;

        if (evenPattern)
            basicVector = basicOddPatternVector;
        else
            basicVector = basicEvenPatternVector;

        SetCorrectVector();
        SetCorrectSpeed();
        SetPatternRotation();
        SetUpPatternStats();
        DistributeBalls();

        if(adjustSimulationSpeed)
            timeScale = Mathf.Abs(simulationSpeed / basicRotYAmount);
    }

    private void SetCorrectVector()
    {
        float yVector, zVector;
        yVector = basicVector.y / patternSpeed;
        print(yVector);
        zVector = basicVector.z * basicVector.y / yVector;
        print(zVector);
        basicVector = new Vector3(basicVector.x, yVector, zVector);
    }

    private void SetCorrectSpeed()
    {
        float ballMaths;

        if (evenPattern)
            ballMaths = (ballsInPattern - 1f) / 3f;
        else
            ballMaths = (ballsInPattern - 1) / 2;

        basicRotYAmount *= patternSpeed * ballMaths;
        basicRotZAmount *= patternSpeed * ballMaths;

        basicROT_Y_TIME /= (patternSpeed * ballMaths);
        basicROT_Z_TIME /= (patternSpeed * ballMaths);

        basicHandTime /= (patternSpeed * ballMaths);
        basicAirTime /= (patternSpeed * ballMaths);

        cosScale = (cosScale / basicROT_Y_TIME);
        COS_SCALE = cosScale;
    }

    private void SetPatternRotation()
    {
        leftSide.rotZSpeed = basicRotZAmount;
        leftSide.rotYSpeed = basicRotYAmount;
        leftSide.ROT_Z_TIME = basicROT_Z_TIME;
        leftSide.ROT_Y_TIME = basicROT_Y_TIME;

        rightSide.rotZSpeed = basicRotZAmount;
        rightSide.rotYSpeed = -basicRotYAmount;
        rightSide.ROT_Z_TIME = basicROT_Z_TIME;
        rightSide.ROT_Y_TIME = basicROT_Y_TIME;

        if(patternType == "Reverse Cascade")
        {
            correctCycle = 1.5f;
            leftSide.rotZSpeed *= -1;
            rightSide.rotZSpeed *= -1;

            if (ballsInPattern % 2 == 0)
                basicVector.z *= -1;
        }
    }

    private void SetUpPatternStats()
    {
        leftHand.maxErrorZ = maxErrorZ;
        leftHand.maxErrorY = maxErrorY;
        rightHand.maxErrorZ = maxErrorZ;
        rightHand.maxErrorY = maxErrorY;

        leftHand.properVector = basicVector;
        rightHand.properVector = new Vector3(basicVector.x, basicVector.y, -basicVector.z);

        leftHand.initialDelay = basicHandTime * correctCycle + offset * 2;
        rightHand.initialDelay = basicHandTime * correctCycle + offset * 2;

        if (ballsInPattern % 2 == 1)
        {
            leftHand.patternTime = basicAirTime * 2 + offset;
            rightHand.patternTime = basicAirTime * 2 + offset;
        }
        else
        {
            leftHand.patternTime = basicAirTime * 2f + offset;
            rightHand.patternTime = basicAirTime * 2f + offset;
        }
    }

    private void DistributeBalls()
    {
        for (int i = 0; i < ballCount && i < balls.Length; i++)
        {
            balls[i].gameObject.SetActive(true);

            if (i % 2 == 0)
            {
                leftHand.myBalls.Add(balls[i]);
                leftHand.ballsInHand.Push(balls[i]);
            }
            else
            {
                rightHand.myBalls.Add(balls[i]);
                rightHand.ballsInHand.Push(balls[i]);
            }
        }
    }

    public static void AddCollision()
    {
        NUM_OF_COLLISON++;
    }

    void Start()
    {
        Time.timeScale = timeScale;
        Invoke("StartJuggling", preJuggDelay);
    }

    private void IncreaseTimeScale()
    {
        timeScale += 0.05f;
        Time.timeScale = timeScale;
    }

    void Update()
    {
        numOfCollision = NUM_OF_COLLISON / 2;

        if (Input.GetKeyDown(KeyCode.M))
        {
            userControlledMode = !userControlledMode;
            if (userControlledMode)
            {
                CancelInvoke("StartJuggling");
            }
            else
            {
                Invoke("StartJuggling", preJuggDelay);
            }
        }
    }

    private void ToggleCatchingMode()
    {
        HandlingBall[] scripts = FindObjectsOfType<HandlingBall>();

        foreach (HandlingBall script in scripts)
        {
            script.catchingMode = !script.catchingMode;
        }
    }

    private void StartJuggling()
    {
        MonoBehaviour[] scripts = FindObjectsOfType<RotationPattern>();

        foreach (MonoBehaviour script in scripts)
        {
            script.enabled = !script.enabled;
        }

        Invoke("ToggleCatchingMode", jugglingTime);
        InvokeRepeating("IncreaseTimeScale", speedUpTimeInterval, speedUpTimeInterval);
    }
}
