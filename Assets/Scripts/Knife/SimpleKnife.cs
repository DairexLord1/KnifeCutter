using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleKnife : BaseKnife
{
    public override void MoveKnife(float d)
    {
        transform.position = Vector3.Lerp(startPoint.position, endPoint.position, d);

        if (transform.position.y == endPoint.position.y)
            OnFinishedSlice?.Invoke();
    }
}
