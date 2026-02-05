using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BepInEx.Unity.IL2CPP.Utils;
using UnityEngine;
using IEnumerator = System.Collections.IEnumerator;

namespace modxhunter.modules.TextureLoader;

public class TextureManagerBehaviour : MonoBehaviour
{

    private List<Texture2D> GetAllTexture2D()
    {
        var textures = Resources.FindObjectsOfTypeAll<Texture2D>().ToList();
        return textures;
    }

    private void ReplaceTexture(Texture2D original, byte[] replacement)
    {
        ImageConversion.LoadImage(original, replacement);
    }


    private IEnumerator RunCoroutine()
    {
        var watch = new Stopwatch();
        watch.Start();
        var textures = GetAllTexture2D();
        var replacements = TextureManager.Instance.GetActiveCharacterTextures();

        var totalReplacements = replacements.Count;
        var i = 0;
        foreach (var texture in textures)
        {
            if (totalReplacements <= 0) break;
            i++;
            replacements.TryGetValue(texture.name, out var match);
            if (match == null) continue;
            ReplaceTexture(texture, match);
            totalReplacements--;
            yield return new WaitForEndOfFrame();
        }
        watch.Stop();
        Plugin.Log.LogInfo($"Finished {i} iterations over {textures.Count} textures for {replacements.Count} replacements in {watch.ElapsedMilliseconds}ms");
        yield return new WaitForEndOfFrame();
    }
    public void RunAsync()
    {
        MonoBehaviourExtensions.StartCoroutine(this, RunCoroutine());
    }
}
