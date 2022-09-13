using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace DSPBetterMove
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class DSPBetterMove : BaseUnityPlugin
    {
        private const string PluginGuid = "redhot.plugin.DSPBetterMove";
        private const string PluginName = "DSPBetterMove";
        private const string PluginVersion = "0.0.3";

        Harmony _harmony;
        private static Vector3 _dragBeginMousePosition;
        private static RaycastHit _hitInfo;
        private static bool _hit;

        // Awake is called once when both the game and the plug-in are loaded
        void Awake()
        {
            _harmony = new Harmony(PluginGuid);

            _harmony.PatchAll(typeof(DSPBetterMove));

            Debug.Log("DSPBetterMove started!");
        }

        // Every GameTick, check if player is using the move button, todo: It would be simpled to find whatever disables movement and alter that instead of altering GameTick
        [HarmonyPostfix, HarmonyPatch(typeof(PlayerController), "GameTick")]
        private static void PlayerControllerGameTick_Postfix(long time)
        {
            if (!UIRoot.instance.uiGame.globemap.active) return; // Only modify behaviour when the globe map is open

            if (VFInput._rtsMove.onDown)
            {
                _dragBeginMousePosition = Input.mousePosition;
                if (Camera.main != null)
                    _hit = Physics.Raycast(Camera.main.ScreenPointToRay(_dragBeginMousePosition), out _hitInfo, 800f,
                        8720,
                        QueryTriggerInteraction.Collide);
            }

            // Check if the player moved the mouse a significant distance indicating they want to drag the camera, not move the character.
            else if (VFInput._rtsMove.onUp &&
                     ((_dragBeginMousePosition - Input.mousePosition).sqrMagnitude < 800.0))
            {
                if (!_hit) return;
                GameMain.data.mainPlayer.Order(OrderNode.MoveTo(_hitInfo.point), VFInput._multiOrdering);
                RTSTargetGizmo.Create(_hitInfo.point);
                _hit = false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIVersionText), "Refresh")]
        public static void RefreshPrefix(UIVersionText __instance)
        {
            if ((Object)__instance.textComp != (Object)null)
                __instance.textComp.text =
                    !GameMain.isRunning || GameMain.instance.isMenuDemo ? string.Empty : string.Empty;
        }

        void OnDestroy()
        {
            Debug.Log("Destroying BGM ;)");
            AssetBundle.UnloadAllAssetBundles(true);
            _harmony.UnpatchSelf();
        }
    }
}