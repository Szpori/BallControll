using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlingBall : MonoBehaviour
{
    public KeyCode releaseKey;
    public Stack<GameObject> ballsInHand = new Stack<GameObject>();
    public List<GameObject> myBalls = new List<GameObject>();
    public List<Rigidbody> allBallsRbs = new List<Rigidbody>();
    [SerializeField] GameObject ball;
    Rigidbody ballRb;
    public Vector3 properVector;
    public float maxErrorZ;
    public float maxErrorY;
    public float initialDelay = 2f;
    public float patternTime = 1f;
    public bool catchingMode = false;
    public bool handMode = true;

    Vector3 basicPos;
    private int currentThrowHeight = 3; // Siteswap height (3-7)

void Start()
    {
        releaseKey = (gameObject.name == "LeftHand") ? KeyCode.D : KeyCode.A;
        InitializeBallRigidbodyList();

        if (myBalls.Count > 0)
        {
            SetBallToHand(ballsInHand.Peek());
        }

        // Initialize properVector based on currentThrowHeight (default 3)
        UpdateThrowVectorBasedOnHeight();
    }

    private void InitializeBallRigidbodyList()
    {
        foreach (GameObject b in myBalls)
        {
            allBallsRbs.Add(b.GetComponent<Rigidbody>());
        }
    }

    private void OnTriggerEnter(Collider obj)
    {
        if (obj.CompareTag("Ball") && !ballsInHand.Contains(obj.gameObject))
        {
            CaptureBall(obj.gameObject);

            if (catchingMode && ballsInHand.Count == myBalls.Count)
            {
                catchingMode = false;
                StopAllCoroutines();
            }
        }
    }

    private void CaptureBall(GameObject capturedBall)
    {
        ballsInHand.Push(capturedBall);
        SetBallToHand(capturedBall);
        handMode = true;
    }

    private void SetBallToHand(GameObject newBall)
    {
        ball = newBall;
        ballRb = ball.GetComponent<Rigidbody>();
        ball.transform.position = transform.position;
    }

    void Update()
    {
        if (ball != null)
        {
            ballRb.linearVelocity = ballRb.linearVelocity;
        }

        if (handMode)
        {
            PositionBallsInHand();
        }

        // Listen for numeric keys 3-7 to set throw height
        for (int i = 3; i <= 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                currentThrowHeight = i;
                UpdateThrowVectorBasedOnHeight();
                Debug.Log($"Throw height set to: {i}");
            }
        }

        if (PatternManager.userControlledMode && Input.GetKeyUp(releaseKey) && ballsInHand.Count > 0)
        {
            ReleaseSingleBall();
        }
    }

    private void PositionBallsInHand()
    {
        foreach (GameObject ballInHand in ballsInHand)
        {
            ballInHand.transform.position = transform.position;
        }
    }

    private void ReleaseSingleBall()
    {
        if (ballsInHand.Count > 0)
        {
            ReleaseBall();
        }
    }

    private void ReleaseBall()
    {
        if (ball == null) return;

        float heightFactor = currentThrowHeight / 3f;

        Vector3 scaledVector = new Vector3(
            properVector.x / heightFactor,
            properVector.y * heightFactor,
            properVector.z / heightFactor
        );

        Vector3 releaseVelocity = new Vector3(
            scaledVector.x,
            scaledVector.y * (1 + Random.Range(-maxErrorY, maxErrorY)),
            scaledVector.z * (1 + Random.Range(-maxErrorZ, maxErrorZ))
        );

        ballRb.linearVelocity = releaseVelocity;
        ballsInHand.Pop();
        UpdateBallReference();

        if (!PatternManager.userControlledMode)
        {
            StartCoroutine(BallRelease(patternTime));
        }
    }

    private void UpdateBallReference()
    {
        if (ballsInHand.Count > 0)
        {
            SetBallToHand(ballsInHand.Peek());
        }
        else
        {
            handMode = false;
        }
    }

    public IEnumerator BallRelease(float time)
    {
        if (PatternManager.userControlledMode) yield break;

        yield return new WaitForSeconds(time);

        if (basicPos == Vector3.zero)
            basicPos = ball.transform.position;

        ReleaseBall();
    }

private void UpdateThrowVectorBasedOnHeight()
    {
        // In siteswap: even heights (4,6) throw to same hand, odd heights (3,5,7) throw to other hand
        bool isEvenThrowHeight = currentThrowHeight % 2 == 0;
        
        Vector3 baseVector;
        if (isEvenThrowHeight)
        {
            // Even: lower Y, maintain Z proportion (0.08 scaled down)
            baseVector = new Vector3(0, 2f, -0.41f);
        }
        else
        {
            // Odd: lower Y, maintain Z proportion (0.47 scaled down) 
            baseVector = new Vector3(0, 1.9f, 1.65f);
        }
        
        // Mirror Z for right hand so both hands throw towards each other
        bool isLeftHand = gameObject.name.Contains("Left");
        properVector = isLeftHand ? baseVector : new Vector3(baseVector.x, baseVector.y, -baseVector.z);
    }
}
