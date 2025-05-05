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
            initialY = transform.position.y; // 기준 Y 고정
    }

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 camPos = transform.position;
            camPos.x = target.transform.position.x;

            // y축이 카메라 기준보다 클 때만 따라감
            if (target.transform.position.y > initialY)
                camPos.y = target.transform.position.y;
            else
                camPos.y = initialY;

            camPos.z = offset.z; // 보통 -10 유지
            transform.position = camPos;
        }
    }
}
