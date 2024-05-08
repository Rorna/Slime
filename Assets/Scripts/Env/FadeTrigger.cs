using UnityEngine;

public class FadeTrigger : MonoBehaviour
{
    [SerializeField] private bool m_isEnd;

    private void OnTriggerEnter2D(Collider2D collider)
    {
        var fieldObj = FieldManager.Instance.GetFieldObject(UnityUtil.GetObjectName(collider.gameObject));
        if (fieldObj.IsNull())
            return;

        if (fieldObj.IsPlayer == false)
            return;

        if (m_isEnd)
        {
            PlayerInputController.Instance.UpdateInputLock(true);

            var fadeUI = UIManager.Instance.GetUIObject<UIFade>();
            fadeUI.Show();

            PlayerPrefs.DeleteAll();
            return;
        }

        ExtraSceneManager.Instance.LoadNextScene();
    }
}