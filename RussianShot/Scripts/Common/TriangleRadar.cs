using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class TriangleRadar : Graphic
{
    [SerializeField, Range(0, 1)] public float paramA = 1f;
    [SerializeField, Range(0, 1)] public float paramB = 0.8f;
    [SerializeField, Range(0, 1)] public float paramC = 0.5f;

    [Header("Grid Settings")]
    [SerializeField] private Color gridColor = new Color(1, 1, 1, 0.6f);
    [SerializeField] private float outerLineWidth = 2f;   // 外枠の太さ
    [SerializeField] private float innerLineWidth = 1f;   // 内側の点線の太さ
    [SerializeField] private float dashLength = 5f;       // 点線1本の長さ（短め）
    [SerializeField] private float gapLength = 3f;        // 点線の間隔（短め）

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Vector2 center = rectTransform.rect.center;
        float size = Mathf.Min(rectTransform.rect.width, rectTransform.rect.height) / 2f;

        // 3軸方向
        Vector2 dirA = new Vector2(0, 1);
        Vector2 dirB = Quaternion.Euler(0, 0, -120) * dirA;
        Vector2 dirC = Quaternion.Euler(0, 0, -240) * dirA;

        //===============================
        // 1. レーダーチャートの塗り部分
        //===============================
        Vector2 pA = center + dirA * size * paramA;
        Vector2 pB = center + dirB * size * paramB;
        Vector2 pC = center + dirC * size * paramC;

        vh.AddVert(center, color, Vector2.zero);
        vh.AddVert(pA, color, Vector2.zero);
        vh.AddVert(pB, color, Vector2.zero);
        vh.AddVert(pC, color, Vector2.zero);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);
        vh.AddTriangle(0, 3, 1);

        //===============================
        // 2. グリッド（外枠は実線 / 内側は点線）
        //===============================
        // 外枠 → 実線
        DrawSolidTriangle(vh, center, dirA, dirB, dirC, size, 1f, outerLineWidth);

        // 内側2本 → 点線
        DrawDashedTriangle(vh, center, dirA, dirB, dirC, size, 2f / 3f, innerLineWidth);
        DrawDashedTriangle(vh, center, dirA, dirB, dirC, size, 1f / 3f, innerLineWidth);
    }

    private void DrawSolidTriangle(VertexHelper vh, Vector2 center, Vector2 dirA, Vector2 dirB, Vector2 dirC, float size, float ratio, float width)
    {
        Vector2 gA = center + dirA * size * ratio;
        Vector2 gB = center + dirB * size * ratio;
        Vector2 gC = center + dirC * size * ratio;

        DrawLine(vh, gA, gB, width);
        DrawLine(vh, gB, gC, width);
        DrawLine(vh, gC, gA, width);
    }

    private void DrawDashedTriangle(VertexHelper vh, Vector2 center, Vector2 dirA, Vector2 dirB, Vector2 dirC, float size, float ratio, float width)
    {
        Vector2 gA = center + dirA * size * ratio;
        Vector2 gB = center + dirB * size * ratio;
        Vector2 gC = center + dirC * size * ratio;

        DrawDashedLine(vh, gA, gB, width);
        DrawDashedLine(vh, gB, gC, width);
        DrawDashedLine(vh, gC, gA, width);
    }

    private void DrawDashedLine(VertexHelper vh, Vector2 start, Vector2 end, float width)
    {
        float totalLength = Vector2.Distance(start, end);
        Vector2 dir = (end - start).normalized;

        float drawn = 0f;
        while (drawn < totalLength)
        {
            float segLength = Mathf.Min(dashLength, totalLength - drawn);
            Vector2 segStart = start + dir * drawn;
            Vector2 segEnd = segStart + dir * segLength;

            DrawLine(vh, segStart, segEnd, width);

            drawn += dashLength + gapLength;
        }
    }

    private void DrawLine(VertexHelper vh, Vector2 start, Vector2 end, float width)
    {
        Vector2 dir = (end - start).normalized;
        Vector2 normal = new Vector2(-dir.y, dir.x) * (width * 0.5f);

        int index = vh.currentVertCount;

        vh.AddVert(start - normal, gridColor, Vector2.zero);
        vh.AddVert(start + normal, gridColor, Vector2.zero);
        vh.AddVert(end + normal, gridColor, Vector2.zero);
        vh.AddVert(end - normal, gridColor, Vector2.zero);

        vh.AddTriangle(index, index + 1, index + 2);
        vh.AddTriangle(index, index + 2, index + 3);
    }
}
