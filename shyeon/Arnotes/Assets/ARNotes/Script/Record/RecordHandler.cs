using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecordHandler : MonoBehaviour
{
    public RecordButtonHandler recordButtonHandler;

    public void StartRecording()
    {
        recordButtonHandler.StartRecording();

    }
    public void StopRecording()
    {
        recordButtonHandler.StopRecording();
    }
}