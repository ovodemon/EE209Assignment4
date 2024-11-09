using System;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.Networking;
using Oculus.Interaction.Grab;
using Oculus.Interaction.GrabAPI;
using Oculus.Interaction.Input;
using TMPro;

namespace Oculus.Interaction.HandPosing
{
    public class DataCollection : MonoBehaviour
    {
        
        static DataCollection instance;

        [Header("Participant Index (e.g., P0)")]
        public string _participant = "P1"; // type in P0, P1,P2 ... to help you distinguish data from different users

        [Header("Enable Logging")] 
        public bool enable = true; // this script only collects data when enable is true
        [SerializeField] private HandGrabInteractor handGrab; // drag the dominant hand into this blank in the inspector
        
        [SerializeField] private GameObject cube; // drag the target cube into this blank in the inspector
        [SerializeField] private GameObject cube2;
        [SerializeField] private GameObject cube3;

        public TextMeshPro Text_counter1;
        public TextMeshPro Text_counter2;
        public TextMeshPro Text_counter3;
        
        private bool isGrabbed = false; // if the object is grabbed this frame, isGrabbed is true
        private bool wasGrabbed = false; // if the object was grabbed last frame, wasGrabbed is true
        private bool isStart = false; // true when starting to grab
        private bool isEnd = false; // true when starting to release
        private float grabTime = Mathf.Infinity; // elapsed time of moving the cube from point A to point B
        private float grabDistance = Mathf.Infinity; // the distance between point A to point B
        private float grabSize1 = Mathf.Infinity; // the size of the cube'
        private float grabSize2 = Mathf.Infinity; // the size of the cube
        private float grabSize3 = Mathf.Infinity; // the size of the cube
        private float initialPos1; // the initial position of the cube1
        private float initialPos2; // the initial position of the cube2
        private float initialPos3; // the initial position of the cube3
        private float initialTime; // the initial timestamp of user interaction (moving the cube from A to B)

        // 
        private float T1 = 0.3f; //location of the first cube
        private float T2 = 0.6f; //location of the second cube
        private float T3 = 0.9f; //location of the thrid cube

        private float W1 = 0.1f; //width of the first cube
        private float W2 = 0.15f; //width of the first cube
        private float W3 = 0.05f; //width of the first cube

        private bool check1 = false; // true when starting to release
        private bool check2 = false; // true when starting to release
        private bool check3 = false; // true when starting to release
        /*
        This template script only creates one cube. To investigate Fitts' Law, 
        we need to create many more cubes of various sizes, and move them to various distances.
        
        Please add variables here as per your need.  
        */

        private StreamWriter _writer; // to write data into a file
        private string _filename; // the name of the file
        int counter1 = 0;
        int counter2 = 0;
        int counter3 = 0;
        // Ensure this script will run across scenes. Do not delete it.
        private void Awake()
        {
            
            if (instance != null)
            {
                Destroy(gameObject);
            }
            else{
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

        }

        // Save all data into the file when we quit the app
        private void OnApplicationQuit() {
            Debug.Log("On application quit");
            if (_writer != null) {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
                _writer = null;
            }
        }
        
        // Save all data into the file when we pause the app
        private void OnApplicationPause(bool pauseStatus)
        {
            Debug.Log("On application pause");
            if (_writer != null) {
                _writer.Flush();
                _writer.Close();
                _writer.Dispose();
                _writer = null;
            }
        }
        
        private void Start()
        {
            // Create a csv file to save the data
            string filename = $"{_participant}-{Now}.csv";
            string path = Path.Combine(Application.persistentDataPath, filename); 
            // if you run it in the Unity Editor on Windows, the path is %userprofile%\AppData\LocalLow\<companyname>\<productname>
            // if you run it on Mac, the path is the ~/Library/Application Support/company name/product name
            // if you run it on a standalone VR headset, the path is Oculus/Android/data/<packagename>/files
            // reference here: https://docs.unity3d.com/ScriptReference/Application-persistentDataPath.html
            _writer = new StreamWriter(path);
            string msg = $"grabTime" +
                        $"grabSize1" +
                        $"grabDistance";
            _writer.WriteLine(msg);
            Debug.Log(msg);
            _writer.Flush();
            Text_counter1.text = counter1.ToString();
            Text_counter2.text = counter2.ToString();
            Text_counter3.text = counter3.ToString();
        }

        void Update()
        {
            // only collect data when enable is true
            if (enable == false) return;
            
            // read the cube size
            grabSize1 = cube.transform.localScale.y;
            grabSize2 = cube2.transform.localScale.y;
            grabSize3 = cube3.transform.localScale.y;

            // read the grab status
            isGrabbed = (InteractorState.Select == handGrab.State);
            // print("isGrabbed: "+ isGrabbed);
            isStart = !wasGrabbed && isGrabbed;
            // print("isStart: "+ isStart);
            isEnd = wasGrabbed && !isGrabbed;
            // print("isEnd: "+ isEnd);

            // start counting time and distance once a user grabs the cube
            if (isStart){
                initialPos1 = cube.transform.position.x;
                initialPos2 = cube2.transform.position.x;
                initialPos3 = cube3.transform.position.x;
                initialTime = Time.time;
                check1 = false;
                check2 = false;
                check3 = false;
            }
            // stop counting time and distance once a user releases the cube
            if (isEnd){
                // if(cube.transform.position.x != initialPos1){ Cube 1 is moved}
                // elseif(cube2.transform.position.x != initialPos2){ Cube 2}
                // elseif(cube3.transform.position.x != initialPos3){ Cube 3 is}

                float endPos = cube.transform.position.x;
                float endPos2 = cube2.transform.position.x;
                float endPos3 = cube3.transform.position.x;

                grabTime = Time.time - initialTime;

                if(cube.transform.position.x != initialPos1)
                {
                    grabDistance = Mathf.Abs(endPos - initialPos1);

                    if ((endPos <= T1 + 0.2*W1) && (endPos >= T1 - 0.2*W1)) {
                    check1 = true; 
                    // WriteToFile(grabTime, grabSize1, grabDistance, check1, check2, check3);
                    }

                    if ((endPos <= T2 + 0.2*W1) && (endPos >= T2 - 0.2*W1)) {
                    check2 = true; 
                    // WriteToFile(grabTime, grabSize1, grabDistance, check1, check2, check3);
                    }

                    if ((endPos <= T3 + 0.2*W1) && (endPos >= T3 - 0.2*W1)) {
                    check3 = true;
                    // WriteToFile(grabTime, grabSize1, grabDistance, check1, check2, check3);
                    }
                    WriteToFile(grabTime, W1, grabDistance, check1, check2, check3);
                }
                else if(cube2.transform.position.x != initialPos2){
                    grabDistance = Mathf.Abs(endPos2 - initialPos2);
                    ////////////////////////////////////////////////////
                    if ((endPos2 <= T1 + 0.2*W1) && (endPos2 >= T1 - 0.2*W2)) {
                    check1 = true;
                    // WriteToFile(grabTime, grabSize2, grabDistance, check1, check2, check3);
                    }

                    if ((endPos2 <= T2 + 0.2*W1) && (endPos2 >= T2 - 0.2*W2)) {
                    check2 = true;
                    // WriteToFile(grabTime, grabSize2, grabDistance, check1, check2, check3);
                    }

                    if ((endPos2 <= T3 + 0.2*W1) && (endPos2 >= T3 - 0.2*W2)) {
                    check3 = true;
                    // WriteToFile(grabTime, grabSize2, grabDistance, check1, check2, check3);
                    }
                    WriteToFile(grabTime, W2, grabDistance, check1, check2, check3);
                }

                else if(cube3.transform.position.x != initialPos3){
                    grabDistance = Mathf.Abs(endPos3 - initialPos3);

                    //////////////////////////////////////////////////////
                    if ((endPos3 <= T1 + 0.2*W1) && (endPos3 >= T1 - 0.2*W3)) {
                    check1 = true;
                    // WriteToFile(grabTime, grabSize3, grabDistance, check1, check2, check3);
                    }

                    if ((endPos3 <= T2 + 0.2*W1) && (endPos3 >= T2 - 0.2*W3)) {
                    check2 = true;
                    // WriteToFile(grabTime, grabSize3, grabDistance, check1, check2, check3);
                    }

                    if ((endPos3 <= T3 + 0.2*W1) && (endPos3 >= T3 - 0.2*W3)) {
                    check3 = true;
                    // WriteToFile(grabTime, grabSize3, grabDistance, check1, check2, check3);
                    }
                    WriteToFile(grabTime, W3, grabDistance, check1, check2, check3);
                }

                // WriteToFile(grabTime, grabSize, grabDistance, check1, check2, check3);
                if (check1) {counter1 ++;}
                if (check2) {counter2 ++;}
                if (check3) {counter3 ++;}
                Text_counter1.text = counter1.ToString();
            Text_counter2.text = counter2.ToString();
            Text_counter3.text = counter3.ToString();
            }   



            wasGrabbed = isGrabbed;
            
        }
        
        // write T, W, D into the file.
        private void WriteToFile(float grabTime, float grabSize, float grabDistance, bool check1, bool check2, bool check3) {
            if (_writer == null) return;
            
            string msg = $"{grabTime}," +
                        $"{grabSize}," +
                        $"{grabDistance},"+
                        $"{check1},"+
                        $"{check2},"+
                        $"{check3}";
            _writer.WriteLine(msg);
            Debug.Log("test msg: "+msg);
            _writer.Flush();
        }
        
        // generate the current timestamp for filename
        private double Now {
            get {
                System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
                return (System.DateTime.UtcNow - epochStart).TotalMilliseconds;
            }
        }
    
    }
}