# Smena Starter — ВиДР мультиплеер (Quest 2/3 + Mirror)

Шаблон смены Национального детского технопарка. Канон железа: **Meta Quest 2/3**, не PICO. Хост — Windows без XR-камеры. Клиент — APK на шлем.

Спека: `docs/superpowers/specs/2026-09-14-smena-unity-mirror-quest-starter-design.md`

## Открыть проект

1. Unity Hub → Open → `D:\Work\ТехнопаркВР\smena-starter`
2. Редактор **6000.6.3f1** (лежит `C:\Program Files\Unity\Hub\Editor\6000.6.3f1\Editor\Unity.exe`)
3. Дождись импорта XR/URP. **Mirror уже лежит в проекте:** `Assets/Mirror` (релиз **v96.11.2**, без Tests/Examples). Не добавляй его через Package Manager git URL — у `Assets/Mirror` нет `package.json`, UPM падает. SteamVR и PICO не ставить.
4. DOTween — из Asset Store (опционально; лобби и так гаснет корутиной).
5. Меню **Smena → Build Starter Scenes** — сцены `00_Lobby`, `01_Room`, префабы, bundle id `by.ndt.vidr.smena2026`.
6. Window → **UnityMCP** — чтобы агент видел Editor.

Если MCP пишет «not connected»: проект открыт этим редактором, окно UnityMCP открыто, потом повтори команду.

## Mirror

Поставлен **внутрь проекта**, не через UPM: `Assets/Mirror`, тег GitHub **v96.11.2**. kcp2k — `Assets/Mirror/Transports/KCP/kcp2k`. Tests/Examples/Hosting/Edgegap не копировали.

Не делать: Package Manager → Add package from git `.../Mirror.git?path=/Assets/Mirror` — там нет `package.json`, консоль UPM красная, отсюда все CS0246 на `Mirror` / `kcp2k`.

После открытия проекта: дождись компиляции `Assets/Mirror` → меню **Smena → Build Starter Scenes**.

## Как играть на занятии

1. На ПК ученика: Play или Windows-билд → **HOST**. Списать IP (жёлтая строка).
2. На Quest: Android-сборка ARM64 → **CONNECT** на этот IP, порт **7777**.
3. В лобби счётчик игроков. Хост жмёт **В КОМНАТУ**.
4. В комнате схватить жёлтый куб. На окне ПК куб должен ехать.
5. Своя идея: слоты на полу, пример таблички `ExamplePlaque`.

Серый трек: вписать IP соседа, зайти в чужое лобби.

## Материалы

`materials/` — пять PDF для ПК ученика: сеть и Mirror, SyncVar, комната ожидания, калибровка, звук и картинка. Исходники печати — `materials/html/`. Памятка по пакетам: `materials/00-pakety.pdf` (исходник `materials/html/00-pakety.html`).

## Что не трогать детям

Объект `NetworkManager`: Transport KCP, порт 7777, Player Prefab. Снял `NetworkIdentity` с куба — grab виден только у себя.
