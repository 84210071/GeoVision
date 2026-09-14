#!/usr/bin/env python3
"""Generate a fictional inspection-vehicle trajectory inside Hebei province."""
import json
import math
from datetime import datetime, timedelta, timezone
from pathlib import Path

OUT = Path(r"d:\game1\GeoVision\Assets\Resources\Data\trajectory.json")

# Hebei cities only. Avoids Beijing / Tianjin / Shandong.
WAYPOINTS = [
    (114.5149, 38.0428),  # Shijiazhuang
    (114.5702, 38.1478),  # Zhengding
    (114.9900, 38.5160),  # Dingzhou
    (115.1470, 38.6970),  # Wangdu
    (115.4640, 38.8740),  # Baoding
    (115.8600, 38.9400),  # Anxin / Xiong'an south
    (116.0990, 38.7060),  # Renqiu
    (116.0990, 38.4460),  # Hejian
    (116.8390, 38.3040),  # Cangzhou
    (116.5780, 38.0740),  # Botou
    (115.6700, 37.7390),  # Hengshui
    (115.5790, 37.5510),  # Jizhou
    (115.3890, 37.3590),  # Nangong
    (114.5040, 37.0680),  # Xingtai
    (114.5120, 37.2870),  # Neiqiu
    (114.5790, 37.6160),  # Gaoyi
    (114.5149, 38.0428),  # back to Shijiazhuang
]


def haversine_m(lon1, lat1, lon2, lat2):
    r = 6371000.0
    p1, p2 = math.radians(lat1), math.radians(lat2)
    dphi = math.radians(lat2 - lat1)
    dlmb = math.radians(lon2 - lon1)
    a = math.sin(dphi / 2) ** 2 + math.cos(p1) * math.cos(p2) * math.sin(dlmb / 2) ** 2
    return 2 * r * math.asin(math.sqrt(a))


def lerp(a, b, t):
    return a + (b - a) * t


def densify(waypoints, target_count=44):
    lengths = []
    total = 0.0
    for i in range(len(waypoints) - 1):
        d = haversine_m(waypoints[i][0], waypoints[i][1], waypoints[i + 1][0], waypoints[i + 1][1])
        lengths.append(d)
        total += d

    pts = [waypoints[0]]
    remaining = target_count - 1
    for i, length in enumerate(lengths):
        share = max(1, int(round(remaining * (length / total)))) if i < len(lengths) - 1 else remaining
        share = max(1, share)
        lon1, lat1 = waypoints[i]
        lon2, lat2 = waypoints[i + 1]
        for s in range(1, share + 1):
            t = s / share
            pts.append((lerp(lon1, lon2, t), lerp(lat1, lat2, t)))
        remaining -= share
        total -= length
        if remaining <= 0 and i < len(lengths) - 1:
            remaining = 1
    return pts


def main():
    coords = densify(WAYPOINTS, 44)
    start = datetime(2026, 9, 14, 8, 0, 0, tzinfo=timezone.utc)
    seconds_per_point = 14.0
    points = []
    for i, (lon, lat) in enumerate(coords):
        speed = 18.0 + (i % 6) * 1.6
        ts = start + timedelta(seconds=i * seconds_per_point)
        points.append(
            {
                "longitude": round(lon, 6),
                "latitude": round(lat, 6),
                "height": 48.0 + (i % 4) * 2.0,
                "timestamp": ts.strftime("%Y-%m-%dT%H:%M:%SZ"),
                "speed": round(speed, 2),
            }
        )

    lons = [p["longitude"] for p in points]
    lats = [p["latitude"] for p in points]
    payload = {
        "vehicleId": "vehicle-patrol-01",
        "name": "巡检车-01",
        "points": points,
    }
    OUT.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
    print(
        "points",
        len(points),
        "lon",
        min(lons),
        max(lons),
        "lat",
        min(lats),
        max(lats),
        "duration_s",
        (len(points) - 1) * seconds_per_point,
        "->",
        OUT,
    )


if __name__ == "__main__":
    main()
