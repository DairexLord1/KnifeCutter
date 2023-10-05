using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ARMSSlicing
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager instance = null;

        [SerializeField] GameObject knifePrefab;
        [SerializeField] KnifeController knifeController;

        private float time = 0;
        public float _time => time;

        List<GameObject> slicedObjects = new List<GameObject>();
        List<GameObject> objCopy = new List<GameObject>();

        [Tooltip("prefab of object that will be sliced")]
        public GameObject prefab;


        [SerializeField]
        private GameObject movingObj; // parent for cutted objs


        [SerializeField]
        private GameObject oldObj; ///object that is zterted to cut


        private bool startSlice = false;
        private bool knifeGotDown = true;

        private bool startFall = false;

        // Start is called before the first frame update
        void Start()
        {

            InitInstance();

            //this is for one obj, but we can modify to ajust any Abstract knife class
            knifeController.InitKnife(knifePrefab);

            ///Event on knife trigger enter
            knifeController.knife.OnStartedSlice += () =>
            {
                knifeGotDown = false;
                startSlice = true;
            };

            knifeController.knife.OnFinishedSlice += () =>
            {
                knifeGotDown = true;
                startFall = true;
            };
        }

        void InitInstance()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance == this)
            {
                Destroy(gameObject);
            }

            DontDestroyOnLoad(gameObject);
        }

        void Update()
        {
            DetectTouch();

            if (!startSlice)
                MoveObject();
            else
                CheckForSliced();

            if (startFall)
            {
                oldObj.transform.position += new Vector3(0, -1, -1) * Time.deltaTime * 3;
                oldObj.transform.eulerAngles += new Vector3(-1, 0, 0) * Time.deltaTime * 60;
            }

        }


        private void DetectTouch()
        {

            if (Input.GetMouseButton(0) || Input.touchCount > 0)
            {
                time += Time.deltaTime;
            }
            else
            {
                time -= Time.deltaTime;
            }
            time = Mathf.Clamp(time, 0.0f, 1f);


            knifeController.MoveKnife(time);


        }

        private void MoveObject()
        {
            if (movingObj == null)
                return;

            movingObj.transform.position += new Vector3(0, 0, -1) * Time.deltaTime;
            if (movingObj.transform.position.z < -9f)
            {
                Recreate();
            }
        }

        /// <summary>
        /// check if Slice completed
        /// </summary>
        void CheckForSliced()
        {
            if (time <= 0 && knifeGotDown)
            {
                startSlice = false;
                Reset();
            }

        }

        private void Recreate()
        {
            slicedObjects = new List<GameObject>();
            objCopy = new List<GameObject>();

            GameObject instantiated = Instantiate(prefab);
            GameObject t = movingObj;
            movingObj = instantiated;
            if (t != null)
                Destroy(t);
        }


        public void AddObjCopy(GameObject subObject)
        {
            objCopy.Add(subObject);
            subObject.transform.parent = movingObj.transform;
        }

        public void AddSlicedObject(GameObject subObject)
        {
            if (slicedObjects.Count == 0)
            {
                oldObj.transform.position = subObject.transform.position;
                
            }
            slicedObjects.Add(subObject);
            subObject.transform.parent = oldObj.transform;
        }

        public void Reset()
        {
            slicedObjects.ForEach(x => Destroy(x.gameObject));
            slicedObjects.Clear();
            startFall = false;
            objCopy.ForEach(x => { if (x != null) x.GetComponent<MeshCollider>().enabled = true; });
        }
    }

}