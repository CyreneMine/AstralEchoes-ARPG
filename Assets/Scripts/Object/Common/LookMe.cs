using System;
using UnityEngine;

public class LookMe : MonoBehaviour
{
    private Transform targetTrs;
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    private void LateUpdate()
    {
        targetTrs = Camera.main.transform;
        if (targetTrs == null)
            return;
        if (rectTransform !=null)
            rectTransform.rotation = Quaternion.LookRotation(targetTrs.position - transform.position);
        transform.rotation = Quaternion.LookRotation(targetTrs.position - transform.position);
    }
}
