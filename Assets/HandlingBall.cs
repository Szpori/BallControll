using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandlingBall : MonoBehaviour
{

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

    private void OnTriggerEnter(Collider obj)
    {

        if (!ballsInHand.Contains(obj.gameObject) && obj.gameObject.tag == "Ball")
        {
            ball = obj.gameObject;
            int index = int.Parse(ball.name);
            ballRb = allBallsRbs[index-1];
            catchPos = ball.transform.position;
            ball.transform.position = transform.position;
            ballsInHand.Enqueue(ball);
            handMode = true;
        }
    }

    void Start()
    {
        for(int i = 0; i < allBalls.Count; i++)
        {
            allBallsRbs.Add(allBalls[i].GetComponent<Rigidbody>());
        }

        numOfBalls = myBalls.Count;
        ball = myBalls[ballNum];
        ballRb = ball.GetComponent<Rigidbody>();
        ball.transform.position = transform.position;
    }

    // Update is called once per frame
    void Update()
    {   
        ballVelocity = ballRb.velocity;

        if(handMode)
        {
            foreach(GameObject ballInHand in ballsInHand)
            {
                ballInHand.transform.position = transform.position;
            }
            if(catchingMode)
            {
                StopAllCoroutines();
                if(ballsInHand.Count == myBalls.Count)
                {
                    ToggleGrandParentScript();
                    catchingMode = false;
                }
               
            }
        }
    }

    private void ToggleGrandParentScript()
    {
        Transform grandparent = transform.parent.parent; // Get the grandparent object

        if (grandparent != null)
        {
            MonoBehaviour grandparentScript = grandparent.GetComponent<MonoBehaviour>(); // Get the script on the grandparent

            if (grandparentScript != null)
            {
                grandparentScript.enabled = !grandparentScript.enabled;
            }
        }
    }

    public IEnumerator BallRelease(float time)
    {
        yield return new WaitForSeconds(time);
        if (basicPos == Vector3.zero)
            basicPos = ball.transform.position;
        else
            previousPos = ball.transform.position;
        Relase();
    }

    private void Relase()
    {
        float scale = ball.transform.position.y - basicPos.y;
        ballsInHand.Dequeue();
        // branie poprawki na nakładający się błąd numeryczny timingu
        Vector3 fixedVetor = new Vector3(properVector.x, properVector.y * (1 + Random.Range(-maxErrorY, maxErrorY)), properVector.z * (1 + Random.Range(-maxErrorZ, maxErrorZ)));
        // wyliczanie w wektor pewnego losowego procentowego błądu
        ballRb.velocity = fixedVetor;

        if (myBalls.Count - 1 > ballNum)
        {
            ballNum++;
            ball = myBalls[ballNum];
            int index = int.Parse(ball.name);
            ballRb = allBallsRbs[index-1];
            //ballRb = ball.GetComponent<Rigidbody>();
            print("Now: " + ball.name);
        }
        else
        {
            handMode = false;
        }

        StartCoroutine(BallRelease(patternTime / (1+scale) ));
    }
}
