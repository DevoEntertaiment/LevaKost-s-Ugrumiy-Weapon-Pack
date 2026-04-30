<!-- markdownlint-disable MD033 MD041 -->
<div align="center">
  <img src="icon.png" alt="LevaKost's Ugrumiy Weapon Pack" width="96" />
  <h1>LevaKost's Ugrumiy Weapon Pack</h1>

  <p>Контент-мод для <a href="https://terraria.org/">Terraria</a> на базе <a href="https://github.com/tModLoader/tModLoader">tModLoader</a>.</p>

  <p>
    <a href="https://github.com/LevaKost/Ugrumiy-Weapon-Pack/actions/workflows/build.yml"><img alt="Build" src="https://github.com/LevaKost/Ugrumiy-Weapon-Pack/actions/workflows/build.yml/badge.svg" /></a>
    <a href="https://github.com/LevaKost/Ugrumiy-Weapon-Pack/releases/latest"><img alt="Latest release" src="https://img.shields.io/github/v/release/LevaKost/Ugrumiy-Weapon-Pack?display_name=tag&sort=semver" /></a>
    <a href="https://github.com/LevaKost/Ugrumiy-Weapon-Pack/releases/latest"><img alt="Downloads" src="https://img.shields.io/github/downloads/LevaKost/Ugrumiy-Weapon-Pack/total" /></a>
    <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-blueviolet" />
    <img alt="tModLoader" src="https://img.shields.io/badge/tModLoader-1.4.4.9-orange" />
  </p>
</div>

> **Авторы:** LelaPo & Kotya
> **Актуальная версия:** см. [последний релиз](https://github.com/LevaKost/Ugrumiy-Weapon-Pack/releases/latest) (бейдж `Latest release` выше)

Мод добавляет оружие, NPC, аксессуары и расходники с фирменным юмором и
механиками, которых нет в ваниле: слизневые ружья, систему ожирения,
ИИ-браслет с Q-Learning, редкого жителя пещер и многое другое.

---

## Содержание

- [Что в моде](#что-в-моде)
  - [Оружие](#оружие)
  - [Маунты](#маунты)
  - [NPC](#npc)
  - [Аксессуары](#аксессуары)
  - [Расходники](#расходники)
  - [Системы](#системы)
- [Установка](#установка)
- [Сборка из исходников](#сборка-из-исходников)
- [Структура проекта](#структура-проекта)
- [Локализация](#локализация)
- [Релизы и версионирование](#релизы-и-версионирование)
- [Авторы](#авторы)
- [Лицензия](#лицензия)

---

## Что в моде

### Оружие

| Предмет | Класс | Описание |
|---------|-------|----------|
| Slime Minigun | Ranged | Автоматическая стрельба каплями слизи. 33% шанс не потратить выстрел. Не требует патронов. |
| Slime Shotgun | Ranged | Дробовик: веер из 5 слизневых капель. Не требует патронов. |
| IBP-3000 | Ranged (throwable) | Метательный блок питания, бьющий разрядом. 69 урона на попадание. |
| VXE R1 SE+ | Magic | Магическая мышь, летящая за курсором. Бьёт врагов и разлетается при сильном ударе. |

Капли слизи (`SlimeGlob`) при попадании в поверхность создают лужу
(`SlimePuddle`), которая растекается и замедляет врагов.

### Маунты

| Предмет | Эффект |
|---------|--------|
| **Oppressor MK2** | Летательный аппарат: бесконечный полёт, иммунитет к урону от падения, рывок по двойному тапу влево/вправо (кд 5 сек, шкала под игроком). Продаётся у Mechanic-Merriweather после Plantera. |

### NPC

| NPC | Где встречается | Особенности |
|-----|-----------------|-------------|
| **John** | Пещеры (Cavern), очень редко | Бродит по пещерам. Если игрок наденет его шляпу — становится враждебным. |
| **Cow** | Town NPC, призыв через CowBell | Дружелюбный «бык», даёт молоко (`CowMilk`). |
| **Mechanic-Merriweather** | Town NPC, заселяется после Wall of Flesh | Альтернативный механик: продаёт провода, прессовые пластины, актуаторы. |

### Аксессуары

| Предмет | Эффект |
|---------|--------|
| **John's Hat** | +10% урон, +5% крит. Дроп с Джона (~14%). При надетой шляпе Джон становится враждебным при встрече. |
| **Cursed Bandle** | Проклятый браслет с ИИ на Q-Learning. После надевания не снимается до смерти — управляет персонажем сам и учится на опыте игрока. |

### Расходники

| Предмет | Эффект |
|---------|--------|
| **Fat Burger** | Хилит 30 HP и повышает уровень жира. Персонаж толстеет, появляются бонусы и штрафы. |
| **Diet Pill** | Снижает уровень жира. |
| **Cow Milk** | Восстанавливает 50 HP, даёт бафф Well Fed. |
| **Cherry** | Полное восстановление HP. Очень редкий дроп после Plantera, кулдаун 1 минута. |
| **Spooky Candy** | Рулетка: 55% мега-хил, 30% взрыв, 15% мгновенная смерть с анекдотом. |
| **Cow Bell** | Призывает Cow рядом с игроком (с кулдауном). |

### Системы

- **Fat System** — поедание бургеров повышает шкалу жира на экране. На высоких
  уровнях персонаж замедляется и медленнее атакует, но получает бонусы к защите
  и максимальному HP. `Diet Pill` и смерть снижают жир.
- **Cursed Bandle AI** — браслет с Q-Learning: запоминает действия игрока,
  сохраняет таблицу в файл и со временем играет за вас всё лучше.
- **John Hat tracking** — глобальная система отслеживает, носит ли игрок шляпу
  Джона, и переключает поведение Джона при встрече.
- **Cherry Drop** — глобальная система, добавляющая редкий дроп Cherry после
  убийства Plantera.

---

## Установка

### Через Steam Workshop

> Если мод опубликован в Workshop — подпишитесь на странице мода и tModLoader
> сам его установит. Если страницы пока нет, используйте релиз ниже.

### Из GitHub Release (`.tmod`)

1. Установите [tModLoader](https://store.steampowered.com/app/1281930/tModLoader/) через Steam.
2. Скачайте файл `*.tmod` со страницы [последнего релиза](https://github.com/LevaKost/Ugrumiy-Weapon-Pack/releases/latest).
3. Положите его в папку модов tModLoader:
   - **Windows:** `%UserProfile%\Documents\My Games\Terraria\tModLoader\Mods\`
   - **Linux:** `~/.local/share/Terraria/tModLoader/Mods/`
   - **macOS:** `~/Library/Application Support/Terraria/tModLoader/Mods/`
4. Запустите tModLoader и включите мод в меню *Mods*.

### Из исходников (для разработчиков)

См. раздел [Сборка из исходников](#сборка-из-исходников).

---

## Сборка из исходников

Требуется .NET SDK 8.0 и установленный tModLoader (Steam) — он предоставляет
`tModLoader.targets`, на который ссылается `LK_Ugrumiy_WP.csproj`.

```bash
# 1. Клонируем репозиторий в папку ModSources tModLoader
git clone https://github.com/LevaKost/Ugrumiy-Weapon-Pack.git \
  "$HOME/.local/share/Terraria/tModLoader/ModSources/LK_Ugrumiy_WP"

# 2. Открываем tModLoader → Workshop → Develop Mods → Build + Reload
```

Альтернативный путь без Steam (как делает CI): скачать `tModLoader.zip` со
страницы релизов tModLoader, распаковать в любой каталог, скопировать
`tMLMod.targets` рядом как `tModLoader.targets` и выполнить:

```bash
dotnet build LK_Ugrumiy_WP.csproj
```

После успешной сборки `.tmod` появится в `Mods/` tModLoader.

---

## Структура проекта

```
.
├── Common/Systems/          # Глобальные системы (трекинг шляпы Джона, дроп Cherry)
├── Content/
│   ├── Accessories/         # Cursed Bandle + ИИ (Q-Learning)
│   ├── Buffs/               # Кулдауны (Cherry, CowBell)
│   ├── Items/
│   │   ├── Accessories/     # John's Hat
│   │   ├── Consumables/     # Fat Burger, Diet Pill, Milk, Cherry, Spooky Candy, Cow Bell
│   │   └── Weapons/         # SlimeMinigun, SlimeShotgun, ibp3000, VxeMouse
│   ├── NPCs/                # John, Cow, Mechanic-Merriweather
│   └── Projectiles/         # SlimeGlob, SlimePuddle, Ibp3000Projectile, VxeMouseProjectile
├── Localization/            # en-US.hjson, ru-RU.hjson
├── Properties/              # launchSettings.json
├── .github/workflows/       # CI: build.yml, release.yml
├── build.txt                # Метаданные мода для tModLoader
├── description.txt          # Описание (in-game) с {ModVersion}/{tMLVersion}
├── description_workshop.txt # Описание для Steam Workshop
├── icon.png / icon_small.png
└── LK_Ugrumiy_WP.cs         # Точка входа (`class LK_Ugrumiy_WP : Mod`)
```

---

## Локализация

Мод поддерживает английский (`Localization/en-US.hjson`) и русский
(`Localization/ru-RU.hjson`). Текстовые ключи ссылаются из кода через
`Language.GetTextValue("Mods.LK_Ugrumiy_WP.…")`. Чтобы добавить язык —
скопируйте `en-US.hjson` под нужным кодом локали и переведите значения.

---

## Релизы и версионирование

Используется [SemVer](https://semver.org/) — `MAJOR.MINOR.PATCH`.

Релиз выпускается через git-тег вида `v0.1.2`:

```bash
git tag v0.1.2
git push origin v0.1.2
```

GitHub Actions ([release.yml](.github/workflows/release.yml)) автоматически:

1. Подставит указанную версию в `build.txt` **только в рабочей копии раннера**
   (репо при этом не меняется — никаких авто-коммитов в `main` и нужды делать
   `git pull` после релиза).
2. Соберёт мод в .NET 8 + tModLoader и найдёт `.tmod`.
3. Создаст GitHub Release с авто-сгенерированными release notes
   (`generate_release_notes: true`) и приложит `.tmod` как ассет.

Помимо этого можно запустить релиз вручную из вкладки *Actions → Release Mod
→ Run workflow*, указав версию.

---

## Авторы

- **VanyaSvetoslav** — гейм-дизайн, контент, спрайты.
- **Kotya** — программирование, ИИ, системы.
- **LelaPo** — придумал мод, даёт идеи

Идеи, баги и пулл-реквесты приветствуются — открывайте
[issue](https://github.com/LevaKost/Ugrumiy-Weapon-Pack/issues) или PR.

## Лицензия

Лицензия на код и ассеты пока не выбрана — все права принадлежат авторам.
Если вы хотите использовать материалы мода — свяжитесь с авторами.
