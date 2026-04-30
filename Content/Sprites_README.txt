SPRITES TO CREATE / ЗАМЕНИТЬ
=============================

1. JohnsHat.png
   Location: Content/Items/Accessories/JohnsHat/JohnsHat.png
   Size: 30x30 pixels (standard accessory size)
   Description: A worn-out cap/hat. Reference: Grizzled Jon's hat from RDR2.
   Colors: Brown/gray tones, weathered look.

2. JohnNPC.png
   Location: Content/NPCs/JohnNPC/JohnNPC.png
   Size: 18x40 pixels (standard town NPC size)
   Frame count: 25 frames (same as Guide NPC)
   Description: An old man NPC wearing a distinctive hat.
   Reference: Old, grizzled man similar to Grizzled Jon from RDR2.

3. GravityGun.png  [PLACEHOLDER]
   Location: Content/Items/Weapons/GravityGun/GravityGun.png
   Size: 64x28 pixels (matches Item.width=64, Item.height=28).
   Description: Inventory icon AND in-hand sprite of the Gravity Gun.
                The same texture is also drawn in the player's hand by
                GravityGunBeam.PreDraw — so it must look reasonable both
                in inventory and rotated to follow the cursor.
   Pivot: rotation/origin is the texture center.
   Orientation: barrel pointing RIGHT (the code mirrors the sprite when the
                player faces left via SpriteEffects.FlipVertically).
   Reference: Half-Life 2 Gravity Gun — orange energy core, dark grey body,
              prominent barrel and grip. Stylise to fit Terraria pixel art.
   Marker: current placeholder has a 1x1 magenta dot in the top-left corner —
           that's the "this is a placeholder" sentinel; remove it in the final art.

4. GravityGunBeam.png  [PLACEHOLDER, currently unused at runtime]
   Location: Content/Projectiles/GravityGunBeam/GravityGunBeam.png
   Size: 16x16 pixels.
   Description: Single beam particle. The runtime currently draws the beam
                with vanilla DustID.Flare in DrawBeam(), so this PNG is only
                used if the projectile starts rendering its own texture.
                Provided for parity / future expansion.

---

How to add sprites:
1. Create your sprite at the specified resolution
2. Save as .png in the location specified above
3. Make sure "Build Action" is set to "Content" in your IDE
4. The game will automatically load the sprite when you reference it in code

---

Как добавить спрайты:
1. Создайте спрайт в указанном разрешении
2. Сохраните как .png в указанной локации
3. Убедитесь что "Build Action" установлен в "Content" в вашей IDE
4. Игра автоматически загрузит спрайт при ссылке на него в коде

---

SPRITE SHEET LAYOUT (JohnNPC.png - 25 frames):
Frame layout follows vanilla Guide NPC pattern. Each frame is 18x40 pixels.
Frames are arranged vertically in a single column.

---

LAYOUT СПРАЙТОВ (JohnNPC.png - 25 кадров):
Расположение кадров следует паттерну ванильного Guide NPC. Каждый кадр 18x40 пикселей.
Кадры расположены вертикально в одной колонке.
