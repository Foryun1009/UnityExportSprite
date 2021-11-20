using UnityEngine;
using UnityEditor;

/// <summary>
/// 导出精灵工具
/// </summary>
public class ExportSpriteEditor
{
    [MenuItem("Tools/导出精灵")]
    static void ExportSprite()
    {
        string resourcesPath = "Assets/Resources/";
        foreach (Object obj in Selection.objects)
        {
            string selectionPath = AssetDatabase.GetAssetPath(obj);
            if (selectionPath.StartsWith(resourcesPath))
            {
                string selectionExt = System.IO.Path.GetExtension(selectionPath);
                if (selectionExt.Length == 0)
                {
                    Debug.LogError($"资源{selectionPath}的扩展名不对，请选择图片资源");
                    continue;
                }
                // 如果selectionPath = "Assets/Resources/UI/Common.png"
                // 那么loadPath = "UI/Common"
                string loadPath = selectionPath.Remove(selectionPath.Length - selectionExt.Length);
                loadPath = loadPath.Substring(resourcesPath.Length);
                // 加载此文件下的所有资源
                Sprite[] sprites = Resources.LoadAll<Sprite>(loadPath);
                if (sprites.Length > 0)
                {
                    // 创建导出目录
                    string exportPath = Application.dataPath + "/ExportSprite/" + loadPath;
                    System.IO.Directory.CreateDirectory(exportPath);

                    foreach (Sprite sprite in sprites)
                    {
                        Texture2D tex = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height,
                            sprite.texture.format, false);
                        tex.SetPixels(sprite.texture.GetPixels((int) sprite.rect.xMin, (int) sprite.rect.yMin,
                            (int) sprite.rect.width, (int) sprite.rect.height));
                        tex.Apply();

                        // 将图片数据写入文件
                        System.IO.File.WriteAllBytes(exportPath + "/" + sprite.name + ".png", tex.EncodeToPNG());
                    }
                    Debug.Log("导出精灵到" + exportPath);
                }
                Debug.Log("导出精灵完成");
                // 刷新资源
                AssetDatabase.Refresh();
            }
            else
            {
                Debug.LogError($"请将资源放在{resourcesPath}目录下");
            }
        }
    }
}