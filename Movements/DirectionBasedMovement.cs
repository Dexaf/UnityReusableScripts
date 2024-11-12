using System;
using UnityEngine;

public class DirectionBasedMovement : MonoBehaviour
{
    private Vector3 targetDirection;
    private Vector3 targetArrivalSpot;
    private bool isTranslating = false;
    private bool isRotating = false;
    //velocity
    public float v = 0;    // m/s
    private readonly float vm = 45;  // m/s
    public float a = 10;    // m/s2
    public float ta = 0;    // s
    //velocity angular 
    public float va = 0;    // deg/s
    private readonly float vam = 120;   // deg/s
    public float ava = 40;    // deg/s2
    public float tava = 0;    // s
    private float angleForDirection;

    public void InitMovement(Vector3 targetDirection)
    {
        //TRANSLATION
        targetArrivalSpot = targetDirection + transform.position;
        this.targetDirection = targetDirection;
        //UpdateTargetDirection();
        isTranslating = true;

        //ROTATION
        isRotating = true;

    }

    // Update is called once per frame
    void Update()
    {
        // update target direction to handle translation repositioning
        UpdateTargetDirection();

        //Rotate until it reached the destination
        if (isRotating)
            HandleRotation();

        //Translate until it reached the ~destination
        if (isTranslating)
            HandleTranslation();
        Debug.DrawRay(transform.position, targetArrivalSpot - transform.position, Color.green);
        Debug.DrawRay(transform.position, transform.forward * 5, Color.blue);
    }

    private void HandleRotation()
    {
        // update angle to handle translation repositioning
        GetAngleBetweenPlayerAndDirectionSigned(out float angle, out float sign);
        angleForDirection = angle;

        if (Math.Abs(va) != Math.Abs(vam))
            tava += Time.deltaTime;

        //TODO - would be better to decelerate and reverse
        va = ava * tava * tava;
        if (sign > 0)
            va = va < vam ? va : vam;
        else
            va = -va > -vam ? -va : -vam;
        /**
         * Calc if next rotation goes over target (clockwise or anti clockwise)
         * if so rotate by the remaining angle and stop rotating
         * if not keep rotating normally
         */
        var rotQta = va * Time.deltaTime;
        var projectedNewAngle = SimulateUnityRotate(transform.rotation.eulerAngles.y + rotQta);
        var targetAngle = SimulateUnityRotate(transform.rotation.eulerAngles.y + angle);
        if (Math.Sign(projectedNewAngle) == Math.Sign(targetAngle) &&
            ((sign > 0 && projectedNewAngle > targetAngle) || (sign < 0 && projectedNewAngle < targetAngle)))
        {
            //prevent overshoot
            transform.Rotate(0, angle, 0);
            //destination reached
            tava = 0;
            va = 0;
            isRotating = false;
        }
        else
            transform.Rotate(0, rotQta, 0);
    }

    public float SimulateUnityRotate(float angle)
    {
        if (angle > 180)
            angle += -360;
        else if (angle < -180)
            angle += +360;
        return angle;
    }

    public void UpdateTargetDirection()
    {
        targetDirection = targetArrivalSpot - transform.position;
    }

    private void GetAngleBetweenPlayerAndDirectionSigned(out float angle, out float sign)
    {
        //we are working on a 2d movement in a 3d world
        angle = Mathf.Rad2Deg * Mathf.Acos(Vector2.Dot(new Vector2(transform.forward.normalized.x, transform.forward.normalized.z), new Vector2(targetDirection.normalized.x, targetDirection.normalized.z)));
        // NOTE - unity uses left-handed rule for vector direction, else the cross should be reversed
        var crossProduct = Vector3.Cross(transform.forward, targetDirection);
        //calculating if negative angle or positive
        if (Vector3.Dot(crossProduct, Vector3.up) < 0)
        {
            angle = -angle;
            sign = -1;
        }
        else
            sign = 1;
    }

    private void HandleTranslation()
    {
        //if rotating we need to decelerate we simulate it by reducing t in vf = at
        //NOTE - would be better to decelerate in proportion to angle
        if (isRotating && (angleForDirection <= -90 || angleForDirection >= 90))
        {
            ta -= Time.deltaTime;
            ta = ta < 0 ? 0 : ta; //we dont wont to go in retro
        }
        else if (v < vm)
            ta += Time.deltaTime;

        //accelerating
        v = 5 + a * ta * ta;
        v = v < vm ? v : vm;

        //TODO apply v withouth overshooting
        var appliedVelocity = v * Time.deltaTime * transform.forward;
        var projectedPosition = transform.position + appliedVelocity;
        var overlay = projectedPosition - targetArrivalSpot;
        if ((overlay.x < 2 && overlay.z < 2) && (overlay.x > -2 && overlay.z > -2))
        {
            isTranslating = false;
            v = 0;
            ta = 0;
            //stop player
        }
        transform.position += appliedVelocity;
    }
}
