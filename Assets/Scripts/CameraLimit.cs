using Cinemachine;
using UnityEngine;

public class CinemachineLineLimit : CinemachineExtension
{
    // 直線が通る点
    [SerializeField] private Vector3 _origin = Vector3.right;

    // 直線の向き
    [SerializeField] private Vector3 _direction = Vector3.up;

    // Extensionコールバック
    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime
    )
    {
        // カメラ移動後のみ処理を実行することとする
        if (stage != CinemachineCore.Stage.Body)
            return;

        // レイを定義
        var ray = new Ray(_origin, _direction);
        // 計算されたカメラ位置
        var point = state.RawPosition;

        // レイ上に投影したカメラ位置を計算
        point -= ray.origin;
        point = Vector3.Project(point, ray.direction);
        point += ray.origin;

        // 投影点をカメラ位置に反映
        state.RawPosition = point;
    }

    #region DrawGizmos

    private const float GizmoLineLength = 1000;

    // 移動範囲をエディタ上で表示(確認用)
    private void OnDrawGizmos()
    {
        if (!isActiveAndEnabled) return;

        var ray = new Ray(_origin, _direction);

        Debug.DrawRay(
            ray.origin - ray.direction * GizmoLineLength / 2,
            ray.direction * GizmoLineLength
        );
    }

    #endregion
}