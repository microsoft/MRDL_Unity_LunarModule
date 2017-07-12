//
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
//
using HUX.Dialogs;
using HUX.Interaction;
using System.Collections;
using UnityEngine;

namespace MRDL
{
    public class RoomScanScreen : GameScreen
    {
        /*[SerializeField]
        private SimpleMenuCollection chooseRoomMenu;*/

        [TextArea]
        public string ScanMessage = "Look around the room to scan your environment";
        [TextArea]
        public string ScanProgress = "Current scan quality: {0}";
        [TextArea]
        public string ScanContinue = "Tap to stop scanning or continue for better quality";
        [TextArea]
        public string ScanFinish = "Finished Scanning";

        public Color PoorColor;
        public Color GoodColor;
        public Color BetterColor;
        public Color BestColor;

        public Renderer FloorBoundsRenderer;

        [SerializeField]
        private SimpleMenuCollection scanOrLoadRoomMenu;

        [SerializeField]
        private SimpleMenuCollection saveRoomMenu;

        [SerializeField]
        private GameObject finishScanButton;

        public override void Activate(ProgramStateEnum state) {
            base.Activate(state);

            Debug.Log("Activating room scan screen for " + state);

            switch (state) {
                case ProgramStateEnum.ChooseRoom:
                    // TEMP disable until finished with mesh caching
                    Deactivate();
                    break;
                    // TEMP
                    /*chooseRoomMenu.gameObject.SetActive(true);
                    directionIndicator.TargetObject = chooseRoomMenu.gameObject;
                    RefreshRoomButtons();
                    break;*/

                case ProgramStateEnum.ScanOrLoadRoom:
                    scanOrLoadRoomMenu.gameObject.SetActive(true);
                    directionIndicator.TargetObject = scanOrLoadRoomMenu.gameObject;
                    RefreshRoomButtons();
                    break;

                case ProgramStateEnum.SavingScan:
                    // TEMP disable until finished with mesh caching
                    Deactivate();
                    break;
                    // TEMP
                    /*saveRoomMenu.gameObject.SetActive(true);
                    directionIndicator.TargetObject = saveRoomMenu.gameObject;
                    break;*/

                case ProgramStateEnum.ScanRoom:
                    // Turn everything off and initiate room scan
                    //chooseRoomMenu.gameObject.SetActive(false);
                    scanOrLoadRoomMenu.gameObject.SetActive(false);
                    saveRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    RoomScanManager.Instance.ScanRoom();
                    StartCoroutine(ShowRoomScanFeedback());
                    break;

                default:
                    Debug.LogError("Unknown state choice in " + name + ": " + state);
                    break;
            }
        }

        public override void Deactivate() {
            base.Deactivate();

            //chooseRoomMenu.gameObject.SetActive(false);
            scanOrLoadRoomMenu.gameObject.SetActive(false);
            saveRoomMenu.gameObject.SetActive(false);
            finishScanButton.SetActive(false);
        }

        protected override void OnTapped(GameObject obj, InteractionManager.InteractionEventArgs eventArgs)
        {
            base.OnTapped(obj, eventArgs);

            switch (obj.name)
            {
                /*case "ConfirmRoom":
                    chooseRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    Deactivate();
                    break;

                case "ChangeRoom":
                    // Clear the current room
                    chooseRoomMenu.gameObject.SetActive(false);
                    scanOrLoadRoomMenu.gameObject.SetActive(true);
                    directionIndicator.TargetObject = scanOrLoadRoomMenu.gameObject;
                    RefreshRoomButtons();
                    break;*/

                case "FinishScanning":
                    RoomScanManager.Instance.FinishScanning();
                    break;

                case "ScanRoom":
                    //chooseRoomMenu.gameObject.SetActive(false);
                    scanOrLoadRoomMenu.gameObject.SetActive(false);
                    saveRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    RoomScanManager.Instance.ScanRoom();
                    StartCoroutine(ShowRoomScanFeedback());
                    break;

                case "LoadRoom1":
                    RoomScanManager.Instance.LoadRoom(RoomScanManager.RoomEnum.Room1);
                    scanOrLoadRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    StartCoroutine(ShowRoomLoadFeedback());
                    break;

                case "LoadRoom2":
                    RoomScanManager.Instance.LoadRoom(RoomScanManager.RoomEnum.Room2);
                    scanOrLoadRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    StartCoroutine(ShowRoomLoadFeedback());
                    break;

                case "LoadRoom3":
                    RoomScanManager.Instance.LoadRoom(RoomScanManager.RoomEnum.Room3);
                    scanOrLoadRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    StartCoroutine(ShowRoomLoadFeedback());
                    break;

                case "SaveRoom1":
                    RoomScanManager.Instance.SaveRoom(RoomScanManager.RoomEnum.Room1);
                    saveRoomMenu.gameObject.SetActive(false);
                    StartCoroutine(ShowRoomSaveFeedback());
                    break;

                case "SaveRoom2":
                    RoomScanManager.Instance.SaveRoom(RoomScanManager.RoomEnum.Room2);
                    saveRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    StartCoroutine(ShowRoomSaveFeedback());
                    break;

                case "SaveRoom3":
                    RoomScanManager.Instance.SaveRoom(RoomScanManager.RoomEnum.Room3);
                    saveRoomMenu.gameObject.SetActive(false);
                    directionIndicator.TargetObject = null;
                    StartCoroutine(ShowRoomSaveFeedback());
                    break;

                default:
                    Debug.LogError("Unknown button choice in " + name + ": " + obj.name);
                    break;
            }
        }

        private void RefreshRoomButtons() {
            scanOrLoadRoomMenu.SetButtonEnabled("LoadRoom1", RoomScanManager.Instance.HasSavedRoom(RoomScanManager.RoomEnum.Room1));
            scanOrLoadRoomMenu.SetButtonEnabled("LoadRoom2", RoomScanManager.Instance.HasSavedRoom(RoomScanManager.RoomEnum.Room2));
            scanOrLoadRoomMenu.SetButtonEnabled("LoadRoom3", RoomScanManager.Instance.HasSavedRoom(RoomScanManager.RoomEnum.Room3));
        }

        private IEnumerator ShowRoomSaveFeedback() {
            // TODO give feedback for saving as it's happening
            while (RoomScanManager.Instance.IsProcessing) {
                yield return null;
            }
            Deactivate();
            yield break;
        }

        private IEnumerator ShowRoomLoadFeedback() {
            // TODO give feedback for loading as it's happening
            while (RoomScanManager.Instance.IsProcessing) {
                yield return null;
            }
            Deactivate();
            yield break;
        }

        private IEnumerator ShowRoomScanFeedback () {

            LoadingDialog.Instance.Open(
                LoadingDialog.IndicatorStyleEnum.None,
                LoadingDialog.ProgressStyleEnum.ProgressBar,
                LoadingDialog.MessageStyleEnum.Visible,
                ScanMessage);

            while (RoomScanManager.Instance.IsProcessing) {

#if UNITY_EDITOR
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    RoomScanManager.Instance.FinishScanning();
                }
#endif

                float progress = 0.1f;
                string message = ScanMessage;
                Color color = PoorColor;
                switch (RoomScanManager.Instance.ScanQuality) {
                    case RoomScanManager.ScanQualityEnum.Poor:
                    default:
                        break;

                    case RoomScanManager.ScanQualityEnum.Good:
                        message = string.Format(ScanProgress, RoomScanManager.Instance.ScanQuality.ToString()) + System.Environment.NewLine + ScanContinue;
                        progress = 0.3f;
                        color = GoodColor;
                        finishScanButton.SetActive(true);
                        break;

                    case RoomScanManager.ScanQualityEnum.Better:
                        message = string.Format(ScanProgress, RoomScanManager.Instance.ScanQuality.ToString()) + System.Environment.NewLine + ScanContinue;
                        progress = 0.6f;
                        finishScanButton.SetActive(true);
                        color = BetterColor;
                        break;

                    case RoomScanManager.ScanQualityEnum.Best:
                        message = string.Format(ScanFinish, RoomScanManager.Instance.ScanQuality.ToString()) + System.Environment.NewLine + ScanContinue; progress = 0.6f;
                        progress = 1f;
                        color = BestColor;
                        finishScanButton.SetActive(true);
                        break;
                }

                LoadingDialog.Instance.SetProgressColor(color);
                LoadingDialog.Instance.SetMessage(message);
                LoadingDialog.Instance.SetProgress(progress);

                yield return null;
            }

            LoadingDialog.Instance.Close();

            while (LoadingDialog.Instance.IsLoading) {
                yield return null;
            }

            // Once we're done scanning, show a save menu

            // TEMP - disable this until we've implemented room caching
            RoomScanManager.Instance.SaveRoom(RoomScanManager.RoomEnum.Room1);
            Deactivate();
            /*saveRoomMenu.gameObject.SetActive(true);
            directionIndicator.TargetObject = saveRoomMenu.gameObject;*/
            // TEMP

            yield break;
        }
    }
}