//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using MixedRealityToolkit.Common;
using UnityEngine;

namespace MRDL
{
    public class LanderAudio : Singleton<LanderAudio>
    {
        public AudioClip CrashClip;
        public AudioClip LandClip;
        public AudioClip ThrusterClip;
        public AudioClip TorqueClip;
        public AudioClip LandingDangerClip;
        public AudioClip DangerEndClip;
        public float ThrusterVolumeMultiplier = 1f;
        public float MaxVolume = 0.5f;
        public float ForceThrustVolume = 0f;

        [SerializeField]
        private AudioSource audioSourceUp;

        [SerializeField]
        private AudioSource audioSourceNavigate;

        [SerializeField]
        private AudioSource audioSourceDanger;
        
        private void Start()
        {
            LanderGameplay.Instance.OnGameplayEnded += OnGameplayEnded;

            audioSourceUp.clip = ThrusterClip;
            audioSourceNavigate.clip = TorqueClip;

            audioSourceUp.volume = 0f;
            audioSourceNavigate.volume = 0f;

            audioSourceUp.loop = true;
            audioSourceNavigate.loop = true;

            audioSourceUp.Play();
            audioSourceNavigate.Play();

            audioSourceDanger.clip = LandingDangerClip;
        }

        private void Update ()
        {
            /*if (!LanderGameplay.Instance.GameInProgress || LanderGameplay.Instance.Paused) {
                audioSourceUp.volume = 0f;
                audioSourceNavigate.volume = 0f;
                return;
            }*/
            
            audioSourceNavigate.volume = Mathf.Clamp(
                Mathf.Abs(LanderPhysics.Instance.AxisUpDown) +
                Mathf.Abs(LanderPhysics.Instance.AxisFrontBack) +
                Mathf.Abs(LanderPhysics.Instance.AxisLeftRight), 0f, MaxVolume);
            audioSourceUp.volume = Mathf.Clamp(ForceThrustVolume + Mathf.Abs(LanderInput.Instance.TargetThrust) * ThrusterVolumeMultiplier, 0f, MaxVolume);

            if (LanderGameplay.Instance.LandingAngleDanger || LanderGameplay.Instance.LandingSpeedDanger) {
                if (!audioSourceDanger.isPlaying) {
                    audioSourceDanger.Play();
                }
            } else {
                audioSourceDanger.PlayOneShot(DangerEndClip, 1f);
                audioSourceDanger.Stop();
            }
        }

        private void OnGameplayEnded()
        {
            AudioSource.PlayClipAtPoint(LanderGameplay.Instance.HasCrashed ? CrashClip : LandClip, LanderGameplay.Instance.LandPoint);
        }
    }
}