/*****************************************************
* 文件名称：RectTransformPrecisionToolEditor.cs
* 创建时间：2025-08-03 12:25:40
* 作者：高博
* 文件版本：1.0
*****************************************************/

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// RectTransform精度控制工具
/// </summary>
public class RectTransformPrecisionToolEditor : EditorWindow
{
    // 精度设置 - 改为静态
    private static int _positionPrecision = 0;
    private static int _sizePrecision = 0;
    private static int _anchorPrecision = 3;
    private static int _pivotPrecision = 3;
    private static int _rotationPrecision = 0;
    private static int _scalePrecision = 3;

    // 启用限制的选项 - 改为静态
    private static bool _enablePosition = true;
    private static bool _enableSize = true;
    private static bool _enableAnchors = true;
    private static bool _enablePivot = true;
    private static bool _enableRotation = false;
    private static bool _enableScale = false;

    // 自动应用选项 - 改为静态
    public static bool _autoApplyOnCreate = true;
    public static bool _autoApplyOnModify = true;
    public Vector2 scrollPosition;

    // 存储最后处理的对象 - 改为静态
    public static GameObject lastProcessedObject;
    public static RectTransform lastTransform;
    public static Vector3 lastPosition;
    public static Vector2 lastSize;
    public static Vector2 lastAnchorMin;
    public static Vector2 lastAnchorMax;
    public static Vector2 lastPivot;
    public static Vector3 lastRotation;
    public static Vector3 lastScale;

    [MenuItem("GBTools/编辑器辅助/RectTransform精度控制", false, 0)]
    public static void ShowWindow()
    {
        GetWindow<RectTransformPrecisionToolEditor>("RectTransform Precision Tool");
    }

    void OnEnable()
    {
        // 加载保存的设置
        LoadSettings();
    }

    void OnDisable()
    {
        // 保存设置
        SaveSettings();
    }

    // GUI绘制部分
    void OnGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.LabelField("RectTransform精度控制工具", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("自动限制RectTransform参数的小数点精度，确保UI布局整洁一致。", MessageType.Info);

        GUILayout.Space(15);

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // ==================== 启用限制选项 ====================
        GUILayout.Label("启用限制选项", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("勾选需要要应用的属性，限制数值为整数，精度为小数点后位数。\n注意：Unity Inspector可能会省略末尾的零（如1.20显示为1.2）", MessageType.None);
        GUILayout.BeginVertical("Box");

        GUILayout.BeginHorizontal("Box");
        _enablePosition = EditorGUILayout.ToggleLeft("位置 (Anchored Position)", _enablePosition, GUILayout.Width(180f));
        EditorGUILayout.LabelField("精度:", GUILayout.Width(30f));
        _positionPrecision = Mathf.Clamp(EditorGUILayout.IntField(_positionPrecision, GUILayout.Width(50f)), 0, 2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        _enableSize = EditorGUILayout.ToggleLeft("尺寸 (Size Delta)", _enableSize, GUILayout.Width(180f));
        EditorGUILayout.LabelField("精度:", GUILayout.Width(30f));
        _sizePrecision = Mathf.Clamp(EditorGUILayout.IntField(_sizePrecision, GUILayout.Width(50f)), 0, 2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        _enableAnchors = EditorGUILayout.ToggleLeft("锚点 (Anchors)", _enableAnchors, GUILayout.Width(180f));
        EditorGUILayout.LabelField("精度:", GUILayout.Width(30f));
        _anchorPrecision = Mathf.Clamp(EditorGUILayout.IntField(_anchorPrecision, GUILayout.Width(50f)), 0, 2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        _enablePivot = EditorGUILayout.ToggleLeft("中心点 (Pivot)", _enablePivot, GUILayout.Width(180f));
        EditorGUILayout.LabelField("精度:", GUILayout.Width(30f));
        _pivotPrecision = Mathf.Clamp(EditorGUILayout.IntField(_pivotPrecision, GUILayout.Width(50f)), 0, 2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        _enableRotation = EditorGUILayout.ToggleLeft("旋转 (Rotation)", _enableRotation, GUILayout.Width(180f));
        EditorGUILayout.LabelField("精度:", GUILayout.Width(30f));
        _rotationPrecision = Mathf.Clamp(EditorGUILayout.IntField(_rotationPrecision, GUILayout.Width(50f)), 0, 2);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal("Box");
        _enableScale = EditorGUILayout.ToggleLeft("缩放 (Scale)", _enableScale, GUILayout.Width(180f));
        EditorGUILayout.LabelField("精度:", GUILayout.Width(30f));
        _scalePrecision = Mathf.Clamp(EditorGUILayout.IntField(_scalePrecision, GUILayout.Width(50f)), 0, 2);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();
        GUILayout.Space(20);

        // ==================== 自动应用选项 ====================
        GUILayout.Label("自动应用", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("自动应用精度限制的场景", MessageType.None);

        GUILayout.BeginVertical("Box");
        _autoApplyOnCreate = EditorGUILayout.ToggleLeft("创建时应用", _autoApplyOnCreate);
        _autoApplyOnModify = EditorGUILayout.ToggleLeft("修改时应用", _autoApplyOnModify);
        GUILayout.EndVertical();

        GUILayout.Space(20);

        // ==================== 操作按钮 ====================
        GUILayout.Label("手动操作", EditorStyles.boldLabel);

        if (GUILayout.Button("应用精度到选中对象", GUILayout.Height(30)))
        {
            ApplyPrecisionToSelection();
        }

        if (GUILayout.Button("应用精度到场景中所有UI", GUILayout.Height(30)))
        {
            ApplyPrecisionToAllUI();
        }

        EditorGUILayout.EndScrollView();

        // 显示当前设置状态
        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            $"当前启用的限制: " +
            (_enablePosition ? $"位置({_positionPrecision}) " : "") +
            (_enableSize ? $"尺寸({_sizePrecision}) " : "") +
            (_enableAnchors ? $"锚点({_anchorPrecision}) " : "") +
            (_enablePivot ? $"中心点({_pivotPrecision}) " : "") +
            (_enableRotation ? $"旋转({_rotationPrecision}) " : "") +
            (_enableScale ? $"缩放({_scalePrecision})" : ""),
            MessageType.Info
        );
    }

    private static void ApplyPrecisionToSelection()
    {
        if (Selection.gameObjects.Length == 0)
        {
            Debug.LogWarning("没有选中任何对象");
            return;
        }

        Undo.RecordObjects(GetSelectedRectTransforms(), "Apply RectTransform Precision");

        int processedCount = 0;
        foreach (GameObject go in Selection.gameObjects)
        {
            RectTransform[] rectTransforms = go.GetComponentsInChildren<RectTransform>();
            foreach (RectTransform rt in rectTransforms)
            {
                ApplyPrecision(rt);
                processedCount++;
            }
        }

        Debug.Log($"已应用精度设置到 {processedCount} 个UI元素");
    }

    private static void ApplyPrecisionToAllUI()
    {
        RectTransform[] allRects = Resources.FindObjectsOfTypeAll<RectTransform>();
        Undo.RecordObjects(allRects, "Apply RectTransform Precision to All");

        int processedCount = 0;
        foreach (RectTransform rt in allRects)
        {
            if (rt.gameObject.hideFlags == HideFlags.NotEditable ||
                rt.gameObject.hideFlags == HideFlags.HideAndDontSave)
                continue;

            ApplyPrecision(rt);
            processedCount++;
        }

        Debug.Log($"已应用精度设置到场景中所有 {processedCount} 个UI元素");
    }

    public static void ApplyPrecision(RectTransform rt)
    {
        if (_enablePosition)
        {
            Vector3 pos = rt.anchoredPosition3D;
            rt.anchoredPosition3D = new Vector3(
                RoundToPrecision(pos.x, _positionPrecision),
                RoundToPrecision(pos.y, _positionPrecision),
                RoundToPrecision(pos.z, _positionPrecision)
            );
        }

        if (_enableSize)
        {
            Vector2 size = rt.sizeDelta;
            rt.sizeDelta = new Vector2(
                RoundToPrecision(size.x, _sizePrecision),
                RoundToPrecision(size.y, _sizePrecision)
            );
        }

        if (_enableAnchors)
        {
            Vector2 anchorMin = rt.anchorMin;
            Vector2 anchorMax = rt.anchorMax;
            rt.anchorMin = new Vector2(
                RoundToPrecision(anchorMin.x, _anchorPrecision),
                RoundToPrecision(anchorMin.y, _anchorPrecision)
            );
            rt.anchorMax = new Vector2(
                RoundToPrecision(anchorMax.x, _anchorPrecision),
                RoundToPrecision(anchorMax.y, _anchorPrecision)
            );
        }

        if (_enablePivot)
        {
            Vector2 pivot = rt.pivot;
            rt.pivot = new Vector2(
                RoundToPrecision(pivot.x, _pivotPrecision),
                RoundToPrecision(pivot.y, _pivotPrecision)
            );
        }

        if (_enableRotation)
        {
            Vector3 rot = rt.localEulerAngles;
            rt.localEulerAngles = new Vector3(
                RoundToPrecision(rot.x, _rotationPrecision),
                RoundToPrecision(rot.y, _rotationPrecision),
                RoundToPrecision(rot.z, _rotationPrecision)
            );
        }

        if (_enableScale)
        {
            Vector3 scale = rt.localScale;
            rt.localScale = new Vector3(
                RoundToPrecision(scale.x, _scalePrecision),
                RoundToPrecision(scale.y, _scalePrecision),
                RoundToPrecision(scale.z, _scalePrecision)
            );
        }
    }

    private static float RoundToPrecision(float value, int precision)
    {
        if (precision == 0) return Mathf.Round(value);

        float multiplier = Mathf.Pow(10, precision);
        return Mathf.Round(value * multiplier) / multiplier;
    }

    private static RectTransform[] GetSelectedRectTransforms()
    {
        List<RectTransform> transforms = new List<RectTransform>();

        foreach (GameObject go in Selection.gameObjects)
        {
            transforms.AddRange(go.GetComponentsInChildren<RectTransform>());
        }

        return transforms.ToArray();
    }

    private static void SaveSettings()
    {
        EditorPrefs.SetInt("RT_Precision_Pos", _positionPrecision);
        EditorPrefs.SetInt("RT_Precision_Size", _sizePrecision);
        EditorPrefs.SetInt("RT_Precision_Anchor", _anchorPrecision);
        EditorPrefs.SetInt("RT_Precision_Pivot", _pivotPrecision);
        EditorPrefs.SetInt("RT_Precision_Rot", _rotationPrecision);
        EditorPrefs.SetInt("RT_Precision_Scale", _scalePrecision);

        EditorPrefs.SetBool("RT_Enable_Pos", _enablePosition);
        EditorPrefs.SetBool("RT_Enable_Size", _enableSize);
        EditorPrefs.SetBool("RT_Enable_Anchor", _enableAnchors);
        EditorPrefs.SetBool("RT_Enable_Pivot", _enablePivot);
        EditorPrefs.SetBool("RT_Enable_Rot", _enableRotation);
        EditorPrefs.SetBool("RT_Enable_Scale", _enableScale);

        EditorPrefs.SetBool("RT_AutoCreate", _autoApplyOnCreate);
        EditorPrefs.SetBool("RT_AutoModify", _autoApplyOnModify);
    }

    public static void LoadSettings()
    {
        _positionPrecision = EditorPrefs.GetInt("RT_Precision_Pos", 0);
        _sizePrecision = EditorPrefs.GetInt("RT_Precision_Size", 0);
        _anchorPrecision = EditorPrefs.GetInt("RT_Precision_Anchor", 3);
        _pivotPrecision = EditorPrefs.GetInt("RT_Precision_Pivot", 3);
        _rotationPrecision = EditorPrefs.GetInt("RT_Precision_Rot", 0);
        _scalePrecision = EditorPrefs.GetInt("RT_Precision_Scale", 3);

        _enablePosition = EditorPrefs.GetBool("RT_Enable_Pos", true);
        _enableSize = EditorPrefs.GetBool("RT_Enable_Size", true);
        _enableAnchors = EditorPrefs.GetBool("RT_Enable_Anchor", true);
        _enablePivot = EditorPrefs.GetBool("RT_Enable_Pivot", true);
        _enableRotation = EditorPrefs.GetBool("RT_Enable_Rot", false);
        _enableScale = EditorPrefs.GetBool("RT_Enable_Scale", false);

        _autoApplyOnCreate = EditorPrefs.GetBool("RT_AutoCreate", true);
        _autoApplyOnModify = EditorPrefs.GetBool("RT_AutoModify", true);
    }
}

// 编辑器启动时自动初始化的类
[InitializeOnLoad]
public static class RectTransformPrecisionAutoStarter
{
    // 静态构造函数，在Unity加载时自动执行
    static RectTransformPrecisionAutoStarter()
    {
        // 注册编辑器启动完成事件
        EditorApplication.update += OnEditorLoaded;
    }

    private static bool hasInitialized = false;

    private static void OnEditorLoaded()
    {
        if (hasInitialized) return;

        // 确保只初始化一次
        hasInitialized = true;
        EditorApplication.update -= OnEditorLoaded;

        // 加载保存的设置
        RectTransformPrecisionToolEditor.LoadSettings();

        // 注册必要的事件
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.hierarchyChanged += OnHierarchyChanged;
        EditorApplication.update += Update;

        Debug.Log("RectTransform精度控制工具已自动启动");
    }

    private static void OnHierarchyChanged()
    {
        if (!RectTransformPrecisionToolEditor._autoApplyOnCreate) return;

        GameObject activeGO = Selection.activeGameObject;
        if (activeGO != null && activeGO != RectTransformPrecisionToolEditor.lastProcessedObject)
        {
            RectTransform rt = activeGO.GetComponent<RectTransform>();
            if (rt != null)
            {
                RectTransformPrecisionToolEditor.ApplyPrecision(rt);
                RectTransformPrecisionToolEditor.lastProcessedObject = activeGO;
            }
        }
    }

    private static void OnSelectionChanged()
    {
        if (!RectTransformPrecisionToolEditor._autoApplyOnModify) return;

        GameObject selected = Selection.activeGameObject;
        if (selected != null)
        {
            RectTransform rt = selected.GetComponent<RectTransform>();
            if (rt != null && rt != RectTransformPrecisionToolEditor.lastTransform)
            {
                RectTransformPrecisionToolEditor.lastTransform = rt;
                RectTransformPrecisionToolEditor.lastPosition = rt.anchoredPosition3D;
                RectTransformPrecisionToolEditor.lastSize = rt.sizeDelta;
                RectTransformPrecisionToolEditor.lastAnchorMin = rt.anchorMin;
                RectTransformPrecisionToolEditor.lastAnchorMax = rt.anchorMax;
                RectTransformPrecisionToolEditor.lastPivot = rt.pivot;
                RectTransformPrecisionToolEditor.lastRotation = rt.localEulerAngles;
                RectTransformPrecisionToolEditor.lastScale = rt.localScale;
            }
        }
    }

    private static void Update()
    {
        if (!RectTransformPrecisionToolEditor._autoApplyOnModify ||
            RectTransformPrecisionToolEditor.lastTransform == null)
            return;

        RectTransform rt = RectTransformPrecisionToolEditor.lastTransform;

        if (rt.anchoredPosition3D != RectTransformPrecisionToolEditor.lastPosition ||
            rt.sizeDelta != RectTransformPrecisionToolEditor.lastSize ||
            rt.anchorMin != RectTransformPrecisionToolEditor.lastAnchorMin ||
            rt.anchorMax != RectTransformPrecisionToolEditor.lastAnchorMax ||
            rt.pivot != RectTransformPrecisionToolEditor.lastPivot ||
            rt.localEulerAngles != RectTransformPrecisionToolEditor.lastRotation ||
            rt.localScale != RectTransformPrecisionToolEditor.lastScale)
        {
            RectTransformPrecisionToolEditor.ApplyPrecision(rt);

            RectTransformPrecisionToolEditor.lastPosition = rt.anchoredPosition3D;
            RectTransformPrecisionToolEditor.lastSize = rt.sizeDelta;
            RectTransformPrecisionToolEditor.lastAnchorMin = rt.anchorMin;
            RectTransformPrecisionToolEditor.lastAnchorMax = rt.anchorMax;
            RectTransformPrecisionToolEditor.lastPivot = rt.pivot;
            RectTransformPrecisionToolEditor.lastRotation = rt.localEulerAngles;
            RectTransformPrecisionToolEditor.lastScale = rt.localScale;
        }
    }
}