using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public GameObject target;
    public Vector3 offset = new Vector3(0, 0, -10);

    private float initialY;

    void Start()
    {
        if (target != null)
            initialY = transform.position.y;
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 camPos = transform.position;
            camPos.x = target.transform.position.x;

            if (target.transform.position.y > initialY)
                camPos.y = target.transform.position.y;
            else
                camPos.y = initialY;

            camPos.z = offset.z;
            transform.position = camPos;
        }
    }
}
