using UnityEngine;

public partial class PlayerInputController
{
    private void UpdateCursorPos()
    {
        if (ExtraSceneManager.Instance.SceneType == SceneTypeEnum.MainMenu)
            return;

        if (m_inputLock)
            return;

        if (CameraManager.Instance.CurrentCamera.IsNull())
            return;

        if (CameraManager.Instance.CameraFocusType == CameraFocusTypeEnum.FocusTarget ||
            CameraManager.Instance.CameraFocusType == CameraFocusTypeEnum.LookFocusedTarget)
        {
            if (m_hitCollider.IsNull())
                return;

            m_cursorPos = m_hitCollider.bounds.center;
        }
        else
        {
            m_cursorPos = CameraManager.Instance.CurrentCamera.ScreenToWorldPoint(Input.mousePosition);
        }

        m_cursor.transform.position = m_cursorPos;
    }

    private void UpdateCursorShape()
    {
        if (CameraManager.Instance.CameraFocusType == CameraFocusTypeEnum.FocusTarget ||
            CameraManager.Instance.CameraFocusType == CameraFocusTypeEnum.LookFocusedTarget)
        {
            var animator = m_cursor.GetComponent<Animator>();
            animator.Play("Target_Cursor");
        }
        else
        {
            var animator = m_cursor.GetComponent<Animator>();
            animator.Play("Normal_Cursor");
        }
    }

    private void UpdateTargetInputKey()
    {
        if (m_inputLock)
            return;

        //Focus Target
        if (Input.GetKeyDown((KeyCode)HotKeyCodeEnum.Target))
        {
            var targetLayer = 1 << LayerMask.NameToLayer(DefineStrings.Enemy);
            m_hitCollider = Physics2D.OverlapPoint(m_cursorPos, targetLayer);
            if (m_hitCollider.IsNull())
                return;

            string objName = UnityUtil.GetObjectName(m_hitCollider.gameObject);
            var fieldObj = FieldManager.Instance.GetFieldObject(objName);
            if (fieldObj.IsNull())
                return;

            if (fieldObj.ObjectType != FieldObjectTypeEnum.Enemy &&
                fieldObj.ObjectType != FieldObjectTypeEnum.BossEnemy)
                return;

            CameraManager.Instance.SetCameraFocusType(CameraFocusTypeEnum.FocusTarget);

            UpdateCursorPos();
            UpdateCursorShape();
        }
    }
}