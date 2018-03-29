//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HoloToolkit.Unity;
using HoloToolkit.Unity.SpatialMapping;
using HUX.Dialogs;
using HUX.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using MixedRealityToolkit.Common;
using UnityEngine;

namespace MRDL
{
    /// <summary>
    /// Manages saving, loading and scanning of room data
    /// </summary>
    public class RoomScanManager : MixedRealityToolkit.Common.Singleton<RoomScanManager>
    {
        const int MaxRoomMeshes = 1000;

        public float GoodSurfaceArea = 0.25f;
        public float BetterSurfaceArea = 0.5f;
        public float BestSurfaceArea = 0.75f;

        /// <summary>
        /// True if the manager is scanning, loading or saving
        /// </summary>
        public bool IsProcessing {
            get {
                return isProcessing;
            }
        }

        public enum RoomEnum
        {
            None,
            Room1,
            Room2,
            Room3,
        }
        
        public enum ScanQualityEnum
        {
            Poor,
            Good,
            Better,
            Best
        }

        public RoomEnum CurrentRoom
        {
            get
            {
                return currentRoom;
            }
        }

        public ScanQualityEnum ScanQuality {
            get {
                return scanQuality;
            }
        }

        public void SaveRoom (RoomEnum room)
        {
            StartCoroutine(SaveRoomOverTime(room));
        }

        public void ScanRoom ()
        {
            currentRoom = RoomEnum.None;
            StartCoroutine(ScanRoomOverTime());
        }

        public void LoadRoom (RoomEnum room)
        {
            StartCoroutine(LoadRoomOverTime(room));
        }

        public void FinishScanning() {
            if (!IsProcessing)
                return;
            
            forceFinishScan = true;
        }

        public bool HasSavedRooms
        {
            get
            {
                return HasSavedRoom (RoomEnum.Room1) 
                    || HasSavedRoom(RoomEnum.Room2)
                    || HasSavedRoom(RoomEnum.Room3);
            }
        }

        public bool HasSavedRoom(RoomEnum room) {
            // TEMP
            bool hasSavedRoom = false;
            switch (room) {
                case RoomEnum.Room1:
                    break;

                case RoomEnum.Room2:
                    break;

                case RoomEnum.Room3:
                    break;
            }
            return hasSavedRoom;
        }

        private IEnumerator LoadRoomOverTime(RoomEnum room)
        {
            isProcessing = true;
            LoadingDialog.Instance.Open(
                LoadingDialog.IndicatorStyleEnum.Prefab,
                LoadingDialog.ProgressStyleEnum.None,
                LoadingDialog.MessageStyleEnum.Visible,
                "Loading room data");

            /*List<Mesh> loadedMeshes = new List<Mesh>();
            IList<Mesh> meshes = null;
            // Keep trying to load meshes until we no longer find a file
            for (int i = 0; i < MaxRoomMeshes; i++) {
                // See if the file exists
                string fileName = GetMeshFileName(i, room);
                string path = System.IO.Path.Combine(MeshSaver.MeshFolderName, fileName + ".room");
                if (System.IO.File.Exists(path)) {
                    loadedMeshes.AddRange(MeshSaver.Load(fileName));
                    yield return new WaitForSeconds(0.05f);
                } else {
                    // If it doesn't, we're done here
                    break;
                }
            }*/

            yield return new WaitForSeconds(0.5f);

            LoadingDialog.Instance.Close();
            while (LoadingDialog.Instance.IsLoading) {
                yield return null;
            }

            // Open again with a progress bar showing room creation progress
            LoadingDialog.Instance.Open(
                LoadingDialog.IndicatorStyleEnum.Prefab,
                LoadingDialog.ProgressStyleEnum.Percentage,
                LoadingDialog.MessageStyleEnum.Visible,
                "Creating room");
            /*for (int i = 0; i < loadedMeshes.Count; i++) {
                LoadingDialog.Instance.SetProgress((float)i / loadedMeshes.Count);
                GameObject newMeshObject = new GameObject(GetMeshFileName(i, room));
                MeshFilter mf = newMeshObject.AddComponent<MeshFilter>();
                mf.sharedMesh = loadedMeshes[i];
                newMeshObject.AddComponent<MeshRenderer>();
                yield return null;
            }*/

            yield return new WaitForSeconds(0.5f);

            LoadingDialog.Instance.Close();

            currentRoom = room;
            isProcessing = false;
        }

        private IEnumerator SaveRoomOverTime(RoomEnum room) {

            // TEMP disable save room until caching is implemented
            currentRoom = room;
            yield break;
            // TEMP

            isProcessing = true;
            LoadingDialog.Instance.Open(
                LoadingDialog.IndicatorStyleEnum.Prefab,
                LoadingDialog.ProgressStyleEnum.Percentage,
                LoadingDialog.MessageStyleEnum.Visible,
                "Saving room");

            //TEMP tag the meshes as moon surface while we're here
            List<MeshFilter> meshes = SpatialMappingManager.Instance.GetMeshFilters();
            for (int i = 0; i < meshes.Count; i++) {
                meshes[i].gameObject.tag = EnvironmentManager.MoonSurfaceTag;
            }

            /*List<MeshFilter> meshes = SpatialMappingManager.Instance.GetMeshFilters();
            //List<MeshFilter> meshes = SpatialUnderstanding.Instance.UnderstandingCustomMesh.GetMeshFilters();
            List<Mesh> saveMesh = new List<Mesh>();
            for (int i = 0; i < meshes.Count; i++) {
                saveMesh.Add(meshes[i].sharedMesh);
                MeshSaver.Save(GetMeshFileName(i, room), saveMesh);
                LoadingDialog.Instance.SetProgress((float)i / meshes.Count);
                yield return new WaitForSeconds (0.05f);
                saveMesh.Clear();
            }*/

            // TEMP test loading screen
            float progress = 0f;
            while (progress < 1f) {
                progress += Time.deltaTime * 0.5f;
                LoadingDialog.Instance.SetProgress(progress);
                yield return null;
            }

            LoadingDialog.Instance.Close();

            currentRoom = room;
            isProcessing = false;
        }

        private IEnumerator ScanRoomOverTime ()
        {
            scanQuality = ScanQualityEnum.Poor;
            surfaceArea = 0f;
            forceFinishScan = false;
            isProcessing = true;

            // Clean up any existing meshes
            SpatialMappingManager.Instance.CleanupObserver();

            yield return new WaitForSeconds(0.25f);

            // Start observer
            SpatialMappingManager.Instance.StartObserver();

            // Wait for a moment to let the observer get started
            yield return new WaitForSeconds(0.25f);

            // Subscribe to the finishing message for the plane scanner
            SurfaceMeshesToPlanes.EventHandler handler = new SurfaceMeshesToPlanes.EventHandler(MakePlanesComplete);
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete += handler;

            // Start scanning the room
            SpatialUnderstanding.Instance.RequestBeginScanning();
            // Wait for scanning to actually commence
            while (SpatialUnderstanding.Instance.ScanState != SpatialUnderstanding.ScanStates.Scanning) {
                yield return null;
            }

            // Unfortunately due to the FUBAR nature of spatial understanding we will not be using its visual meshes
            SpatialUnderstanding.Instance.UnderstandingCustomMesh.DrawProcessedMesh = false;//true;
            
            // Let it scan, then check periodically for planes
            // If we have enough planes, stop scanning
            while (!forceFinishScan) {

                yield return new WaitForSeconds(1f);

                // Request planes
                SurfaceMeshesToPlanes.Instance.MakePlanes();
                waitingForPlanes = true;

                while (waitingForPlanes) {
                    yield return null;
                }

                // Check to see if we have enough floor planes
                surfaceArea = 0f;
                foreach (GameObject activePlane in SurfaceMeshesToPlanes.Instance.ActivePlanes) {
                    // Set the layer of the plane
                    activePlane.layer = EnvironmentManager.MoonSurfaceLayer;
                    SurfacePlane sp = activePlane.GetComponent<SurfacePlane>();
                    if (sp.PlaneType == PlaneTypes.Floor || sp.PlaneType == PlaneTypes.Table) {
                        Bounds bounds = sp.GetComponent<Collider>().bounds;
                        // Get the x-z surface area
                        surfaceArea += (bounds.size.x * bounds.size.z);
                        // Set the tag of this plane so we know it's a surface and not a wall
                        activePlane.tag = EnvironmentManager.MoonSurfaceTag;
                    } else {
                        // Set the tag so we know it's a blocking surface
                        activePlane.tag = EnvironmentManager.BlockingSurfaceTag;
                    }
                }

                scanQuality = ScanQualityEnum.Poor;
                ScanQualityEnum newScanQuality = scanQuality;
                if (surfaceArea >= BestSurfaceArea) {
                    newScanQuality = ScanQualityEnum.Best;
                } else if (surfaceArea >= BetterSurfaceArea) {
                    newScanQuality = ScanQualityEnum.Better;
                } else if (surfaceArea >= GoodSurfaceArea) {
                    newScanQuality = ScanQualityEnum.Good;
                }

                if (scanQuality != newScanQuality) {
                    scanQuality = newScanQuality;
                    yield return new WaitForSeconds(1f);
                }
            }

            // Finish
            isProcessing = false;

            // Tell the scanner to finish and generate meshes
            // Tell the scanner it can stop
            SpatialUnderstanding.Instance.RequestFinishScan();
            // Un-subscribe from plane scanner
            SurfaceMeshesToPlanes.Instance.MakePlanesComplete -= handler;

            yield break;
        }

        private void Start() {
            LanderGameplay.Instance.OnGameplayStarted += OnGameplayStarted;
        }

        private void OnGameplayStarted () {
            // Once gameplay has actually begun, stop all scanning
            SpatialMappingManager.Instance.StopObserver();
        }

        private void MakePlanesComplete(object source, EventArgs args) {
            waitingForPlanes = false;
        }

        private string GetMeshFileName(int meshIndex, RoomEnum room) {
            return room.ToString() + "_" + meshIndex.ToString("D8");
        }

        [SerializeField]
        private RoomEnum currentRoom = RoomEnum.None;

        [SerializeField]
        private ScanQualityEnum scanQuality = ScanQualityEnum.Poor;

        [SerializeField]
        private float surfaceArea;

        private bool waitingForPlanes;
        private bool forceFinishScan;
        private bool isProcessing;
    }
}