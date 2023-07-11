using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ClappingPatternRotation : MonoBehaviour
{
    [SerializeField] Transform yVec;
    [SerializeField] Transform zVec;
    [SerializeField] GameObject hand;
    public GameObject particlePrefab;
    public bool leftSide;
    public bool throwMode = true;

    public float rotZSpeed = 30f;
    public float ROT_Z_TIME = 2f;
    float zTime;

    public float rotYSpeed = 10f;
    public float ROT_Y_TIME = 2f;
    float yTime;

    public float yROT;

    Quaternion basicRotation;

    float PATTERN_TIME;
    float patternTime;

    private float fixedDeltaTime;
    int counter = 0;
    bool clapped = false;

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
            //StartCoroutine(hand.BallRelease(hand.initialDelay));
        }
        //else
            //StartCoroutine(SetUpTiming());
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Time.fixedDeltaTime = this.fixedDeltaTime * Time.timeScale;

        if (throwMode)
        {
            TimeRotation();
            // Cosinus sprawia, że zamiast jakiegoś równległobku/rombu rotacja zatacza elipsę/koło.
            yVec.Rotate(0f, rotYSpeed * Time.fixedDeltaTime * Mathf.Cos(yTime * PatternManager.COS_SCALE), 0f);
            zVec.Rotate(0f, 0f, rotZSpeed * Time.fixedDeltaTime * Mathf.Cos(zTime * PatternManager.COS_SCALE));
            transform.Rotate(0f, rotYSpeed * Time.fixedDeltaTime, 0f);
            //transform.rotation = Quaternion.Euler(0f, yVec.rotation.y * 100, zVec.rotation.z * 100);
            // Obracanie jednocześnie wokół osi z i y powoduje zmianę w rotacji x, co psuje odpowiednie zapętlanie się rotacji, dlatego
            // Trzeba osobno obracać jakiś obiekt wokoł osi y, inny wokół osi z i pożadana rotacja będzie po prostu sumą tych dwóch rotacji
        }

    }

    private void TimeRotation()
    {
      
        zTime -= Time.fixedDeltaTime;
        yTime -= Time.fixedDeltaTime;
        // co połowę okresu zmienamy znak danej rotacji na przeciwny
        if (yTime < 0f)
        {
            rotYSpeed *= -1;
            yTime = ROT_Y_TIME;
            if (clapped)
            {
                GameObject particle = Instantiate(particlePrefab, hand.transform.position, Quaternion.identity);
                Destroy(particle, 0.2f);
            }
            clapped = !clapped;
        }
        if (zTime < 0f)
        {
            rotZSpeed *= -1;
            zTime = ROT_Z_TIME;
        }
    }

    public IEnumerator SetUpTiming()
    {
        throwMode = false;
        yield return new WaitForSeconds(ROT_Z_TIME);
        throwMode = true;
        //StartCoroutine(hand.BallRelease(hand.initialDelay));
    }

}
