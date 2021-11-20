using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Tools.NGUIAtlasToSprite
{
    public class NGUIAtlasToSpriteEditorWindow : EditorWindow
    {
        private static Vector2 m_ScrollPosition = Vector2.zero;
        private static ReorderableList m_RuleList;
        private static ResourceRuleEditorData m_Configuration;

        [MenuItem("Tools/NGUIAtlasToSprite - NGUI图集转单图")]
        private static void Open()
        {
            NGUIAtlasToSpriteEditorWindow window = GetWindow<NGUIAtlasToSpriteEditorWindow>("NGUIAtlasToSprite", true);
            window.minSize = new Vector2(500, 500);
        }

        private void OnGUI()
        {
            if (m_Configuration == null)
            {
                Load();
            }

            if (m_RuleList == null)
            {
                InitRuleListDrawer();
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(position.width), GUILayout.Height(position.height));
            {
                GUILayout.Space(5f);
                EditorGUILayout.LabelField("操作按钮", EditorStyles.boldLabel);
                EditorGUILayout.BeginHorizontal("box");
                {
                    if (GUILayout.Button("检索所有NGUI Atlas"))
                    {
                        FindAllUIAtlas();
                    }

                    if (GUILayout.Button("导出单图"))
                    {
                        ExportNGUIAtlasToSprite();
                    }
                }
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(5f);
                EditorGUILayout.LabelField("数据列表", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                {
                    // 滚动视图开始
                    m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition);
                    {
                        // 绘制列表
                        m_RuleList.DoLayoutList();
                    }
                    // 滚动视图结束
                    GUILayout.EndScrollView();
                }
                GUILayout.EndVertical();
            }
            EditorGUILayout.EndVertical();

            if (GUI.changed)
                EditorUtility.SetDirty(m_Configuration);
        }

        /// <summary>
        /// 校验数值
        /// </summary>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="value">目标数值</param>
        /// <param name="valueName">数值变量名</param>
        /// <returns></returns>
        private static int CheckValue(int min, int max, int value, string valueName)
        {
            if (value < min)
            {
                value = min;
                Debug.LogError("UISpriteData's " + valueName + " is " + value + ", less than " + min + ", Auto correct to " + min);
            }

            if (value > max)
            {
                value = max;
                Debug.LogError("UISpriteData's " + valueName + " is " + value + ", more than " + max + ", Auto correct to " + max);
            }

            return value;
        }

        /// <summary>
        /// 将NGUI图集导出成单图
        /// </summary>
        private static void ExportNGUIAtlasToSprite()
        {
            if (m_Configuration.rules.Count <= 0)
            {
                return;
            }
            // 图集导出进度
            int progress = 0;
            EditorUtility.DisplayProgressBar(
                "导出单图中",
                "",
                progress / m_Configuration.rules.Count);

            // 校验输出目录根目录是否存在
            string exportRootPath = Application.dataPath;
            if (!Directory.Exists(exportRootPath))
            {
                Directory.CreateDirectory(exportRootPath);
            }

            // 校验输出目录是否存在
            string exportPath = exportRootPath + "/ExportSprite";
            if (!Directory.Exists(exportPath))
            {
                Directory.CreateDirectory(exportPath);
            }

            foreach (var rule in m_Configuration.rules)
            {
                // 单个图集里每张单图的导出进度
                int progressIndex = 0;

                int textureMaxWidth = rule.atlas.texture.width;
                int textureMaxHeight = rule.atlas.texture.height;
                List<UISpriteData> list = rule.atlas.spriteList;
                // GetPixels必须要可读属性，这里开启了可读属性
                Texture2D texture = Utility.DuplicateTexture(rule.texture);
                UISpriteData sd;
                for (int i = 0; i < list.Count; i++)
                {
                    progressIndex++;

                    sd = list[i];
                    // 反转y坐标
                    int sdY = CheckValue(0, textureMaxHeight, texture.height - (sd.y + sd.height), "sdY");
                    int sdX = CheckValue(0, textureMaxWidth, sd.x, "sdX");
                    int sdWidth = CheckValue(0, textureMaxWidth, sd.width, "sdWidth");
                    int sdHeight = CheckValue(0, textureMaxHeight, sd.height, "sdHeight");

                    // GetPixels()读取像素的顺序是从左到右，从下到上。
                    Color[] colors = texture.GetPixels(sdX, sdY, sdWidth, sdHeight);

                    Texture2D tex = new Texture2D(sdWidth, sdHeight, TextureFormat.RGBA32, false);
                    tex.SetPixels(0, 0, sdWidth, sdHeight, colors);
                    tex.Apply();

                    string path = exportPath + "/" + rule.atlas.name;
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }

                    string filePath = Path.Combine(path, sd.name + ".png");

                    // 进度增量
                    float progressDelta = 1.0f / m_Configuration.rules.Count;
                    // 进度
                    float progressValue = (float)progress / m_Configuration.rules.Count + progressDelta * progressIndex / list.Count;

                    EditorUtility.DisplayProgressBar(
                        "导出单图中",
                        rule.atlas.name + "/" + sd.name + ".png",
                        progressValue);

                    Utility.SavePNG(filePath, tex);
                }
                
                progress++;
            }

            EditorUtility.ClearProgressBar();

            AssetDatabase.Refresh();
            
            Debug.Log("导出完成，路径：" + exportPath);
        }

        /// <summary>
        /// 找到所有UIAtlas
        /// </summary>
        private static void FindAllUIAtlas()
        {
            string[] guids1 = AssetDatabase.FindAssets("t:prefab", new List<string>().ToArray());
            foreach (string guid1 in guids1)
            {
                string targetPath = AssetDatabase.GUIDToAssetPath(guid1);
                GameObject t = (GameObject) AssetDatabase.LoadAssetAtPath(targetPath, typeof(GameObject));
                UIAtlas atlas = t.GetComponent<UIAtlas>();
                if (atlas != null)
                {
                    m_Configuration.rules.Add(new ResourceRule() {atlas = atlas, atlasName = t.name, texture = (Texture2D) atlas.texture});
                }
            }

            // return contents;
        }

        private static void Load()
        {
            if (m_Configuration == null)
            {
                m_Configuration = ScriptableObject.CreateInstance<ResourceRuleEditorData>();
            }
        }

        /// <summary>
        /// 初始化列表
        /// </summary>
        private void InitRuleListDrawer()
        {
            m_RuleList = new ReorderableList(m_Configuration.rules, typeof(ResourceRule));
            m_RuleList.drawElementCallback = OnListElementGUI;
            m_RuleList.drawHeaderCallback = OnListHeaderGUI;
            m_RuleList.draggable = true;
            m_RuleList.elementHeight = 22;
            m_RuleList.onAddCallback = (list) => Add();
        }

        /// <summary>
        /// 添加按钮回调
        /// </summary>
        private void Add()
        {
            var rule = new ResourceRule();
            m_Configuration.rules.Add(rule);
        }

        /// <summary>
        /// 绘制列表项，显示列表每行内容
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isactive"></param>
        /// <param name="isfocused"></param>
        private void OnListElementGUI(Rect rect, int index, bool isactive, bool isfocused)
        {
            const float GAP = 5;

            ResourceRule rule = m_Configuration.rules[index];
            rect.y++;

            Rect r = rect;
            r.width = 160;
            r.height = 18;
            rule.atlasName = EditorGUI.TextField(r, rule.atlasName);

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 160;
            rule.atlas = (UIAtlas) EditorGUI.ObjectField(r, rule.atlas, typeof(UIAtlas));

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 160;
            rule.texture = (Texture2D) EditorGUI.ObjectField(r, rule.texture, typeof(Texture2D));
        }

        /// <summary>
        /// 绘制列表的表头，显示每列具体意义
        /// </summary>
        /// <param name="rect"></param>
        private void OnListHeaderGUI(Rect rect)
        {
            const float GAP = 5;
            GUI.enabled = false;

            Rect r = new Rect(0, 0, rect.width, rect.height);
            r.width = 20;
            r.height = 18;
            EditorGUI.TextField(r, "");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 160;
            EditorGUI.TextField(r, "AtlasName");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 160;
            EditorGUI.TextField(r, "UIAtlas");

            r.xMin = r.xMax + GAP;
            r.xMax = r.xMax + 160;
            EditorGUI.TextField(r, "Texture");
            GUI.enabled = true;
        }
    }
}