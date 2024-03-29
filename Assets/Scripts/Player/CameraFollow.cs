﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraFollow : MonoBehaviour
{
	public Camera mainCamera;
	public float dungeonCameraSize;
	public float villageCameraSize;

	public float FollowSpeed = 2f;
	public Transform Target;

	// Transform of the camera to shake. Grabs the gameObject's transform
	// if null.
	private Transform camTransform;

	// How long the object should shake for.
	public float shakeDuration = 0f;

	// Amplitude of the shake. A larger value shakes the camera harder.
	public float shakeAmount = 0.1f;
	public float decreaseFactor = 1.0f;

	public bool shouldFollow;
	public string firstSceneName = "FirstLevel";
	public string villageSceneName = "Village1";

	Vector3 originalPos;

	void Awake()
	{
		Cursor.visible = false;
		if (camTransform == null)
		{
			camTransform = GetComponent(typeof(Transform)) as Transform;
		}
	}

	void OnEnable()
	{
		originalPos = camTransform.localPosition;
	}

	private void Update()
	{
		if (SceneManager.GetActiveScene().name == firstSceneName)
		{
			shouldFollow = true;
			mainCamera.orthographicSize = dungeonCameraSize;
		}

		if (SceneManager.GetActiveScene().name == villageSceneName)
		{
			shouldFollow = false;
			mainCamera.orthographicSize = villageCameraSize;
			mainCamera.transform.position = new Vector3(0, -1, -0.3f);
		}

		if (Target == null)
        {
			GameObject playerObject = GameObject.Find("Player");
			if (playerObject != null)
			{
				Target = playerObject.GetComponent<CharacterController2D>().transform;
			}
			else
			{
				Debug.LogError("CanvasPlayer not found in the scene.");
			}
			Debug.Log("Target null");
        }

		if (shouldFollow) 
		{
			Vector3 newPosition = Target.position;
			newPosition.z = -10;
			transform.position = Vector3.Slerp(transform.position, newPosition, FollowSpeed * Time.deltaTime);

			if (shakeDuration > 0)
			{
				camTransform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

				shakeDuration -= Time.deltaTime * decreaseFactor;
			}
		}
	}

	public void ShakeCamera()
	{
		originalPos = camTransform.localPosition;
		shakeDuration = 0.2f;
	}
}
