using System.Collections;
using UnityEngine;

public enum CameraStateEnum
{
    Follow,
    Zoom,
    Move,
}

public class CameraManager : BaseManager
{
    public static CameraManager Instance;

    private FieldObject m_target;
    private Camera m_currentCamera;
    private Vector3 m_cameraDistance;
    private float m_smooth = 5.0f;
    private CameraFocusTypeEnum m_cameraFocusType;

    private float m_zoomSpeed = 2.0f;
    private float m_zoomDuration = 1.5f;
    private float m_zoomSizeChange = 5f;
    private float m_moveSpeed = 5.0f;
    private float m_minZoomSize = 3.5f;
    private float m_maxZoomSize = 5f;

    private float m_originalSize;

    public Camera CurrentCamera => m_currentCamera;

    public CameraFocusTypeEnum CameraFocusType => m_cameraFocusType;
    private bool m_moveLock = false;

    public override void Init()
    {
        if (Instance.IsNull())
            Instance = this;

        InitMainCamera();
        InitVariable();

        Managers.ClearEvent += Clear;
        Managers.UpdateManagersEvent += UpdateManager;
    }

    private void LateUpdate()
    {
        if (m_target.IsNull())
            return;

        if (m_target.IsDead)
            return;

        FollowTarget();
    }

    private void InitMainCamera()
    {
        if (m_currentCamera.IsNull())
        {
            var mainCamera = Camera.main;
            m_currentCamera = mainCamera.GetComponent<Camera>();
        }
    }

    private void InitVariable()
    {
        //set variable
        m_smooth = DefineTable.GetFloat(DefineValueEnums.CameraMoveSmooth);
        m_cameraDistance = new Vector3(0.0f, 0.0f, -10.0f);
        m_cameraFocusType = CameraFocusTypeEnum.Normal;
        m_originalSize = m_currentCamera.orthographicSize;
    }

    public void ZoomIn()
    {
        m_moveLock = true;
        float targetSize = Mathf.Clamp(m_currentCamera.orthographicSize - m_zoomSizeChange, m_minZoomSize, m_maxZoomSize);
        StartCoroutine(ZoomCoroutine(targetSize, m_zoomDuration));
    }

    private IEnumerator ZoomCoroutine(float targetSize, float duration)
    {
        float startSize = m_currentCamera.orthographicSize;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime * m_zoomSpeed;
            float t = Mathf.Clamp01(timeElapsed / duration);
            m_currentCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            yield return null;
        }

        m_moveLock = false; 
    }

    public void ZoomOut()
    {
        m_moveLock = true;
        float targetSize = Mathf.Clamp(m_currentCamera.orthographicSize + m_zoomSizeChange, m_minZoomSize, m_maxZoomSize);
        StartCoroutine(ZoomCoroutine(targetSize, m_zoomDuration));
    }

    public void SetTarget(FieldObject fieldObj)
    {
        m_target = fieldObj;
    }

    public void Shake(float duration = 0f, float intensity = 0f)
    {
        StopCoroutine(ShakeCoroutine());
        StartCoroutine(ShakeCoroutine(duration, intensity));
    }

    private IEnumerator ShakeCoroutine(float duration = 0f, float intensity = 0f)
    {
        Vector3 startRotation = m_currentCamera.transform.eulerAngles;

        while (duration > 0.0f)
        {
            float x = Random.Range(-1f, 1f);
            float y = Random.Range(-1f, 1f);
            float z = Random.Range(-1f, 1f);
            m_currentCamera.transform.rotation = Quaternion.Euler(startRotation + new Vector3(x, y, z) * intensity);

            duration -= Time.deltaTime;
            yield return null;
        }

        m_currentCamera.transform.rotation = Quaternion.Euler(startRotation);
        m_currentCamera.transform.eulerAngles = Vector3.zero;
    }

    public void MoveToTarget(Vector3 targetPosition)
    {
        m_moveLock = true;
        Vector3 moveTarget = new Vector3(targetPosition.x, targetPosition.y, m_currentCamera.transform.position.z);
        float distance = Vector3.Distance(m_currentCamera.transform.position, moveTarget);
        float duration = (distance / m_moveSpeed);
        StartCoroutine(MoveCoroutine(m_currentCamera.transform.position, moveTarget, m_moveSpeed, duration));
    }

    private IEnumerator MoveCoroutine(Vector3 startPos, Vector3 targetPos, float speed, float duration)
    {
        float timeElapsed = 0f;
        while (timeElapsed < duration)
        {
            timeElapsed += Time.deltaTime * speed;
            float t = Mathf.Clamp01(timeElapsed / duration);
            m_currentCamera.transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        m_moveLock = false;
    }

    private void FollowTarget()
    {
        if (m_moveLock)
            return;

        if (m_currentCamera.IsNull())
            return;

        Vector3 pos = Vector3.Lerp(m_currentCamera.transform.position, m_target.Body.transform.position + m_cameraDistance, m_smooth * Time.deltaTime);
        switch (m_cameraFocusType)
        {
            case CameraFocusTypeEnum.Normal:
                {
                    pos = Vector3.Lerp(m_currentCamera.transform.position, m_target.Body.transform.position + m_cameraDistance, m_smooth * Time.deltaTime);
                }
                break;

            case CameraFocusTypeEnum.LookAround:
            case CameraFocusTypeEnum.LookFocusedTarget:
                {
                    if (PlayerInputController.Instance.CursorObject.IsNull())
                        return;

                    var aPos = m_target.Body.transform.position;
                    var bPos = m_currentCamera.ScreenToWorldPoint(Input.mousePosition);
                    if (CameraFocusType == CameraFocusTypeEnum.LookFocusedTarget)
                        bPos = m_currentCamera.ScreenToWorldPoint(Input.mousePosition);

                    Vector2 midPos = Vector2.Lerp(aPos, bPos, 0.5f);
                    Vector3 midPos3d = new Vector3(midPos.x, midPos.y, -10f);

                    float maxDistance = 6f;
                    Vector3 direction = (midPos3d - m_target.Body.transform.position).normalized;
                    float distance = Vector3.Distance(m_target.Body.transform.position, midPos3d);
                    if (distance > maxDistance)
                    {
                        midPos3d = m_target.Body.transform.position + direction * maxDistance;
                    }

                    pos = Vector3.Lerp(m_currentCamera.transform.position, midPos3d, m_smooth * Time.deltaTime);
                }
                break;
        }

        m_currentCamera.transform.position = pos;
    }

    public void SetCameraFocusType(CameraFocusTypeEnum cameraFocusType)
    {
        switch (m_cameraFocusType)
        {
            case CameraFocusTypeEnum.LookAround:
                if (cameraFocusType == CameraFocusTypeEnum.FocusTarget)
                    cameraFocusType = CameraFocusTypeEnum.LookFocusedTarget;
                if (cameraFocusType == CameraFocusTypeEnum.LookAround)
                    cameraFocusType = CameraFocusTypeEnum.Normal;
                break;

            case CameraFocusTypeEnum.FocusTarget:
                {
                    if (cameraFocusType == CameraFocusTypeEnum.LookAround)
                        cameraFocusType = CameraFocusTypeEnum.LookFocusedTarget;

                    if (cameraFocusType == CameraFocusTypeEnum.FocusTarget)
                        cameraFocusType = CameraFocusTypeEnum.Normal;
                }

                break;

            case CameraFocusTypeEnum.LookFocusedTarget:
                {
                    if (cameraFocusType == CameraFocusTypeEnum.LookAround)
                        cameraFocusType = CameraFocusTypeEnum.FocusTarget;
                    else if (cameraFocusType == CameraFocusTypeEnum.FocusTarget)
                        cameraFocusType = CameraFocusTypeEnum.LookAround;
                }
                break;
        }

        m_cameraFocusType = cameraFocusType;
    }

    public override void UpdateManager()
    {
        InitMainCamera();
    }

    public override void Clear()
    {
        m_cameraFocusType = CameraFocusTypeEnum.Normal;
        m_target = null;
        m_currentCamera = null;
    }

    public void OnDestroy()
    {
        Managers.ClearEvent -= Clear;
    }
}