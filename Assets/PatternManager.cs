using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatternManager : MonoBehaviour
{


    //Error z = 0.05, Error y = 0.1
    // 5 ball 1.2 speed dla timeScale 0.8-0.95
    // 5 ball 1.3 speed dla timeScale 0.7
    // 5 ball 1.1 can't find right scaling
    // 5 ball 1.25 speed also can't find weird

    // prezentacja 10 min
    // optymalna wysokosc
    // dokumentacja
    // filmik z tymi patternami


    // 3 ball collision
    // pattern speed 1.7-2.0
    // error 0.05 0.09
    // 5 ball collision
    // pattern speed 1.2-1.5
    // eror 0.03 0.06
    // 7 ball collision
    // pattern speed 1.2-1.6
    // error 0.021 0.041


    // 3 ball collision
    // pattern speed 1.9-2.2
    // error 0.08 0.08
    // 5 ball collision
    // pattern speed 1.2-1.4
    // error 0.06 0.06
    // 7 ball collision
    // pattern speed 1.1-1.25
    // error 0.043 0.043
    // 9 ball collision
    // pattern speed 0.95-1.1
    // error 0.03 0.03

    // wysokosc = (7.1/2)/9.81 * (7.1/2)/2
    // wysokosc = (7.1/patternSpeed)/9.81 * (7.1/patternSpeed)/2

    // 9 ball collision
    // pattern speed 1-1.05, 2.33-2.57m
    // error 0.027 0.027
    // 7 ball collision
    // pattern speed 1.2-1.3, 1.52-1.78m
    // error 0.036 0.036 
    // 5 ball collision
    // pattern speed 1.25-1.4, 1.31-1.64m
    // error 0.05 0.05

    // wyskosc = (10.7/2)/9.81 * (10.7/2)/2
    // wyokosc = (10.7/patternSpeed)/9.81 * (10.7/patternSpeed)/2
    public static bool userControlledMode = false;


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
    // new Vector3(0f, 6.867f, 0.4642857f) wyliczone, new Vector3(0f, 7.1f, 0.47f) lepsze
    [SerializeField] Vector3 basicEvenPatternVector = new Vector3(0f, 10.7f, -(0.09f));
    // new Vector3(0f, 10.3005f, -(0.11f / 1.05f)) wyliczone, new Vector3(0f, 10.7f, -(0.09f)) lepsze
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

    [SerializeField] RotationPattern leftSide;
    [SerializeField] RotationPattern rightSide;

    [SerializeField] HandlingBall leftHand;
    [SerializeField] HandlingBall rightHand;
    bool evenPattern = false;

    public bool timeScaleSpeed = false;
    [SerializeField] float speedUpTimeInterval = 1f;

    public string scriptName = "HandlingBall"; // Name of the script you want to turn on/off
    public float preJuggDelay = 1f; // Delay in seconds
    public float jugglingTime = 3f;

    private void Awake()
    {


        if (ballsInPattern % 2 == 0)
            evenPattern = true;


        // teoretycznie odleglosc rowna srednicy kola, które zatacza ręka
        if (evenPattern)
            basicVector = basicOddPatternVector;
        else
            basicVector = basicEvenPatternVector;
        // teoretycznie odleglosc rowna odstępem między rękoma

        SetCorrectVector();
        SetCorrectSpeed();
        SetPatternRotation();
        SetUpPatternStats();
        DistributeBalls();

        if(adjustSimulationSpeed)
            timeScale = Mathf.Abs(simulationSpeed / basicRotYAmount);
        // jesli przyspieszam rotacje, to automatycznie zwalniam czas
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
        // wzor na patterny z parzysta iloscią piłek, wynika z niego np., że żeby żonglować na takiej samej wysokości sześcioma piłkami, 
        // co jest się w stanie czteroma, trzeba przyspieszyć rotację (6-1)/3 razy, tj. 5/3 razy
        else
            ballMaths = (ballsInPattern - 1) / 2;
        // wzor na patterny z nieparzysta iloscią piłek, wynika z niego np., że żeby żonglować na takiej samej wysokości pięcioma piłkami, 
        // co jest się w stanie trzema, trzeba przyspieszyć rotację (5-1)/2 razy, tj. 2 razy


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
            //leftSide.rotYSpeed *= -1;

            rightSide.rotZSpeed *= -1;
            //rightSide.rotYSpeed *= -1;

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
        for (int i = 0; i < ballsInPattern; i++)
        {
            balls[i].gameObject.SetActive(true);

            if (i % 2 == 0)
            {
                leftHand.myBalls.Add(balls[i]);
                leftHand.ballsInHand.Enqueue(balls[i]);
            }
            else
            {
                rightHand.myBalls.Add(balls[i]);
                rightHand.ballsInHand.Enqueue(balls[i]);
            }

            leftHand.allBalls.Add(balls[i]);
            rightHand.allBalls.Add(balls[i]);
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

        if (Input.GetKeyDown(KeyCode.M)) // Add a key to toggle between modes
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
        InvokeRepeating("IncreaseTimeScale", speedUpTimeInterval, speedUpTimeInterval);
    }
}
