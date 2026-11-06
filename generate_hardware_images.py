import math
import os
from PIL import Image, ImageDraw, ImageFilter, ImageFont

os.makedirs("wwwroot/images/products", exist_ok=True)

def create_lg_ultragear():
    width, height = 1024, 1024
    img = Image.new("RGBA", (width, height), (15, 17, 23, 255))
    draw = ImageDraw.Draw(img)

    # Ambient studio radial glow
    for r in range(500, 50, -10):
        alpha = int(25 * (1 - r / 500))
        draw.ellipse([width//2 - r, height//2 - 40 - r, width//2 + r, height//2 - 40 + r], fill=(20, 35, 60, alpha))
    
    # Red accent halo behind monitor
    for r in range(260, 60, -10):
        alpha = int(45 * (1 - r / 260))
        draw.ellipse([width//2 - r, 420 - r, width//2 + r, 420 + r], fill=(220, 38, 38, alpha))

    # Monitor Stand Base (V-shape)
    v_base = [
        (width//2 - 240, 780),
        (width//2, 720),
        (width//2 + 240, 780),
        (width//2 + 210, 795),
        (width//2, 735),
        (width//2 - 210, 795),
    ]
    draw.polygon(v_base, fill=(28, 31, 38, 255))
    # Red trim on stand legs
    draw.line([(width//2 - 230, 783), (width//2 - 10, 727)], fill=(239, 68, 68, 255), width=3)
    draw.line([(width//2 + 10, 727), (width//2 + 230, 783)], fill=(239, 68, 68, 255), width=3)

    # Stand vertical pole
    draw.polygon([
        (width//2 - 35, 720),
        (width//2 + 35, 720),
        (width//2 + 25, 450),
        (width//2 - 25, 450)
    ], fill=(36, 40, 48, 255))
    # Cable management hole & red accent
    draw.rounded_rectangle([width//2 - 12, 540, width//2 + 12, 630], radius=8, fill=(18, 20, 26, 255), outline=(220, 38, 38, 255), width=2)

    # Outer Monitor Bezel
    bezel_box = [120, 150, 904, 610]
    draw.rounded_rectangle(bezel_box, radius=12, fill=(26, 29, 36, 255), outline=(50, 56, 68, 255), width=2)

    # Inner Screen (Zero-bezel style)
    screen_box = [130, 160, 894, 595]
    draw.rounded_rectangle(screen_box, radius=6, fill=(8, 10, 15, 255))

    # Wallpaper on screen: vibrant neon cyberpunk gradient with grid & mountains
    screen_img = Image.new("RGBA", (764, 435))
    s_draw = ImageDraw.Draw(screen_img)
    # Sky gradient
    for y in range(435):
        ratio = y / 435.0
        r = int(15 + ratio * 85)
        g = int(10 + ratio * 35)
        b = int(45 + ratio * 140)
        s_draw.line([(0, y), (764, y)], fill=(r, g, b, 255))

    # Neon sun
    s_draw.ellipse([382 - 90, 130 - 90, 382 + 90, 130 + 90], fill=(255, 110, 160, 255))
    for i in range(5):
        s_draw.line([(382 - 90, 130 + i*14), (382 + 90, 130 + i*14)], fill=(20, 15, 40, 255), width=4)

    # Synthwave grid
    for x in range(0, 765, 40):
        s_draw.line([(x, 260), (int(382 + (x - 382) * 2.8), 435)], fill=(0, 230, 255, 120), width=1)
    for y_pos in [270, 285, 305, 335, 375, 425]:
        s_draw.line([(0, y_pos), (764, y_pos)], fill=(255, 0, 128, 160), width=2)

    # Paste screen image
    img.paste(screen_img, (130, 160), screen_img)

    # Glossy glass reflection streak across screen
    streak = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    st_draw = ImageDraw.Draw(streak)
    st_draw.polygon([(250, 160), (450, 160), (220, 595), (130, 595)], fill=(255, 255, 255, 25))
    img = Image.alpha_composite(img, streak)

    # Bottom bezel badge: "LG UltraGear"
    draw = ImageDraw.Draw(img)
    draw.rectangle([width//2 - 40, 595, width//2 + 40, 608], fill=(35, 40, 48, 255))
    draw.text((width//2 - 32, 596), "ULTRAGEAR", fill=(239, 68, 68, 255))

    # Subtle bottom shadow on table
    shadow = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    sh_draw = ImageDraw.Draw(shadow)
    sh_draw.ellipse([width//2 - 280, 785, width//2 + 280, 840], fill=(0, 0, 0, 180))
    shadow = shadow.filter(ImageFilter.GaussianBlur(15))
    final = Image.alpha_composite(shadow, img).convert("RGB")
    final.save("wwwroot/images/products/lg-ultragear.png", "PNG", quality=95)
    print("Saved lg-ultragear.png")

def create_rtx_4080_super():
    width, height = 1024, 1024
    img = Image.new("RGBA", (width, height), (14, 16, 22, 255))
    draw = ImageDraw.Draw(img)

    # Ambient GeForce green & cyan rim glow
    for r in range(480, 50, -10):
        alpha = int(22 * (1 - r / 480))
        draw.ellipse([width//2 - r, height//2 - r, width//2 + r, height//2 + r], fill=(16, 85, 50, alpha))

    # Shadow under the GPU
    shadow = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    sh_draw = ImageDraw.Draw(shadow)
    sh_draw.ellipse([140, 680, 884, 750], fill=(0, 0, 0, 200))
    shadow = shadow.filter(ImageFilter.GaussianBlur(18))
    img = Image.alpha_composite(img, shadow)
    draw = ImageDraw.Draw(img)

    # GPU Main Card Body
    card_x1, card_y1 = 120, 280
    card_x2, card_y2 = 904, 660

    # PCIe bracket on left
    draw.rounded_rectangle([90, 250, 125, 680], radius=4, fill=(80, 85, 95, 255), outline=(130, 140, 155, 255), width=2)
    # DisplayPort cutouts
    for cy in [320, 370, 420, 470]:
        draw.rounded_rectangle([98, cy, 117, cy + 30], radius=3, fill=(30, 32, 38, 255))

    # Main Card Shroud (Matte Gunmetal with Chamfered Edges)
    draw.rounded_rectangle([card_x1, card_y1, card_x2, card_y2], radius=16, fill=(30, 34, 42, 255), outline=(75, 85, 105, 255), width=3)

    # Brushed metal accent plates
    draw.polygon([(140, 300), (360, 300), (340, 640), (140, 640)], fill=(22, 25, 32, 255))
    draw.polygon([(380, 300), (640, 300), (640, 640), (360, 640)], fill=(26, 30, 38, 255))
    draw.polygon([(660, 300), (884, 300), (884, 640), (660, 640)], fill=(22, 25, 32, 255))

    # Top LED bar: "GEFORCE RTX" in luminous green/white
    draw.rounded_rectangle([250, 290, 520, 322], radius=4, fill=(10, 12, 16, 255), outline=(0, 230, 118, 180), width=1)
    draw.text((270, 295), "GEFORCE RTX 4080 SUPER", fill=(118, 255, 3, 255))

    # 3 Large High-Performance Axial Fans
    fan_centers = [(260, 470), (512, 470), (764, 470)]
    for cx, cy in fan_centers:
        # Fan Recess ring
        draw.ellipse([cx - 110, cy - 110, cx + 110, cy + 110], fill=(16, 18, 24, 255), outline=(0, 229, 255, 80), width=2)
        # Fan Blades (11 curved blades)
        for b in range(11):
            angle = b * (2 * math.pi / 11)
            bx1 = cx + int(28 * math.cos(angle))
            by1 = cy + int(28 * math.sin(angle))
            bx2 = cx + int(102 * math.cos(angle + 0.35))
            by2 = cy + int(102 * math.sin(angle + 0.35))
            draw.line([(bx1, by1), (bx2, by2)], fill=(45, 52, 65, 255), width=9)
        # Center Hub (Metallic logo cap)
        draw.ellipse([cx - 32, cy - 32, cx + 32, cy + 32], fill=(40, 46, 56, 255), outline=(100, 115, 135, 255), width=2)
        draw.ellipse([cx - 18, cy - 18, cx + 18, cy + 18], fill=(0, 230, 118, 160))

    # PCIe Connector Pins on bottom right edge
    draw.rectangle([card_x1 + 60, card_y2 + 2, card_x1 + 320, card_y2 + 22], fill=(30, 35, 40, 255))
    for p in range(card_x1 + 70, card_x1 + 310, 4):
        draw.line([(p, card_y2 + 4), (p, card_y2 + 20)], fill=(212, 175, 55, 255), width=2)

    # Subtle neon edge glow
    glow = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    g_draw = ImageDraw.Draw(glow)
    g_draw.line([(card_x1 + 20, card_y1 + 5), (card_x2 - 20, card_y1 + 5)], fill=(0, 230, 118, 200), width=3)
    g_draw.line([(card_x1 + 20, card_y2 - 5), (card_x2 - 20, card_y2 - 5)], fill=(0, 229, 255, 200), width=3)
    glow = glow.filter(ImageFilter.GaussianBlur(4))
    img = Image.alpha_composite(img, glow)

    final = img.convert("RGB")
    final.save("wwwroot/images/products/rtx-4080-super.png", "PNG", quality=95)
    print("Saved rtx-4080-super.png")

def create_kingston_ddr5():
    width, height = 1024, 1024
    img = Image.new("RGBA", (width, height), (13, 15, 20, 255))
    draw = ImageDraw.Draw(img)

    # Ambient Magenta/Cyan RGB Glow
    for r in range(460, 50, -10):
        alpha = int(24 * (1 - r / 460))
        draw.ellipse([width//2 - r, height//2 - 60 - r, width//2 + r, height//2 - 60 + r], fill=(80, 20, 85, alpha))

    # Base shadow
    shadow = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    sh_draw = ImageDraw.Draw(shadow)
    sh_draw.ellipse([160, 720, 864, 800], fill=(0, 0, 0, 220))
    shadow = shadow.filter(ImageFilter.GaussianBlur(16))
    img = Image.alpha_composite(img, shadow)

    # Function to draw one RAM stick
    def draw_ram_stick(x_offset, y_offset, is_front=True):
        stick = Image.new("RGBA", (width, height), (0, 0, 0, 0))
        s_draw = ImageDraw.Draw(stick)

        x1, y1 = 150 + x_offset, 320 + y_offset
        x2, y2 = 820 + x_offset, 650 + y_offset

        # Main Black PCB & Heatspreader
        s_draw.rounded_rectangle([x1, y1 + 35, x2, y2], radius=10, fill=(24, 26, 32, 255), outline=(55, 62, 75, 255), width=2)

        # Aggressive geometric cuts and ridges
        for rx in range(x1 + 40, x2 - 80, 70):
            s_draw.polygon([
                (rx, y1 + 55),
                (rx + 30, y1 + 55),
                (rx + 15, y2 - 40),
                (rx - 15, y2 - 40)
            ], fill=(16, 18, 22, 255))

        # "FURY BEAST" Logo & "DDR5"
        s_draw.rectangle([x1 + 80, y1 + 90, x1 + 320, y1 + 140], fill=(14, 16, 20, 255))
        s_draw.text((x1 + 95, y1 + 102), "KINGSTON FURY", fill=(220, 225, 235, 255))
        s_draw.text((x1 + 550, y1 + 102), "BEAST DDR5", fill=(239, 68, 68, 255))
        s_draw.text((x1 + 550, y1 + 125), "32GB (2x16GB) 6000MHz", fill=(148, 163, 184, 255))

        # Gold contact pins on bottom edge
        notch_x = x1 + (x2 - x1) * 0.45
        for px in range(x1 + 25, x2 - 25, 5):
            if abs(px - notch_x) > 18:
                s_draw.line([(px, y2 + 1), (px, y2 + 24)], fill=(218, 165, 32, 255), width=2)
            else:
                s_draw.line([(px, y2 + 1), (px, y2 + 12)], fill=(30, 30, 30, 255), width=2)

        # Translucent RGB Diffuser Top Bar (Rainbow spectrum)
        rgb_bar = [x1 + 10, y1, x2 - 10, y1 + 40]
        s_draw.rounded_rectangle(rgb_bar, radius=8, fill=(30, 30, 30, 255))
        for sx in range(int(x1 + 12), int(x2 - 12)):
            ratio = (sx - x1) / (x2 - x1)
            # Cycle through Hue: Red -> Green -> Cyan -> Blue -> Magenta
            r = int(127 * (1 + math.cos(ratio * 2 * math.pi)))
            g = int(127 * (1 + math.cos(ratio * 2 * math.pi - 2 * math.pi / 3)))
            b = int(127 * (1 + math.cos(ratio * 2 * math.pi - 4 * math.pi / 3)))
            s_draw.line([(sx, y1 + 2), (sx, y1 + 38)], fill=(r, g, b, 240), width=1)

        return stick

    # Draw Back Stick
    back_stick = draw_ram_stick(60, -70, is_front=False)
    img = Image.alpha_composite(img, back_stick)

    # Draw Front Stick
    front_stick = draw_ram_stick(-30, 40, is_front=True)
    img = Image.alpha_composite(img, front_stick)

    # Glow filter over the RGB top bars
    final = img.convert("RGB")
    final.save("wwwroot/images/products/kingston-ddr5.png", "PNG", quality=95)
    print("Saved kingston-ddr5.png")

def create_rtx_5090_beast():
    width, height = 1024, 1024
    img = Image.new("RGBA", (width, height), (10, 12, 18, 255))
    draw = ImageDraw.Draw(img)

    # Hyper-cyberpunk golden/amber and electric blue rim aura
    for r in range(500, 50, -10):
        alpha = int(25 * (1 - r / 500))
        draw.ellipse([width//2 - r, height//2 - r, width//2 + r, height//2 + r], fill=(60, 30, 10, alpha))
    for r in range(300, 40, -10):
        alpha = int(30 * (1 - r / 300))
        draw.ellipse([width//2 - r, height//2 - 60 - r, width//2 + r, height//2 - 60 + r], fill=(0, 90, 160, alpha))

    # Heavy grounding shadow
    shadow = Image.new("RGBA", (width, height), (0, 0, 0, 0))
    sh_draw = ImageDraw.Draw(shadow)
    sh_draw.ellipse([110, 710, 914, 780], fill=(0, 0, 0, 230))
    shadow = shadow.filter(ImageFilter.GaussianBlur(20))
    img = Image.alpha_composite(img, shadow)
    draw = ImageDraw.Draw(img)

    # 3.5-Slot Titanium Beast Body
    x1, y1 = 110, 260
    x2, y2 = 914, 690

    # Triple slot steel bracket
    draw.rounded_rectangle([75, 230, 115, 710], radius=5, fill=(70, 75, 88, 255), outline=(140, 150, 170, 255), width=2)
    for cy in [300, 360, 420, 480, 540]:
        draw.rounded_rectangle([84, cy, 106, cy + 35], radius=3, fill=(20, 22, 28, 255))

    # Titanium Armor Frame with Carbon Inlays
    draw.rounded_rectangle([x1, y1, x2, y2], radius=20, fill=(25, 28, 36, 255), outline=(245, 158, 11, 200), width=3)

    # Carbon fiber patterned sections
    draw.polygon([(130, 280), (360, 280), (320, 670), (130, 670)], fill=(18, 20, 25, 255))
    draw.polygon([(680, 280), (894, 280), (894, 670), (660, 670)], fill=(18, 20, 25, 255))

    # Center illuminated badge: "RTX 5090 BEAST EDITION"
    draw.rounded_rectangle([280, 275, 740, 318], radius=6, fill=(12, 14, 18, 255), outline=(245, 158, 11, 255), width=2)
    draw.text((310, 286), "GEFORCE RTX 5090 // BEAST EDITION", fill=(251, 191, 36, 255))

    # 3 Ringed Holographic High-Static-Pressure Fans
    fan_centers = [(260, 485), (512, 485), (764, 485)]
    for cx, cy in fan_centers:
        # Outer ring light
        draw.ellipse([cx - 115, cy - 115, cx + 115, cy + 115], fill=(12, 14, 18, 255), outline=(56, 189, 248, 220), width=3)
        # Fan Blades (13 frosted aero blades)
        for b in range(13):
            angle = b * (2 * math.pi / 13)
            bx1 = cx + int(32 * math.cos(angle))
            by1 = cy + int(32 * math.sin(angle))
            bx2 = cx + int(108 * math.cos(angle + 0.4))
            by2 = cy + int(108 * math.sin(angle + 0.4))
            draw.line([(bx1, by1), (bx2, by2)], fill=(50, 58, 72, 255), width=8)
        # Gold beast core hub
        draw.ellipse([cx - 36, cy - 36, cx + 36, cy + 36], fill=(30, 35, 45, 255), outline=(245, 158, 11, 255), width=2)
        draw.polygon([(cx - 12, cy + 10), (cx, cy - 14), (cx + 12, cy + 10)], fill=(245, 158, 11, 255))

    # Copper heatpipe ends visible
    for hy in [350, 390, 580, 620]:
        draw.ellipse([885, hy, 903, hy + 18], fill=(184, 115, 51, 255), outline=(130, 70, 20, 255), width=1)

    # Gold PCIe 5.0 connection pins
    for px in range(x1 + 80, x1 + 380, 4):
        draw.line([(px, y2 + 2), (px, y2 + 22)], fill=(245, 158, 11, 255), width=2)

    final = img.convert("RGB")
    final.save("wwwroot/images/products/rtx-5090-beast.png", "PNG", quality=95)
    print("Saved rtx-5090-beast.png")

create_lg_ultragear()
create_rtx_4080_super()
create_kingston_ddr5()
create_rtx_5090_beast()
print("All 4 hardware images generated successfully!")

