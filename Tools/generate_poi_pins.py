#!/usr/bin/env python3
"""Write simple map-pin PNGs so Unity has fallback icons without WebGIS assets."""
import struct
import zlib
from pathlib import Path

OUT = Path(r"d:\game1\GeoVision\Assets\Resources\PoiIcons")
SIZE = 64

COLORS = {
    "project": (51, 140, 255),
    "camera": (38, 217, 242),
    "alert": (255, 46, 46),
    "monitor": (51, 230, 89),
    "vehicle": (26, 115, 255),
}


def png_rgba(pixels):
    raw = b"".join(b"\x00" + bytes(row) for row in pixels)

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(
            ">I", zlib.crc32(tag + data) & 0xFFFFFFFF
        )

    ihdr = struct.pack(">IIBBBBB", SIZE, SIZE, 8, 6, 0, 0, 0)
    return b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", ihdr) + chunk(b"IDAT", zlib.compress(raw, 9)) + chunk(b"IEND", b"")


def make_pin(rgb):
    r, g, b = rgb
    rows = []
    head = (31.5, 40.0)
    for y in range(SIZE):
        row = bytearray()
        for x in range(SIZE):
            dx = x - head[0]
            dy = y - head[1]
            head_dist = (dx * dx + dy * dy) ** 0.5
            in_head = head_dist <= 16.5
            in_hole = head_dist <= 6.5
            in_tip = y < 28 and abs(x - 31.5) < (28 - y) * 0.42 and y > 6
            if in_hole:
                row.extend((255, 255, 255, 255))
            elif in_head or in_tip:
                row.extend((r, g, b, 255))
            else:
                row.extend((0, 0, 0, 0))
        rows.append(row)
    return png_rgba(rows)


def main():
    OUT.mkdir(parents=True, exist_ok=True)
    for name, color in COLORS.items():
        path = OUT / f"{name}.png"
        path.write_bytes(make_pin(color))
        print("wrote", path, path.stat().st_size, "bytes")


if __name__ == "__main__":
    main()
