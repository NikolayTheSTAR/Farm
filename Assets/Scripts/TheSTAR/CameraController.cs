using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private CinemachineVirtualCamera virtualCamera;
    
    public void FocusTo(ICameraFocusable focus)
    {
        virtualCamera.m_Follow = focus?.transform;
    }
}

public interface ICameraFocusable
{
    Transform transform { get; }
}