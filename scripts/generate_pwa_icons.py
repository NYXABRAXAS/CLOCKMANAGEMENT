"""Generates STLMS's PWA icon set (a clock glyph on the app's indigo brand color, matching the
sidebar logo) - a one-off script, not part of the build; the output PNGs are committed directly."""
from PIL import Image, ImageDraw
import math
import os

BRAND_COLOR = (79, 70, 229, 255)  # indigo-600, matching oklch(0.55 0.2 265) used as --primary
WHITE = (255, 255, 255, 255)

OUT_DIR = os.path.join(os.path.dirname(__file__), "..", "frontend", "public", "icons")
os.makedirs(OUT_DIR, exist_ok=True)


def draw_clock(draw, cx, cy, radius, stroke_width, color=WHITE):
    draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], outline=color, width=stroke_width)
    # Hour hand pointing to ~10, minute hand pointing to ~2 (matches lucide's Clock glyph look)
    hour_len = radius * 0.5
    minute_len = radius * 0.75
    hour_angle = math.radians(-60)  # ~10 o'clock
    minute_angle = math.radians(30)  # ~2 o'clock
    draw.line(
        [cx, cy, cx + hour_len * math.cos(hour_angle), cy + hour_len * math.sin(hour_angle)],
        fill=color, width=stroke_width,
    )
    draw.line(
        [cx, cy, cx + minute_len * math.cos(minute_angle), cy + minute_len * math.sin(minute_angle)],
        fill=color, width=stroke_width,
    )


def rounded_square_icon(size, corner_ratio, clock_ratio, stroke_ratio):
    img = Image.new("RGBA", (size, size), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    corner = int(size * corner_ratio)
    draw.rounded_rectangle([0, 0, size - 1, size - 1], radius=corner, fill=BRAND_COLOR)
    draw_clock(draw, size / 2, size / 2, size * clock_ratio, max(2, int(size * stroke_ratio)))
    return img


def maskable_icon(size):
    # Maskable icons must fill the full square (no transparency, no rounding) and keep meaningful
    # content within the inner ~80% "safe zone" - background fills edge-to-edge here.
    img = Image.new("RGBA", (size, size), BRAND_COLOR)
    draw = ImageDraw.Draw(img)
    draw_clock(draw, size / 2, size / 2, size * 0.30, max(2, int(size * 0.035)))
    return img


# Standard icons (rounded square, transparent corners)
rounded_square_icon(192, 0.22, 0.30, 0.045).save(os.path.join(OUT_DIR, "icon-192.png"))
rounded_square_icon(512, 0.22, 0.30, 0.045).save(os.path.join(OUT_DIR, "icon-512.png"))

# Maskable icon (edge-to-edge background, content in the safe zone) for adaptive Android icons
maskable_icon(512).save(os.path.join(OUT_DIR, "icon-512-maskable.png"))

# Apple touch icon - iOS renders these with its own rounding, so a plain filled square looks right
apple = Image.new("RGBA", (180, 180), BRAND_COLOR)
draw_clock(ImageDraw.Draw(apple), 90, 90, 180 * 0.30, max(2, int(180 * 0.045)))
apple.save(os.path.join(OUT_DIR, "apple-touch-icon.png"))

print("Generated icon-192.png, icon-512.png, icon-512-maskable.png, apple-touch-icon.png")
