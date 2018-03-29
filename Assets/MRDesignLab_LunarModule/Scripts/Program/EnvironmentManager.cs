//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HoloToolkit.Unity.SpatialMapping;
//using HUX;
using System.Collections;
using System.Collections.Generic;
using MixedRealityToolkit.Common;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Creates scattered moon rocks and controls the skybox / stars / earth
    /// </summary>
    public class EnvironmentManager : Singleton<EnvironmentManager>
    {
        public const int MoonSurfaceLayer = 9;
        public const int RoomSurfaceLayer = 12;
        public const int LandingPadLayer = 13;
        public const string MoonSurfaceTag = "MoonSurface";
        public const string BlockingSurfaceTag = "BlockingSurface";
        public const string LandingPadTag = "LandingPad";

        public float MinRoomWallsAlpha = 0.2f;

        public int MaxRockPlacementAttemptsPerFrame = 50;
        public float MinRockSize = 0.5f;
        public float MaxRockSize = 1.5f;
        public int MaxGeneratedRocks = 500;
        public float MinRockPlacementDot = 0.8f;

        public AudioClip ScanPulseClip;

        [SerializeField]
        private Light moonLight;

        [SerializeField]
        private Material gridMaterial;

        [SerializeField]
        private Material surfaceMaterial;

        [SerializeField]
        private GameObject earth;

        [SerializeField]
        private GameObject[] moonRockPrefabs;

        [SerializeField]
        private AudioSource audio;

        [SerializeField]
        private GameObject skyBox;

        [SerializeField]
        private float cielingHeight;

        public RaycastHit EnvironmentHit {
            get {
                return environmentHit;
            }
        }

        public void GenerateDynamicEnvironment ()
        {
            StartCoroutine(GenerateDynamicEnvironmentOverTime());
        }

        private IEnumerator GenerateDynamicEnvironmentOverTime ()
        {
            if (environmentRocksParent == null) {
                environmentRocksParent = new GameObject("Environment Rocks").transform;
            }

            if (generatedRocks == null) {
                generatedRocks = new GameObject[MaxGeneratedRocks];
                for (int i = 0; i < generatedRocks.Length; i++) {
                    generatedRocks[i] = GameObject.Instantiate(moonRockPrefabs[UnityEngine.Random.Range(0, moonRockPrefabs.Length)], environmentRocksParent);
                }
            }

            // Deactivate all the rocks to start with
            for (int i = 0; i < generatedRocks.Length; i++) {
                generatedRocks[i].SetActive(false);
            }

            // Get the lay of the land from the spatial mapping manager
            roomBounds = new Bounds();
            foreach (MeshFilter mf in SpatialMappingManager.Instance.GetMeshFilters()) {
                // Get the renderer from the mesh filter
                Renderer r = mf.GetComponent<Renderer>();
                roomBounds.Encapsulate(r.bounds);
            }

            // Raycast down from a random point inside the bounds
            Vector3 raycastPoint = Vector3.zero;
            Vector3 rockPosition = Vector3.zero;
            RaycastHit hit;
            int numPlacementAttempts = 0;
            for (int i = 0; i < generatedRocks.Length; i++) {

                bool foundSpot = false;
                while (!foundSpot) {
                    raycastPoint.x = Random.Range(roomBounds.min.x, roomBounds.max.x);
                    raycastPoint.y = roomBounds.max.y;
                    raycastPoint.z = Random.Range(roomBounds.min.z, roomBounds.max.z);
                    if (Physics.Raycast (raycastPoint, Vector3.down, out hit, roomBounds.size.y * 2, 1 << RoomSurfaceLayer, QueryTriggerInteraction.Ignore)) {
                        if (Vector3.Dot(hit.normal, Vector3.up) > MinRockPlacementDot) {
                            rockPosition = hit.point;
                            foundSpot = true;
                        }
                    }
                    numPlacementAttempts++;
                    if (numPlacementAttempts > MaxRockPlacementAttemptsPerFrame) {
                        numPlacementAttempts = 0;
                        yield return null;
                    }
                }

                generatedRocks[i].SetActive(true);
                generatedRocks[i].transform.position = rockPosition;
                generatedRocks[i].transform.localScale = Vector3.one * Random.Range(MinRockSize, MaxRockSize);
                generatedRocks[i].transform.localEulerAngles = Random.onUnitSphere * 360f;
            }

            yield break;
        }

        private void Start() {
            gridMaterials[0] = gridMaterial;
            surfaceMaterials[0] = surfaceMaterial;
        }

        private void Update() {
            if (CameraCache.Main == null || CameraCache.Main.transform == null)
                return;

            // Don't show earth or moonlight when a menu is open
            switch (LunarModuleProgram.Instance.State) {
                case ProgramStateEnum.Initializing:
                case ProgramStateEnum.StartupScreen:
                case ProgramStateEnum.ScanOrLoadRoom:
                    moonLight.enabled = false;
                    earth.SetActive(false);
                    break;

                default:
                    moonLight.enabled = true;
                    earth.SetActive(true);
                    break;
            }

            Vector3 shaderFocusPoint = CameraCache.Main.transform.position;

            if (!LandingPadManager.Instance.LandingPlacementConfirmed || !LandingPadManager.Instance.StartupPlacementConfirmed) {
                // Get a raycast hit from our environment
                int layerMask = 1 << EnvironmentManager.MoonSurfaceLayer | 1 << EnvironmentManager.RoomSurfaceLayer;
                if (Physics.Raycast(
                    CameraCache.Main.transform.position,
                    CameraCache.Main.transform.forward,
                    out environmentHit,
                    float.MaxValue,
                    layerMask,
                    QueryTriggerInteraction.Ignore)) {
                    shaderFocusPoint = environmentHit.point;
                }
            }

            // Check to see what state our shaders should be in
            Color surfaceColor = Color.white;
            Color pulseColor = Color.clear;
            float pulseSize = Mathf.Repeat(Time.time * 5, 6f);
            float minRoomWalls = MinRoomWallsAlpha;
            List<MeshFilter> meshes = SpatialMappingManager.Instance.GetMeshFilters();

            switch (LunarModuleProgram.Instance.State) {
                case ProgramStateEnum.ScanOrLoadRoom:
                case ProgramStateEnum.ScanRoom:
                case ProgramStateEnum.SavingScan:
                case ProgramStateEnum.LoadingScan:
                    if (pulseSize < lastPulseSize) {
                        // We've looped, emit a scan sound
                        audio.PlayOneShot(ScanPulseClip, 0.15f);
                    }
                    lastPulseSize = pulseSize;
                    pulseColor = Color.white;
                    surfaceColor.a = 0f;
                    minRoomWalls = 0f;
                    //TODO replace this with a BETTER SPATIAL MAPPING MANAGER
                    for (int i = 0; i < meshes.Count; i++) {
                        meshes[i].tag = EnvironmentManager.MoonSurfaceTag;
                        meshes[i].GetComponent<MeshRenderer>().sharedMaterials = gridMaterials;
                    }
                    SpatialMappingManager.Instance.SurfaceMaterials = gridMaterials;
                    break;

                case ProgramStateEnum.Gameplay:
                case ProgramStateEnum.GameplayFinished:
                default:
                    //TODO replace this with a BETTER SPATIAL MAPPING MANAGER
                    for (int i = 0; i < meshes.Count; i++) {
                        meshes[i].tag = EnvironmentManager.MoonSurfaceTag;
                        meshes[i].GetComponent<MeshRenderer>().sharedMaterials = surfaceMaterials;
                    }
                    SpatialMappingManager.Instance.SurfaceMaterials = surfaceMaterials;
                    cielingHeight = LandingPadManager.Instance.LanderStartupPosition.y;
                    break;
            }

            gridMaterial.SetVector("_PulseCenter", shaderFocusPoint);
            gridMaterial.SetFloat("_PulseSize", pulseSize);
            gridMaterial.SetColor("_Color", pulseColor);
            surfaceMaterial.SetColor("_Color", surfaceColor);
            surfaceMaterial.SetFloat("_MinAlpha", minRoomWalls);
            surfaceMaterial.SetFloat("_CeilingHeight", cielingHeight);

            // Set the first gradient
            if (LandingPadManager.Instance.LandingPlacementConfirmed) {
                surfaceMaterial.SetVector("_GradientCenterA", LandingPadManager.Instance.LandingPad.transform.position);
            } else {
                surfaceMaterial.SetVector("_GradientCenterA", shaderFocusPoint);
            }

            shaderFocusPoint = Vector3.zero;
            // Set the second gradient
            if (LanderGameplay.Instance.GameInProgress) {
                // If the game is in progress, set to current position
                shaderFocusPoint = LanderPhysics.Instance.LanderPosition;
            } else if (LanderGameplay.Instance.HasCrashed) {
                shaderFocusPoint = LanderGameplay.Instance.LandPoint;
            } /*else if (!LandingPadManager.Instance.StartupPlacementValid) {
                shaderFocusPoint = LandingPadManager.Instance.StartupPad.transform.position;
            }*/ else {
                // Set the gradient to nowhere
                shaderFocusPoint.y = -100000f;
            }
            surfaceMaterial.SetVector("_GradientCenterB", shaderFocusPoint);
        }

        private Bounds roomBounds;
        private Transform environmentRocksParent;
        private RaycastHit environmentHit;
        private GameObject[] generatedRocks;
        private Material[] gridMaterials = new Material[1];
        private Material[] surfaceMaterials = new Material[1];
        private float lastPulseSize;
    }
}