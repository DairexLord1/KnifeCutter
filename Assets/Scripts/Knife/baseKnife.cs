using System;
using System.Collections;
using System.Collections.Generic;
using ARMSSlicing;
using Assets.Scripts;
using UnityEngine;

public abstract class BaseKnife : MonoBehaviour
{
    #region public properties
    public Transform startPoint { get; set; }
    public Transform endPoint { get; set; }

    /// <summary>
    /// event when knife enters trigger
    /// </summary>
    public Action OnStartedSlice;

    /// <summary>
    /// event when knife fully down
    /// </summary>
    public Action OnFinishedSlice;

    #endregion

    #region private properties

    [SerializeField] Transform tipPos;
    [SerializeField] Transform basePos;
    [SerializeField] Transform exitTipPos;

    [SerializeField]
    Material material;

    Vector3 offset = new Vector3(0, 0, 0.02f);
    #endregion



    public abstract void MoveKnife(float d);


    private void OnTriggerEnter(Collider collider)
    {
        //here we do slice
        Slice(collider);
    }

    private Collider Slice(Collider other)
    {
        if (other.GetComponent<Sliceable>() == null)
            return null;

        ///send ivent to stop move 
        OnStartedSlice?.Invoke();

        Collider newRelatedObject = null;

        if (other.GetComponent<Sliceable>().RelatedObject != null)
        {
            newRelatedObject = Slice(other.GetComponent<Sliceable>().RelatedObject);
        }

        GameObject[] slices = Slicer.SliceMesh(basePos.position, tipPos.position, exitTipPos.position, other);

        if (slices == null) return null;

        GameObject[] falled = Slicer.SliceMesh(basePos.position - offset, tipPos.position - offset, exitTipPos.position - offset, slices[0].GetComponent<Collider>());
        GameObject[] moved = Slicer.SliceMesh(basePos.position + offset, tipPos.position + offset, exitTipPos.position + offset, slices[1].GetComponent<Collider>());

        ///part of the obj to fall
        falled[0].AddComponent<SliceableScrolling>();
        moved[0].AddComponent<SliceableScrolling>();

        moved[0].GetComponent<MeshRenderer>().material = material;
        falled[1].GetComponent<MeshRenderer>().material = material;

        GameManager.instance.AddObjCopy(moved[1]);
        GameManager.instance.AddSlicedObject(falled[0]);


        Destroy(moved[0].gameObject);
        Destroy(falled[1].gameObject);


        if (newRelatedObject != null)
        {
            moved[1].GetComponent<Sliceable>().RelatedObject = newRelatedObject;
        }

        return moved[1].GetComponent<Collider>();
    }
}

