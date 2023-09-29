using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grapple : MonoBehaviour
{
    //public GameObject player;
    public GameObject grappleOriginLnR;
    public GameObject grappleOriginUp;
    public Rigidbody2D PlayerRB;

    private GameObject target;
    private GameObject grappleOrigin;
    private Vector2 grappleDirection;
    private Vector2 targetPosition;
    private Vector2 targetWidthOrHeight;
    private float grappleDirectionCheck;
    //private Vector2 grapplePoint;
    //private bool isGrappling = false;

    public float grappleRange = 10f;
    public float grappleSpeed = 15f;

    private void Update()
    {
        // Find the direction the player is facing to throw the grapple

        if (Input.GetKey(KeyCode.W))
        {
            grappleOrigin = grappleOriginUp;
            grappleDirection = new Vector2(0f, grappleOrigin.transform.position.y - transform.position.y);

            // Perform the raycast and store the result in a RaycastHit2D object
            RaycastHit2D hitUp = Physics2D.Raycast(grappleOrigin.transform.position, grappleDirection, grappleRange);

            // Draw the ray as a debug line
            Debug.DrawRay(grappleOrigin.transform.position, grappleDirection * grappleRange, Color.green);

            if (hitUp == false)
            {
                target = null;
            }
            else
            {
                target = hitUp.collider.gameObject;

                if (target.CompareTag("Wall") || target.CompareTag("Enemy"))
                {
                    if (Input.GetKey(KeyCode.Q))
                    {
                        PlayerRB.gravityScale = -grappleSpeed;
                        //grappleDirectionCheck = GetGrappleDirectionCheck(targetPosition, transform.position);
                        targetWidthOrHeight = GetTargetWidthOrHeight(hitUp.collider.gameObject, 0);
                        targetPosition = GetTargetPosition(transform.position.x, hitUp.collider.gameObject.transform.position.y, targetWidthOrHeight);
                        GrapplePull(transform.position, targetPosition);
                    }
                    if (Input.GetKeyUp(KeyCode.Q))
                    {
                        PlayerRB.gravityScale = 5f;
                    }
                }
            }
        }
        else 
        {
            PlayerRB.gravityScale = 5f;
            grappleOrigin = grappleOriginLnR;
            grappleDirection = new Vector2(grappleOrigin.transform.position.x - transform.position.x, 0f);

            // Perform the raycast and store the result in a RaycastHit2D object
            RaycastHit2D hit = Physics2D.Raycast(grappleOrigin.transform.position, grappleDirection, grappleRange);

            // Draw the ray as a debug line
            Debug.DrawRay(grappleOrigin.transform.position, grappleDirection * grappleRange, Color.green);

            if (hit == false) 
            {
                target = null;
            }
            else
            {
                target = hit.collider.gameObject;

                if (target.CompareTag("Wall") || target.CompareTag("Enemy"))
                {
                    if (Input.GetKey(KeyCode.Q))
                    {
                        grappleDirectionCheck = GetGrappleDirectionCheck(targetPosition, transform.position);
                        targetWidthOrHeight = GetTargetWidthOrHeight(target, grappleDirectionCheck);
                        targetPosition = GetTargetPosition(target.transform.position.x, transform.position.y, targetWidthOrHeight);
                        GrapplePull(transform.position, targetPosition);
                    }
                }
            }
        }
    }
    public void GetGrappleTarget(RaycastHit2D hit) 
    {
        if ((hit.collider.gameObject.tag == "Wall" || hit.collider.gameObject.tag == "Enemy")
            && Input.GetKey(KeyCode.Q))
        {
            grappleDirectionCheck = GetGrappleDirectionCheck(targetPosition, transform.position);
            targetWidthOrHeight = GetTargetWidthOrHeight(hit.collider.gameObject, grappleDirectionCheck);
            targetPosition = GetTargetPosition(hit.collider.gameObject.transform.position.x, transform.position.y, targetWidthOrHeight);
            GrapplePull(transform.position, targetPosition);
        }
    }

    public Vector2 GetTargetPosition(float xTarget, float yTarget,Vector2 widthOrHeight)
    {
        Vector2 position = new Vector2(xTarget + widthOrHeight.x, yTarget + widthOrHeight.y);
        return position;
    }

    public Vector2 GetTargetWidthOrHeight(GameObject target, float playerDirection) 
    {
        Renderer objectRenderer = target.GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            if (playerDirection !=0)
            {
                // Get the width (x-axis size) of the object
                float objectWidth = objectRenderer.bounds.size.x / 2;

                if (grappleDirectionCheck > 0)
                {
                    objectWidth = -objectWidth;
                }

                return new Vector2(objectWidth, 0);
            }
            else
            {
                float objectHeight = objectRenderer.bounds.size.y / 2;
                objectHeight = -objectHeight;
                return new Vector2(0, objectHeight);
            }
        }
        else
        {
            Debug.LogWarning("Renderer component not found on the hit object.");
            return new Vector2(0,0);
        }
    }

    public float GetGrappleDirectionCheck(Vector2 target, Vector2 currentPosition) 
    {
        float directionCheck = target.x - currentPosition.x;
        return directionCheck;
    }

    public void GrapplePull(Vector2 cPosition, Vector2 tPosition) 
    {
        /*
        if (cPosition == null || tPosition == null)
        {

        }
        else
        {
            float distanceToTarget = Vector2.Distance(cPosition, tPosition);
            float step = grappleSpeed * Time.deltaTime;

            transform.position = Vector2.Lerp(cPosition, tPosition, Mathf.Clamp01(step / distanceToTarget));
        }
        */

        float distanceToTarget = Vector2.Distance(cPosition, tPosition);

        // Calculate the step based on the grappleSpeed
        float step = grappleSpeed * Time.deltaTime;

        // Make sure step is never greater than 1 to prevent overshooting
        step = Mathf.Clamp01(step / distanceToTarget);

        // Lerp only if the distance to the target is greater than a small threshold
        if (distanceToTarget > 0.1f)
        {
            transform.position = Vector2.Lerp(cPosition, tPosition, step);
        }

        /*
        isGrappling = true;
        grapplePoint = hit.point;

        if (isGrappling)
        {
            Vector2 targetPosition = grapplePoint;
            Vector2 currentPosition = transform.position;
            float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);

            if (Input.GetKey(KeyCode.Q))
            {
                // Continue to stick to the wall
                float step = grappleSpeed * Time.deltaTime;
                transform.position = Vector2.Lerp(currentPosition, targetPosition, Mathf.Clamp01(step / distanceToTarget));
            }
            else
            {
                // Release the grapple
                isGrappling = false;
            }
        }
        */
    }
}
