# Simple Blocks — Backend (API)

Бэкенд для генератора стандартных блоков [simple-blocks-frontend](https://github.com/gabriellesunrise/simple-blocks/).
Обеспечивает **анонимную авторизацию по сид-фразе** (аналог криптокошелька) и **сохранение блоков с настройками**.

## Философия: Zero-Knowledge и полная анонимность

Проект принципиально **не собирает персональные данные**: ни email, ни телефона, ни паролей.
Единственный «аккаунт» — это **сид-фраза**, которую пользователь генерирует и хранит у себя.

- Сид-фраза → **Ed25519-ключевая пара** (выводится **только на клиенте**).
- На сервер попадает **только публичный ключ** (анонимный «адрес») и подпись над вызовом-челленджем.
- Даже при полном доступе к базе данных сервер **не может** войти ни за одного пользователя —
  у него лишь публичные ключи, без приватных.

```
Клиент (сид-фраза)
   │  PBKDF2(seed, salt, 600k) → 32 байта → Ed25519
   │  приватный ключ НИКОГДА не покидает клиент
   ▼
Регистрация:  { publicKey }                    → сервер хранит publicKey
Логин:        { challenge → подпись(nonce) }   → сервер проверяет подпись публичным ключом
```

## Структура (Clean Architecture)

```
SimpleBlocks.slnx
├── SimpleBlocks.Domain          # сущности: SeedAccount, SavedBlock
├── SimpleBlocks.Application     # Use Cases: IAuthService, IBlockService, DTO
├── SimpleBlocks.Infrastructure  # EF Core + SQLite, репозитории, Ed25519, JWT, челленджи
└── SimpleBlocks.API             # ASP.NET Core: контроллеры, конфигурация, Program.cs
```

## Как запустить локально

Есть два пути: напрямую из исходников (нужен .NET SDK 10) или через Docker.

### Вариант A — прямо из исходников

```bash
cd simple-blocks-backend
dotnet restore
dotnet publish SimpleBlocks.API -c Release   # или просто dotnet build
dotnet run --project SimpleBlocks.API
```

По умолчанию API слушает:
- HTTP  `http://localhost:5182`
- HTTPS `https://localhost:7264`  (dev-сертификат)

При первом запуске создаётся SQLite-база `simple-blocks.db` (`EnsureCreated`).

### Вариант B — Docker (локально и на VPS одинаково)

```bash
cd simple-blocks-backend

# 1) Сгенерировать секрет и положить в .env (в репозитории есть .env.example)
openssl rand -hex 32
# скопировать .env.example в .env и подставить значение в JWT_SECRET

# 2) Собрать образ и запустить контейнер
docker compose build
docker compose up -d

# API будет доступен на http://localhost:8080
```

- Конфигурация — в `docker-compose.yml` (порт, CORS, строка подключения).
- Данные (SQLite) живут в папке `./data` на хосте и переживают перезапуск.
- Остановить: `docker compose stop`; удалить образ/контейнер: `docker compose down`.

> При необходимости переведите `EnsureCreated` на EF Core-миграции (`dotnet ef migrations`),
> чтобы легко развивать схему.

## Конфигурация (`SimpleBlocks.API/appsettings.json`)

| Ключ | Назначение | VPS |
| --- | --- | --- |
| `ConnectionStrings:Default` | SQLite-строка подключения | можно заменить на PostgreSQL |
| `Jwt:Secret` | Секрет подписи JWT (>= 32 символа) | **обязательно** переопределить через env `Jwt__Secret` |
| `Jwt:Issuer` / `Jwt:Audience` | Эмитент/аудитория токена | по умолчанию `SimpleBlocks` |
| `Jwt:ExpiryMinutes` | Срок жизни токена (мин) | |
| `Cors:AllowedOrigins` | Разрешённые источники фронтенда | добавить адрес продакшена |

## Эндпоинты

### Публичные

| Метод | Путь | Описание |
| --- | --- | --- |
| GET | `/api/seed/wordlist` | Список из 2048 слов (BIP39) для генерации сид-фразы на клиенте |

### Авторизация

| Метод | Путь | Тело → Ответ |
| --- | --- | --- |
| POST | `/api/auth/register` | `{ publicKey, displayName? }` → `{ accountId, publicKey, displayName, createdAtUtc }` |
| POST | `/api/auth/challenge` | `{ accountId }` → `{ accountId, nonce }` |
| POST | `/api/auth/login` | `{ accountId, nonce, signature }` → `{ token, accountId, publicKey, displayName }` |
| GET | `/api/auth/me` *(Bearer)* | → `{ accountId, publicKey, displayName, createdAtUtc }` |

**Протокол логина:**
1. Клиент запрашивает `nonce` (`POST /api/auth/challenge`) — одноразовый, живёт 5 минут.
2. Клиент подписывает **сырые байты** nonce (Base64-декодированные) приватным ключом (Ed25519).
3. Клиент отправляет `signature` (Base64) в `POST /api/auth/login`.
4. Сервер проверяет подпись сохранённым публичным ключом и выдаёт JWT.

### Сохранение блоков *(требуется Bearer-токен)*

| Метод | Путь | Описание |
| --- | --- | --- |
| GET | `/api/blocks` | Список блоков аккаунта (порядок: `sortOrder`, затем дата) |
| POST | `/api/blocks` | Создать блок |
| PUT | `/api/blocks/{id}` | Обновить (только владелец) |
| DELETE | `/api/blocks/{id}` | Удалить (только владелец) |
| POST | `/api/blocks/reorder` | `{ "ids": [ ... ] }` — сохранить порядок (только владелец) |

Тело создания/обновления:

```json
{
  "blockType": "vertical",
  "name": "Наши услуги",
  "settings": { "count": "3", "img": true, "heading-text": "Наши услуги", "block-0-title-text": "..." },
  "html": "<div class=\"v-container\">...</div>",
  "css": ".v-container { ... }",
  "js": ""
}
```

- `blockType` — одно из: `vertical, horizontal, overlay, faq, scroll, table, gallery, banner`.
- `settings` — плоский JSON-объект настроек (полный снапшот формы: статичные поля + поля контента).
- `html`, `css`, `js` — сгенерированный код на момент сохранения (для копирования и превью в «Мои блоки»).
- `sortOrder` присваивается сервером автоматически (последний + 1); переупорядочивание — через `/reorder`.

## Контракт для фронтенда (реализовано)

Клиентский код уже реализован во фронтенде и использует именно этот протокол:

1. `GET /api/seed/wordlist` — словарь (2048 слов).
2. Генерация 24 слов (криптографически случайно, на клиенте).
3. Ключ: `PBKDF2(seed, "simple-blocks-client", 600_000, SHA256, 32)` → Ed25519 (WebCrypto + собственный модуль вывода ключа; без библиотек).
4. `POST /api/auth/register` с `publicKey` (Base64, 32 байта публичного ключа).
5. `POST /api/auth/challenge` → подписать сырые байты `nonce` → `POST /api/auth/login`.
6. Дальше JWT в `Authorization: Bearer <token>`.

Файлы фронтенда:
- `js/auth.js` — сид → ключ, регистрация/вход/выход, модалка, сессия (токен в `localStorage`, сид в `sessionStorage`).
- `js/save.js` — «Сохранить в Мои блоки» в каждой секции, редактирование (применение настроек к форме).
- `js/myblocks.js` — экран «Мои блоки» (список, порядок, удаление, копирование кода, превью в Shadow DOM).
- `js/ed25519.js` — собственный модуль вывода публичного ключа Ed25519 из seed (RFC 8032); подпись делает нативный WebCrypto (PKCS#8). Внешних библиотек нет.

## Запуск на VPS

1. Убедитесь, что на сервере есть Docker Engine + compose-плагин (Linux; amd64/arm64).
2. Склонируйте репозиторий: `git clone ...` (понадобятся исходники — `docker compose build`).
3. Создайте файл `.env` с надёжным `JWT_SECRET` (`openssl rand -hex 32`).
4. Пропишите в `docker-compose.yml` (или env) адрес фронтенда в `Cors:AllowedOrigins`.
5. Запустите: `docker compose up -d --build`.
6. Поставьте перед контейнером reverse-proxy (Caddy/nginx) на **HTTPS** с привязкой к вашему домену
   (Let's Encrypt). Наружу открыты порты 80/443; внутренний порт 8080 наружу не пробрасывать.
7. Данные — в `./data` на диске сервера; регулярно делайте бэкап этой папки.

Приложение отдаёт `/healthz` (200 OK) для проверки работоспособности.

### Полезные команды
- `docker compose ps` — статус
- `docker compose logs -f api` — логи
- `docker compose stop` / `docker compose down` — остановить/удалить
- `docker compose build --no-cache` — пересобрать с чистого кэша

## Безопасность

- Сид и приватный ключ **не** хранятся и **не** передаются на сервер.
- Пароль для вывода ключа выполняет клиент; сервер хранит только публичный ключ.
- `nonce` одноразовый (удаляется после `login`) — защита от повтора.
- JWT подписывается HMAC-SHA256 (HS256) секретом сервера.
