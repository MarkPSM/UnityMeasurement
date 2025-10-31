using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MeasureScript : MonoBehaviour
{
    public ARRaycastManager ARRaycastManager;

    public GameObject dotPrefab;
    public TextMeshPro textPrefab;
    public Material lineMaterial;

    private List<GameObject> dots = new List<GameObject>();
    private LineRenderer line;
    private TextMeshPro measureText;

    void Start()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.startWidth = 0.005f;
        line.endWidth = 0.005f;
        line.useWorldSpace = true;
        line.material = lineMaterial;
        line.material.color = Color.green;
        line.positionCount = 0;
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Input.mousePosition;
            Debug.Log("Mouse click detected at position: " + mousePosition);

            if (Physics.Raycast(Camera.main.ScreenPointToRay(mousePosition), out RaycastHit hit))
            {
                OnTouch(hit.point);
                Debug.Log("Raycast hit at position: " + hit.point);
            }
        }
#else
    if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
    {
        Vector2 touchPosition = Input.GetTouch(0).position;
        List<ARRaycastHit> hits = new List<ARRaycastHit>();

        Debug.Log("Touch detected at position: " + touchPosition);

        if (ARRaycastManager.Raycast(touchPosition, hits, TrackableType.Planes))
        {
            Pose hitPose = hits[0].pose;
            OnTouch(hitPose.position);
        }
    }
#endif

        if (dots.Count == 1)
        {
            UpdateMeasurement();
        }
    }



    void OnTouch(Vector3 position)
    {
        var dot = Instantiate(dotPrefab, position, Quaternion.identity);
        dots.Add(dot);

        if (dots.Count == 2)
        {
            FinalizeMeasurement();
        }
    }

    void UpdateMeasurement()
    {
        Vector3 cameraPos = Camera.main.transform.position;
        Vector3 dot1 = dots[0].transform.position;

        line.positionCount = 2;
        line.SetPosition(0, dot1);
        line.SetPosition(1, cameraPos);

        float distance = Vector3.Distance(cameraPos, dot1);
        UpdateText(distance, (dot1 + cameraPos) / 2);
    }

    void UpdateText(float distance, Vector3 position)
    {
        if (measureText == null)
        {
            measureText = Instantiate(textPrefab, position, Quaternion.identity);
        }

        measureText.transform.position = Vector3.Lerp(
            measureText.transform.position,
            position + Vector3.up * 0.02f,
            Time.deltaTime * 10
        );

        measureText.text = $"{distance:F2}m";
        measureText.transform.LookAt(Camera.main.transform);
        measureText.transform.Rotate(0, 180, 0);
    }

    void FinalizeMeasurement()
    {
        Vector3 dot1 = dots[0].transform.position;
        Vector3 dot2 = dots[1].transform.position;
        float distance = Vector3.Distance(dot1, dot2);

        line.positionCount = 2;
        line.SetPosition(0, dot1);
        line.SetPosition(1, dot2);

        measureText.transform.position = (dot1 + dot2) / 2 + Vector3.up * 0.02f;
        measureText.text = $"{distance:F2}m";

        dots.Clear();
        measureText = null;
    }
}
