using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnifeController : MonoBehaviour
{
    [SerializeField] Transform startPoint;
    [SerializeField] Transform endPoint;


    public BaseKnife knife;

    /// <summary>
    /// inits knife, knife inherits from "BaseKnife.cs"
    /// </summary>
    /// <param name="prefab"></param>
    public void InitKnife(GameObject prefab)
    {
        knife = prefab.GetComponent<BaseKnife>();

        knife.startPoint = this.startPoint;
        knife.endPoint = this.endPoint;
    }

    /// <summary>
    /// moves knife according to time clamp
    /// </summary>
    /// <param name="d"></param>
    public void MoveKnife(float d)
    {
        knife.MoveKnife(d);
    }
}
