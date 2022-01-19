using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MelonLoader;
using UIExpansionKit.API;
using UnityEngine;
using UnityEngine.UI;
using VRCSDK2;


[assembly: MelonInfo(typeof(LocalCamera.Main), "LocalCameraMod", "1.0.9", "Nirvash")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace LocalCamera
{
    public class Main : MelonMod
    {
        public static object activeCoroutine;

        public GameObject _localCam = null;
        public GameObject _stick = null;
        public Boolean CameraRes4k = false;
        public Boolean parentToTracking = false;
        public Boolean _followSelfie = false;
        public Boolean _pickupable = true;
        public Boolean hideCam = false;
        public static Dictionary<string, Transform> buttonList = new Dictionary<string, Transform>();

        public static MelonPreferences_Entry<bool> parentTracking;

        public override void OnApplicationStart()
        {
            MelonPreferences.CreateCategory("LocalCamera", "LocalCamera");
            MelonPreferences.CreateEntry("LocalCamera", "CameraRes4k", false, "4k Camera Texture Res (1080p default)");
            parentTracking = MelonPreferences.CreateEntry("LocalCamera", "ParentToTracking", false, "Parent Camera to Tracking Space");
            //MelonPreferences.CreateEntry("LocalCamera", "DelayButton", false, "Put button last on the list (Req restart)");

            //if (MelonPreferences.GetEntryValue<bool>("LocalCamera", "DelayButton")) MelonCoroutines.Start(SetupUI()); //Dirty hack broke with ML 0.4.0
            //else ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Local Camera", () => ToggleCamMenu());
            ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Local Camera", () => ToggleCamMenu());
        }

        //private IEnumerator SetupUI()//I want the button to be last on the list
        //{
        //    while (QuickMenu.prop_QuickMenu_0 == null) yield return null;
        //    yield return new WaitForSeconds(10f);
        //    MelonLogger.Msg($"Adding QM button late and reloading the menu");
        //    ExpansionKitApi.GetExpandedMenu(ExpandedMenu.QuickMenu).AddSimpleButton("Local Camera", () => ToggleCamMenu());
        //    MelonPreferences.Save();
        //}

        public void ToggleCamMenu()
        {
            var localcamMenu = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescriptionCustom.QuickMenu);
            buttonList.Clear();
            localcamMenu.AddSimpleButton("Local Camera\n-----\nClose menu", () => localcamMenu.Hide());
            localcamMenu.AddSimpleButton($"Camera is\n{(_localCam != null ? "Enabled" : "Disabled") }", (() => ToggleCamera()), (button) => buttonList["toggleBut"] = button.transform);
            localcamMenu.AddToggleButton($"Hide Camera", ((action) => HideMesh()), () => hideCam);

            localcamMenu.AddSimpleButton("Larger", (() => CamScale(1)));
            localcamMenu.AddSimpleButton("Smaller", (() => CamScale(-1)));
            localcamMenu.AddSimpleButton("Reset", (() => CamScale(0)));

            localcamMenu.AddSimpleButton("Zoom+", (() => CamFov(-1)));
            localcamMenu.AddSimpleButton("Zoom-", (() => CamFov(1)));
            localcamMenu.AddSimpleButton("Reset", (() => CamFov(0)));

            localcamMenu.AddSimpleButton("Selfie Stick", (() => Stick(true)));
            localcamMenu.AddSimpleButton("Rotate to Head", (() => RotateHead()));
            localcamMenu.AddToggleButton($"Follow Tracking Space", ((action) => TrackingSpace()), () => parentTracking.Value);

            localcamMenu.AddToggleButton($"Pickupable", ((action) => CamPickup()), () => _pickupable);

            localcamMenu.Show();

        }




        public override void OnPreferencesSaved()
        {
            if (_localCam != null && MelonPreferences.GetEntryValue<bool>("LocalCamera", "ParentToTracking") != parentToTracking)
            {
                if (MelonPreferences.GetEntryValue<bool>("LocalCamera", "ParentToTracking")) _localCam.transform.SetParent(GameObject.Find("_Application/TrackingVolume/PlayerObjects").transform, true);
                else _localCam.transform.SetParent(null);
            }

            CameraRes4k = MelonPreferences.GetEntryValue<bool>("LocalCamera", "CameraRes4k");
            parentToTracking = MelonPreferences.GetEntryValue<bool>("LocalCamera", "ParentToTracking");

        }

        private void ToggleCamera()
        {
            if (_localCam != null)
            {
                UnityEngine.Object.Destroy(_localCam);
                _localCam = null;

                Stick(false); //If camera gets destroyed, remove stick

            }
            else
            {
                VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
                Vector3 pos = GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/HeadEffector").transform.position + (player.transform.forward * .25f); // Gets position of Head 
                GameObject localcam = GameObject.CreatePrimitive(PrimitiveType.Quad);
                localcam.transform.position = pos;
                //MelonLogger.Msg($"x:{pos.x}. y{pos.y}, z:{pos.z}");
                localcam.transform.rotation = player.transform.rotation;
                localcam.name = "LocalCam";
                UnityEngine.Object.Destroy(localcam.GetComponent<Collider>());
                localcam.transform.localScale = new Vector3((16f / 80f), (9f / 80f), .05f);

                localcam.GetOrAddComponent<BoxCollider>().size = new Vector3((16f / 80f), (9f / 80f), .05f);// * 2;
                localcam.GetOrAddComponent<BoxCollider>().isTrigger = true;

                localcam.GetOrAddComponent<MeshRenderer>().enabled = false;

                localcam.GetOrAddComponent<VRC_Pickup>().proximity = 3f;
                localcam.GetOrAddComponent<VRC_Pickup>().pickupable = _pickupable;
                localcam.GetOrAddComponent<VRC_Pickup>().allowManipulationWhenEquipped = true;
                localcam.GetOrAddComponent<VRC_Pickup>().orientation = VRC_Pickup.PickupOrientation.Any;
                localcam.GetOrAddComponent<Rigidbody>().useGravity = false;
                localcam.GetOrAddComponent<Rigidbody>().isKinematic = true;


                GameObject Screen = GameObject.CreatePrimitive(PrimitiveType.Quad);
                Screen.name = "Screen";
                Screen.transform.SetParent(localcam.transform, false);
                UnityEngine.Object.Destroy(Screen.GetComponent<Collider>());
                Screen.transform.localScale = new Vector3(-1,1,1); //Mirror screen

                Material mat = new Material(Shader.Find("Unlit/Texture"));
                RenderTexture text = null;
                if (CameraRes4k) text = new RenderTexture(3840, 2160, 24, RenderTextureFormat.ARGB32);
                else text = new RenderTexture(1920, 1080, 24, RenderTextureFormat.ARGB32);
                mat.mainTexture = text;
                Screen.GetOrAddComponent<MeshRenderer>().material = mat;
        

                GameObject photoCam = GameObject.CreatePrimitive(PrimitiveType.Quad);
                photoCam.name = "PhotoCamera";
                photoCam.transform.SetParent(localcam.transform, false);
                photoCam.transform.localScale = new Vector3(.01f, .01f, .05f);
                UnityEngine.Object.Destroy(photoCam.GetComponent<Collider>());

                photoCam.transform.rotation = photoCam.transform.rotation * Quaternion.AngleAxis(180, Vector3.up);  // Rotate to face you
                photoCam.GetOrAddComponent<Camera>().targetTexture = text;
                photoCam.GetOrAddComponent<Camera>().nearClipPlane = .01f;
                photoCam.GetOrAddComponent<Camera>().cullingMask = -529645;//-5153;


                if (parentTracking.Value) localcam.transform.SetParent(GameObject.Find("_Application/TrackingVolume/PlayerObjects").transform, true);

                _localCam = localcam;
            }
            buttonList["toggleBut"].GetComponentInChildren<Text>().text = $"Camera is\n{ (_localCam != null ? "Enabled" : "Disabled")}";
        }
        private void CamScale(int dir)
        {
            if (_localCam is null) return;
            if (dir == 1) _localCam.transform.localScale *= 1.25f;
            else if (dir == -1) _localCam.transform.localScale /= 1.25f;
            else _localCam.transform.localScale = new Vector3((16f / 80f), (9f / 80f), .05f);
        }

        private void CamFov(int dir)
        {
            if (_localCam is null) return;
            GameObject photoCam = _localCam.transform.Find("PhotoCamera").gameObject;
            Camera cam = photoCam.GetComponentInChildren<Camera>();
            if (dir == 1) cam.fieldOfView += 5;
            else if (dir == -1) cam.fieldOfView -= 5;
            else cam.fieldOfView = 60;
        }

        private void CamPickup()
        {
            if (_localCam is null) return;
            var pickup = _localCam.GetComponent<VRC_Pickup>();
            if (pickup.pickupable == true) pickup.pickupable = false;
            else pickup.pickupable = true;
            _pickupable = pickup.pickupable;

           //buttonList["pickupBut"].GetComponentInChildren<Text>().text = $"{(_pickupable ? "Pickupable" : "Not Pickupable")}";
        }

        private void TrackingSpace()
        { //This feels messy
            if (_localCam is null) return;
            parentTracking.Value =  !parentTracking.Value;
            if (parentTracking.Value) _localCam.transform.SetParent(GameObject.Find("_Application/TrackingVolume/PlayerObjects").transform, true);
            else _localCam.transform.SetParent(null);
            parentToTracking = MelonPreferences.GetEntryValue<bool>("LocalCamera", "ParentToTracking");

            //buttonList["trackingBut"].GetComponentInChildren<Text>().text = $"Toggle Tracking Space\n {(parentToTracking ? "-Following-" : "-Not Following-")} ";
        }

        private void RotateHead()
        {
            if (_localCam is null) return;
            if (_stick != null ) Stick(false); //If stick exists, remove it before trying to rotate 
            VRCPlayer player = VRCPlayer.field_Internal_Static_VRCPlayer_0;
            _localCam.transform.LookAt(GameObject.Find(player.gameObject.name + "/AnimationController/HeadAndHandIK/HeadEffector").transform);
            _localCam.transform.rotation = _localCam.transform.rotation * Quaternion.AngleAxis(180, Vector3.up); //Flip so screen is visible 
        }

        private void HideMesh()
        {
            if (_localCam is null) return;
            hideCam = !hideCam;
            _localCam.transform.GetChild(0).GetComponent<MeshRenderer>().enabled = !hideCam;
        }

        private void Stick(Boolean create)
        {
            if (_stick != null)
            {
                _followSelfie = false;
                MelonCoroutines.Stop(activeCoroutine);
                //Stop the follow task and then destory
                UnityEngine.Object.Destroy(_stick);
                _stick = null;
            }
            else if(create == true)//Only enable the stick if you are meaning to
            {
                if (_localCam is null) ToggleCamera();//If stick is enabled without camera, make the camera first
                float stickLen = 5f;
                GameObject stick = GameObject.CreatePrimitive(PrimitiveType.Cube);
                stick.name = "Stick";
                stick.transform.localScale = new Vector3(.025f, .025f, stickLen);
                stick.transform.position = _localCam.transform.position - _localCam.transform.forward * (stickLen/2) - _localCam.transform.forward * 0.2f; //Put at end of stick and offset a bit
                stick.transform.rotation = _localCam.transform.rotation;
                stick.GetOrAddComponent<MeshRenderer>().enabled = true;
                stick.GetOrAddComponent<Collider>().isTrigger = true;
                stick.GetOrAddComponent<VRC_Pickup>().proximity = 5f;
                stick.GetOrAddComponent<VRC_Pickup>().pickupable = true;
                stick.GetOrAddComponent<VRC_Pickup>().allowManipulationWhenEquipped = true;
                stick.GetOrAddComponent<VRC_Pickup>().orientation = VRC_Pickup.PickupOrientation.Any;
                stick.GetOrAddComponent<Rigidbody>().useGravity = false;
                stick.GetOrAddComponent<Rigidbody>().isKinematic = true;
                Material mat = new Material(Shader.Find("Unlit/Color"));
                mat.color = Color.gray;
                stick.GetOrAddComponent<MeshRenderer>().material = mat;

                _stick = stick;
                activeCoroutine = MelonCoroutines.Start(FollowSelfie());
            }
        }

        private IEnumerator FollowSelfie()
        {
            _followSelfie = true;
            while (_followSelfie)
            {

                yield return new WaitForSeconds(.01f);
                if (_localCam is null || _stick is null || _localCam.Equals(null) || _stick.Equals(null)) {  yield break; } 
                //https://answers.unity.com/questions/1420784/if-something-null-not-good-enough.html
                _localCam.transform.position = _stick.transform.position + _stick.transform.forward * (_stick.transform.localScale.z/2) + _stick.transform.forward * 0.2f; //Same as above, but cam at end with a bit of offset 
                _localCam.transform.rotation = _stick.transform.rotation;
            }

        }
    }
}

    namespace UIExpansionKit.API
    {
        public struct LayoutDescriptionCustom
        {
            public static LayoutDescription QuickMenu = new LayoutDescription { NumColumns = 3, RowHeight = 380 / 5, NumRows = 5 };
    }
    }


public static class Utils
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            return gameObject.AddComponent<T>();
        }
        return component;
    }
}