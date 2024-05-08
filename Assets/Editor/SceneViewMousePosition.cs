using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;


public class SceneViewMousePosition
{
    /*[InitializeOnLoadMethod]
    private static void Init()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        GameObject gameObject = Selection.activeGameObject;
        if (gameObject.IsNull())
        {
            return;
        }

        // SceneViewの3D空間の座標からGUI用の座標に変換する
        Vector2 guiPoint = HandleUtility.WorldToGUIPoint(gameObject.transform.position);

        // SceneViewのカメラの範囲外なら表示しない
        Rect viewSize = sceneView.position;
        viewSize.x = 0;
        viewSize.y = 0;
        if (!viewSize.Contains(guiPoint))
        {
            return;
        }

        // GUIの開始を宣言をします
        Handles.BeginGUI();

        // GUIの表示範囲を設定
        Vector2 offset = new Vector2(10f, 10f);
        Rect guiAreaRect = new Rect(guiPoint.x + offset.x, guiPoint.y + offset.y, 200f, 100f);

        // GUIStyleをウィンドウ形式にして、エリアを表示する
        using (new GUILayout.AreaScope(guiAreaRect, "ゲームオブジェクトの情報", GUI.skin.window))
        {
            // ゲームオブジェクトの名前を表示
            GUILayout.Label(gameObject.name);

            // 位置情報を表示
            Vector3 pos = gameObject.transform.position;
            GUILayout.Label(string.Format("x:{0:f2} y:{1:f2} z:{2:f2}", pos.x, pos.y, pos.z));

            Event e = Event.current;
            Vector2 mousePosition = e.mousePosition;
            mousePosition.y = sceneView.camera.pixelHeight - mousePosition.y;

            Vector2 worldPosition = sceneView.camera.ScreenToWorldPoint(mousePosition);
            GUILayout.Label(string.Format($"MousePos:{worldPosition}"));
        }

        // GUIの終了を宣言します
        Handles.EndGUI();
    }*/

    /*static void OnSceneGUI(SceneView view)
    {
        Event e = Event.current;
        if (e.type == EventType.MouseDown && e.button == 0) // 왼쪽 마우스 클릭 확인
        {
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(mouseRay, out hit))
            {
                Debug.Log("클릭한 지점의 좌표: " + hit.point.ToString());
            }
        }
    }*/
}
