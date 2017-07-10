//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HoloToolkit.Unity;
using HUX.Dialogs;
using HUX.Interaction;
using HUX.Receivers;
using UnityEngine;

namespace MRDL
{
    public abstract class GameScreen : InteractionReceiver
    {
        public bool IsActive {
            get {
                return isActive;
            }
        }

        [SerializeField]
        protected HeadsUpDirectionIndicator directionIndicator;

        [SerializeField]
        private GameObject headsUpTarget;

        public virtual void Activate(ProgramStateEnum state) {
            isActive = true;
            gameScreenResult = state;
            directionIndicator.TargetObject = headsUpTarget;
        }

        public virtual void Deactivate() {
            isActive = false;
            directionIndicator.TargetObject = null;
        }

        protected virtual void Start() {
            Deactivate();
        }

        public ProgramStateEnum Result {
            get {
                return gameScreenResult;
            }
            protected set {
                gameScreenResult = value;
            }
        }

        private ProgramStateEnum gameScreenResult = ProgramStateEnum.Initializing;
        private bool isActive = false;
    }
}