#!/usr/bin/env node
const fs = require("fs");
const path = require("path");
const { Document, Packer, Paragraph, TextRun, HeadingLevel, Table, TableRow, TableCell,
  WidthType, ShadingType, BorderStyle } = require("docx");
const PptxGenJS = require("pptxgenjs");

const outDir = path.join(__dirname, "..", "materials");
fs.mkdirSync(outDir, { recursive: true });

const Y = "FFF3C4";
const G = "E4E7EE";
const thin = { style: BorderStyle.SINGLE, size: 4, color: "CCCCCC" };
const borders = { top: thin, bottom: thin, left: thin, right: thin };

function p(text, opts = {}) {
  return new Paragraph({
    spacing: { after: 160 },
    ...opts,
    children: [new TextRun({ text, font: "Calibri", size: opts.size || 22, bold: opts.bold, color: opts.color || "1A1A1A" })],
  });
}
function h(text, level = HeadingLevel.HEADING_1) {
  return new Paragraph({ heading: level, spacing: { before: 240, after: 160 }, children: [new TextRun({ text, font: "Calibri", bold: true })] });
}
function box(title, body, fill) {
  return new Table({
    width: { size: 9360, type: WidthType.DXA },
    columnWidths: [9360],
    rows: [new TableRow({ children: [new TableCell({
      borders,
      width: { size: 9360, type: WidthType.DXA },
      shading: { type: ShadingType.CLEAR, fill },
      children: [p(title, { bold: true, size: 22 }), ...body.split("\n").map((line) => p(line))],
    })] })],
  });
}
function shot(label) {
  return p("[скрин из шаблона smena-starter: " + label + "]", { color: "666666", size: 20 });
}

async function saveDoc(name, children) {
  const doc = new Document({ sections: [{ properties: { page: { margin: { top: 720, bottom: 720, left: 720, right: 720 } } }, children }] });
  const buf = await Packer.toBuffer(doc);
  fs.writeFileSync(path.join(outDir, name), buf);
  console.log("wrote", name);
}

async function t1() {
  await saveDoc("T1-unity-host.docx", [
    h("Тетрадь Т1. Unity шаблона и HOST"),
    p("Смена ВиДР, Quest 2/3, Mirror. 2 учебных часа теории + практика."),
    p("Цель: открыть проект, понять окна, нажать HOST на своём ПК, записать IP."),
    box("Жёлтый (обязательно)", "Сделай шаги 1–12. Серый не нужен, чтобы закрыть тетрадь.", Y),
    box("Серый (по желанию)", "Найди в Hierarchy объект NetworkManager и запиши, какой Transport стоит.", G),
    h("Что уже должно быть", HeadingLevel.HEADING_2),
    p("Unity 6000.4.5f1. Проект D:\\Work\\ТехнопаркВР\\smena-starter. Сцены собраны: меню Smena → Build Starter Scenes."),
    h("Шаги", HeadingLevel.HEADING_2),
    p("1. Unity Hub → Open → папка smena-starter. Должно быть видно Assets/SmenaStarter.", { bold: true }),
    shot("окно проекта, папка SmenaStarter"),
    p("Должно быть видно: папки Scripts, Scenes (00_Lobby, 01_Room), Prefabs."),
    p("Если не так: проект не тот. Не открывай Admin_VR1SemT и не ставь PICO."),
    p("2. File → Build Settings. Слева сцены 00_Lobby (0) и 01_Room (1).", { bold: true }),
    shot("Build Settings, две сцены галочками"),
    p("Должно быть видно: Platform Windows и отдельно Android. Две платформы — потому что ПК хост, шлем клиент."),
    p("3. Открой Assets/SmenaStarter/Scenes/00_Lobby двойным кликом.", { bold: true }),
    shot("Hierarchy лобби: NetworkManager, LobbyCanvas, LobbyFloor"),
    p("4. Окна: Hierarchy слева, Scene посередине, Inspector справа, Game — как будет выглядеть Play.", { bold: true }),
    p("5. Hierarchy → NetworkManager. Inspector: компонент Smena Network Manager и Kcp Transport. Порт 7777. Это НЕ трогать.", { bold: true }),
    shot("Inspector NetworkManager, KCP Port 7777"),
    p("Если не так: сняли Transport — сеть умрёт. Верни через Git / преподавателя."),
    p("6. Нажми Play (треугольник сверху).", { bold: true }),
    shot("Play Mode, жёлтые кнопки HOST и CONNECT"),
    p("Должно быть видно: «IP этого ПК: … порт 7777», поле IP, HOST, CONNECT."),
    p("Если чёрный экран: выключи Play, меню Smena → Build Starter Scenes, снова Play."),
    p("7. Нажми HOST. Не снимай Play.", { bold: true }),
    shot("после HOST: статус ХОСТ, кнопка В КОМНАТУ"),
    p("Должно быть видно: статус про хост и кнопку «В КОМНАТУ». Пока НЕ жми её."),
    p("8. Спиши IP в поле ниже. Это адрес твоего ПК в Wi‑Fi лаборатории.", { bold: true }),
    p("Мой IP: _______________________   Порт: 7777"),
    p("9. File → Build Settings → Windows. Пока достаточно Play. Windows-билд соберёт преподаватель, если 10 редакторов тормозят.", { bold: true }),
    p("10. Выключи Play. Сцена должна остаться 00_Lobby.", { bold: true }),
    p("11. Лист идеи: выбери направление (образование / игра / …). Объект ещё не ставь.", { bold: true }),
    p("Моё направление: _______________________"),
    p("12. Сдай тетрадь себе: IP записан, HOST нажимался, PICO в проекте нет.", { bold: true }),
    h("Если не так", HeadingLevel.HEADING_2),
    p("Нет кнопки HOST — не собраны сцены. SteamVR/PICO в пакетах — снести, это другой курс. IP 127.0.0.1 — Wi‑Fi выключен."),
  ]);
}

async function t2() {
  await saveDoc("T2-mirror-lobby.docx", [
    h("Тетрадь Т2. Mirror и лобби"),
    p("Цель: понять NetworkIdentity, зайти со шлема в лобби своего ПК, увидеть счётчик."),
    box("Жёлтый", "Шаги 1–10: свой хост + свой Quest. Счётчик ≥ 1.", Y),
    box("Серый", "Шаг 11: CONNECT на IP соседа. Чужое лобби = чужой мир.", G),
    h("Не путать", HeadingLevel.HEADING_2),
    p("Mirror — сеть Unity. Oculus Mirror в программе — трансляция картинки шлема. Это разные вещи."),
    p("1. Hierarchy → NetworkManager. Inspector: Player Prefab = VrPlayer. Это объект, который Mirror спавнит каждому клиенту.", { bold: true }),
    shot("Player Prefab поле, не пустое"),
    p("Должно быть видно: VrPlayer и HostMarker. Пустое поле — никого не будет в комнате."),
    p("2. Открой Prefabs/VrPlayer. Компонент Network Identity. Без него объект «только у тебя».", { bold: true }),
    shot("Inspector VrPlayer, Network Identity"),
    p("3. SyncVar на пальцах: число «Игроков в лобби» одно и то же у ПК и у шлема. Это LobbyRoster.", { bold: true }),
    shot("LobbyRoster на NetworkManager"),
    p("4. ПК: Play → HOST. IP тот же, что в Т1.", { bold: true }),
    p("5. Quest: Developer Mode, установлен APK by.ndt.vidr.smena2026 (ставит преподаватель).", { bold: true }),
    p("6. На шлеме впиши IP своего ПК → CONNECT.", { bold: true }),
    shot("поле IP на Quest / на ПК после коннекта счётчик 2 если хост тоже считается, иначе 1"),
    p("Должно быть видно: «Игроков в лобби» выросло. Тебя посчитали."),
    p("Меня посчитали: да / нет. Число на ПК: ____  на шлеме: ____"),
    p("7. Не жми «В КОМНАТУ», пока счётчик не совпал.", { bold: true }),
    p("8. Чек-лист, если не зашёл:", { bold: true }),
    p("Один Wi‑Fi, без гостевой изоляции. HOST горит. Порт 7777. Файрвол Windows: разрешить Unity / билд. Quest не в мобильном интернете."),
    p("9. Выйди со шлема, HOST не выключай, зайди снова — счётчик должен ожить.", { bold: true }),
    p("10. Выключи Play на ПК только когда шлем уже на столе.", { bold: true }),
    box("Серый", "11. Возьми IP одногруппника. CONNECT на ЧУЖОЙ IP. Счётчик чужого лобби. Свой HOST в этот момент выключи.", G),
    p("Чужой IP: _______________________"),
  ]);
}

async function t3() {
  await saveDoc("T3-grab.docx", [
    h("Тетрадь Т3. Grab"),
    p("Цель: схватить учебный куб в комнате. На окне ПК куб едет. Хост руками не хватает."),
    box("Жёлтый", "Шаги 1–8. Скрипт grab не пиши.", Y),
    box("Серый", "Шаг 9: сравни Inspector куба с табличкой ExamplePlaque.", G),
    p("1. Т2 закрыт: лобби считает тебя. На ПК HOST, шлем CONNECT.", { bold: true }),
    p("2. На ПК нажми «В КОМНАТУ». Оба должны оказаться в 01_Room.", { bold: true }),
    shot("сцена комнаты: пол, два жёлтых куба, слоты"),
    p("Если шлем остался в лобби: хост не нажал кнопку или сцена 01_Room не в Build Settings."),
    p("3. На Quest подойди к кубу GrabCube_A. Схвати триггером.", { bold: true }),
    shot("куб в руке на Quest"),
    p("4. Смотри на окно ПК. Куб должен ехать там же.", { bold: true }),
    shot("окно хоста, куб смещён"),
    p("Должно быть видно: движение на ПК без XR-камеры. Мышью с ПК куб не хватают."),
    p("5. Hierarchy → GrabCube_A. Список компонентов как в шаблоне: Mesh, Box Collider, Network Identity, Network Transform, XR Grab Interactable, Grab Network Authority.", { bold: true }),
    shot("Inspector GrabCube_A полный список"),
    p("6. Антипапример (делай только если преподаватель рядом): сними галку с Network Identity, Play, схвати. Куб едет только у тебя. Верни компонент.", { bold: true }),
    p("7. Второй куб GrabCube_B — то же самое.", { bold: true }),
    p("Куб А виден на ПК: да / нет. Куб Б: да / нет"),
    p("8. Не удаляй Grab Network Authority «для чистоты». Без него чужой шлем не заберёт объект.", { bold: true }),
    box("Серый", "9. Открой ExamplePlaque. Те же сетевые компоненты + Color Sync Example. Это образец для своей идеи.", G),
  ]);
}

async function t4() {
  await saveDoc("T4-lobby-idea.docx", [
    h("Тетрадь Т4. Лобби целиком и своя идея"),
    p("Цель: провести лобби как хост; поставить в слот свой объект по рецепту таблички."),
    box("Жёлтый", "Скопируй пример «табличка экспоната» в слот 1.", Y),
    box("Серый", "Свой префаб + Network Identity. По силам — Grab как у куба или Color Sync.", G),
    h("Лобби ещё раз", HeadingLevel.HEADING_2),
    p("1. HOST на ПК. Шлемы CONNECT. Счётчик = число шлемов (+маркер хоста, если виден).", { bold: true }),
    p("2. «В КОМНАТУ» жмёт только хост. Клиенты ждут.", { bold: true }),
    shot("кнопка В КОМНАТУ только при HOST"),
    h("Пример: табличка экспоната", HeadingLevel.HEADING_2),
    p("3. Выключи Play. Сцена 01_Room.", { bold: true }),
    p("4. Hierarchy → ExamplePlaque. Ctrl+D. Имя: Idea_Фамилия.", { bold: true }),
    shot("дубликат таблички над слотом"),
    p("5. Перетащи объект на IdeaSlot_1 (жёлтая плитка)."),
    p("6. Не снимай Network Identity и Grab.", { bold: true }),
    p("7. Play → HOST → шлем CONNECT → В КОМНАТУ. Схвати свою табличку. На ПК она едет.", { bold: true }),
    p("Моя идея одной фразой: ________________________________"),
    p("8. Жёлтый на этом закрывает тетрадь. Не начинай новый пустой 3D-проект.", { bold: true }),
    box("Серый", "9. Свой меш вместо куба: те же компоненты, что у GrabCube. Color Sync Example — смена цвета с хоста (контекст-меню Cycle Color). Не ставь Photon/NGO.", G),
    h("Когда звать преподавателя", HeadingLevel.HEADING_2),
    p("APK, файрвол, «я удалил Identity», розовый консоль полный ошибок, чужой Wi‑Fi."),
  ]);
}

async function idea() {
  await saveDoc("list-idei.docx", [
    h("Лист идеи (1 страница)"),
    p("В этих часах идея = один сетевой объект в комнате 01_Room. Не симулятор отрасли. Grab можно по образцу куба."),
    p("Образование → табличка / модель, которую видят оба."),
    p("Игра → цветной ориентир или зона (слот)."),
    p("Медицина → «аппарат»-объект с подписью."),
    p("Социалка → место встречи, два маркера."),
    p("Промышленность → деталь на подставке."),
    p("Спорт → мяч-куб (grab)."),
    p("Маркетинг → витрина-табличка."),
    p("Инклюзия → крупный контрастный объект."),
    p("Туризм → «экспонат музея» (пример Т4)."),
    p("Направление: ______________  Объект: ______________  Grab: да / нет"),
    p("32 часа защиты на этот лист не вешать."),
  ]);
}

async function teacher() {
  await saveDoc("shpargalka-prepod.docx", [
    h("Шпаргалка преподавателя"),
    p("Проект: D:\\Work\\ТехнопаркВР\\smena-starter  Unity 6000.4.5f1"),
    p("Меню Smena → Build Starter Scenes. Bundle id by.ndt.vidr.smena2026 ARM64."),
    p("Пакеты: URP, OpenXR, Oculus XR, XRI. Mirror уже в Assets/Mirror (v96.11.2), не через git UPM. Не ставить SteamVR, PICO, Photon, NGO, Relay."),
    p("DOTween — Asset Store, опционально. Панель лобби и без него гаснет."),
    p("Сборка Android: Developer Mode на Quest, MQDH или adb install. Один APK на смену."),
    p("Сеть: один сегмент Wi‑Fi, выключить AP isolation. Порт 7777 KCP. Файрвол: разрешить Unity Editor и Windows-билд."),
    p("Проверка за день: два ПК, два шлема, чужой IP, grab куба виден на хосте."),
    p("10× Play убивают GPU → Windows-хост, не Editor."),
    p("Резать практику: (1) свой скрипт → табличка Т4; (2) серый IP; (3) кастомный grab → учебный куб; (4) не резать коннект лобби."),
    p("Авария UI лобби: хост сразу в комнату, Т4 в этот час только со слайда."),
    p("Хост без XR: grab мышкой не проверять. Смотреть окно ПК или второй Quest."),
    p("Oculus Mirror ≠ библиотека Mirror."),
    p("Скрины железа не выдумывать. Только Quest 2/3 этого зала."),
  ]);
}

async function slides() {
  const pres = new PptxGenJS();
  pres.layout = "LAYOUT_WIDE";
  pres.defineLayout({ name: "WIDE", width: 13.33, height: 7.5 });
  pres.layout = "WIDE";
  const bg = "0B1020";
  const yel = "F5C518";
  const ice = "E8EEF7";
  function slide(title) {
    const s = pres.addSlide();
    s.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 13.33, h: 7.5, fill: { color: bg } });
    s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.5, y: 0.4, w: 0.35, h: 0.35, fill: { color: yel }, rectRadius: 0.06 });
    s.addText(title, { x: 1.05, y: 0.32, w: 11.5, h: 0.55, fontFace: "Calibri", fontSize: 32, bold: true, color: ice, margin: 0 });
    return s;
  }
  let s = pres.addSlide();
  s.addShape(pres.shapes.RECTANGLE, { x: 0, y: 0, w: 13.33, h: 7.5, fill: { color: bg } });
  s.addText("ПК варит мир.\nШлем в него заходит.", { x: 0.7, y: 1.6, w: 12, h: 2.2, fontFace: "Calibri", fontSize: 40, bold: true, color: ice, margin: 0 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.7, y: 4.2, w: 5.4, h: 2.2, fill: { color: "161C2C" }, rectRadius: 0.12 });
  s.addText("ХОСТ — Windows", { x: 0.95, y: 4.4, w: 5, h: 0.5, fontFace: "Calibri", fontSize: 22, bold: true, color: yel, margin: 0 });
  s.addText("Нет XR-камеры. Кнопка HOST. Видит чужой grab.", { x: 0.95, y: 5.0, w: 5, h: 1.1, fontFace: "Calibri", fontSize: 18, color: ice, margin: 0 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 7.2, y: 4.2, w: 5.4, h: 2.2, fill: { color: "161C2C" }, rectRadius: 0.12 });
  s.addText("КЛИЕНТ — Quest 2/3", { x: 7.45, y: 4.4, w: 5, h: 0.5, fontFace: "Calibri", fontSize: 22, bold: true, color: yel, margin: 0 });
  s.addText("CONNECT на IP своего ПК. Не PICO.", { x: 7.45, y: 5.0, w: 5, h: 1.1, fontFace: "Calibri", fontSize: 18, color: ice, margin: 0 });

  s = slide("Unity этого проекта");
  s.addText("Сцена · Префаб · Play · две платформы (Windows и Android).\nНе курс движка — тетрадь Т1.", { x: 0.7, y: 1.3, w: 12, h: 1.4, fontFace: "Calibri", fontSize: 24, color: ice, margin: 0 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.7, y: 3.0, w: 5.6, h: 3.2, fill: { color: "161C2C" }, rectRadius: 0.12 });
  s.addText("00_Lobby", { x: 0.95, y: 3.25, w: 5.1, h: 0.5, fontFace: "Calibri", fontSize: 22, bold: true, color: yel, margin: 0 });
  s.addText("HOST, IP, счётчик, «В комнату»", { x: 0.95, y: 3.9, w: 5.1, h: 1.8, fontFace: "Calibri", fontSize: 20, color: ice, margin: 0 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 7.0, y: 3.0, w: 5.6, h: 3.2, fill: { color: "161C2C" }, rectRadius: 0.12 });
  s.addText("01_Room", { x: 7.25, y: 3.25, w: 5.1, h: 0.5, fontFace: "Calibri", fontSize: 22, bold: true, color: yel, margin: 0 });
  s.addText("Кубы grab и слоты идеи", { x: 7.25, y: 3.9, w: 5.1, h: 1.8, fontFace: "Calibri", fontSize: 20, color: ice, margin: 0 });

  s = slide("Mirror — сеть, не зеркало шлема");
  s.addText("NetworkManager. Transport KCP. Порт 7777.\nПоля Player Prefab и Transport детям не чистить.", { x: 0.7, y: 1.3, w: 12, h: 1.6, fontFace: "Calibri", fontSize: 24, color: ice, margin: 0 });
  s.addText("Тетрадь Т1–Т2", { x: 0.7, y: 6.4, w: 12, h: 0.4, fontFace: "Calibri", fontSize: 16, color: yel, margin: 0 });

  s = slide("NetworkIdentity");
  s.addText("Объект, который Mirror имеет право показать всем.", { x: 0.7, y: 1.4, w: 12, h: 1.0, fontFace: "Calibri", fontSize: 28, color: ice, margin: 0 });
  s.addText("SyncVar одной фразой: число на ПК и на шлеме одно и то же.\nСчётчик лобби — это оно.", { x: 0.7, y: 2.7, w: 12, h: 1.4, fontFace: "Calibri", fontSize: 22, color: ice, margin: 0 });
  s.addShape(pres.shapes.ROUNDED_RECTANGLE, { x: 0.7, y: 4.4, w: 12, h: 1.8, fill: { color: "161C2C" }, rectRadius: 0.12 });
  s.addText("Снял Identity с куба — куб едет только у тебя.", { x: 0.95, y: 4.85, w: 11.5, h: 0.9, fontFace: "Calibri", fontSize: 22, color: yel, margin: 0 });

  s = slide("Комната ожидания");
  s.addText("Зачем ждать: все должны оказаться в одной комнате разом.", { x: 0.7, y: 1.3, w: 12, h: 0.8, fontFace: "Calibri", fontSize: 24, color: ice, margin: 0 });
  s.addText("Счётчик = тебя посчитали.\n«В комнату» жмёт только ХОСТ.", { x: 0.7, y: 2.4, w: 12, h: 1.6, fontFace: "Calibri", fontSize: 26, color: ice, margin: 0 });
  s.addText("Тетрадь Т2 и Т4", { x: 0.7, y: 6.4, w: 12, h: 0.4, fontFace: "Calibri", fontSize: 16, color: yel, margin: 0 });

  s = slide("Quest: CONNECT в лобби");
  s.addText("IP своего ПК → CONNECT.\nПризнак успеха: счётчик живой, не чёрный экран.", { x: 0.7, y: 1.5, w: 12, h: 1.8, fontFace: "Calibri", fontSize: 28, color: ice, margin: 0 });
  s.addText("Жёлтый — свой IP. Серый — IP соседа.", { x: 0.7, y: 3.8, w: 12, h: 0.8, fontFace: "Calibri", fontSize: 22, color: yel, margin: 0 });

  s = slide("Grab");
  s.addText("Хватаем предмет с Identity.\nХост видит. Хост не хватает руками.", { x: 0.7, y: 1.5, w: 12, h: 1.8, fontFace: "Calibri", fontSize: 28, color: ice, margin: 0 });
  s.addText("Тетрадь Т3. Компоненты куба не разбирать «для красоты».", { x: 0.7, y: 3.8, w: 12, h: 1.0, fontFace: "Calibri", fontSize: 20, color: ice, margin: 0 });

  s = slide("Чужой IP = чужое лобби");
  s.addText("Чек-лист: Wi‑Fi · HOST горит · 7777 · файрвол · одна сеть.", { x: 0.7, y: 1.6, w: 12, h: 1.4, fontFace: "Calibri", fontSize: 26, color: ice, margin: 0 });
  s.addText("Гостевая сеть лаборатории убивает серый трек.", { x: 0.7, y: 3.4, w: 12, h: 1.0, fontFace: "Calibri", fontSize: 22, color: yel, margin: 0 });

  s = slide("Своя идея = объект в 01_Room");
  s.addText("Не новый Unity-проект.\nРецепт таблички — тетрадь Т4.", { x: 0.7, y: 1.5, w: 12, h: 1.8, fontFace: "Calibri", fontSize: 28, color: ice, margin: 0 });
  s.addText("Лист идеи: одно направление → один сетевой объект.", { x: 0.7, y: 3.8, w: 12, h: 1.0, fontFace: "Calibri", fontSize: 22, color: ice, margin: 0 });

  s = slide("Сегодня тетрадь, не этот слайд");
  s.addText("Т1 Unity/HOST · Т2 лобби · Т3 grab · Т4 идея", { x: 0.7, y: 1.5, w: 12, h: 1.2, fontFace: "Calibri", fontSize: 26, color: ice, margin: 0 });
  s.addText("Звать преподавателя: APK, Wi‑Fi, «я удалил Identity».", { x: 0.7, y: 3.2, w: 12, h: 1.2, fontFace: "Calibri", fontSize: 24, color: yel, margin: 0 });

  await pres.writeFile({ fileName: path.join(outDir, "smena-10-slides.pptx") });
  console.log("wrote slides");
}

(async () => {
  await t1(); await t2(); await t3(); await t4(); await idea(); await teacher(); await slides();
})().catch((e) => { console.error(e); process.exit(1); });
