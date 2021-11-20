using System;
using System.IO;
using UnityEngine;

namespace Tools
{
    public class Utility
    {
        /// <summary>
        /// 拷贝一份Texture2D并将属性改成可读，格式改成RGBA32
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static Texture2D DuplicateTexture(Texture2D source)
        {
            RenderTexture renderTexture = RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.Default,
                RenderTextureReadWrite.Linear);
            // 通过shader的方式将Texture数据拷贝到renderTexture
            Graphics.Blit(source, renderTexture);
            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTexture;
            Texture2D readableTexture = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readableTexture.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            readableTexture.Apply();
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTexture);
            return readableTexture;
        }
        
        /// <summary>
        /// 保存Texture2D为png图片
        /// </summary>
        /// <param name="filePath">保存文件的路径</param>
        /// <param name="texture">图片</param>
        public static void SavePNG(string filePath, Texture2D texture)
        {
            try
            {
                byte[] pngData = texture.EncodeToPNG();
                File.WriteAllBytes(filePath, pngData);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.StackTrace);
            }
        }
    }
}