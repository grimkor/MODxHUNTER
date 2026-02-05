using System.IO;
using UnityEngine;

namespace modxhunter.modules.utils;

public static class TextureFileUtils
{
    public static void ExportTexture(Texture texture, string exportPath)
    {
        var tempRT = RenderTexture.GetTemporary(
            texture.width,
            texture.height,
            0,
            RenderTextureFormat.Default,
            RenderTextureReadWrite.Linear
        );

        Graphics.Blit(texture, tempRT);

        var texture2D = new Texture2D(texture.width, texture.height, TextureFormat.RGBA32, false);

        var previous = RenderTexture.active;
        RenderTexture.active = tempRT;

        texture2D.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        texture2D.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(tempRT);

        byte[] bytes = ImageConversion.EncodeToPNG(texture2D);
        var directory = Path.GetDirectoryName(exportPath);
        if (directory == null)
        {
            Plugin.Log.LogError($"Failed to get directory path from export path: {exportPath}");
            return;
        }
        Directory.CreateDirectory(directory);
        File.WriteAllBytes(exportPath, bytes);

        Object.Destroy(texture2D);
        Plugin.Log.LogInfo($"Saved texture to: {exportPath}");
    }

    public static byte[] LoadTexture(string textureFile)
    {
        var bytes = File.ReadAllBytes(textureFile);
        return bytes;
    }
    public static Texture2D LoadTexture2D(string textureFile)
    {
        var bytes = File.ReadAllBytes(textureFile);
        var texture = new Texture2D(2, 2);
        ImageConversion.LoadImage(texture, bytes);
        return texture;
    }
}
