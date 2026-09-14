from PIL import Image, ImageDraw
import os

out = r"d:\game1\GeoVision\Assets\Resources\PoiIcons"
os.makedirs(out, exist_ok=True)

styles = {
    "project": ((45, 139, 255, 230), (248, 248, 249, 255)),
    "camera": ((34, 211, 238, 230), (248, 248, 249, 255)),
    "alert": ((242, 28, 28, 230), (248, 248, 249, 255)),
    "monitor": ((34, 197, 94, 230), (248, 248, 249, 255)),
    "vehicle": ((2, 86, 255, 230), (255, 140, 95, 255)),
}

W, H = 128, 256


def draw_pin(name, fill, accent):
    img = Image.new("RGBA", (W, H), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    cx, cy, r = 64, 56, 44
    d.ellipse((cx - 28, H - 28, cx + 28, H - 8), outline=fill[:3] + (120,), width=2)
    d.ellipse((cx - 14, H - 22, cx + 14, H - 10), fill=fill[:3] + (180,))
    d.rectangle((cx - 3, cy + r - 8, cx + 3, H - 18), fill=fill)
    d.ellipse((cx - r, cy - r, cx + r, cy + r), fill=fill)
    d.ellipse((cx - r + 6, cy - r + 6, cx + r - 6, cy + r - 6), outline=accent, width=3)

    if name == "project":
        d.rectangle((cx - 14, cy - 4, cx + 14, cy + 16), fill=accent)
        d.polygon([(cx - 18, cy - 4), (cx, cy - 20), (cx + 18, cy - 4)], fill=accent)
    elif name == "camera":
        d.rectangle((cx - 16, cy - 8, cx + 10, cy + 10), fill=accent)
        d.ellipse((cx + 4, cy - 6, cx + 20, cy + 10), fill=accent)
        d.ellipse((cx - 8, cy - 4, cx + 4, cy + 8), fill=fill[:3] + (255,))
    elif name == "alert":
        d.polygon([(cx, cy - 20), (cx + 16, cy + 14), (cx - 16, cy + 14)], fill=accent)
        d.rectangle((cx - 3, cy - 8, cx + 3, cy + 4), fill=fill[:3] + (255,))
        d.ellipse((cx - 4, cy + 8, cx + 4, cy + 16), fill=fill[:3] + (255,))
    elif name == "monitor":
        d.ellipse((cx - 16, cy - 16, cx + 16, cy + 16), outline=accent, width=5)
        d.ellipse((cx - 6, cy - 6, cx + 6, cy + 6), fill=accent)
    elif name == "vehicle":
        d.rounded_rectangle((cx - 18, cy - 6, cx + 18, cy + 8), radius=4, fill=accent)
        d.ellipse((cx - 14, cy + 4, cx - 4, cy + 14), fill=accent)
        d.ellipse((cx + 4, cy + 4, cx + 14, cy + 14), fill=accent)
        d.rectangle((cx - 10, cy - 16, cx + 10, cy - 4), fill=(255, 140, 95, 255))

    path = os.path.join(out, f"{name}.png")
    img.save(path, "PNG")
    print("wrote", path)


for key, value in styles.items():
    draw_pin(key, value[0], value[1])
