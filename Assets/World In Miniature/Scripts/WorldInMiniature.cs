﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WorldInMiniature : MonoBehaviour {

    /* World In Miniature implementation by Kieran May
     * University of South Australia
     * 
     * */

    internal SteamVR_TrackedObject trackedObj;
    internal SteamVR_TrackedObject trackedObjO; //tracked object other
    private SteamVR_Controller.Device controller;
    internal SteamVR_Controller.Device controllerO; //controller other
    public GameObject worldInMinParent;
    GameObject[] allSceneObjects;
    public static bool WiMrunning = false;
    public bool WiMactive = false;
    public List<string> ignorableObjectsString = new List<string>{ "[CameraRig]", "Directional Light", "background"};
    public float scaleAmount = 20f;
    public LayerMask interactableLayer;
    public Material outlineMaterial;

    public enum InteractionType { Selection, Manipulation_Movement, Manipulation_Full };
    public InteractionType interacionType;

    public enum ControllerPicked { Left_Controller, Right_Controller };
    public ControllerPicked controllerPicked;

	public GameObject controllerRight;
	public GameObject controllerLeft;
	public GameObject cameraHead;
    private int counter = 0;

	private List<GameObject> listOfChildren = new List<GameObject>();
	private void findClonedObject(GameObject obj){
		if (null == obj)
			return;
		foreach (Transform child in obj.transform){
			if (null == child)
				continue;
			if (child.gameObject.GetComponent<Rigidbody> () != null) {
				child.gameObject.GetComponent<Rigidbody> ().isKinematic = true;
			}
            if(child.GetComponent<ObjectID>() != null) {
                listOfIDs[child.GetComponent<ObjectID>().ID-1].GetComponent<ObjectID>().clonedObject = child.gameObject;
            }
            //listOfIDs[child.gameObject.GetComponent<ObjectID>().ID].GetComponent<ObjectID>().clonedObject = child.gameObject;
            //listOfIDs[child.gameObject.GetComponent]
            //listOfIDs[counter].GetComponent<ObjectID>().clonedObject = child.gameObject;
            counter++;
            listOfChildren.Add(child.gameObject);
			findClonedObject(child.gameObject);
		}
	}

	private List<GameObject> listOfIDs = new List<GameObject>();
	private void setIDObject(GameObject obj){
		if (null == obj)
			return;
		foreach (Transform child in obj.transform){
			if (null == child)
				continue;
			//if (child.gameObject.GetComponent<Rigidbody> () != null) {
				this.GetComponent<WIM_IDHandler> ().addID (child.gameObject);
			//}
			listOfIDs.Add(child.gameObject);
			setIDObject(child.gameObject);
		}
	}

    void createWiM() {
        if (controller.GetPressDown(SteamVR_Controller.ButtonMask.ApplicationMenu)) {
            if (WiMactive == false) {
                WiMactive = true;
                WiMrunning = true;
                print("Create world clone");
                for (int i = 0; i < allSceneObjects.Length; i++) {
                    if (!ignorableObjectsString.Contains(allSceneObjects[i].name)) {
                        GameObject cloneObject = Instantiate(allSceneObjects[i], new Vector3(0f, 0f, 0f), Quaternion.identity) as GameObject;
                        //cloneObject.transform.name = allSceneObjects[i].name;
                        cloneObject.transform.SetParent(worldInMinParent.transform, false);
                        if (cloneObject.gameObject.GetComponent<Rigidbody>() == null) {
                            cloneObject.gameObject.AddComponent<Rigidbody>();
                        }/* else {
                            if (cloneObject.gameObject.GetComponent<Rigidbody>().isKinematic == false) {

                            }
                        }*/
                        cloneObject.gameObject.GetComponent<Rigidbody>().isKinematic = true;
                        //cloneObject.gameObject.AddComponent<Collider>();
                        //cloneObject.GetComponent<Collider>().attachedRigidbody.isKinematic = true;
                        cloneObject.transform.localScale = new Vector3(allSceneObjects[i].transform.lossyScale.x / scaleAmount, allSceneObjects[i].transform.lossyScale.y / scaleAmount, allSceneObjects[i].transform.lossyScale.z / scaleAmount);
                        cloneObject.transform.localRotation = Quaternion.identity;
                        if (cloneObject.transform.GetComponent<Renderer>() != null) {
                            //cloneObject.transform.GetComponent<Renderer>().material.color = Color.red;
                        }
                        float posX = allSceneObjects[i].transform.position.x / scaleAmount;
                        float posY = allSceneObjects[i].transform.position.y / scaleAmount;
                        float posZ = allSceneObjects[i].transform.position.z / scaleAmount;
                        cloneObject.transform.localPosition = new Vector3(posX, posY, posZ);
                    }
                }
				findClonedObject (worldInMinParent);
                //worldInMinParent.transform.SetParent(null);
                //worldInMinParent.transform.localEulerAngles = new Vector3(0f, cameraHead.transform.localEulerAngles.y-45f, 0f);
                worldInMinParent.transform.localEulerAngles = new Vector3(0f, trackedObj.transform.localEulerAngles.y - 45f, 0f);
                worldInMinParent.transform.Rotate(0, tiltAroundY, 0);
                //worldInMinParent.transform.localPosition -= new Vector3(0f, worldInMinParent.transform.position.y / 1.25f, 0f);
            } else if (WiMactive == true) {
                WiMactive = false;
                WiMrunning = false;
                foreach (Transform child in worldInMinParent.transform) {
                    Destroy(child.gameObject);
                }
                worldInMinParent.transform.localPosition = new Vector3(0f, 0f, 0f);
                worldInMinParent.transform.SetParent(trackedObj.transform);
                resetAllProperties();
            }
        }
    }

    public GameObject selectedObject;

    public GameObject currentObjectCollided;


    internal bool objectPicked = false;
    internal Transform oldParent;

    private void resetAllProperties() {
        worldInMinParent.transform.localScale = new Vector3(1f, 1f, 1f);
        worldInMinParent.transform.localPosition = new Vector3(0f, 0f, 0f);
        worldInMinParent.transform.localEulerAngles = new Vector3(0f, 0f, 0f);
    }

	// Use this for initialization
	void Start () {
		
        allSceneObjects = SceneManager.GetActiveScene().GetRootGameObjects();
		for (int i = 0; i<allSceneObjects.Length; i++) {
			setIDObject (allSceneObjects[i]);
		}
        
        worldInMinParent.transform.SetParent(trackedObj.transform);
        resetAllProperties();

		//adding colliders and collider scripts to controllers for WIM if they don't allready exist
		SphereCollider col;
		if ((col = trackedObj.transform.gameObject.GetComponent<SphereCollider> ()) == null) {
			
			col = trackedObj.transform.gameObject.AddComponent<SphereCollider> ();
			col.isTrigger = true;
			col.radius = 0.05f;
            col.center = new Vector3(0f,-0.05f,0f);
			trackedObj.transform.gameObject.AddComponent<ControllerColliderWIM> ();
		}
		SphereCollider col0;
		if((col0 = trackedObjO.transform.gameObject.GetComponent<SphereCollider> ()) == null) {
			
			col0 = trackedObjO.transform.gameObject.AddComponent<SphereCollider> ();
			col0.isTrigger = true;
			col0.radius = 0.05f;
            col0.center = new Vector3(0f, -0.05f, 0f);
            trackedObjO.transform.gameObject.AddComponent<ControllerColliderWIM> ();
		}
    }

	public GameObject findRealObject(GameObject selectedObject) {
		for (int i = 0; i < listOfIDs.Count; i++) {
			//print("looking for:" + )
			if (selectedObject.GetComponent<ObjectID> ().ID == listOfIDs [i].GetComponent<ObjectID> ().ID) {
				return listOfIDs [i];
			}
		}
		return null;
	}

    void Awake() {
        worldInMinParent = this.transform.Find("WorldInMinParent").gameObject;
        if (controllerPicked == ControllerPicked.Right_Controller) {
            trackedObj = controllerRight.GetComponent<SteamVR_TrackedObject>();
            trackedObjO = controllerLeft.GetComponent<SteamVR_TrackedObject>();
        } else if (controllerPicked == ControllerPicked.Left_Controller) {
            trackedObj = controllerLeft.GetComponent<SteamVR_TrackedObject>();
            trackedObjO = controllerRight.GetComponent<SteamVR_TrackedObject>();
        } else {
            print("Couldn't detect trackedObject, please specify the controller type in the settings.");
            Application.Quit();
        }
    }

	public bool isMoving() {
		if (realObject != null && realObject.transform.GetComponent<Rigidbody> () != null) {
			return !realObject.transform.GetComponent<Rigidbody> ().IsSleeping ();
		}
		return false;
	}

	private GameObject realObject;
    private float tiltAroundY = 0f;
    public float tiltSpeed = 2f; //2x quicker than normal
	private bool startedMoving = false;
    // Update is called once per frame
    void Update () {
		//print (isMoving ());
		if (WiMactive == true && isMoving() == true && selectedObject != null && selectedObject.GetComponent<ObjectID>() != null && realObject != null && realObject.GetComponent<ObjectID>() != null && selectedObject.GetComponent<ObjectID>().ID == realObject.GetComponent<ObjectID>().ID) {
			startedMoving = true;
			selectedObject.transform.localPosition = realObject.transform.localPosition;
			selectedObject.transform.localEulerAngles = realObject.transform.localEulerAngles;
		} else if (isMoving() == false && startedMoving == true) {
			startedMoving = false;
		}
        controller = SteamVR_Controller.Input((int)trackedObj.index);
        controllerO = SteamVR_Controller.Input((int)trackedObjO.index);
        createWiM();
        if (WiMactive == true) {
            tiltAroundY = controller.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0).y;
            if (controller.GetTouch(SteamVR_Controller.ButtonMask.Touchpad)) {
                worldInMinParent.transform.Rotate(0, tiltAroundY* tiltSpeed, 0);
            }
        }
        if (controllerO.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && selectedObject == true) {
            selectedObject.transform.SetParent(oldParent);
			realObject = findRealObject(selectedObject);
            realObject.transform.localPosition = selectedObject.transform.localPosition;
            realObject.transform.localEulerAngles = selectedObject.transform.localEulerAngles;
            objectPicked = false;
        }

        }
}