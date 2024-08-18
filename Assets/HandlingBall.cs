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
            ballRb.velocity = ballRb.velocity;
        }

        if (handMode)
        {
            PositionBallsInHand();
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

        Vector3 releaseVelocity = new Vector3(
            properVector.x,
            properVector.y * (1 + Random.Range(-maxErrorY, maxErrorY)),
            properVector.z * (1 + Random.Range(-maxErrorZ, maxErrorZ))
        );

        ballRb.velocity = releaseVelocity;
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