using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 在屏幕像素坐标中绘制 4 条 1px 直线；你可调整每条线的角度和相对屏幕中心的像素偏移；
/// 组件会把 4 条线自动聚成两组近似平行线，输出 axisRow / axisCol（单位向量）以及角度、斜率、像素间距。
/// 挂到带 Camera 的物体上（例如 Main Camera）。
/// </summary>
[ExecuteAlways]
public class FourLinesCalibrator : MonoBehaviour
{
    [Header("Line Settings (角度为相对 +X 轴，度)")]
    [Tooltip("第1条线：角度(度) 0°=水平向右，逆时针为正")]
    public float angle0 = 16f;
    [Tooltip("第2条线：角度(度)")]
    public float angle1 = 196f;
    [Tooltip("第3条线：角度(度)")]
    public float angle2 = 112f;
    [Tooltip("第4条线：角度(度)")]
    public float angle3 = 292f;

    [Space(6)]
    [Tooltip("相对屏幕中心的法向像素偏移（+ 代表沿单位法线方向移动）")]
    public float offset0 = -120f;
    public float offset1 = +120f;
    public float offset2 = -120f;
    public float offset3 = +120f;

    [Header("绘制")]
    public Material lineMat;        // 可不填，脚本会用 Hidden/Internal-Colored 自动创建
    public float lineLength = 5000; // 直线的“足够长”两端点
    public bool roundToPixel = true;// 端点对齐像素中心，避免采样模糊

    [Header("输出 (只读)")]
    [SerializeField] private Vector2 axisRow; // 单位向量
    [SerializeField] private Vector2 axisCol; // 单位向量
    [SerializeField] private float rowAngleDeg;
    [SerializeField] private float colAngleDeg;
    [SerializeField] private string rowSlope;
    [SerializeField] private string colSlope;
    [SerializeField] private float rowSpacingPx;
    [SerializeField] private float colSpacingPx;

    // ————————————— helpers —————————————
    struct Line
    {
        public Vector2 dir;     // 单位方向向量 (cos,sin)
        public Vector2 n;       // 单位法线 (-sin,cos)
        public float offset;    // 像素偏移（沿 n）
        public float angleRad;  // 角度（弧度）
        public float angleDeg;  // 角度（度）
    }

    Material _mat;
    Camera _cam;

    void Reset()
    {
        // 给出一个大致类似示意图的初始角度与偏移
        angle0 = 16f;  offset0 = -120;
        angle1 = 16f;  offset1 = +120;
        angle2 = 112f; offset2 = -120;
        angle3 = 112f; offset3 = +120;
    }

    void OnEnable()
    {
        _cam = GetComponent<Camera>();
        if (!_cam) _cam = Camera.main;
        EnsureMaterial();
    }

    void EnsureMaterial()
    {
        if (lineMat != null) { _mat = lineMat; return; }
        var shader = Shader.Find("Hidden/Internal-Colored");
        if (shader != null)
        {
            _mat = new Material(shader);
            _mat.hideFlags = HideFlags.HideAndDontSave;
            _mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _mat.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            _mat.SetInt("_ZWrite", 0);
        }
    }

    static Vector2 DirFromAngleDeg(float deg)
    {
        float rad = deg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;
    }

    static Vector2 Perp(Vector2 v) => new Vector2(-v.y, v.x);

    static Vector2 RoundToPixel(Vector2 p) => new Vector2(Mathf.Round(p.x) + 0.5f, Mathf.Round(p.y) + 0.5f);

    static string SlopeText(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) < 1e-6f) return "∞ (vertical)";
        float m = dir.y / dir.x;
        return m.ToString("0.######");
    }

    static Vector2 AverageAligned(List<Vector2> vs)
    {
        if (vs.Count == 0) return Vector2.right;
        var refv = vs[0];
        Vector2 acc = Vector2.zero;
        foreach (var v in vs)
        {
            var a = v;
            if (Vector2.Dot(a, refv) < 0f) a = -a; // 方向统一
            acc += a;
        }
        return acc.normalized;
    }

    static float AngleDeg(Vector2 v) => Mathf.Repeat(Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg, 360f);

    Line BuildLine(float deg, float offsetPx)
    {
        var d = DirFromAngleDeg(deg);
        var n = Perp(d); // 与 d 正交的单位法线
        return new Line
        {
            dir = d,
            n = n,
            offset = offsetPx,
            angleRad = Mathf.Deg2Rad * deg,
            angleDeg = deg
        };
    }

    void ClassifyAndCompute(Line[] lines)
    {
        // 以第 1 条线为参考，把“与它夹角 < 45°（|dot|>cos45）”的分为一组
        const float COS45 = 0.70710678f;
        List<int> A = new List<int> { 0 };
        List<int> B = new List<int>();

        for (int i = 1; i < 4; ++i)
        {
            float dot = Mathf.Abs(Vector2.Dot(lines[i].dir, lines[0].dir));
            (dot > COS45 ? A : B).Add(i);
        }
        // 防御：若分类极端（某组空），挪一个过去
        if (B.Count == 0) { B.Add(A[A.Count - 1]); A.RemoveAt(A.Count - 1); }

        // 平均方向（单位向量）
        var aList = new List<Vector2>(); var bList = new List<Vector2>();
        foreach (var idx in A) aList.Add(lines[idx].dir);
        foreach (var idx in B) bList.Add(lines[idx].dir);
        axisRow = AverageAligned(aList);
        axisCol = AverageAligned(bList);

        rowAngleDeg = AngleDeg(axisRow);
        colAngleDeg = AngleDeg(axisCol);
        rowSlope = SlopeText(axisRow);
        colSlope = SlopeText(axisCol);

        // 每组像素间距（同组两条线的法向距离）
        // 取同组中任意两条：距离 = |(o2 - o1)·n|，但这里我们直接用 offset 的差，因为 offset 就是沿 n 的位移（单位法线）
        float? dA = null, dB = null;
        if (A.Count >= 2)
        {
            // 为了让 offset 可比，需要统一法线方向（与 axisRow 的法线一致）
            var nRow = Perp(axisRow); // 单位
            float o0 = ProjectOffset(lines[A[0]], nRow);
            float o1 = ProjectOffset(lines[A[1]], nRow);
            dA = Mathf.Abs(o1 - o0);
        }
        if (B.Count >= 2)
        {
            var nCol = Perp(axisCol);
            float o0 = ProjectOffset(lines[B[0]], nCol);
            float o1 = ProjectOffset(lines[B[1]], nCol);
            dB = Mathf.Abs(o1 - o0);
        }
        rowSpacingPx = dA ?? 0f;
        colSpacingPx = dB ?? 0f;
    }

    // 把“该线的单位法线 * offset”投到目标法线（已单位化）上，得到可比的有符号偏移
    static float ProjectOffset(Line L, Vector2 targetNUnit)
    {
        // 线的位置：center + L.n * L.offset
        // 沿 targetN 的投影：offset * (L.n · targetNUnit)
        return L.offset * Vector2.Dot(L.n, targetNUnit);
    }

    void OnRenderObject()
    {
        if (_cam != Camera.current) return; // 确保在本相机渲染通道
        EnsureMaterial();
        if (_mat == null) return;

        // 准备 4 条线
        var l0 = BuildLine(angle0, offset0);
        var l1 = BuildLine(angle1, offset1);
        var l2 = BuildLine(angle2, offset2);
        var l3 = BuildLine(angle3, offset3);
        var lines = new[] { l0, l1, l2, l3 };

        // 计算轴向与间距
        ClassifyAndCompute(lines);

        // 绘制
        _mat.SetPass(0);
        GL.PushMatrix();
        GL.LoadPixelMatrix(0, Screen.width, Screen.height, 0); // 像素坐标
        GL.Begin(GL.LINES);

        DrawLine(lines[0], new Color(1, 0, 0, 1));
        DrawLine(lines[1], new Color(1, 0.5f, 0, 1));
        DrawLine(lines[2], new Color(0, 0.6f, 1, 1));
        DrawLine(lines[3], new Color(0, 1, 0.2f, 1));

        // 可选：画出 axisRow / axisCol（从屏幕中心射出的方向参考线）
        DrawAxis(axisRow, new Color(1, 1, 1, 0.8f));
        DrawAxis(axisCol, new Color(1, 1, 1, 0.8f));

        GL.End();
        GL.PopMatrix();
    }

    void DrawLine(Line L, Color c)
    {
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 o = center + L.n * L.offset;            // 线上一点（像素）
        Vector2 p0 = o - L.dir * lineLength;
        Vector2 p1 = o + L.dir * lineLength;
        if (roundToPixel) { p0 = RoundToPixel(p0); p1 = RoundToPixel(p1); }

        GL.Color(c);
        GL.Vertex3(p0.x, p0.y, 0);
        GL.Vertex3(p1.x, p1.y, 0);
    }

    void DrawAxis(Vector2 axis, Color c)
    {
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        Vector2 p0 = center - axis * lineLength;
        Vector2 p1 = center + axis * lineLength;
        if (roundToPixel) { p0 = RoundToPixel(p0); p1 = RoundToPixel(p1); }
        GL.Color(c);
        GL.Vertex3(p0.x, p0.y, 0);
        GL.Vertex3(p1.x, p1.y, 0);
    }

#if UNITY_EDITOR
    // 运行时在屏幕左上角显示计算结果
    void OnGUI()
    {
        var style = new GUIStyle(GUI.skin.label) { fontSize = 14 };
        float y = 10;
        void Line(string s) { GUI.Label(new Rect(10, y, 1000, 22), s, style); y += 20; }

        Line($"axisRow = ({axisRow.x:0.######}, {axisRow.y:0.######})  angle={rowAngleDeg:0.###}°  slope={rowSlope}  spacing≈{rowSpacingPx:0.###} px");
        Line($"axisCol = ({axisCol.x:0.######}, {axisCol.y:0.######})  angle={colAngleDeg:0.###}°  slope={colSlope}  spacing≈{colSpacingPx:0.###} px");
    }
#endif
}
