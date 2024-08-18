using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationPattern : MonoBehaviour
{
    [SerializeField] Transform yVec;
    [SerializeField] Transform zVec;
    [SerializeField] HandlingBall hand;
    public bool leftSide;
    public bool throwMode = true;

    public float rotZSpeed = 30f;
    public float ROT_Z_TIME = 2f;
    float zTime;

    public float rotYSpeed = 10f;
    public float ROT_Y_TIME = 2f;
    float yTime;

    public bool userControlledRotation = false; // Checkbox to toggle user control over rotation

    public float yROT;

    Quaternion basicRotation;

    float PATTERN_TIME;
    float patternTime;

    private float fixedDeltaTime;

    private void Awake()
    {
        this.fixedDeltaTime = Time.fixedDeltaTime;
    }

    void Start()
    {
        PATTERN_TIME = ROT_Y_TIME * 2;
        patternTime = PATTERN_TIME;
        basicRotation = transform.rotation;

        zTime = ROT_Z_TIME / 2;
        yTime = ROT_Y_TIME;

        if (leftSide)
        {
            StartCoroutine(hand.BallRelease(hand.initialDelay));
        }
        else
        {
            StartCoroutine(SetUpTiming());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;

        if (throwMode)
        {
            if (!userControlledRotation)
            {
                TimeRotation();
                PerformAutomaticRotation(); // Perform the automatic rotation based on timing
            }
            else
            {
                HandleManualRotation(); // Handle manual rotation with arrow keys
            }
        }
    }

    private void TimeRotation()
    {
        zTime -= Time.fixedDeltaTime;
        yTime -= Time.fixedDeltaTime;
        // Change rotation direction every half cycle
        if (yTime < 0f)
        {
            rotYSpeed *= -1;
            yTime = ROT_Y_TIME;
        }
        if (zTime < 0f)
        {
            rotZSpeed *= -1;
            zTime = ROT_Z_TIME;
        }
    }

    private void PerformAutomaticRotation()
    {
        // Cosine ensures that instead of a parallelogram/rhombus, the rotation traces an ellipse/circle.
        yVec.Rotate(0f, rotYSpeed * Time.fixedDeltaTime * Mathf.Cos(yTime * PatternManager.COS_SCALE), 0f);
        zVec.Rotate(0f, 0f, rotZSpeed * Time.fixedDeltaTime * Mathf.Cos(zTime * PatternManager.COS_SCALE));
        transform.rotation = Quaternion.Euler(0f, yVec.rotation.y * 100, zVec.rotation.z * 100);
    }

    private void HandleManualRotation()
    {
        // Check for left or right arrow key press
        if (Input.GetKey(KeyCode.LeftArrow) && !leftSide)
        {
            // Simulate automatic rotation for the right hand
            TimeRotation();
            PerformAutomaticRotation();
        }
        else if (Input.GetKey(KeyCode.RightArrow) && leftSide)
        {
            // Simulate automatic rotation for the left hand
            TimeRotation();
            PerformAutomaticRotation();
        }
    }

    public IEnumerator SetUpTiming()
    {
        throwMode = false;
        yield return new WaitForSeconds(ROT_Z_TIME);
        throwMode = true;
        StartCoroutine(hand.BallRelease(hand.initialDelay));
    }
}