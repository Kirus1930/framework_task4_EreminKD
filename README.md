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

В данном проекте реализована веб-служба на ASP.NET Core, моделирующая процесс бронирования комнаты с использованием машины состояний, поддержкой идемпотентности, компенсации при сбоях и средствами наблюдаемости.

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
# Создание новой записи (изменение состояния: None -> Created)
curl -X POST "http://localhost:5000/booking?processId=1&action=create" \
 -H "Idempotency-Key: 1" \
 -H "X-Correlation-Id: abc"

# Бронирование (если использовать перед create, не будет перехода: Created -> RoomReserved)
curl -X POST "http://localhost:5000/booking?processId=1&action=reserve" \
 -H "Idempotency-Key: 2" \
 -H "X-Correlation-Id: abc"

# Оплата (RoomReserved -> PaymentProcessed, если возникнет ошибка "Payment failed", то: RoomReserved -> Error: Payment failed -> Сompensation -> Created)
curl -X POST "http://localhost:5000/booking?processId=1&action=pay" \
 -H "Idempotency-Key: 3" \
 -H "X-Correlation-Id: abc"

# Завершение (PaymentProcessed -> Completed)
curl -X POST "http://localhost:5000/booking?processId=1&action=complete" \
 -H "Idempotency-Key: 4" \
 -H "X-Correlation-Id: abc"
```

### Проверка состояния сервиса:
```bash
http://localhost:5000/health/live
http://localhost:5000/health/ready
```

### Пример запуска:
```bash
PS C:\Users\Kiril\Documents\GitHub\framework_task4_EreminKD\BookingService> dotnet run
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\Kiril\Documents\GitHub\framework_task4_EreminKD\BookingService
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/ - - -
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/ - 404 0 - 90.2057ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[16]
      Request reached the end of the middleware pipeline without being handled by application code. Request path: GET http://localhost:5000/, Response status code: 404
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/health/live - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'Health checks'
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'Health checks'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/health/live - 200 - text/plain 72.5472ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/health/ready - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'Health checks'
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'Health checks'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/health/ready - 200 - text/plain 7.7108ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 POST http://localhost:5000/booking?processId=1&action=create - - 0
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'BookingService.Controllers.BookingController.Process (BookingService)'
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[102]
      Route matched with {action = "Process", controller = "Booking"}. Executing controller action with signature System.Threading.Tasks.Task`1[Microsoft.AspNetCore.Mvc.IActionResult] Process(System.String, System.String, System.String, System.String) on controller BookingService.Controllers.BookingController (BookingService).
info: BookingService.Services.BookingService[0]
      State: Created
info: Microsoft.AspNetCore.Mvc.StatusCodeResult[1]
      Executing StatusCodeResult, setting HTTP status code 200
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[105]
      Executed action BookingService.Controllers.BookingController.Process (BookingService) in 57.9397ms
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'BookingService.Controllers.BookingController.Process (BookingService)'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://localhost:5000/booking?processId=1&action=create - 200 0 - 184.7438ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 POST http://localhost:5000/booking?processId=1&action=reserve - - 0
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'BookingService.Controllers.BookingController.Process (BookingService)'
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[102]
      Route matched with {action = "Process", controller = "Booking"}. Executing controller action with signature System.Threading.Tasks.Task`1[Microsoft.AspNetCore.Mvc.IActionResult] Process(System.String, System.String, System.String, System.String) on controller BookingService.Controllers.BookingController (BookingService).
info: BookingService.Services.BookingService[0]
      State: RoomReserved
info: Microsoft.AspNetCore.Mvc.StatusCodeResult[1]
      Executing StatusCodeResult, setting HTTP status code 200
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[105]
      Executed action BookingService.Controllers.BookingController.Process (BookingService) in 1.2706ms
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'BookingService.Controllers.BookingController.Process (BookingService)'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://localhost:5000/booking?processId=1&action=reserve - 200 0 - 1.9769ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 POST http://localhost:5000/booking?processId=1&action=pay - - 0
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'BookingService.Controllers.BookingController.Process (BookingService)'
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[102]
      Route matched with {action = "Process", controller = "Booking"}. Executing controller action with signature System.Threading.Tasks.Task`1[Microsoft.AspNetCore.Mvc.IActionResult] Process(System.String, System.String, System.String, System.String) on controller BookingService.Controllers.BookingController (BookingService).
info: BookingService.Services.BookingService[0]
      State: PaymentProcessed
info: Microsoft.AspNetCore.Mvc.StatusCodeResult[1]
      Executing StatusCodeResult, setting HTTP status code 200
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[105]
      Executed action BookingService.Controllers.BookingController.Process (BookingService) in 25.8695ms
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'BookingService.Controllers.BookingController.Process (BookingService)'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 POST http://localhost:5000/booking?processId=1&action=pay - 200 0 - 26.5686ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/health/live - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'Health checks'
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'Health checks'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/health/live - 200 - text/plain 0.5612ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/health/ready - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'Health checks'
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'Health checks'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/health/ready - 200 - text/plain 0.6612ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/favicon.ico - - -
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/favicon.ico - 404 0 - 0.2428ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[16]
      Request reached the end of the middleware pipeline without being handled by application code. Request path: GET http://localhost:5000/favicon.ico, Response status code: 404
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/booking?processId=1&action=create - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint '405 HTTP Method Not Supported'
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint '405 HTTP Method Not Supported'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/booking?processId=1&action=create - 405 0 - 4.8020ms
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/booking?processId=1 - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint '405 HTTP Method Not Supported'
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint '405 HTTP Method Not Supported'
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/booking?processId=1 - 405 0 - 0.4241ms
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
```

### Пример тестирования:
```bash
PS C:\Users\Kiril\Documents\GitHub\framework_task4_EreminKD\BookingService.Tests> dotnet test   
Восстановление завершено (0,7 с)
  BookingService net8.0 успешно выполнено (5,6 с) → C:\Users\Kiril\Documents\GitHub\framework_task4_EreminKD\BookingService\bin\Debug\net8.0\BookingService.dll
  BookingService.Tests net8.0 успешно выполнено (1,2 с) → bin\Debug\net8.0\BookingService.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.0.1+5ebf84cd75 (64-bit .NET 8.0.25)
[xUnit.net 00:00:00.09]   Discovering: BookingService.Tests
[xUnit.net 00:00:00.14]   Discovered:  BookingService.Tests
[xUnit.net 00:00:00.14]   Starting:    BookingService.Tests
[xUnit.net 00:00:00.40]   Finished:    BookingService.Tests
  BookingService.Tests (тест) net8.0 успешно выполнено (1,6 с)

Сводка теста: всего: 3; сбой: 0; успешно: 3; пропущено: 0; длительность: 1,6 с
Сборка успешно выполнено через 9,9 с
```

Проект реализует веб-службу с машиной состояний, которая управляет процессом бронирования. Сервис обрабатывает события идемпотентно, поддерживает компенсацию при сбоях и обеспечивает наблюдаемость через логи и метрики. Это демонстрирует базовые принципы построения устойчивых распределённых систем.