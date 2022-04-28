using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunSwayAnim : MonoBehaviour {

    [Header ("Rotation delay")]
    public float roationDelay = 5f;

    private Quaternion originLocalRotation;

    private void Start() {
        originLocalRotation = transform.localRotation;
    }

    void Update() {
        float t_xLookInput = Input.GetAxis("Mouse X");
        float t_yLookInput = Input.GetAxis("Mouse Y");
        //Calculate the weapon rotation
        Quaternion t_xAngleAdjustment = Quaternion.AngleAxis(-t_xLookInput * roationDelay, Vector3.up);
        Quaternion t_yAngleAdjustment = Quaternion.AngleAxis(t_yLookInput * roationDelay, Vector3.right);
        Quaternion t_targerRotation = originLocalRotation * t_xAngleAdjustment * t_yAngleAdjustment;
        //Rotate towards tarjet rotation 
        transform.localRotation = Quaternion.Lerp(transform.localRotation, t_targerRotation, Time.deltaTime * 10f);
    }
}