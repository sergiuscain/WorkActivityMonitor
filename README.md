# Work Activity Monitor

Тестовое задание: клиент-серверное приложение для мониторинга рабочей активности сотрудников.

## Что делает

Клиент (Windows) работает в фоне на компьютере сотрудника: раз в 10 секунд отправляет на сервер heartbeat с данными о машине и пользователе. Сервер ведёт список подключённых клиентов, показывает время последней активности и по запросу получает скриншот рабочего стола.

## Стек

- **Сервер:** ASP.NET Core Web API (.NET 10), EF Core + SQLite, Swagger.
- **Клиент:** WinForms без формы (.NET 10), работает в фоне.
- На клиенте — только BCL, сторонних NuGet-пакетов нет (условие задания).

## Структура решения

```
WorkActivityMonitor.sln
├── WorkActivityMonitor/          — сервер (ASP.NET Core Web API)
│   ├── Controllers/
│   ├── Data/
│   ├── Models/
│   ├── Services/
│   ├── Migrations/
│   └── Storage/screenshots/      — сохранённые скриншоты
└── Client/                       — клиент (WinForms без формы)
    ├── Core/
    ├── Models/
    └── logs/                     — локальный лог клиента
```

## Протокол

Обмен по HTTP + JSON.

### Heartbeat

Клиент раз в 10 секунд шлёт:

```http
POST /api/clients/heartbeat
Content-Type: application/json

{
  "machineName": "DESKTOP-ABC",
  "userName": "Ivanov",
  "domain": "CORP"
}
```

Сервер по тройке `(MachineName, UserName, Domain)` находит клиента и обновляет `LastActiveTime`. Если клиента нет — создаёт. IP определяется на сервере из `HttpContext.Connection.RemoteIpAddress`.

Ответ:

```json
{
  "clientId": 3,
  "takeScreenshot": false,
  "heartbeatIntervalSeconds": 10
}
```

Если `takeScreenshot = true`, клиент делает скриншот и отправляет его отдельным запросом.

### Скриншот

```
POST /api/clients/{id}/screenshot   — загрузка PNG (multipart/form-data)
GET  /api/clients/{id}/screenshot   — последний скриншот
GET  /api/clients/{id}/screenshots  — список метаданных
```

## API

| Метод | Путь | Описание |
|---|---|---|
| POST | `/api/clients/heartbeat` | Heartbeat от клиента |
| GET | `/api/clients` | Список всех клиентов |
| GET | `/api/clients/{id}` | Информация о клиенте |
| POST | `/api/clients/{id}/request-screenshot` | Запросить скриншот |
| POST | `/api/clients/{id}/screenshot` | Загрузить скриншот |
| GET | `/api/clients/{id}/screenshot` | Последний скриншот |
| GET | `/api/clients/{id}/screenshots` | Список скриншотов |

Swagger: `https://localhost:7068/swagger`.

## Запуск сервера

```
cd WorkActivityMonitor
dotnet ef database update    # или Update-Database в PMC
dotnet run
```

База — SQLite, файл `workmonitor.db` создаётся автоматически при первой миграции.

## Запуск клиента

```
cd Client
dotnet run
```

Или открыть `Client.exe` из `bin/Debug/net10.0-windows/`.

Клиент не показывает окна — работает в фоне. Лог пишется в `bin/Debug/net10.0-windows/logs/client.log`.

## Автозапуск

При первом запуске клиент добавляет себя в:

```
HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Run
```

Параметр `WorkActivityMonitor` со значением — полный путь к `.exe`. Запись в пользовательской ветке реестра не требует прав администратора и срабатывает при входе текущего пользователя в Windows.

Чтобы убрать автозапуск — удалить параметр `WorkActivityMonitor` из указанной ветки. Проверить также `WOW6432Node\Microsoft\Windows\CurrentVersion\Run` — зависит от разрядности процесса.

## Статус клиента

Сервер не хранит флаг «онлайн/оффлайн». Он хранит `LastActiveTime` и вычисляет статус на лету:

```
IsOnline = (Now - LastActiveTime).TotalSeconds < 30
```

Порог 30 секунд — три интервала heartbeat. Если сигнала нет дольше, клиент считается оффлайн.

## Обработка ошибок

Клиент устойчив к недоступности сервера: при ошибке соединения логирует сообщение и продолжает попытки каждые 10 секунд. При восстановлении сервера heartbeat возобновляется без перезапуска клиента.

## Ограничения

- Захват экрана — только основной монитор (`Screen.PrimaryScreen`). Многомониторные конфигурации не поддержаны.
- Клиент принимает самоподписанный сертификат сервера (`DangerousAcceptAnyServerCertificateValidator`) — только для локальной отладки. В продакшене это нужно убрать.
- Хранение скриншотов — файлы на диске, путь в БД. Ротация и очистка не реализованы.
- Аутентификации нет. В реальной системе heartbeat должен быть защищён токеном.
- 
