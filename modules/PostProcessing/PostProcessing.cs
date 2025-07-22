using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Henley.ECP;
using Henley.ECP.Volume;
using Henley.Scene;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace modxhunter.modules.PostProcessing;

public class PostProcessing
{
    public static PostProcessing Instance = new();
    private PostProcessingBehaviour? _behaviour;
    private ECPDiffusionFilter? _diffusion;
    private UniversalAdditionalCameraData? _cameraData;
    private ECPBloom? _bloom;
    public float DiffusionValue { get; private set; } = Plugin.DiffusionValue.Value;
    public bool DiffusionEnabled { get; private set; } = Plugin.DiffusionEnable.Value;
    public AntialiasingType Antialiasing { get; private set; } =
        Plugin.SubpixelAntialiasing.Value ? AntialiasingType.Subpixel : AntialiasingType.Fast;
    public bool Bloom { get; private set; } = Plugin.BloomEnable.Value;

    public void Init()
    {
        var go = new GameObject() {name = "PostProcessVolume"};
        go.hideFlags = HideFlags.HideAndDontSave;
        Object.DontDestroyOnLoad(go);
        Instance._behaviour = go.AddComponent<PostProcessingBehaviour>();
        Instance._behaviour.enabled = false;
    }

    private void Apply()
    {
        if (Instance._diffusion != null)
        {
            Instance.EnableDiffusion(Instance.DiffusionEnabled);
            Instance.SetDiffusionValue(Instance.DiffusionValue);
        }

        if (Instance._cameraData != null)
        {
            Instance.SetAntiAliasing(Instance.Antialiasing);
        }

        if (Instance._bloom != null)
        {
            Instance.SetBloom(Instance.Bloom);
        }
    }

    public void EnableDiffusion(bool value)
    {
        Instance.DiffusionEnabled = value;
        Plugin.DiffusionEnable.Value = value;
        if (Instance._diffusion == null) return;
        Instance._diffusion.active = value;
    }

    public void SetDiffusionValue(float value)
    {
        Instance.DiffusionValue = value;
        Plugin.DiffusionValue.Value = value;
        if (Instance._diffusion == null) return;
        Instance._diffusion._blendAlpha.value = value;
    }

    public void SetAntiAliasing(AntialiasingType type)
    {
        Instance.Antialiasing = type;
        Plugin.SubpixelAntialiasing.Value = type == AntialiasingType.Subpixel;
        if (Instance._cameraData == null) return;
        if (type == AntialiasingType.Fast)
        {
            Instance._cameraData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
        }
        else
        {
            Instance._cameraData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
        }
    }

    public void SetBloom(bool value)
    {
        Instance.Bloom = value;
        Plugin.BloomEnable.Value = value;
        if (Instance._bloom == null) return;
        Instance._bloom.active = value;
    }

    [HarmonyPatch(typeof(SceneStartupManager), nameof(SceneStartupManager.ChangeSceneOnLoad))]
    public static class SceneStartupManagerChangeSceneOnLoad
    {
        private static void Prefix(string sceneId, bool unload)
        {
            if (sceneId == Scene.Battle2D)
            {
                Instance._behaviour?.StartSearch();
            }
            else
            {
                Instance._behaviour?.StopSearch();
            }
        }
    }

    public class PostProcessingBehaviour : MonoBehaviour
    {
        private int _debounce = 2;
        private float _lastUpdate = 0;
        private List<bool> _foundChecklists = new();

        void OnEnable()
        {
            _foundChecklists.Clear();
            for (int i = 0; i < Enum.GetValues(typeof(SearchItem)).Length; i++)
            {
                _foundChecklists.Add(false);
            }
            StartSearch();
        }


        private void Update()
        {
            if (Time.time - _lastUpdate < _debounce) return;
            _lastUpdate = Time.time;
            Plugin.Log.LogInfo("Searching...");
            var parent = GameObject.Find("BattleBackground");
            if (parent == null) return;
            var go = parent?.transform?.FindChild("PostProcessVolume");
            if (!IsFound(SearchItem.Diffusion) && go != null)
            {
                Plugin.Log.LogInfo("Found PostProcessVolume");
                var volume = go.GetComponent<Volume>();
                if (volume != null)
                {
                    Plugin.Log.LogInfo("Found Volume");
                    volume.profile.components.ForEach(new Action<VolumeComponent>(x =>
                    {
                        if (x.name.Contains("ECPDiffusionFilter"))
                        {
                            Instance._diffusion = x.Cast<ECPDiffusionFilter>();
                            UpdateFound(SearchItem.Diffusion);
                            Instance.Apply();
                        }

                        if (x.name.Contains("ECPBloom"))
                        {
                            Instance._bloom = x.Cast<ECPBloom>();
                            UpdateFound(SearchItem.Bloom);
                            Instance.Apply();
                        }
                    }));
                }
            }
            Plugin.Log.LogInfo("Searching for camera...");
            var cameraParent = parent?.transform?.FindChild("CameraSystem");
            if (!IsFound(SearchItem.Camera) && cameraParent != null)
            {
                Plugin.Log.LogInfo("Found CameraSystem");
                var cameraData = cameraParent.GetComponent<UniversalAdditionalCameraData>();
                if (cameraData)
                {
                    Plugin.Log.LogInfo("Found CameraData");
                    Instance._cameraData = cameraData;
                    UpdateFound(SearchItem.Camera);
                    Instance.Apply();
                }
            }
        }

        private void UpdateFound(SearchItem item)
        {
            switch (item)
            {
                case SearchItem.Diffusion:
                    _foundChecklists[(int)item] = true;
                    break;
                case SearchItem.Camera:
                    _foundChecklists[(int)item] = true;
                    break;
                case SearchItem.Bloom:
                    _foundChecklists[(int)item] = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(item), item, null);
            }
            if (_foundChecklists.All(x => x))
            {
                StopSearch();
            }
        }

        private bool IsFound(SearchItem item)
        {
            return _foundChecklists[(int)item];
        }

        public void StartSearch()
        {
            Plugin.Log.LogInfo("StartSearch");
            enabled = true;
        }
        public void StopSearch()
        {
            Plugin.Log.LogInfo("StopSearch");
            enabled = false;
        }

        private enum SearchItem
        {
            Diffusion,
            Camera,
            Bloom,
        }
    }
}

public enum AntialiasingType
{
    Fast,
    Subpixel,
}
