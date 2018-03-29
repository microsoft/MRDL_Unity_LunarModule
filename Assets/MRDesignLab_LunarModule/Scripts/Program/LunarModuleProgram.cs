//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
//using HUX.Utility;
using MixedRealityToolkit.Common;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

namespace MRDL
{
    public enum ProgramStateEnum
    {
        Initializing,
        StartupScreen,
        ChooseRoom,
        ScanOrLoadRoom,
        ScanRoom,
        LoadingScan,
        SavingScan,
        LandingPadPlacement,
        ControlsDisplay,
        Gameplay,
        GameplayFinished,
        Quitting,
    }

    /// <summary>
    /// Manages overall flow of control
    /// </summary>
    public class LunarModuleProgram : Singleton<LunarModuleProgram>
    {
        public AudioClip TitleMusic;
        public AudioMixer MusicMixer;
        public float TitleMusicVolume = 0.2f;
        public float TitleMusicCutoffHigh = 5000f;
        public float TitleMusicCutoffLow = 400f;

        public ProgramStateEnum State
        {
            get
            {
                return state;   
            }private set
            {
                state = value;
            }
        }

        private IEnumerator Start ()
        {
            lowpass = TitleMusicCutoffHigh;

            State = ProgramStateEnum.Initializing;

            logoObject.SetActive(false);
            while (fitBox != null && fitBox.activeSelf)
            {
                // Wait for the fitbox to be dismissed
                yield return null;
            }
            
            // Start the main program loop
            StartCoroutine(Startup());
            yield break;
        }

        private IEnumerator Startup() {
            
            State = ProgramStateEnum.StartupScreen;

            // Wait for startup screen to finish
            startupScreen.Activate(State);
            logoObject.SetActive(true);
            while (startupScreen.IsActive) {
                yield return null;
            }
            logoObject.SetActive(false);

            // See whether the player asked to quit
            switch (startupScreen.Result) {
                case ProgramStateEnum.Quitting:
                    // Done!
                    Application.Quit();
                    yield break;

                case ProgramStateEnum.ChooseRoom:
                default:
                    // Move forward to room scanning
                    break;
            }

            // Room scanning!
            StartCoroutine(RoomScanning());
            
            // Room scanning will launch another loop, so break here
            yield break;
        }

        private IEnumerator RoomScanning() {

            // Choose which room scanning state we want
            // If we haven't chosen a room, scan or load a room
            if (RoomScanManager.Instance.CurrentRoom == RoomScanManager.RoomEnum.None) {
                // If we have saved rooms, let the player choose one
                if (RoomScanManager.Instance.HasSavedRooms) {
                    State = ProgramStateEnum.ScanOrLoadRoom;
                } else {
                    // Otherwise, just begin scan immediately
                    State = ProgramStateEnum.ScanRoom;
                }
            } else {
                // If we have chosen a room, let them continue or change
                State = ProgramStateEnum.ChooseRoom;
            }

            // Wait for room scan screen to finish
            roomScanScreen.Activate(State);
            while (roomScanScreen.IsActive) {
                yield return null;
            }

            // Tell the environment generator to start placing rocks and stuff
            EnvironmentManager.Instance.GenerateDynamicEnvironment();

            // Gameplay!
            StartCoroutine(Gameplay());

            // Gameplay will launch another loop, so break here
            yield break;
        }

        private IEnumerator Gameplay () {

            // Reset the old gameplay state
            LanderGameplay.Instance.Reset();

            // Show the controls screen
            State = ProgramStateEnum.ControlsDisplay;
            // Wait for the player to dismiss
            controlsScreen.Activate(State);
            while (controlsScreen.IsActive) {
                yield return null;
            }

            // Now the player will choose where to place the landing pad
            State = ProgramStateEnum.LandingPadPlacement;

            // Wait for landing pad placement to finish
            placementScreen.Activate(State);
            while (placementScreen.IsActive) {
                yield return null;
            }

            State = ProgramStateEnum.Gameplay;
            // Launch gameplay screen and wait for the result
            gameplayScreen.Activate(State);
            while (gameplayScreen.IsActive) {
                yield return null;
            }

            State = ProgramStateEnum.GameplayFinished;
            gameplayFinishedScreen.Activate(State);
            while (gameplayFinishedScreen.IsActive) {
                yield return null;
            }

            // Launch another loop based on the player's choice
            switch (gameplayFinishedScreen.Result) {
                case ProgramStateEnum.StartupScreen:
                    StartCoroutine(Startup());
                    break;

                case ProgramStateEnum.ChooseRoom:
                default:
                    StartCoroutine(Gameplay());
                    break;
            }
            yield break;
        }

        private void Update() {
            switch (State) {
                default:
                    if (!audio.isPlaying) {
                        audio.volume = TitleMusicVolume;
                        audio.clip = TitleMusic;
                        audio.Play();
                    }
                    lowpass = Mathf.Lerp(lowpass, TitleMusicCutoffLow, Time.deltaTime);
                    break;

                case ProgramStateEnum.Initializing:
                    break;

                case ProgramStateEnum.StartupScreen:
                case ProgramStateEnum.ChooseRoom:
                case ProgramStateEnum.ScanOrLoadRoom:
                case ProgramStateEnum.LoadingScan:
                case ProgramStateEnum.SavingScan:
                case ProgramStateEnum.ScanRoom:
                    if (!audio.isPlaying) {
                        audio.volume = TitleMusicVolume;
                        audio.clip = TitleMusic;
                        audio.Play();
                    }
                    lowpass = Mathf.Lerp(lowpass, TitleMusicCutoffHigh, Time.deltaTime);
                    break;

                case ProgramStateEnum.Gameplay:
                    if (audio.isPlaying) {
                        audio.volume = Mathf.Lerp(audio.volume, 0f, Time.deltaTime);
                        if (audio.volume < 0.001f) {
                            audio.Stop();
                        }
                    }
                    lowpass = Mathf.Lerp(lowpass, TitleMusicCutoffLow, Time.deltaTime);
                    break;

                case ProgramStateEnum.GameplayFinished:
                    if (!audio.isPlaying) {
                        audio.volume = 0f;
                        audio.Play();
                    }
                    audio.volume = Mathf.Lerp(audio.volume, TitleMusicVolume, Time.deltaTime * 0.25f);
                    lowpass = Mathf.Lerp(lowpass, TitleMusicCutoffHigh, Time.deltaTime);
                    break;
            }

            MusicMixer.SetFloat("Lowpass", lowpass);
        }

        [SerializeField]
        private AudioSource audio;
        private float lowpass;
        // The screens used to present stuff to the player
        /*[SerializeField]
        private GameScreen logoScreen;*/

        [SerializeField]
        private GameObject logoObject;

        [SerializeField]
        private GameScreen startupScreen;

        [SerializeField]
        private GameScreen roomScanScreen;

        [SerializeField]
        private GameScreen placementScreen;

        [SerializeField]
        private GameScreen controlsScreen;

        [SerializeField]
        private GameScreen gameplayScreen;

        [SerializeField]
        private GameScreen gameplayFinishedScreen;

        [SerializeField]
        private ProgramStateEnum state = ProgramStateEnum.Initializing;

        [SerializeField]
        private GameObject fitBox;
    }
}