using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraCursorLock : MonoBehaviour
{
    private CinemachineInputAxisController ciac;
    private void Awake()
    {
        ciac = GetComponent<CinemachineInputAxisController>();
    }

    private void OnEnable()
    {
        HandleCursorChanged(PlayerMgr.Instance.CurrentIsCursor);
        PlayerMgr.Instance.OnCursorChanged += HandleCursorChanged;
    }

    private void OnDisable()
    {
        PlayerMgr.Instance.OnCursorChanged -= HandleCursorChanged;
    }

    private void HandleCursorChanged(bool isCursor)
    {
        ciac.enabled = !isCursor;
    }
}
