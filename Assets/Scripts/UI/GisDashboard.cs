using System.Globalization;
using GeoVision.Data;
using UnityEngine;
using UnityEngine.UI;

namespace GeoVision.Map
{
    public class GisDashboard : MonoBehaviour
    {
        private static readonly string[] MockAlarmTimes = { "20:14:32", "20:11:08", "20:03:55", "19:58:12" };
        private static readonly string[] MockAlarmTypes = { "基坑位移", "围挡倾斜", "扬尘超标", "塔吊限位" };
        private static readonly string[] MockAlarmStates = { "未处理", "处理中", "已确认", "未处理" };

        [Header("Header")]
        [SerializeField] private Text timeText;

        [Header("Overview")]
        [SerializeField] private Text poiCountText;
        [SerializeField] private Text onlineCountText;
        [SerializeField] private Text alarmCountText;

        [Header("POI Statistics")]
        [SerializeField] private Text projectCountText;
        [SerializeField] private Text cameraCountText;
        [SerializeField] private Text vehicleStatCountText;
        [SerializeField] private Text alertStatCountText;
        [SerializeField] private Image projectBarFill;
        [SerializeField] private Image cameraBarFill;
        [SerializeField] private Image vehicleBarFill;
        [SerializeField] private Image alertBarFill;

        [Header("Layers")]
        [SerializeField] private Toggle poiToggle;
        [SerializeField] private Toggle boundaryToggle;
        [SerializeField] private Toggle trajectoryToggle;
        [SerializeField] private Toggle alarmToggle;

        [Header("Vehicle")]
        [SerializeField] private Text vehicleNameText;
        [SerializeField] private Text vehicleStatusText;
        [SerializeField] private Text vehicleSpeedText;
        [SerializeField] private Text vehicleLonText;
        [SerializeField] private Text vehicleLatText;
        [SerializeField] private Image vehicleStatusDot;

        [Header("Alarms")]
        [SerializeField] private Transform alarmListContent;
        [SerializeField] private AlarmItemView alarmItemPrefab;

        [Header("Selected")]
        [SerializeField] private GameObject emptyState;

        private PoiSpawner _spawner;
        private LayerManager _layers;
        private TrajectoryPlayer _player;
        private PickController _pick;
        private bool _wired;
        private float _clockTimer;

        public void Bind(PoiSpawner spawner, LayerManager layers, TrajectoryPlayer player, PickController pick)
        {
            _spawner = spawner;
            _layers = layers;
            _player = player;
            _pick = pick;
            if (_player != null)
            {
                _player.ProgressChanged -= RefreshVehicle;
                _player.ProgressChanged += RefreshVehicle;
            }

            if (_pick != null)
            {
                _pick.SelectionChanged -= OnSelectionChanged;
                _pick.SelectionChanged += OnSelectionChanged;
            }

            WireEvents();
            RefreshStats();
            RefreshVehicle();
            SpawnAlarms();
            OnSelectionChanged(_pick != null ? _pick.Current : null);
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.ProgressChanged -= RefreshVehicle;
            }

            if (_pick != null)
            {
                _pick.SelectionChanged -= OnSelectionChanged;
            }
        }

        private void Update()
        {
            _clockTimer += Time.deltaTime;
            if (_clockTimer < 1f || timeText == null)
            {
                return;
            }

            _clockTimer = 0f;
            timeText.text = System.DateTime.Now.ToString("HH:mm:ss  yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private void WireEvents()
        {
            if (_wired)
            {
                return;
            }

            _wired = true;
            Listen(poiToggle, on => { if (_layers != null) { _layers.SetPoi(on); } });
            Listen(boundaryToggle, on => { if (_layers != null) { _layers.SetBoundary(on); } });
            Listen(trajectoryToggle, on => { if (_layers != null) { _layers.SetTrajectory(on); } });
            Listen(alarmToggle, on => { if (_layers != null) { _layers.SetAlert(on); } });
        }

        private static void Listen(Toggle toggle, UnityEngine.Events.UnityAction<bool> handler)
        {
            if (toggle == null)
            {
                return;
            }

            toggle.onValueChanged.RemoveListener(handler);
            toggle.onValueChanged.AddListener(handler);
        }

        private void SpawnAlarms()
        {
            if (alarmListContent == null || alarmItemPrefab == null)
            {
                return;
            }

            for (int i = alarmListContent.childCount - 1; i >= 0; i--)
            {
                Destroy(alarmListContent.GetChild(i).gameObject);
            }

            for (int i = 0; i < MockAlarmTimes.Length; i++)
            {
                AlarmItemView item = Instantiate(alarmItemPrefab, alarmListContent);
                item.gameObject.SetActive(true);
                item.name = "AlarmItem_" + (i + 1);
                Color color = MockAlarmStates[i] == "未处理"
                    ? UiWidgets.Danger
                    : (MockAlarmStates[i] == "处理中" ? UiWidgets.Warning : UiWidgets.Ok);
                item.Set(MockAlarmTimes[i], MockAlarmTypes[i], MockAlarmStates[i], color);
            }
        }

        private void RefreshStats()
        {
            int total;
            int online;
            int alarms;
            int project;
            int camera;
            int vehicle;
            int alert;
            CountPois(out total, out online, out alarms, out project, out camera, out vehicle, out alert);
            SetText(poiCountText, total.ToString());
            SetText(onlineCountText, online.ToString());
            SetText(alarmCountText, alarms.ToString());
            SetText(projectCountText, project.ToString());
            SetText(cameraCountText, camera.ToString());
            SetText(vehicleStatCountText, vehicle.ToString());
            SetText(alertStatCountText, alert.ToString());
            int max = Mathf.Max(1, Mathf.Max(project, Mathf.Max(camera, Mathf.Max(vehicle, alert))));
            SetFill(projectBarFill, project / (float)max);
            SetFill(cameraBarFill, camera / (float)max);
            SetFill(vehicleBarFill, vehicle / (float)max);
            SetFill(alertBarFill, alert / (float)max);
        }

        private void CountPois(
            out int total,
            out int online,
            out int alarms,
            out int project,
            out int camera,
            out int vehicle,
            out int alert)
        {
            total = 0;
            online = 0;
            alarms = 0;
            project = 0;
            camera = 0;
            vehicle = 0;
            alert = 0;
            if (_spawner == null)
            {
                return;
            }

            for (int i = 0; i < _spawner.SpawnedPois.Count; i++)
            {
                GeoPoi poi = _spawner.SpawnedPois[i];
                GeoPointData data = poi != null ? poi.Data : null;
                if (data == null)
                {
                    continue;
                }

                total++;
                if (data.status != "offline" &&
                    (data.type == "camera" || data.type == "monitor" || data.type == "vehicle"))
                {
                    online++;
                }

                if (data.type == "alert" || data.status == "alarm")
                {
                    alarms++;
                }

                switch (data.type)
                {
                    case "project":
                        project++;
                        break;
                    case "camera":
                        camera++;
                        break;
                    case "vehicle":
                        vehicle++;
                        break;
                    case "alert":
                        alert++;
                        break;
                }
            }
        }

        private void RefreshVehicle()
        {
            GeoVehicle vehicle = _player != null ? _player.Vehicle : null;
            SetText(vehicleNameText, vehicle != null ? vehicle.DisplayName : "--");

            string status = "待命";
            Color statusColor = UiWidgets.Muted;
            if (_player != null && _player.IsPlaying)
            {
                status = "运行中";
                statusColor = UiWidgets.Ok;
            }
            else if (_player != null && _player.CurrentTime > 0.0)
            {
                status = "暂停";
                statusColor = UiWidgets.Warning;
            }

            SetText(vehicleStatusText, status);
            if (vehicleStatusText != null)
            {
                vehicleStatusText.color = statusColor;
            }

            if (vehicleStatusDot != null)
            {
                vehicleStatusDot.color = statusColor;
            }

            SetText(vehicleSpeedText, _player != null ? (_player.CurrentSpeed * 3.6).ToString("F1") + " km/h" : "--");
            SetText(vehicleLonText, vehicle != null ? vehicle.Longitude.ToString("F6") : "--");
            SetText(vehicleLatText, vehicle != null ? vehicle.Latitude.ToString("F6") : "--");
        }

        private void OnSelectionChanged(IGeoSelectable selectable)
        {
            if (emptyState != null)
            {
                emptyState.SetActive(selectable == null);
            }
        }

        private static void SetText(Text label, string value)
        {
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetFill(Image image, float amount)
        {
            if (image != null)
            {
                image.fillAmount = Mathf.Clamp01(amount);
            }
        }
    }
}
