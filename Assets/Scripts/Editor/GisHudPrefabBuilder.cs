using GeoVision.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GeoVision.Tools
{
    public static class GisHudPrefabBuilder
    {
        private const string PrefabFolder = "Assets/Prefabs/UI";
        private const string HudPrefabPath = "Assets/Prefabs/UI/GisHud.prefab";
        private const string AlarmPrefabPath = "Assets/Prefabs/UI/AlarmItem.prefab";
        private const string ScenePath = "Assets/Scenes/SampleScene.unity";

        [InitializeOnLoadMethod]
        private static void AutoBuildIfMissing()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isPlayingOrWillChangePlaymode)
                {
                    return;
                }

                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(HudPrefabPath);
                if (existing == null)
                {
                    BuildAndSave();
                    return;
                }

                PlaceHudIfSampleSceneAlreadyOpen(existing);
            };
        }

        [MenuItem("GeoVision/Rebuild GisHud Prefab")]
        public static void BuildAndSave()
        {
            EnsureFolders();
            Sprite uiSprite = LoadUiSprite();
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            GameObject alarmPrefabRoot = BuildAlarmItem(uiSprite, font);
            GameObject alarmPrefab = PrefabUtility.SaveAsPrefabAsset(alarmPrefabRoot, AlarmPrefabPath);
            Object.DestroyImmediate(alarmPrefabRoot);

            GameObject hudRoot = BuildHud(uiSprite, font, alarmPrefab);
            GameObject hudPrefab = PrefabUtility.SaveAsPrefabAsset(hudRoot, HudPrefabPath);
            Object.DestroyImmediate(hudRoot);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            PlaceHudInSampleScene(hudPrefab);
            Debug.Log("GisHudPrefabBuilder: saved " + HudPrefabPath + " and " + AlarmPrefabPath);
        }

        private static void PlaceHudIfSampleSceneAlreadyOpen(GameObject hudPrefab)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                return;
            }

            PlaceHudInScene(scene, hudPrefab);
        }

        private static void PlaceHudInSampleScene(GameObject hudPrefab)
        {
            Scene scene = EditorSceneManager.GetActiveScene();
            if (!scene.IsValid() || scene.path != ScenePath)
            {
                scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            PlaceHudInScene(scene, hudPrefab);
        }

        private static void PlaceHudInScene(Scene scene, GameObject hudPrefab)
        {
            bool changed = false;
            GisDashboard dashboard = Object.FindObjectOfType<GisDashboard>();
            if (dashboard == null)
            {
                PrefabUtility.InstantiatePrefab(hudPrefab, scene);
                changed = true;
            }

            if (Object.FindObjectOfType<EventSystem>() == null)
            {
                GameObject eventObject = new GameObject("EventSystem");
                eventObject.AddComponent<EventSystem>();
                eventObject.AddComponent<StandaloneInputModule>();
                changed = true;
            }

            if (!changed)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(PrefabFolder))
            {
                AssetDatabase.CreateFolder("Assets/Prefabs", "UI");
            }
        }

        private static Sprite LoadUiSprite()
        {
            Sprite sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (sprite == null)
            {
                sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Background.psd");
            }

            return sprite;
        }

        private static GameObject BuildHud(Sprite uiSprite, Font font, GameObject alarmPrefab)
        {
            GameObject canvasObject = new GameObject("GisHud");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();
            canvasObject.AddComponent<GisHudFontApplier>();

            Text timeText;
            BuildHeader(canvasObject.transform, uiSprite, font, out timeText);

            Text poiCount;
            Text onlineCount;
            Text alarmCount;
            Text projectCount;
            Text cameraCount;
            Text vehicleStatCount;
            Text alertStatCount;
            Image projectFill;
            Image cameraFill;
            Image vehicleFill;
            Image alertFill;
            Toggle poiToggle;
            Toggle boundaryToggle;
            Toggle trajectoryToggle;
            Toggle alarmToggle;
            BuildLeft(
                canvasObject.transform,
                uiSprite,
                font,
                out poiCount,
                out onlineCount,
                out alarmCount,
                out projectCount,
                out cameraCount,
                out vehicleStatCount,
                out alertStatCount,
                out projectFill,
                out cameraFill,
                out vehicleFill,
                out alertFill,
                out poiToggle,
                out boundaryToggle,
                out trajectoryToggle,
                out alarmToggle);

            Text vehicleName;
            Text vehicleStatus;
            Text vehicleSpeed;
            Text vehicleLon;
            Text vehicleLat;
            Image vehicleDot;
            Transform alarmContent;
            GameObject emptyState;
            BuildRight(
                canvasObject.transform,
                uiSprite,
                font,
                out vehicleName,
                out vehicleStatus,
                out vehicleSpeed,
                out vehicleLon,
                out vehicleLat,
                out vehicleDot,
                out alarmContent,
                out emptyState);

            BuildTrajectory(canvasObject.transform, uiSprite, font);

            GisDashboard dashboard = canvasObject.AddComponent<GisDashboard>();
            SerializedObject so = new SerializedObject(dashboard);
            so.FindProperty("timeText").objectReferenceValue = timeText;
            so.FindProperty("poiCountText").objectReferenceValue = poiCount;
            so.FindProperty("onlineCountText").objectReferenceValue = onlineCount;
            so.FindProperty("alarmCountText").objectReferenceValue = alarmCount;
            so.FindProperty("projectCountText").objectReferenceValue = projectCount;
            so.FindProperty("cameraCountText").objectReferenceValue = cameraCount;
            so.FindProperty("vehicleStatCountText").objectReferenceValue = vehicleStatCount;
            so.FindProperty("alertStatCountText").objectReferenceValue = alertStatCount;
            so.FindProperty("projectBarFill").objectReferenceValue = projectFill;
            so.FindProperty("cameraBarFill").objectReferenceValue = cameraFill;
            so.FindProperty("vehicleBarFill").objectReferenceValue = vehicleFill;
            so.FindProperty("alertBarFill").objectReferenceValue = alertFill;
            so.FindProperty("poiToggle").objectReferenceValue = poiToggle;
            so.FindProperty("boundaryToggle").objectReferenceValue = boundaryToggle;
            so.FindProperty("trajectoryToggle").objectReferenceValue = trajectoryToggle;
            so.FindProperty("alarmToggle").objectReferenceValue = alarmToggle;
            so.FindProperty("vehicleNameText").objectReferenceValue = vehicleName;
            so.FindProperty("vehicleStatusText").objectReferenceValue = vehicleStatus;
            so.FindProperty("vehicleSpeedText").objectReferenceValue = vehicleSpeed;
            so.FindProperty("vehicleLonText").objectReferenceValue = vehicleLon;
            so.FindProperty("vehicleLatText").objectReferenceValue = vehicleLat;
            so.FindProperty("vehicleStatusDot").objectReferenceValue = vehicleDot;
            so.FindProperty("alarmListContent").objectReferenceValue = alarmContent;
            so.FindProperty("alarmItemPrefab").objectReferenceValue = alarmPrefab.GetComponent<AlarmItemView>();
            so.FindProperty("emptyState").objectReferenceValue = emptyState;
            so.ApplyModifiedPropertiesWithoutUndo();
            return canvasObject;
        }

        private static void BuildHeader(Transform canvas, Sprite sprite, Font font, out Text timeText)
        {
            GameObject header = Panel("Header", canvas, sprite, UiWidgets.HeaderBg, false);
            RectTransform rect = header.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, 50f);

            Image edge = CreateImage("BottomEdge", header.transform, sprite, new Color(UiWidgets.Accent.r, UiWidgets.Accent.g, UiWidgets.Accent.b, 0.22f));
            StretchBottom(edge.rectTransform, 1f);
            Image titleLine = CreateImage("TitleLine", header.transform, sprite, new Color(UiWidgets.Accent.r, UiWidgets.Accent.g, UiWidgets.Accent.b, 0.85f));
            RectTransform lineRect = titleLine.rectTransform;
            lineRect.anchorMin = new Vector2(0.5f, 0f);
            lineRect.anchorMax = new Vector2(0.5f, 0f);
            lineRect.pivot = new Vector2(0.5f, 0f);
            lineRect.anchoredPosition = new Vector2(0f, 7f);
            lineRect.sizeDelta = new Vector2(240f, 1.5f);

            Text province = Label("ProvinceText", header.transform, font, "河北省", 13, UiWidgets.Muted, TextAnchor.MiddleLeft);
            province.color = new Color(UiWidgets.Muted.r, UiWidgets.Muted.g, UiWidgets.Muted.b, 0.55f);
            TopLeft(province.rectTransform, new Vector2(18f, 0f), new Vector2(180f, 50f));

            Text title = Label("TitleText", header.transform, font, "河北智慧交通监测", 28, UiWidgets.Title, TextAnchor.MiddleCenter);
            title.fontStyle = FontStyle.Bold;
            RectTransform titleRect = title.rectTransform;
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(280f, 8f);
            titleRect.offsetMax = new Vector2(-280f, 0f);

            timeText = Label("TimeText", header.transform, font, "00:00:00  2026-01-01", 14, UiWidgets.Muted, TextAnchor.MiddleRight);
            timeText.color = new Color(UiWidgets.Muted.r, UiWidgets.Muted.g, UiWidgets.Muted.b, 0.55f);
            RectTransform timeRect = timeText.rectTransform;
            timeRect.anchorMin = new Vector2(1f, 0f);
            timeRect.anchorMax = new Vector2(1f, 1f);
            timeRect.pivot = new Vector2(1f, 0.5f);
            timeRect.anchoredPosition = new Vector2(-18f, 0f);
            timeRect.sizeDelta = new Vector2(260f, 0f);
        }

        private static void BuildLeft(
            Transform canvas,
            Sprite sprite,
            Font font,
            out Text poiCount,
            out Text onlineCount,
            out Text alarmCount,
            out Text projectCount,
            out Text cameraCount,
            out Text vehicleStatCount,
            out Text alertStatCount,
            out Image projectFill,
            out Image cameraFill,
            out Image vehicleFill,
            out Image alertFill,
            out Toggle poiToggle,
            out Toggle boundaryToggle,
            out Toggle trajectoryToggle,
            out Toggle alarmToggle)
        {
            GameObject left = SidePanel("LeftPanel", canvas, sprite, true);
            SectionTitle("MonitorTitle", left.transform, sprite, font, "监测总览", new Vector2(14f, -12f), 310f);

            GameObject overview = Section("OverviewSection", left.transform, new Vector2(14f, -42f), new Vector2(310f, 148f));
            StatCard("RegionCard", overview.transform, sprite, font, new Vector2(0f, 0f), "当前区域", "河北省", UiWidgets.Title);
            StatCard("PoiCountCard", overview.transform, sprite, font, new Vector2(159f, 0f), "POI总数", "0", UiWidgets.Number);
            StatCard("OnlineCountCard", overview.transform, sprite, font, new Vector2(0f, -78f), "在线设备", "0", UiWidgets.Ok);
            StatCard("AlarmCountCard", overview.transform, sprite, font, new Vector2(159f, -78f), "告警数量", "0", UiWidgets.Danger);
            poiCount = overview.transform.Find("PoiCountCard/Value").GetComponent<Text>();
            onlineCount = overview.transform.Find("OnlineCountCard/Value").GetComponent<Text>();
            alarmCount = overview.transform.Find("AlarmCountCard/Value").GetComponent<Text>();

            GameObject stats = Section("PoiStatisticsSection", left.transform, new Vector2(14f, -208f), new Vector2(310f, 172f));
            SectionTitle("StatsTitle", stats.transform, sprite, font, "POI 分类统计", new Vector2(0f, 0f), 310f);
            BarRow("ProjectRow", stats.transform, sprite, font, new Vector2(0f, -32f), "项目", new Color(0.22f, 0.55f, 1f, 1f), out projectCount, out projectFill);
            BarRow("CameraRow", stats.transform, sprite, font, new Vector2(0f, -70f), "摄像头", new Color(0.18f, 0.88f, 0.95f, 1f), out cameraCount, out cameraFill);
            BarRow("VehicleRow", stats.transform, sprite, font, new Vector2(0f, -108f), "车辆", new Color(0.45f, 0.42f, 0.98f, 1f), out vehicleStatCount, out vehicleFill);
            BarRow("AlarmRow", stats.transform, sprite, font, new Vector2(0f, -146f), "告警点", new Color(1f, 0.28f, 0.34f, 1f), out alertStatCount, out alertFill);

            GameObject layers = Section("LayerSection", left.transform, new Vector2(14f, -398f), new Vector2(310f, 160f));
            SectionTitle("LayerTitle", layers.transform, sprite, font, "图层控制", new Vector2(0f, 0f), 310f);
            poiToggle = LayerToggle("PoiToggle", layers.transform, sprite, font, new Vector2(0f, -34f), "POI");
            boundaryToggle = LayerToggle("BoundaryToggle", layers.transform, sprite, font, new Vector2(0f, -66f), "河北边界");
            trajectoryToggle = LayerToggle("TrajectoryToggle", layers.transform, sprite, font, new Vector2(0f, -98f), "车辆轨迹");
            alarmToggle = LayerToggle("AlarmToggle", layers.transform, sprite, font, new Vector2(0f, -130f), "告警");
        }

        private static void BuildRight(
            Transform canvas,
            Sprite sprite,
            Font font,
            out Text vehicleName,
            out Text vehicleStatus,
            out Text vehicleSpeed,
            out Text vehicleLon,
            out Text vehicleLat,
            out Image vehicleDot,
            out Transform alarmContent,
            out GameObject emptyState)
        {
            GameObject right = SidePanel("RightPanel", canvas, sprite, false);
            SectionTitle("MonitorTitle", right.transform, sprite, font, "实时监测", new Vector2(14f, -12f), 310f);

            GameObject vehicle = Section("VehicleSection", right.transform, new Vector2(14f, -48f), new Vector2(310f, 168f));
            SectionTitle("VehicleTitle", vehicle.transform, sprite, font, "当前车辆", new Vector2(0f, 0f), 310f);
            GameObject vehicleCard = Panel("VehicleCard", vehicle.transform, sprite, UiWidgets.PanelInner, true);
            TopLeft(vehicleCard.GetComponent<RectTransform>(), new Vector2(0f, -32f), new Vector2(310f, 136f));
            vehicleName = Kv(vehicleCard.transform, sprite, font, "NameRow", 8f, "名称");
            vehicleStatus = Kv(vehicleCard.transform, sprite, font, "StatusRow", 32f, "状态");
            vehicleDot = CreateImage("StatusDot", vehicleCard.transform, sprite, UiWidgets.Ok);
            TopLeft(vehicleDot.rectTransform, new Vector2(86f, -39f), new Vector2(7f, 7f));
            vehicleSpeed = Kv(vehicleCard.transform, sprite, font, "SpeedRow", 56f, "速度");
            vehicleLon = Kv(vehicleCard.transform, sprite, font, "LonRow", 80f, "经度");
            vehicleLat = Kv(vehicleCard.transform, sprite, font, "LatRow", 104f, "纬度");

            GameObject alarms = Section("AlarmSection", right.transform, new Vector2(14f, -230f), new Vector2(310f, 210f));
            SectionTitle("AlarmTitle", alarms.transform, sprite, font, "告警信息", new Vector2(0f, 0f), 310f);
            GameObject list = new GameObject("AlarmListContent", typeof(RectTransform));
            list.transform.SetParent(alarms.transform, false);
            TopLeft(list.GetComponent<RectTransform>(), new Vector2(0f, -32f), new Vector2(310f, 176f));
            VerticalLayoutGroup layout = list.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 6f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            alarmContent = list.transform;

            GameObject selected = Section("SelectedPoiSection", right.transform, new Vector2(14f, -456f), new Vector2(310f, 220f));
            SectionTitle("SelectedTitle", selected.transform, sprite, font, "当前选中", new Vector2(0f, 0f), 310f);
            Color selectedCardBg = new Color(10f / 255f, 45f / 255f, 70f / 255f, 110f / 255f);
            GameObject selectedCard = Panel("SelectedCard", selected.transform, sprite, selectedCardBg, true);
            TopLeft(selectedCard.GetComponent<RectTransform>(), new Vector2(0f, -32f), new Vector2(310f, 188f));

            emptyState = new GameObject("EmptyState", typeof(RectTransform));
            emptyState.transform.SetParent(selectedCard.transform, false);
            Stretch(emptyState.GetComponent<RectTransform>());
            Image emptyLine = CreateImage("HintLine", emptyState.transform, sprite, new Color(UiWidgets.Accent.r, UiWidgets.Accent.g, UiWidgets.Accent.b, 0.16f));
            TopLeft(emptyLine.rectTransform, new Vector2(90f, -62f), new Vector2(130f, 1f));
            Text emptyTitle = Label("EmptyTitle", emptyState.transform, font, "未选择监测对象", 16, UiWidgets.Muted, TextAnchor.MiddleCenter);
            emptyTitle.fontStyle = FontStyle.Bold;
            emptyTitle.color = new Color(0.48f, 0.62f, 0.72f, 0.50f);
            TopLeft(emptyTitle.rectTransform, new Vector2(16f, -74f), new Vector2(278f, 28f));
            Text emptyHint = Label("EmptyHint", emptyState.transform, font, "点击地图 POI 查看详情", 14, UiWidgets.Muted, TextAnchor.MiddleCenter);
            emptyHint.color = new Color(0.42f, 0.55f, 0.64f, 0.40f);
            TopLeft(emptyHint.rectTransform, new Vector2(16f, -104f), new Vector2(278f, 24f));

            GameObject detail = new GameObject("PoiDetail", typeof(RectTransform));
            detail.transform.SetParent(selectedCard.transform, false);
            Stretch(detail.GetComponent<RectTransform>());
            Button close = GhostButton("CloseButton", detail.transform, sprite, font, "关闭", new Vector2(242f, -8f), new Vector2(52f, 24f));
            Text nameValue = Kv(detail.transform, sprite, font, "NameRow", 10f, "名称");
            Text typeValue = Kv(detail.transform, sprite, font, "TypeRow", 36f, "类型");
            Text statusValue = Kv(detail.transform, sprite, font, "StatusRow", 62f, "状态");
            Text coordValue = Kv(detail.transform, sprite, font, "CoordRow", 88f, "经纬度");
            Text descLabel = Label("DescLabel", detail.transform, font, "描述", 14, UiWidgets.Label, TextAnchor.MiddleLeft);
            TopLeft(descLabel.rectTransform, new Vector2(12f, -114f), new Vector2(64f, 20f));
            Text descValue = Label("DescText", detail.transform, font, "", 14, UiWidgets.Title, TextAnchor.UpperLeft);
            TopLeft(descValue.rectTransform, new Vector2(12f, -136f), new Vector2(286f, 22f));
            Button locate = GhostButton("LocateButton", detail.transform, sprite, font, "定位", new Vector2(12f, -160f), new Vector2(72f, 24f));
            detail.SetActive(false);

            GeoInfoPanel info = selected.AddComponent<GeoInfoPanel>();
            SerializedObject so = new SerializedObject(info);
            so.FindProperty("root").objectReferenceValue = detail;
            so.FindProperty("nameText").objectReferenceValue = nameValue;
            so.FindProperty("typeText").objectReferenceValue = typeValue;
            so.FindProperty("statusText").objectReferenceValue = statusValue;
            so.FindProperty("coordText").objectReferenceValue = coordValue;
            so.FindProperty("descText").objectReferenceValue = descValue;
            so.FindProperty("closeButton").objectReferenceValue = close;
            so.FindProperty("locateButton").objectReferenceValue = locate;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void BuildTrajectory(Transform canvas, Sprite sprite, Font font)
        {
            GameObject root = Panel("TrajectoryPanel", canvas, sprite, UiWidgets.PanelBg, true);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = new Vector2(0f, 32f);
            rect.sizeDelta = new Vector2(760f, 72f);

            Button play = HudButton("PlayButton", root.transform, sprite, font, "播放", new Vector2(14f, 32f), new Vector2(56f, 26f), UiWidgets.Ok);
            Button pause = HudButton("PauseButton", root.transform, sprite, font, "暂停", new Vector2(76f, 32f), new Vector2(56f, 26f), UiWidgets.Warning);
            Button stop = HudButton("StopButton", root.transform, sprite, font, "停止", new Vector2(138f, 32f), new Vector2(56f, 26f), UiWidgets.Danger);
            Button x1 = HudButton("Speed1Button", root.transform, sprite, font, "1x", new Vector2(208f, 32f), new Vector2(40f, 26f), UiWidgets.Title);
            Button x2 = HudButton("Speed2Button", root.transform, sprite, font, "2x", new Vector2(252f, 32f), new Vector2(40f, 26f), UiWidgets.Title);
            Button x4 = HudButton("Speed4Button", root.transform, sprite, font, "4x", new Vector2(296f, 32f), new Vector2(40f, 26f), UiWidgets.Title);
            Toggle follow = LayerToggle("FollowToggle", root.transform, sprite, font, new Vector2(352f, -14f), "跟随车辆");
            follow.isOn = false;
            BottomLeft(follow.GetComponent<RectTransform>(), new Vector2(352f, 32f), new Vector2(130f, 26f));

            Text time = Label("TimeText", root.transform, font, "时间  --:--:--", 15, UiWidgets.Muted, TextAnchor.MiddleLeft);
            BottomLeft(time.rectTransform, new Vector2(490f, 32f), new Vector2(130f, 26f));
            Text speed = Label("SpeedText", root.transform, font, "速度  -- km/h", 15, UiWidgets.Title, TextAnchor.MiddleLeft);
            BottomLeft(speed.rectTransform, new Vector2(620f, 32f), new Vector2(128f, 26f));

            Slider slider = CreateSlider("ProgressBar", root.transform, sprite, new Vector2(14f, 10f), new Vector2(732f, 12f));

            TrajectoryPanel panel = root.AddComponent<TrajectoryPanel>();
            SerializedObject so = new SerializedObject(panel);
            so.FindProperty("playButton").objectReferenceValue = play;
            so.FindProperty("pauseButton").objectReferenceValue = pause;
            so.FindProperty("stopButton").objectReferenceValue = stop;
            so.FindProperty("speed1Button").objectReferenceValue = x1;
            so.FindProperty("speed2Button").objectReferenceValue = x2;
            so.FindProperty("speed4Button").objectReferenceValue = x4;
            so.FindProperty("followToggle").objectReferenceValue = follow;
            so.FindProperty("timeText").objectReferenceValue = time;
            so.FindProperty("speedText").objectReferenceValue = speed;
            so.FindProperty("progressBar").objectReferenceValue = slider;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject BuildAlarmItem(Sprite sprite, Font font)
        {
            GameObject item = Panel("AlarmItem", null, sprite, UiWidgets.PanelInner, true);
            RectTransform rect = item.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(310f, 42f);
            LayoutElement layout = item.AddComponent<LayoutElement>();
            layout.minHeight = 42f;
            layout.preferredHeight = 42f;
            Image mark = CreateImage("StatusMark", item.transform, sprite, UiWidgets.Danger);
            TopLeft(mark.rectTransform, Vector2.zero, new Vector2(3f, 42f));
            Text time = Label("TimeText", item.transform, font, "00:00:00", 12, UiWidgets.Muted, TextAnchor.MiddleLeft);
            TopLeft(time.rectTransform, new Vector2(12f, -3f), new Vector2(90f, 16f));
            Text type = Label("TypeText", item.transform, font, "告警", 14, UiWidgets.Title, TextAnchor.MiddleLeft);
            type.fontStyle = FontStyle.Bold;
            TopLeft(type.rectTransform, new Vector2(12f, -20f), new Vector2(170f, 18f));
            Text state = Label("StateText", item.transform, font, "未处理", 13, UiWidgets.Danger, TextAnchor.MiddleRight);
            state.fontStyle = FontStyle.Bold;
            TopLeft(state.rectTransform, new Vector2(196f, -12f), new Vector2(102f, 20f));

            AlarmItemView view = item.AddComponent<AlarmItemView>();
            SerializedObject so = new SerializedObject(view);
            so.FindProperty("timeText").objectReferenceValue = time;
            so.FindProperty("typeText").objectReferenceValue = type;
            so.FindProperty("stateText").objectReferenceValue = state;
            so.FindProperty("statusMark").objectReferenceValue = mark;
            so.ApplyModifiedPropertiesWithoutUndo();
            return item;
        }

        private static GameObject SidePanel(string name, Transform parent, Sprite sprite, bool left)
        {
            GameObject panel = Panel(name, parent, sprite, UiWidgets.PanelBg, true);
            RectTransform rect = panel.GetComponent<RectTransform>();
            if (left)
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 1f);
                rect.pivot = new Vector2(0f, 1f);
                rect.offsetMin = new Vector2(14f, 16f);
                rect.offsetMax = new Vector2(352f, -58f);
            }
            else
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(1f, 1f);
                rect.offsetMin = new Vector2(-352f, 16f);
                rect.offsetMax = new Vector2(-14f, -58f);
            }

            return panel;
        }

        private static GameObject Section(string name, Transform parent, Vector2 pos, Vector2 size)
        {
            GameObject section = new GameObject(name, typeof(RectTransform));
            section.transform.SetParent(parent, false);
            TopLeft(section.GetComponent<RectTransform>(), pos, size);
            return section;
        }

        private static void SectionTitle(string name, Transform parent, Sprite sprite, Font font, string text, Vector2 pos, float width)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            TopLeft(root.GetComponent<RectTransform>(), pos, new Vector2(width, 24f));
            Image mark = CreateImage("Mark", root.transform, sprite, UiWidgets.Accent);
            TopLeft(mark.rectTransform, new Vector2(0f, -4f), new Vector2(3f, 16f));
            Text title = Label("Title", root.transform, font, text, 17, UiWidgets.Title, TextAnchor.MiddleLeft);
            title.fontStyle = FontStyle.Bold;
            TopLeft(title.rectTransform, new Vector2(10f, 0f), new Vector2(140f, 22f));
            Image line = CreateImage("Line", root.transform, sprite, new Color(UiWidgets.Accent.r, UiWidgets.Accent.g, UiWidgets.Accent.b, 0.16f));
            TopLeft(line.rectTransform, new Vector2(150f, -13f), new Vector2(Mathf.Max(24f, width - 150f), 1f));
        }

        private static Text StatCard(string name, Transform parent, Sprite sprite, Font font, Vector2 pos, string label, string value, Color valueColor)
        {
            GameObject card = Panel(name, parent, sprite, UiWidgets.PanelInner, true);
            TopLeft(card.GetComponent<RectTransform>(), pos, new Vector2(151f, 70f));
            Text title = Label("Label", card.transform, font, label, 12, UiWidgets.Label, TextAnchor.MiddleLeft);
            TopLeft(title.rectTransform, new Vector2(10f, -6f), new Vector2(131f, 18f));
            Text number = Label("Value", card.transform, font, value, 24, valueColor, TextAnchor.MiddleLeft);
            number.fontStyle = FontStyle.Bold;
            TopLeft(number.rectTransform, new Vector2(10f, -28f), new Vector2(131f, 34f));
            return number;
        }

        private static void BarRow(string name, Transform parent, Sprite sprite, Font font, Vector2 pos, string label, Color fillColor, out Text count, out Image fill)
        {
            GameObject row = new GameObject(name, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            TopLeft(row.GetComponent<RectTransform>(), pos, new Vector2(310f, 34f));
            Text title = Label("Label", row.transform, font, label, 14, UiWidgets.Label, TextAnchor.MiddleLeft);
            TopLeft(title.rectTransform, Vector2.zero, new Vector2(80f, 18f));
            count = Label("Count", row.transform, font, "0", 15, UiWidgets.Number, TextAnchor.MiddleRight);
            count.fontStyle = FontStyle.Bold;
            TopLeft(count.rectTransform, new Vector2(270f, 0f), new Vector2(40f, 18f));
            Image track = CreateImage("BarTrack", row.transform, sprite, UiWidgets.BarTrack);
            TopLeft(track.rectTransform, new Vector2(0f, -22f), new Vector2(310f, 7f));
            fill = CreateImage("BarFill", track.transform, sprite, fillColor);
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0.5f;
            Stretch(fill.rectTransform);
        }

        private static Toggle LayerToggle(string name, Transform parent, Sprite sprite, Font font, Vector2 pos, string label)
        {
            GameObject row = Panel(name, parent, sprite, Color.white, false);
            TopLeft(row.GetComponent<RectTransform>(), pos, new Vector2(310f, 28f));
            Image rowImage = row.GetComponent<Image>();
            Toggle toggle = row.AddComponent<Toggle>();
            toggle.isOn = true;
            ColorBlock colors = toggle.colors;
            colors.normalColor = new Color(1f, 1f, 1f, 0.03f);
            colors.highlightedColor = new Color(0.22f, 0.78f, 0.95f, 0.12f);
            colors.pressedColor = new Color(0.22f, 0.78f, 0.95f, 0.18f);
            colors.selectedColor = new Color(1f, 1f, 1f, 0.04f);
            colors.fadeDuration = 0.08f;
            toggle.colors = colors;
            toggle.targetGraphic = rowImage;

            Image box = CreateImage("Background", row.transform, sprite, new Color(0.08f, 0.16f, 0.24f, 0.95f));
            RectTransform boxRect = box.rectTransform;
            boxRect.anchorMin = new Vector2(0f, 0.5f);
            boxRect.anchorMax = new Vector2(0f, 0.5f);
            boxRect.pivot = new Vector2(0f, 0.5f);
            boxRect.anchoredPosition = new Vector2(6f, 0f);
            boxRect.sizeDelta = new Vector2(14f, 14f);
            AddOutline(box.gameObject, new Color(UiWidgets.Accent.r, UiWidgets.Accent.g, UiWidgets.Accent.b, 0.45f));
            Image check = CreateImage("Checkmark", box.transform, sprite, UiWidgets.Accent);
            Stretch(check.rectTransform);
            check.rectTransform.offsetMin = new Vector2(2f, 2f);
            check.rectTransform.offsetMax = new Vector2(-2f, -2f);
            toggle.graphic = check;

            Text text = Label("Label", row.transform, font, label, 15, UiWidgets.Title, TextAnchor.MiddleLeft);
            RectTransform textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(28f, 0f);
            textRect.offsetMax = Vector2.zero;
            return toggle;
        }

        private static Text Kv(Transform parent, Sprite sprite, Font font, string rowName, float y, string key)
        {
            GameObject row = new GameObject(rowName, typeof(RectTransform));
            row.transform.SetParent(parent, false);
            TopLeft(row.GetComponent<RectTransform>(), new Vector2(12f, -y), new Vector2(286f, 22f));
            Text label = Label("Label", row.transform, font, key, 14, UiWidgets.Label, TextAnchor.MiddleLeft);
            TopLeft(label.rectTransform, Vector2.zero, new Vector2(64f, 22f));
            Text value = Label("Value", row.transform, font, "--", 14, UiWidgets.Title, TextAnchor.MiddleLeft);
            TopLeft(value.rectTransform, new Vector2(88f, 0f), new Vector2(198f, 22f));
            return value;
        }

        private static Button HudButton(string name, Transform parent, Sprite sprite, Font font, string text, Vector2 pos, Vector2 size, Color labelColor)
        {
            GameObject go = Panel(name, parent, sprite, UiWidgets.ButtonBg, false);
            BottomLeft(go.GetComponent<RectTransform>(), pos, size);
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            Text label = Label("Text", go.transform, font, text, 14, labelColor, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            return button;
        }

        private static Button GhostButton(string name, Transform parent, Sprite sprite, Font font, string text, Vector2 pos, Vector2 size)
        {
            GameObject go = Panel(name, parent, sprite, new Color(0.05f, 0.22f, 0.36f, 0.22f), true);
            TopLeft(go.GetComponent<RectTransform>(), pos, size);
            AddOutline(go, new Color(UiWidgets.Accent.r, UiWidgets.Accent.g, UiWidgets.Accent.b, 0.55f));
            Button button = go.AddComponent<Button>();
            button.targetGraphic = go.GetComponent<Image>();
            Text label = Label("Text", go.transform, font, text, 14, UiWidgets.Title, TextAnchor.MiddleCenter);
            Stretch(label.rectTransform);
            return button;
        }

        private static Slider CreateSlider(string name, Transform parent, Sprite sprite, Vector2 pos, Vector2 size)
        {
            GameObject root = new GameObject(name, typeof(RectTransform));
            root.transform.SetParent(parent, false);
            BottomLeft(root.GetComponent<RectTransform>(), pos, size);
            Slider slider = root.AddComponent<Slider>();
            Image bg = CreateImage("Background", root.transform, sprite, new Color(0.10f, 0.18f, 0.26f, 0.95f));
            Stretch(bg.rectTransform);
            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            fillArea.transform.SetParent(root.transform, false);
            Stretch(fillArea.GetComponent<RectTransform>());
            Image fill = CreateImage("Fill", fillArea.transform, sprite, UiWidgets.Accent);
            Stretch(fill.rectTransform);
            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            handleArea.transform.SetParent(root.transform, false);
            Stretch(handleArea.GetComponent<RectTransform>());
            Image handle = CreateImage("Handle", handleArea.transform, sprite, UiWidgets.Title);
            handle.rectTransform.sizeDelta = new Vector2(12f, 0f);
            slider.fillRect = fill.rectTransform;
            slider.handleRect = handle.rectTransform;
            slider.targetGraphic = handle;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static GameObject Panel(string name, Transform parent, Sprite sprite, Color color, bool outline)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            if (parent != null)
            {
                go.transform.SetParent(parent, false);
            }

            UnityEngine.UI.Image image = go.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = UnityEngine.UI.Image.Type.Simple;
            if (outline)
            {
                AddOutline(go, UiWidgets.Border);
            }

            return go;
        }

        private static UnityEngine.UI.Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            UnityEngine.UI.Image image = go.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Text Label(string name, Transform parent, Font font, string text, int size, Color color, TextAnchor align)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            Text label = go.AddComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.raycastTarget = false;
            return label;
        }

        private static void AddOutline(GameObject go, Color color)
        {
            Outline outline = go.AddComponent<Outline>();
            outline.effectColor = color;
            outline.effectDistance = new Vector2(1f, -1f);
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void StretchBottom(RectTransform rect, float height)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(0f, height);
        }

        private static void TopLeft(RectTransform rect, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }

        private static void BottomLeft(RectTransform rect, Vector2 pos, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
        }
    }
}
