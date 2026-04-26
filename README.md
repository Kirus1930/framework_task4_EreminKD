# Технологии разработки приложений на базе фреймворков. Задание № 4.
## Выполнил: Еремин Кирилл Денисович, ЭФБО-10-23

### Запуск
```bash
cd BookingService

// Если решение еще не собрано
dotnet build

// Если решение собрано
dotnet restore

dotnet run

// Запуск тестов
cd BookingService.Tests

dotnet test
```

В данном проекте реализована учебная веб-служба на ASP.NET Core, моделирующая процесс бронирования переговорной комнаты с использованием машины состояний, поддержкой идемпотентности, компенсации при сбоях и средствами наблюдаемости.

Проект демонстрирует:

1. Реализацию машины состояний (state machine)
2. Обработку событий с учетом идемпотентности
3. Устойчивость к повторной доставке событий
4. Обработку частичных сбоев
5. Механизм компенсации (rollback)
6. Централизованное логирование с correlation id
7. Сбор метрик (успешные/ошибочные переходы, повторы, компенсации, задержки)
8. Проверки состояния сервиса (health checks)

### Рабочий цикл:
При обработке запроса выполняются следующие шаги:
1. Принимается HTTP-запрос с processId, action
2. Из заголовков извлекаются:
* Idempotency-Key
* X-Correlation-Id
3. Загружается или создается процесс из in-memory хранилища
4. Проверяется идемпотентность:
* если событие уже обработано — оно игнорируется
5. Выполняется переход состояния:
* None -> Created
* Created -> RoomReserved
* RoomReserved -> PaymentProcessed
* PaymentProcessed -> Completed
6. При ошибке на шаге оплаты:
* выполняется компенсация (возврат в состояние Created)
* Записываются логи с correlation id
7. Обновляются метрики:
* успешные переходы
* ошибки
* повторы
* компенсации
* время выполнения шага

### Паттерны:
1. State Machine
Где используется:
```
BookingService
```

Суть:

Управление жизненным циклом процесса через состояния и переходы.

2. Idempotency Pattern
Где используется:
```
ProcessedEvents (HashSet)
```

Суть:

Повторная обработка одного и того же события не влияет на состояние системы.

3. Saga / Compensation Pattern
Где используется:
```
BookingService (rollback)
```

Суть:

При сбое выполняется откат предыдущего шага.

Решает:
* частичные сбои
* неконсистентность данных

4. Dependency Injection (DI)
Где используется:
```
builder.Services.AddSingleton()
```

Суть:

Передача зависимостей через контейнер.

5. Singleton
Где используется:
```
InMemoryProcessRepository
```

Суть:

Хранение состояния процессов в одном экземпляре.

6. Observability
Где используется:
```
ILogger; Metrics
```

Суть:

Позволяет отслеживать поведение системы и причины ошибок.

7. Health Check Pattern
```
/health/live
/health/ready 
```

Суть:

Проверка работоспособности и готовности сервиса.

### Пример API:
```bash
curl -X POST "http://localhost:5000/booking?processId=1&action=create" \
 -H "Idempotency-Key: 1" \
 -H "X-Correlation-Id: abc"

curl -X POST "http://localhost:5000/booking?processId=1&action=reserve" \
 -H "Idempotency-Key: 2" \
 -H "X-Correlation-Id: abc"

curl -X POST "http://localhost:5000/booking?processId=1&action=pay" \
 -H "Idempotency-Key: 3" \
 -H "X-Correlation-Id: abc"
```

### Проверка состояния сервиса:
```bash
http://localhost:5000/health/live
http://localhost:5000/health/ready
```

### Пример запуска:
```bash

```

### Пример тестирования:
```bash

```