//----------------------------------------------
//            NGUI: Next-Gen UI kit
// Copyright © 2011-2013 Tasharen Entertainment
//----------------------------------------------

using UnityEngine;
using System.Collections.Generic;
using System;

[ExecuteInEditMode, RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LabelSprite : MonoBehaviour 
{
    public enum Effect
    {
        None,
        Shadow,
        Outline,
    }

    public UIFont Font;
    public string Text = "";
    public Color TextColor1 = Color.white;
    public Color TextColor2 = Color.white;
    public int LineWidth = 0;
    public Effect EffectStyle = Effect.None;
    public Color EffectColor = Color.black;
    public Vector2 EffectDistance = Vector2.one;

    public bool RefreshNow = true;
    public bool MakePerfect = false;

    private Mesh mMesh;

    void Awake()
    {
        mMesh = new Mesh();
#if UNITY_EDITOR
        mMesh.name = "LabelSprite";
        mMesh.hideFlags = HideFlags.HideAndDontSave;
#endif

        GetComponent<MeshFilter>().mesh = mMesh;

        Refresh();
    }
    void OnEnable()
    {
        Refresh();
    }

#if UNITY_EDITOR
    void Update()
    {
        if (RefreshNow)
        {
            RefreshNow = false;
            Refresh();
        }

        if (MakePerfect)
        {
            MakePerfect = false;
            MakePixelPerfect();
        }
    }
#endif

    public void MakePixelPerfect()
    {
        Vector3 scale = transform.localScale;
        scale.x = Font.size * Font.pixelSize;
        scale.y = scale.x;
        scale.z = 1f;
        transform.localScale = scale;
    }

    public void SetText(string text)
    {
        Text = text;
        RefreshText();
    }

    public void Refresh()
    {
        RefreshMaterial();
        RefreshText();
    }


    void RefreshMaterial()
    {
        if (Font != null)
        {
            renderer.material = Font.material;
        }
        else
        {
            renderer.material = null;
        }
    }

    static readonly BetterList<Vector3> s_verts = new BetterList<Vector3>();
    static readonly BetterList<Vector2> s_uvs = new BetterList<Vector2>();
    static readonly BetterList<Color32> s_cols = new BetterList<Color32>();
    public void RefreshText()
    {
        if (Font == null) return;

        mMesh.Clear();
        var text = Text.Replace("\\n", "\n");
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        s_verts.Clear();
        s_uvs.Clear();
        s_cols.Clear();
        Vector3 offsetVec = Font.CalculatePrintedSize(text, true, UIFont.SymbolStyle.Uncolored) * -0.5f;
        Font.Print(text, TextColor1, TextColor2, UIWidget.E_Direction.Vertical, s_verts, s_uvs, s_cols, false, UIFont.SymbolStyle.Uncolored, UIFont.Alignment.Center, LineWidth, false);

        int drawCnt = 1;
        if (EffectStyle == Effect.Outline)
        {
            drawCnt = 5;
        }
        else if (EffectStyle == Effect.Shadow)
        {
            drawCnt = 2;
        }

        var uvs = new Vector2[s_uvs.size * drawCnt];
        var cols = new Color32[s_cols.size * drawCnt];
        Array.Copy(s_uvs.buffer, uvs, s_uvs.size);
        Array.Copy(s_cols.buffer, cols, s_cols.size);

        var verts = new Vector3[s_verts.size * drawCnt];
        for (int i = 0; i < s_verts.size; ++i)
        {
            verts[i] = s_verts[i] + offsetVec;
        }
        if (EffectStyle != Effect.None)
        {
            float pixel = 1f / Font.size;

            float fx = pixel * EffectDistance.x;
            float fy = pixel * EffectDistance.y;
            if (EffectStyle == Effect.Outline)
            {
                ApplyShadow(verts, uvs, cols, s_verts.size * 0, s_verts.size, new Vector3(fx, fy));
                ApplyShadow(verts, uvs, cols, s_verts.size * 1, s_verts.size, new Vector3(fx, -fy));
                ApplyShadow(verts, uvs, cols, s_verts.size * 2, s_verts.size, new Vector3(-fx, fy));
                ApplyShadow(verts, uvs, cols, s_verts.size * 3, s_verts.size, new Vector3(-fx, -fy));
            }
            else if (EffectStyle == Effect.Shadow)
            {
                ApplyShadow(verts, uvs, cols, 0, s_verts.size, new Vector3(fx, fy));
            }
        }

        int rcCount = verts.Length / 4;
        var triangles = new int[rcCount * 6];
        for (int i = 0; i < rcCount; ++i)
        {
            triangles[i * 6 + 0] = i * 4 + 0;
            triangles[i * 6 + 1] = i * 4 + 1;
            triangles[i * 6 + 2] = i * 4 + 2;

            triangles[i * 6 + 3] = i * 4 + 2;
            triangles[i * 6 + 4] = i * 4 + 3;
            triangles[i * 6 + 5] = i * 4 + 0;
        }


        mMesh.vertices = verts;
        mMesh.triangles = triangles;
        mMesh.uv = uvs;
        mMesh.colors32 = cols;
    }

    void ApplyShadow(Vector3[] verts, Vector2[] uvs, Color32[] cols, int start, int len, Vector3 offset)
    {
        int end = start + len;
        for (int i = start; i < end; ++i)
        {
            verts[i + len] = verts[i];
            uvs[i + len] = uvs[i];
            cols[i + len] = cols[i];

            verts[i] += offset;
            cols[i] = EffectColor;
        }
    }
}
