using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Henley.Battle;
using Henley.UI;
using modxhunter.modules.utils;
using UnityEngine;

namespace modxhunter.modules.TextureLoader;

public class TextureManager
{
    public static readonly TextureManager Instance = new();

    private TextureManager()
    {
    }

    Dictionary<string, CharacterTextures> _characterTextures = new();
    bool _loaded;
    private bool _enabled = Plugin.TextureLoadingEnabled.Value;
    private GameObject _gameObject { get; set; }
    private TextureManagerBehaviour? _behaviour { get; set; }
    private Dictionary<string, string> _previousCharactersLoaded = new();

    public void Init()
    {
        if (Instance._behaviour == null)
        {
            Instance._gameObject = new GameObject() { name = "mxh-overlay-menu-behaviour" };
            Instance._gameObject.hideFlags = HideFlags.HideAndDontSave;
            Object.DontDestroyOnLoad(Instance._gameObject);
            Instance._behaviour = _gameObject.AddComponent<TextureManagerBehaviour>();
            Instance.PreloadTexturesFromDisk();
        }
    }

    public void SetEnabled(bool value)
    {
        Instance._enabled = value;
        Plugin.TextureLoadingEnabled.Value = value;
    }

    public bool IsEnabled => Instance._enabled;

    public void ReloadTexturesFromDisk()
    {
        Instance._loaded = false;
        Instance._characterTextures.Clear();
        Instance.PreloadTexturesFromDisk();
        Instance.OverrideCharacterTextures();
    }

    public void PreloadTexturesFromDisk()
    {
        if (Instance._loaded) return;
        var path = Path.Join(Paths.PluginPath, "textures");
        if (!Directory.Exists(path))
        {
            Plugin.Log.LogError($"Directory {path} does not exist, creating...");
            Directory.CreateDirectory(path);
            return;
        }

        var characterDirs = Directory.GetDirectories(path);

        foreach (var characterDir in characterDirs)
        {
            var characterName = Path.GetFileName(characterDir);
            var costumeDirs = Directory.GetDirectories(characterDir);
            Plugin.Log.LogInfo($"Preloading: {characterName}");
            foreach (var costumeDir in costumeDirs)
            {
                Plugin.Log.LogInfo($"Preloading: costumeDir: {costumeDir}");
                var costumeId = Path.GetFileName(costumeDir);
                var colourDirs = Directory.GetDirectories(costumeDir);
                if (colourDirs.Length == 0) continue;
                if (!Instance._characterTextures.ContainsKey(characterName))
                {
                    Plugin.Log.LogInfo($"Key not found: {characterName}, adding new key: {characterName}");
                    var characterTextures = new CharacterTextures();
                    Instance._characterTextures.Add(characterName, characterTextures);
                }

                Instance._characterTextures?[characterName]?.Costumes.Add(costumeId, new Costume());

                foreach (var colourDir in colourDirs)
                {
                    var colorId = Path.GetFileName(colourDir);
                    Instance._characterTextures?[characterName]?.Costumes[costumeId]?.Colours?.Add(colorId, new());
                    var textureFiles = Directory.GetFiles(colourDir, "*.png");
                    foreach (var textureFile in textureFiles)
                    {
                        var texture = TextureFileUtils.LoadTexture(textureFile);
                        var textureName = Path.GetFileNameWithoutExtension(textureFile);
                        Instance.LoadTexture(characterName, costumeId, colorId, textureName, texture);
                    }
                }
            }
        }
        Instance._loaded = true;
    }

    private void LoadTexture(string characterId, string costumeId, string colourId, string textureName,
        byte[] textureFile)
    {
        Plugin.Log.LogInfo($"Loading texture: {characterId} {costumeId} {colourId} {textureName}");
        if (Instance._characterTextures.TryGetValue(characterId, out var characterTextures))
        {
            if (characterTextures.Costumes.TryGetValue(costumeId, out var costume))
            {
                if (costume.Colours.TryGetValue(colourId, out var colour))
                {
                    Plugin.Log.LogInfo($"Adding texture: {textureName} for {colourId}");
                    colour.Textures.Add(textureName, textureFile);
                }
            }
        }
    }

    public void OverrideCharacterTextures()
    {
        if (Instance._behaviour == null) return;
        Instance._behaviour.RunAsync();
    }

    private Dictionary<string, byte[]>? GetCharacterTextures(string characterName, string costumeId, string colourId)
    {
        Dictionary<string, byte[]>? result = null;
        if (!Instance._characterTextures.TryGetValue(characterName, out var character)) return result;
        if (!character.Costumes.TryGetValue(costumeId, out var costume)) return result;
        if (!costume.Colours.TryGetValue(colourId, out var colour)) return result;
        return colour.Textures;
    }

    public static string GenerateCharacterPrefix(string characterName, string costumeId, string colourId) => $"T_chr_{characterName}_{costumeId}_{colourId}_";
    public static void ExportTextures()
    {
        var characterMap = new Dictionary<string, CharacterTextureFileProps>();
        foreach (var pcb in BattleSystemAssembly.Instance._charBuilder)
        {
            var ctfp = new CharacterTextureFileProps()
            {
                Prefix = $"T_chr_{pcb._characterId}_",
                AltBodyPrefix = $"T_chr_{pcb._characterId.Replace("p", "f")}_",
                ColorPrefix = GenerateCharacterPrefix(pcb._characterId, pcb._eftCostumeId, pcb._eftColorId),
                CharacterName = CharaSelectManager.Instance.GetCharacterName(pcb._characterId),
                CharacterId = pcb._characterId,
                CostumeId = pcb._eftCostumeId,
                ColorId = pcb._eftColorId,
            };
            characterMap.Add(ctfp.ColorPrefix, ctfp);
        }

        var gameTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
        foreach (var gameTexture in gameTextures)
        {
            KeyValuePair<string, CharacterTextureFileProps> match = default;
            match = characterMap.FirstOrDefault(x => gameTexture.name.StartsWith(x.Value.Prefix));
            if (string.IsNullOrEmpty(match.Key))
            {
                match = characterMap.FirstOrDefault(x => gameTexture.name.StartsWith(x.Value.AltBodyPrefix));
            }

            if (string.IsNullOrEmpty(match.Key)) continue;
            var character = match.Value;
            var dirPath = Path.Join(Paths.PluginPath, "exports", character.CharacterName, character.CostumeId,
                character.ColorId);
            string filePath = Path.Join(dirPath, gameTexture.name + ".png");
            TextureFileUtils.ExportTexture(gameTexture, filePath);
        }
    }

    public Dictionary<string, byte[]> GetActiveCharacterTextures()
    {
        var characterTextures = new Dictionary<string, byte[]>();
        var characters = new List<CharacterCostumeColor>();
        foreach (var character in BattleSystemAssembly.Instance._charBuilder)
        {
            var ccc = new CharacterCostumeColor()
            {
                CharacterName = CharaSelectManager.Instance.GetCharacterName(character._characterId),
                CostumeID = character._eftCostumeId,
                ColorId = character._eftColorId,
            };
            characters.Add(ccc);
        }
        foreach (var character in characters)
        {
            var textures = Instance
                .GetCharacterTextures(character.CharacterName, character.CostumeID, character.ColorId)?
                .ToList();
            if (textures == null) continue;
            foreach (var texture in textures)
            {
                characterTextures.Add(texture.Key, texture.Value);
            }
        }
        return characterTextures;
    }

}

public class CharacterTextureFileProps
{
    public string Prefix { get; set; }
    public string AltBodyPrefix  {get; set;}
    public string ColorPrefix  {get; set;}
    public string CharacterName  {get; set;}
    public string CharacterId  {get; set;}
    public string CostumeId  {get; set;}
    public string ColorId  {get; set;}
}

public class CharacterCostumeColor
{
    public string CharacterName;
    public string CostumeID;
    public string ColorId;
}

class CharacterTextures
{
    public Dictionary<string, Costume> Costumes = new();
}

class Costume
{
    public Dictionary<string, CostumeColour> Colours = new();
}

class CostumeColour
{
    public Dictionary<string, byte[]> Textures = new();
}

[HarmonyPatch(typeof(BattleSystemAssembly))]
[HarmonyPatch(nameof(BattleSystemAssembly.SetupBattleSystem))]
public static class CharaSelectBlueprintGetMemberId
{
    public static void Postfix()
    {
        if (!TextureManager.Instance.IsEnabled) return;
        TextureManager.Instance.OverrideCharacterTextures();
    }
}
