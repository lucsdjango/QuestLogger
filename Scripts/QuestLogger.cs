using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class QuestLogger : MonoBehaviour
{

    public Camera headCam;
    public Transform left, right;

    public bool logFingers;

    private OVRSkeleton leftSkeleton, rightSkeleton;
    private OVRHand leftHand, rightHand;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float step;
    private bool isIndexFingerPinching;

    //private LineRenderer line;
    private Transform p0;
    private Transform p1;
    private Transform p2;

    private Transform handIndexTipTransform;

    public Logger logger;
    public Logger loggerPreFab;

    private Text debugText;
    
    public OVRManager ovrManager;

    public bool logFACS = false;
    public OVRFaceExpressions face;
    float[] facs;
    string[] feNames;

    public OVRInput.Button startButton;

    public QTMListener qtmlr;

    private int logFiles = 0;

    public AudioSource startSound, stopSound;

    void Awake()
    {

        ovrManager = GameObject.FindObjectOfType<OVRManager>();
        face = GameObject.FindObjectOfType<OVRFaceExpressions>();
        left = GameObject.Find("LeftHandAnchor").transform;
        right = GameObject.Find("RightHandAnchor").transform;
        headCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

    }

    public void CallWhenAddedToScenceInEditor()
    {

        ovrManager = GameObject.FindObjectOfType<OVRManager>();
        face = GameObject.FindObjectOfType<OVRFaceExpressions>();
        left = GameObject.Find("LeftHandAnchor").transform;
        right = GameObject.Find("RightHandAnchor").transform;
        headCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

    }


    // Start is called before the first frame update
    void Start()
    {
       

        leftHand = left.GetComponentInChildren<OVRHand>(true);      // include inactive
        rightHand = right.GetComponentInChildren<OVRHand>(true);    // include inactive

        
        logFingers =    (   logFingers &&
                            (leftHand != null) && 
                            (rightHand != null)
                        );

        // GetActiveController returns "None". Hands not reocognized at startup?
        // print("logfingers: " + OVRInput.GetActiveController().ToString() + "  " + (leftHand == null ? "NULLHAND": "LEFTHANDOK") );

        if (logFingers) 
        {
            leftHand.gameObject.SetActive(true);
            rightHand.gameObject.SetActive(true);
            leftSkeleton = left.GetComponentInChildren<OVRSkeleton>(); 
            rightSkeleton = right.GetComponentInChildren<OVRSkeleton>(); 
        }

        logFACS = logFACS && face != null && face.ValidExpressions;

        if (logFACS)
            facs = new float[63];

        //ovrManager.isInsightPassthroughEnabled = false;

        //Invoke("StartLogging", 5f);

    }



    // Update is called once per frame
    void FixedUpdate()
    {
        

        if (logger != null && logger.isActiveAndEnabled && logger.logging)
        {
            
            logger.UpdateEntry("HMD", headCam.transform.rotation.ToString("F4") + " " + headCam.transform.eulerAngles.ToString("F4") + " " + headCam.transform.position.ToString("F4"));

            if (logFingers) {

                var fingers = Enum.GetValues(typeof(OVRHand.HandFinger));

                if (leftHand.IsTracked && rightHand.IsTracked)
                {
                    logger.UpdateEntry("HandDist", Vector3.Distance(leftHand.transform.position, rightHand.transform.position).ToString("F4"));
                }

                if (leftHand.IsTracked)
                {
                    //float d = headCam.transform.InverseTransformPoint(leftHand.transform.position).magnitude;
                    logger.UpdateEntry("LHandConf", leftHand.HandConfidence.ToString());
                    logger.UpdateEntry("LeftHand",
                        //d.ToString("F4") + " " +
                        headCam.transform.InverseTransformPoint(leftHand.transform.position).ToString("F4") + " " +
                        leftHand.transform.rotation.ToString("F4") + " " + 
                        leftHand.transform.eulerAngles.ToString("F4") + " " +  
                        leftHand.transform.position.ToString("F4")
                    );

                    if(true) //leftHand.HandConfidence == OVRHand.TrackingConfidence.High) 
                    { 
                        foreach (OVRHand.HandFinger finger in fingers)
                        {
                            if (finger == OVRHand.HandFinger.Max)
                                continue;
                            string fingerName = finger.ToString().Replace("HandFinger.", "");
                            //Debug.Log(finger);
                            
                            logger.UpdateEntry("L" + fingerName + "Pinch", leftHand.GetFingerPinchStrength(finger).ToString("F4"));
                        }
                    }

                    foreach (OVRBone bone in leftSkeleton.Bones)
                    {
                        logger.UpdateEntry("L"+OVRSkeleton.BoneLabelFromBoneId( OVRSkeleton.SkeletonType.HandLeft,bone.Id), bone.Transform.localRotation.ToString("F4") + " " + bone.Transform.localEulerAngles.ToString("F4") + " " + bone.Transform.position.ToString("F4"));
                    }

                    logger.UpdateEntry("LHandScale", leftHand.HandScale.ToString("F4"));


                } else 
                {
                    logger.UpdateEntry("LHandConf","NOT_TRACKED"); 
                }
                if (rightHand.IsTracked)
                {
                    logger.UpdateEntry("RHandConf",rightHand.HandConfidence.ToString());
                    float d = headCam.transform.InverseTransformPoint(leftHand.transform.position).magnitude;
                    logger.UpdateEntry("RightHand",
                        //d.ToString("F4") + " " +
                        headCam.transform.InverseTransformPoint(rightHand.transform.position).ToString("F4") + " " +
                        rightHand.transform.rotation.ToString("F4") + " " + 
                        rightHand.transform.eulerAngles.ToString("F4") + " " +  
                        rightHand.transform.position.ToString("F4")
                    );

                    if(true) // (rightHand.HandConfidence == OVRHand.TrackingConfidence.High)
                    {
                        foreach (OVRHand.HandFinger finger in fingers)
                        {
                            if (finger == OVRHand.HandFinger.Max)
                                continue;
                            string fingerName = finger.ToString().Replace("HandFinger.", "");
                            //print(finger);
                            logger.UpdateEntry("R" + fingerName + "Pinch", rightHand.GetFingerPinchStrength(finger).ToString("F4"));
                        }
                    }

                    foreach (OVRBone bone in rightSkeleton.Bones){
                        
                        logger.UpdateEntry("R"+OVRSkeleton.BoneLabelFromBoneId( OVRSkeleton.SkeletonType.HandRight,bone.Id), bone.Transform.localRotation.ToString("F4") + " " +  bone.Transform.localEulerAngles.ToString("F4") + " " + bone.Transform.position.ToString("F4"));
                    }

                    logger.UpdateEntry("RHandScale", rightHand.HandScale.ToString("F4"));

                } else {

                    logger.UpdateEntry("RHandConf","NOT_TRACKED"); 
                }

            } else {
                logger.UpdateEntry("LeftHand", left.transform.rotation.ToString("F4") + " " +  left.transform.eulerAngles.ToString("F4") + " " + left.transform.position.ToString("F4"));
                logger.UpdateEntry("RightHand", right.transform.rotation.ToString("F4") + " " +  right.transform.eulerAngles.ToString("F4") + " " +  right.transform.position.ToString("F4"));
            }

            if (logFACS && face.ValidExpressions ) {

                face.CopyTo(facs,0);

                for (int i = 0; i < facs.Length; i++){
                    logger.UpdateEntry(feNames[i], facs[i].ToString("F4"));
                }
                    
            
            }

            
            if (qtmlr.HasReceivedStop())
            {
                StopLogging();
                //Application.Quit();
            }
            


        } else
        {
        
            long ms = qtmlr.StartReceivedThisLongAgo();

            if (ms > 0)
            {
                Debug.Log("started");

                StartLogging(Time.time - ms/1000f);
            }

            

        }


    }

    private void StopLogging()
    {
        GameObject.Destroy(logger);
        UnBlankIt();
    }

    private void StartLogging()
    {
        StartLogging(Time.time);
    }


    private void StartLogging(float t){


        logger = Instantiate<Logger>(loggerPreFab);

        logger.AddEntry("HMD");
        logger.AddEntry("LeftHand");
        logger.AddEntry("RightHand");
        logger.AddEntry("HandDist");

        if (logFingers) {

            logger.AddEntry("LHandConf");
            logger.AddEntry("RHandConf");
            

            var fingers = Enum.GetValues(typeof(OVRHand.HandFinger));

            foreach (OVRHand.HandFinger finger in fingers)
            {
                if (finger == OVRHand.HandFinger.Max)
                    continue;
                string fingerName = finger.ToString().Replace("HandFinger.", "");
                logger.AddEntry("L" + fingerName + "Pinch");
                logger.AddEntry("R" + fingerName + "Pinch");
            }
           

            foreach (OVRBone bone in leftSkeleton.Bones)
            {

                logger.AddEntry("L"+OVRSkeleton.BoneLabelFromBoneId( OVRSkeleton.SkeletonType.HandLeft,bone.Id));
            }

            foreach (OVRBone bone in rightSkeleton.Bones){

                logger.AddEntry("R"+OVRSkeleton.BoneLabelFromBoneId( OVRSkeleton.SkeletonType.HandRight,bone.Id));
                    
            }

            logger.AddEntry("LHandScale");
            logger.AddEntry("RHandScale");
        }

        if (logFACS){

            feNames = System.Enum.GetNames (typeof(OVRFaceExpressions.FaceExpression));
            foreach (string feName in feNames){
                logger.AddEntry(feName);
            }

        }



        //logger.StartLogging((++logFiles).ToString("D")+"Log", t);
        logger.StartLogging(QTMListener.fileName, t);
        BlankIt();
        
    }

    private void BlankIt()
    {
        leftHand.GetComponent<OVRMeshRenderer>().enabled = false;
        rightHand.GetComponent<OVRMeshRenderer>().enabled = false;
        leftHand.GetComponent<SkinnedMeshRenderer>().enabled = false;
        rightHand.GetComponent<SkinnedMeshRenderer>().enabled = false;
        startSound.Play();
        ovrManager.GetComponent<OVRPassthroughLayer>().overlayType = OVROverlay.OverlayType.Overlay;

        //ovrManager.isInsightPassthroughEnabled = true;
        //headCam.clearFlags = CameraClearFlags.SolidColor;
        //headCam.backgroundColor = Color.clear;
    }
    private void UnBlankIt()
    {
        leftHand.GetComponent<OVRMeshRenderer>().enabled = true;
        rightHand.GetComponent<OVRMeshRenderer>().enabled = true;
        leftHand.GetComponent<SkinnedMeshRenderer>().enabled = true;
        rightHand.GetComponent<SkinnedMeshRenderer>().enabled = true;
        stopSound.Play();
        ovrManager.GetComponent<OVRPassthroughLayer>().overlayType = OVROverlay.OverlayType.Underlay;

        //ovrManager.isInsightPassthroughEnabled = false;
        //headCam.clearFlags = CameraClearFlags.SolidColor;
        //headCam.backgroundColor = Color.clear;
    }

    void MaskHands()
    {
        GameObject ovrCameraRig = GameObject.Find("OVRCameraRig");
        OVRPassthroughLayer layer = ovrCameraRig.GetComponent<OVRPassthroughLayer>();
        layer.AddSurfaceGeometry(leftHand.gameObject, true);
        layer.AddSurfaceGeometry(rightHand.gameObject, true);
        // Disable the mesh renderer to avoid rendering the surface within Unity
        MeshRenderer mr = leftHand.GetComponent<MeshRenderer>();
        if (mr)
        {
            mr.enabled = false;
        }
        mr = rightHand.GetComponent<MeshRenderer>();
        if (mr)
        {
            mr.enabled = false;
        }
    }

}
