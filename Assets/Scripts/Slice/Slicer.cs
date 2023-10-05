using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Scripts
{
    class Slicer 
    {
        /// <summary>
        /// Slice the object by the plane 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="objectToCut"></param>
        /// <returns></returns>
        public static GameObject[] Slice(Plane plane, GameObject objectToCut)
        {            
            //Get the current mesh and its verts and tris
            Mesh mesh = objectToCut.GetComponent<MeshFilter>().mesh;
            var a = mesh.GetSubMesh(0);
            Sliceable sliceable = objectToCut.GetComponent<Sliceable>();

            if(sliceable == null)
            {
                return null;
                //throw new NotSupportedException("Cannot slice non sliceable object, add the sliceable script to the object or inherit from sliceable to support slicing");
            }
            
            //Create left and right slice of hollow object
            SlicesMetadata slicesMeta = new SlicesMetadata(plane, mesh, sliceable.IsSolid, sliceable.ReverseWireTriangles, sliceable.ShareVertices, sliceable.SmoothVertices);            

            GameObject positiveObject = CreateMeshGameObject(objectToCut);
            positiveObject.name = string.Format("{0}_positive", objectToCut.name);

            GameObject negativeObject = CreateMeshGameObject(objectToCut);
            negativeObject.name = string.Format("{0}_negative", objectToCut.name);

            var positiveSideMeshData = slicesMeta.PositiveSideMesh;
            var negativeSideMeshData = slicesMeta.NegativeSideMesh;

            positiveObject.GetComponent<MeshFilter>().mesh = positiveSideMeshData;
            negativeObject.GetComponent<MeshFilter>().mesh = negativeSideMeshData;

            SetupCollidersAndRigidBodys(ref positiveObject, positiveSideMeshData, sliceable.UseGravity);
            SetupCollidersAndRigidBodys(ref negativeObject, negativeSideMeshData, sliceable.UseGravity);

            return new GameObject[] { positiveObject, negativeObject};
        }        

        /// <summary>
        /// Creates the default mesh game object.
        /// </summary>
        /// <param name="originalObject">The original object.</param>
        /// <returns></returns>
        private static GameObject CreateMeshGameObject(GameObject originalObject)
        {
            var originalMaterial = originalObject.GetComponent<MeshRenderer>().materials;

            GameObject meshGameObject = new GameObject();
            Sliceable originalSliceable = originalObject.GetComponent<Sliceable>();

            meshGameObject.AddComponent<MeshFilter>();
            meshGameObject.AddComponent<MeshRenderer>();
            Sliceable sliceable = meshGameObject.AddComponent<Sliceable>();

            sliceable.IsSolid = originalSliceable.IsSolid;
            sliceable.ReverseWireTriangles = originalSliceable.ReverseWireTriangles;
            sliceable.UseGravity = originalSliceable.UseGravity;
            sliceable.PaintMesh = originalSliceable.PaintMesh;

            meshGameObject.GetComponent<MeshRenderer>().materials = originalMaterial;

            meshGameObject.transform.localScale = originalObject.transform.localScale;
            meshGameObject.transform.rotation = originalObject.transform.rotation;
            meshGameObject.transform.position = originalObject.transform.position;

            meshGameObject.tag = originalObject.tag;

            return meshGameObject;
        }

        /// <summary>
        /// Add mesh collider and rigid body to game object
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="mesh"></param>
        private static void SetupCollidersAndRigidBodys(ref GameObject gameObject, Mesh mesh, bool useGravity)
        {                     
            MeshCollider meshCollider = gameObject.AddComponent<MeshCollider>();
            meshCollider.sharedMesh = mesh;
            meshCollider.convex = true;
            meshCollider.enabled = false;

            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = useGravity;
            rb.isKinematic = true;//my
        }


        public static GameObject[] SliceMesh(Vector3 baseEnterPos, Vector3 tipEnterPos, Vector3 exitTipPos, Collider other)
        {
            //Create a triangle between the tip and base so that we can get the normal
            Vector3 side1 = exitTipPos - tipEnterPos;
            Vector3 side2 = exitTipPos - baseEnterPos;

            //Get the point perpendicular to the triangle above which is the normal
            Vector3 normal = Vector3.Cross(side1, side2).normalized;

            //Transform the normal so that it is aligned with the object we are slicing's transform.
            Vector3 transformedNormal = ((Vector3)(other.gameObject.transform.localToWorldMatrix.transpose * normal)).normalized;

            //Get the enter position relative to the object we're cutting's local transform
            Vector3 transformedStartingPoint = other.gameObject.transform.InverseTransformPoint(tipEnterPos);

            Plane plane = new Plane();

            plane.SetNormalAndPosition(
                    transformedNormal,
                    transformedStartingPoint);

            var direction = Vector3.Dot(Vector3.up, transformedNormal);

            //Flip the plane so that we always know which side the positive mesh is on
            if (direction < 0)
            {
                plane = plane.flipped;
            }

            GameObject[] slices = Slice(plane, other.gameObject);

            UnityEngine.Object.Destroy(other.gameObject);

            return slices;
        }

    }
}
