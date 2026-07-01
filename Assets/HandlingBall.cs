using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlingBall : MonoBehaviour
{
    public KeyCode releaseKey;
    public Queue<GameObject> ballsInHand = new Queue<GameObject>();
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

        private int currentThrowHeight = 3; // Siteswap throw height (3-7)

    public void SetThrowHeight(int height)
    {
        if (height >= 3 && height <= 7)
            currentThrowHeight = height;
    }

void Start()
    {
        releaseKey = (gameObject.name == "LeftHand") ? KeyCode.D : KeyCode.A;
        InitializeBallRigidbodyList();

        if (myBalls.Count > 0)
        {
            SetBallToHand(myBalls[0]);
        }
    }

    private void InitializeBallRigidbodyList()
    {
        foreach (GameObject ball in myBalls)
        {
            allBallsRbs.Add(ball.GetComponent<Rigidbody>());
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
        ballsInHand.Enqueue(capturedBall);
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
        for (int i = 3; i <= 7; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i))
            {
                currentThrowHeight = i;
                Debug.Log("Throw height set to: " + i);
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

        // Scale Y for throw height, but inverse-scale X and Z to keep same landing distance
        // Higher Y = longer flight time = ball travels further
        // So we reduce X and Z to compensate
        float heightFactor = currentThrowHeight / 3f;
        
        Vector3 scaledVector = new Vector3(
            properVector.x / heightFactor,  // Reduce X (inverse scaling)
            properVector.y * heightFactor,  // Increase Y (for height)
            properVector.z / heightFactor   // Reduce Z (inverse scaling)
        );

        Vector3 releaseVelocity = new Vector3(
            scaledVector.x,
            scaledVector.y * (1 + Random.Range(-maxErrorY, maxErrorY)),
            scaledVector.z * (1 + Random.Range(-maxErrorZ, maxErrorZ))
        );

        ballRb.linearVelocity = releaseVelocity;
        ballsInHand.Dequeue();
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
}