using UnityEngine;

public class Cameramap : MonoBehaviour
{
    [SerializeField] Mesh mesh;
    [SerializeField] Shader proj;
    [SerializeField] Renderer calclationRenderer;
    [SerializeField] Renderer simulationRenderer;
    [SerializeField] Texture2D tex;

    #region private properties
    Material matProjection_;
    Camera cam_;
    Matrix4x4 projMatrix_;
    Vector3 posCache_;
    Quaternion rotCache_ = Quaternion.identity;
    RenderTexture tmp_;
    #endregion

    void OnEnable()
    {
        cam_ = GetComponent<Camera>();
        matProjection_ = new Material(proj);
        CameraSetting();
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        var local_ = transform.worldToLocalMatrix;
        var store_ = RenderTexture.active;

        if (tmp_ == null) tmp_ = new RenderTexture(tex.width, tex.height, 0);

        Graphics.SetRenderTarget(tmp_);
        matProjection_.SetPass(0);
        matProjection_.SetMatrix("_WorldToCam", local_);
        matProjection_.SetMatrix("_ProjMatrix", projMatrix_);
        matProjection_.SetTexture("_RtCamera", src);
        Graphics.DrawMeshNow(mesh, posCache_, rotCache_);

        simulationRenderer.material.SetTexture("_MainTex", tmp_);

        RenderTexture.active = store_;
        Graphics.Blit(src, dst);

        posCache_ = calclationRenderer.transform.position;
        rotCache_ = calclationRenderer.transform.rotation;
    }

    void OnDisable()
    {
        Destroy(matProjection_);
        Destroy(tmp_); tmp_ = null;
    }

    #region gizmo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(transform.position, calclationRenderer.bounds.center);
    }
    #endregion

    void CameraSetting()
    {
        var center_ = calclationRenderer.bounds.center;
        var size_ = calclationRenderer.bounds.size;
        var x = size_.x / 2;
        var y = size_.y / 2;
        var z = size_.z / 2;
        var vertices_ = 8;
        var vtxWorldPos_ = new Vector3[vertices_];
        var vtxLocalPos_ = new Vector3[vertices_];
        var matrix_ = transform.worldToLocalMatrix;

        vtxWorldPos_ = new Vector3[]
        {
                center_ + new Vector3( x,  y,  z),
                center_ + new Vector3( x, -y,  z),
                center_ + new Vector3( x,  y, -z),
                center_ + new Vector3( x, -y, -z),
                center_ + new Vector3(-x,  y,  z),
                center_ + new Vector3(-x, -y,  z),
                center_ + new Vector3(-x,  y, -z),
                center_ + new Vector3(-x, -y, -z),
        };

        for (int i = 0; i < vertices_; i++)
        {
            vtxLocalPos_[i] = matrix_.MultiplyPoint(vtxWorldPos_[i]);
            vtxLocalPos_[i] /= vtxLocalPos_[i].z;
        }

        var c_ = matrix_.MultiplyPoint(center_);
        c_ /= c_.z;

        float distantX_ = 0;
        float distantY_ = 0;
        for (int i = 0; i < vertices_; i++)
        {
            var x_ = Mathf.Abs(vtxLocalPos_[i].x - c_.x);
            var y_ = Mathf.Abs(vtxLocalPos_[i].y - c_.y);
            if (x_ > distantX_) distantX_ = x_;
            if (y_ > distantY_) distantY_ = y_;
        }

        cam_.ResetProjectionMatrix();
        cam_.aspect = distantX_ / distantY_;
        cam_.fieldOfView = 2f * Mathf.Atan2(distantY_, 1) * Mathf.Rad2Deg;

        var ndsOffset_ = new Vector2(c_.x / distantX_, c_.y / distantY_);
        var pj = cam_.projectionMatrix;
        pj[0, 2] = ndsOffset_.x;
        pj[1, 2] = ndsOffset_.y;
        cam_.projectionMatrix = pj;

        // projection setting
        projMatrix_ = Matrix4x4.Perspective(cam_.fieldOfView, cam_.aspect, 0, 1000);
        projMatrix_[0, 2] = -ndsOffset_.x;
        projMatrix_[1, 2] = -ndsOffset_.y;
    }
}
