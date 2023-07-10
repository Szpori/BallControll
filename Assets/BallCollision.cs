using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallCollision : MonoBehaviour
{
    private void OnCollisionEnter(Collision obj)
    {
        if (obj.gameObject.tag == "Ball")
        {
            PatternManager.AddCollision();
            print(this.name + " Collide with " + obj.gameObject.name);
        }
         
    }
}
