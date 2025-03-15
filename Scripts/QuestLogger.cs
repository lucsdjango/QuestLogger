using System;
using System.Collections;
using System.Collections.Generic;
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
	
	private Dictionary<OVRBone, string> leftBones;
    private Dictionary<OVRBone, string> rightBones;

    private const string dps = "F4";

    void Awake()
    {

        ovrManager = GameObject.FindObjectOfType<OVRManager>();
        face = GameObject.FindObjectOfType<OVRFaceExpressions>();
        left = GameObject.Find("LeftHandAnchor").transform;
        right = GameObject.Find("RightHandAnchor").transform;
        headCam = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();

        OVRPlugin.systemDisplayFrequency = 120.0f;

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
                    
                    logger.UpdateEntry("LHandConf", leftHand.HandConfidence.ToString());
                    logger.UpdateEntry("LeftHand",
                        headCam.transform.InverseTransformPoint(leftHand.transform.position).ToString("F4") + " " +
                        leftHand.transform.rotation.ToString("F4") + " " + 
                        leftHand.transform.eulerAngles.ToString("F4") + " " +  
                        leftHand.transform.position.ToString("F4")
                    );

                    
					foreach (OVRHand.HandFinger finger in fingers)
					{
						if (finger == OVRHand.HandFinger.Max)
							continue;
						string fingerName = finger.ToString().Replace("HandFinger.", "");
						//Debug.Log(finger);
						
						logger.UpdateEntry("L" + fingerName + "Pinch", leftHand.GetFingerPinchStrength(finger).ToString("F4"));
					}
                    

					foreach (KeyValuePair<OVRBone, string> labeledBone in leftBones)
                    {
                        OVRBone bone = labeledBone.Key;
                        logger.UpdateEntry(labeledBone.Value, bone.Transform.localRotation.ToString(dps) + " " + bone.Transform.localEulerAngles.ToString(dps) + " " + bone.Transform.position.ToString(dps));
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

                    foreach (OVRHand.HandFinger finger in fingers)
                    {
						if (finger == OVRHand.HandFinger.Max)
							continue;
						string fingerName = finger.ToString().Replace("HandFinger.", "");
						//print(finger);
						logger.UpdateEntry("R" + fingerName + "Pinch", rightHand.GetFingerPinchStrength(finger).ToString("F4"));
					}
				

                    foreach (KeyValuePair<OVRBone, string> labeledBone in rightBones)
                    {
                        OVRBone bone = labeledBone.Key;
                        logger.UpdateEntry(labeledBone.Value, bone.Transform.localRotation.ToString(dps) + " " + bone.Transform.localEulerAngles.ToString(dps) + " " + bone.Transform.position.ToString(dps));
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
		
		leftBones = new Dictionary<OVRBone, string>();
		rightBones = new Dictionary<OVRBone, string>();
		

        if (logFingers)
        {

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
                string boneLabel = "L" + OVRSkeleton.BoneLabelFromBoneId(leftSkeleton.GetSkeletonType(), bone.Id);
                if (boneLabel != null && !boneLabel.Contains("Unknown"))
                {
                    logger.AddEntry(boneLabel);
                    leftBones.Add(bone, boneLabel);
                    print(boneLabel);
                }


            }

            foreach (OVRBone bone in rightSkeleton.Bones)
            {

                string boneLabel = "R" + OVRSkeleton.BoneLabelFromBoneId(rightSkeleton.GetSkeletonType(), bone.Id);
                if (boneLabel != null && !boneLabel.Contains("Unknown"))
                {
                    logger.AddEntry(boneLabel);
                    rightBones.Add(bone, boneLabel);
                    print(boneLabel);

                }

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
        /* leftHand.GetComponent<OVRMeshRenderer>().enabled = false;
        rightHand.GetComponent<OVRMeshRenderer>().enabled = false;
        leftHand.GetComponent<SkinnedMeshRenderer>().enabled = false;
        rightHand.GetComponent<SkinnedMeshRenderer>().enabled = false; 
		ovrManager.GetComponent<OVRPassthroughLayer>().overlayType = OVROverlay.OverlayType.Overlay;
		*/
        startSound.Play();
        

        //ovrManager.isInsightPassthroughEnabled = true;
        //headCam.clearFlags = CameraClearFlags.SolidColor;
        //headCam.backgroundColor = Color.clear;
    }
    private void UnBlankIt()
    {
/*         leftHand.GetComponent<OVRMeshRenderer>().enabled = true;
        rightHand.GetComponent<OVRMeshRenderer>().enabled = true;
        leftHand.GetComponent<SkinnedMeshRenderer>().enabled = true;
        rightHand.GetComponent<SkinnedMeshRenderer>().enabled = true;
		ovrManager.GetComponent<OVRPassthroughLayer>().overlayType = OVROverlay.OverlayType.Underlay; */
        stopSound.Play();
        

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
