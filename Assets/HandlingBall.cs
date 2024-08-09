using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlingBall : MonoBehaviour
{
    public KeyCode releaseKey; // Release key to be set dynamically

    public Queue<GameObject> ballsInHand = new Queue<GameObject>();
    public List<GameObject> myBalls = new List<GameObject>();
    public List<GameObject> allBalls = new List<GameObject>();
    public List<Rigidbody> allBallsRbs = new List<Rigidbody>();
    [SerializeField] GameObject ball;
    Rigidbody ballRb;
    public Vector3 properVector;
    public float maxErrorZ;
    public float maxErrorY;
    public Vector3 catchPos;
    public Vector3 ballVelocity;
    public Vector3 previousPos;
    public Vector3 basicPos;
    public float smallEnoughError = 0.1f;

    public float initialDelay = 2f;
    public float patternTime = 1f;

    public bool catchingMode = false;
    public bool handMode = true;
    int numOfBalls;
    int ballNum = 0;

    void Start()
    {
        // Set the release key based on the hand object
        if (gameObject.name == "LeftHand") // Replace with your left hand object name
        {
            releaseKey = KeyCode.D;
        }
        else if (gameObject.name == "RightHand") // Replace with your right hand object name
        {
            releaseKey = KeyCode.A;
        }
        else
        {
            Debug.LogError("Hand object name not recognized. Please set the release key manually.");
        }

        for (int i = 0; i < allBalls.Count; i++)
        {
            allBallsRbs.Add(allBalls[i].GetComponent<Rigidbody>());
        }

        numOfBalls = myBalls.Count;
        if (numOfBalls > 0)
        {
            ball = myBalls[ballNum];
            ballRb = ball.GetComponent<Rigidbody>();
            ball.transform.position = transform.position;
        }
    }

    private void OnTriggerEnter(Collider obj)
    {
        if (!ballsInHand.Contains(obj.gameObject) && obj.gameObject.CompareTag("Ball"))
        {
            ball = obj.gameObject;
            int index = int.Parse(ball.name);
            ballRb = allBallsRbs[index - 1];
            catchPos = ball.transform.position;
            ball.transform.position = transform.position;
            ballsInHand.Enqueue(ball);
            handMode = true;

            if (catchingMode)
            {
                print("ssfsf: " + ballsInHand.Count);
                StopAllCoroutines();
                if (ballsInHand.Count == myBalls.Count)
                {
                    catchingMode = false;
                }
            }
        }
    }

    void Update()
    {
        if (ball != null)
        {
            ballVelocity = ballRb.velocity;
        }

        if (handMode)
        {
            foreach (GameObject ballInHand in ballsInHand)
            {
                ballInHand.transform.position = transform.position;
            }
        }

        if (PatternManager.userControlledMode && Input.GetKeyUp(releaseKey))
        {
            if (ballsInHand.Count > 0)
            {
                ReleaseSingleBall();
            }
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
        if (ball == null) return; // Ensure ball is not null

        float scale = ball.transform.position.y - basicPos.y;
        float scaleZ = ball.transform.position.z - basicPos.z;
        ball = ballsInHand.Dequeue();
        ballRb = ball.GetComponent<Rigidbody>();
        Vector3 fixedVector = new Vector3(properVector.x, properVector.y * (1 + Random.Range(-maxErrorY, maxErrorY)), properVector.z * (1 + Random.Range(-maxErrorZ, maxErrorZ)));
        ballRb.velocity = fixedVector;

        if (myBalls.Count - 1 > ballNum)
        {
            ballNum++;
            ball = myBalls[ballNum];
            int index = int.Parse(ball.name);
            ballRb = allBallsRbs[index - 1];
        }
        else
        {
            handMode = false;
        }

        if (!PatternManager.userControlledMode)
        {
            StartCoroutine(BallRelease(patternTime / (1 + scale + scaleZ)));
        }
    }

    public IEnumerator BallRelease(float time)
    {
        if (PatternManager.userControlledMode)
        {
            yield break; // Exit the coroutine if in user-controlled mode
        }
        else
        {
            yield return new WaitForSeconds(time);
            if (basicPos == Vector3.zero)
                basicPos = ball.transform.position;
            else
                previousPos = ball.transform.position;
            ReleaseBall();
        }
    }
}