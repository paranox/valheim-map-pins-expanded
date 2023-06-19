using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;

namespace Paranox.MapPinsExpanded
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency(Jotunn.Main.ModGuid)]
    //[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    internal class MapPinsExpanded : BaseUnityPlugin
    {
        public const string PluginGUID = "com.paranox.map-pins-expanded";
        public const string PluginName = "MapPinsExpanded";
        public const string PluginVersion = "0.0.1";
        
        // Use this class to add your own localization to the game
        // https://valheim-modding.github.io/Jotunn/tutorials/localization.html
        public static CustomLocalization Localization = LocalizationManager.Instance.GetLocalization();

        private readonly Harmony harmony = new Harmony("com.paranox.map-pins-expanded");

        private void Awake()
        {
            harmony.PatchAll();

            // Jotunn comes with its own Logger class to provide a consistent Log style for all mods using it
            Jotunn.Logger.LogInfo(FormatLogMessage("initializing..."));

            // To learn more about Jotunn's features, go to
            // https://valheim-modding.github.io/Jotunn/tutorials/overview.html

            Jotunn.Logger.LogInfo("MapPinsExpanded: registering data load event!");
            MinimapManager.OnVanillaMapDataLoaded += OnMapDataLoaded;
        }

        private void OnDestroy()
        {
            Jotunn.Logger.LogInfo(FormatLogMessage("unregistering data load event!"));
            MinimapManager.OnVanillaMapDataLoaded -= OnMapDataLoaded;
        }

        private void OnMapDataLoaded()
        {
            Jotunn.Logger.LogInfo(FormatLogMessage("map data has been loaded!"));
            Jotunn.Logger.LogInfo(FormatLogMessage("pin data count: " + Minimap.instance.m_pins.Count));

            //Minimap.PinData pinData;
            PinDataExpanded pinExpanded;
            Renderer iconRenderer;

            for (int i = 0; i < Minimap.instance.m_pins.Count; i++)
            {
                //pinData = Minimap.instance.m_pins[i];
                pinExpanded = Minimap.instance.m_pins[i] as PinDataExpanded;

                if (pinExpanded != null)
                {
                    Jotunn.Logger.LogInfo(FormatLogMessage(string.Format("Pin[{0:D4}] name:'{1}', type:{2}, owner:'{3}', custom datat: [id:'{4}', color:{5}]",
                        i, pinExpanded.m_name, pinExpanded.m_type, pinExpanded.m_ownerID, pinExpanded.m_id, pinExpanded.m_color)));

                    if (pinExpanded.m_iconElement != null && pinExpanded.m_iconElement.canvasRenderer != null)
                        pinExpanded.m_iconElement.canvasRenderer.SetColor(pinExpanded.m_color);
                    else
                        Jotunn.Logger.LogWarning(FormatLogMessage(string.Format("unable to set multiply color {0}!", pinExpanded.m_color)));
                }
                //else
                //{
                //    Jotunn.Logger.LogInfo(FormatLogMessage(string.Format("Pin[{0:D4}] name:'{1}', type:{2}, owner:'{3}'",
                //        i, pinData.m_name, pinData.m_type, pinData.m_ownerID)));
                //}
            }

            
        }

        private static void MakePinsVanillaAgain(List<Minimap.PinData> pins)
        {
            Minimap.PinData pinData;
            PinDataExpanded pinExpanded;

            for (int i = 0; i < pins.Count; i++)
            {
                pinData = pins[i];
                pinExpanded = pinData as PinDataExpanded;

                if (pinExpanded != null && pinExpanded.m_id >= 0)
                {
                    pinData.m_name = pinExpanded.GetSaveString();

                    Jotunn.Logger.LogInfo(FormatLogMessage(string.Format("Pin[{0:D4}] name:'{1}', type:{2}, owner:'{3}', custom ID:'{4}'",
                        i, pinExpanded.m_name, pinExpanded.m_type, pinExpanded.m_ownerID, pinExpanded.m_id)));
                }
                else
                {
                    Jotunn.Logger.LogInfo(FormatLogMessage(string.Format("Pin[{0:D4}] name:'{1}', type:{2}, owner:'{3}'",
                        i, pinData.m_name, pinData.m_type, pinData.m_ownerID)));
                }
            }
        }

        public static string FormatLogMessage(string message)
        {
            return "MapPinsExpanded: " + message;
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.GetMapData))]
        class GetMapData_Patch
        {
            static void Prefix(ref List<Minimap.PinData> ___m_pins)
            {
                if (___m_pins != null && ___m_pins.Count > 0)
                {
                    Jotunn.Logger.LogInfo(FormatLogMessage(string.Format("map data is about to be serialized with {0} pins worth of data!", ___m_pins.Count)));
                    MakePinsVanillaAgain(___m_pins);
                }
                else
                    Jotunn.Logger.LogInfo(FormatLogMessage("map data is about to be serialized but pin data was unavailable!"));
            }
        }

        [HarmonyPatch(typeof(Minimap), nameof(Minimap.AddPin))]
        class AddPin_Patch
        {
            static bool Prefix(Minimap __instance, ref Minimap.PinData __result, Vector3 pos, ref Minimap.PinType type, ref string name, bool save, bool isChecked, long ownerID = 0L)
            {
                Jotunn.Logger.LogInfo(FormatLogMessage(string.Format("map pin is about to be added: {0}", name)));
                if ((int)type >= __instance.m_visibleIconTypes.Length || type < Minimap.PinType.Icon0)
                {
                    ZLog.LogWarning($"Trying to add invalid pin type: {type}");
                    type = Minimap.PinType.Icon3;
                }
                if (name == null)
                {
                    name = "";
                }
                PinDataExpanded data = new PinDataExpanded(name);
                data.m_type = type;
                data.m_pos = pos;
                data.m_icon = __instance.GetSprite(type);
                data.m_save = save;
                data.m_checked = isChecked; 
                data.m_ownerID = ownerID;
                if (!string.IsNullOrEmpty(data.m_name))
                {
                    data.m_NamePinData = new Minimap.PinNameData(data);
                }
                __instance.m_pins.Add(data);
                if ((int)type < __instance.m_visibleIconTypes.Length && !__instance.m_visibleIconTypes[(int)type])
                {
                    __instance.ToggleIconFilter(type);
                }

                __result = data;
                return false;
            }
        }
    }
}