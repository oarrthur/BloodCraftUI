using System;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using BloodCraftUI.Config;
using BloodCraftUI.Patches;
using BloodCraftUI.Services;
using BloodCraftUI.UI;
using BloodCraftUI.UI.CustomLib.Util;
using BloodCraftUI.UI.ModernLib;
using BloodCraftUI.Utils;
using HarmonyLib;
using Unity.Entities;
using UnityEngine;

namespace BloodCraftUI
{
    [BepInProcess("VRising.exe")]
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BasePlugin
    {
        public static Plugin Instance { get; private set; }
        public static ManualLogSource LogInstance => Instance.Log;
        public static Settings Settings { get; private set; }
        private static World _client;
        public static EntityManager EntityManager => _client.EntityManager;
        //public static ServerGameManager ServerGameManager => _client.GetExistingSystemManaged<ServerScriptMapper>()._ServerGameManager;
        public static bool IsInitialized { get; private set; }
        public static bool IsGameDataInitialized { get; set; }
        public static BCUIManager UIManager { get; set; }
        public static CoreUpdateBehavior CoreUpdateBehavior { get; set; }
        public static bool IsClient { get; private set; }
        public static Entity LocalCharacter { get; set; }

        public static bool IsClientNull() => _client == null;

        public const bool IS_TESTING = false;


        public static void Reset()
        {
            _client = null;
            IsInitialized = false;
            IsGameDataInitialized = false;
        }

        private static Harmony _harmonyBootPatch;
        private static Harmony _harmonyChatPatch;
        private static Harmony _harmonyInitPatch;
        private static Harmony _harmonyCanvasPatch;
        private static Harmony _harmonyMenuPatch;
        internal static Harmony HarmonyVersionStringPatch;
        private static FrameTimer _uiInitializedTimer;

        public override void Load()
        {
            IsClient = Application.productName != "VRisingServer";
            LogUtils.Init(Log);
            Instance = this;


            if (!IsClient)
            {
                LogUtils.LogInfo($"{PluginInfo.PLUGIN_NAME}[{PluginInfo.PLUGIN_VERSION}] is a client mod! ({Application.productName})");
                return;
            }

            Settings = new Settings().InitConfig();
            Theme.Opacity = Settings.UITransparency;

            UIManager = new BCUIManager();
            CoreUpdateBehavior = new CoreUpdateBehavior();
            CoreUpdateBehavior.Setup();
            //todo CoreUpdateBehavior.ExecuteOnUpdate = MessageService.ProcessAllMessages;

            IsInitialized = true;

            _harmonyBootPatch = Harmony.CreateAndPatchAll(typeof(GameManagerPatch));
            _harmonyMenuPatch = Harmony.CreateAndPatchAll(typeof(EscapeMenuPatch));
            _harmonyCanvasPatch = Harmony.CreateAndPatchAll(typeof(UICanvasSystemPatch));
            HarmonyVersionStringPatch = Harmony.CreateAndPatchAll(typeof(VersionStringPatch));
            _harmonyChatPatch = Harmony.CreateAndPatchAll(typeof(ClientChatPatch));
            _harmonyInitPatch = Harmony.CreateAndPatchAll(typeof(InitializationPatch));
            //_eclipsePatch = Harmony.CreateAndPatchAll(typeof(EclipseClientChatSystemPatch));

            Log.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} version {PluginInfo.PLUGIN_VERSION} is loaded!");

            if(IS_TESTING)
                AddTestUI();
        }


        public override bool Unload()
        {
            _harmonyBootPatch.UnpatchSelf();
            _harmonyCanvasPatch.UnpatchSelf();
            _harmonyMenuPatch.UnpatchSelf();
            HarmonyVersionStringPatch.UnpatchSelf();
            _harmonyChatPatch.UnpatchSelf();
            _harmonyInitPatch.UnpatchSelf();
            //_eclipsePatch.UnpatchSelf();
            return true;
        }

        private void AddTestUI()
        {
            UIManager.SetupAndShowUI();
        }

        //run on game start
        public static void GameDataOnInitialize(World world)
        {
            if (!IsGameDataInitialized && IsClient)
            {
                _client = world;
                IsGameDataInitialized = true;
                // We only want to run this once, so unpatch the hook that initiates this callback.
                _harmonyBootPatch.UnpatchSelf();
                _uiInitializedTimer = new FrameTimer();

                _uiInitializedTimer.Initialise(() =>
                    {
                        _uiInitializedTimer.Stop();
                    },
                    TimeSpan.FromSeconds(5),
                    true).Start();
            }
        }

        public static void UIOnInitialize()
        {
            UIManager.SetupAndShowUI();
            if(Settings.AutoEnableFamiliarEquipment)
                MessageService.StartAutoEnableFamiliarEquipmentSequence();
            BloodCraftStateService.Initialize();
            LogUtils.LogInfo($"UI Manager initialized");
        }
    }

    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "panthernet.BloodCraftUI";
        public const string PLUGIN_NAME = "BloodCraftUI";
        public const string PLUGIN_VERSION = "1.1.0";
    }
}
