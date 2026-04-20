# Технологии разработки приложений на базе фреймворков. Задание № 4.
## Выполнил: Еремин Кирилл Денисович, ЭФБО-10-23

### Запуск
```bash
cd Task3

// Если решение еще не собрано
dotnet build

// Если решение собрано
dotnet restore

dotnet run
```
```bash
// Запуск тестов
cd Task3.Tests

dotnet test
```

В данном проекте реализована веб-служба на ASP.NET Core с поддержкой гибкой конфигурации и базовой защитой от некорректных и вредоносных запросов.

Проект демонстрирует:
1. загрузку конфигурации из нескольких источников
2. управление приоритетом настроек
3. раннюю валидацию конфигурации (fail fast)
4. защиту от несанкционированных источников (CORS)
5. ограничение частоты запросов (rate limiting)
6. использование защитных HTTP-заголовков
7. поддержку двух режимов работы (dev / prod)

### Рабочий цикл:
При запуске выполняются следующие шаги:
1. Читается конфигурация из appsettings.json
2. Переопределяются значения из переменных окружения
3. Переопределяются значения из аргументов командной строки
4. Выполняется валидация конфигурации
5. При ошибке конфигурации приложение завершает работу
6. Регистрируются сервисы в DI контейнере
7. Запускается веб-сервер
8. Каждый запрос проходит через middleware:
* проверка rate limit
* проверка origin
* добавление защитных заголовков

### Паттерны:
1. Configuration Pattern
Где используется:
```
ConfigService
```

Суть:

Централизованное управление настройками приложения.

2. Factory
Где используется:
```
ConfigService.Load()
```

Суть:

Создание объекта конфигурации с инкапсуляцией логики.

3. Middleware Pattern
Где используется:
```
SecurityMiddleware
```

Суть:

Обработка запросов через цепочку промежуточных обработчиков.

4. Dependency Injection (DI)
Где используется:
```
builder.Services.AddSingleton(...)
```

Суть:

Зависимости передаются через контейнер.

5. Singleton
Где используется:
```
Config и RateLimiterService
```

Суть:

Один экземпляр используется во всём приложении.

6. Guard / Fail Fast
Где используется:
```
Validate()
```

Суть:

Приложение сразу завершает работу при некорректной конфигурации.

Решает:

* запуск в небезопасном состоянии

7. Separation of Concerns
```
ConfigService - конфигурация  
Middleware - безопасность  
Controllers - API  
Services - логика  
Tests - проверка  
```

### Пример запуска:
```bash
PS C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3> dotnet run    
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Microsoft.Hosting.Lifetime[0]
      Hosting environment: Production
info: Microsoft.Hosting.Lifetime[0]
      Content root path: C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3
info: Microsoft.AspNetCore.Hosting.Diagnostics[1]
      Request starting HTTP/1.1 GET http://localhost:5000/items - - -
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[0]
      Executing endpoint 'ItemsController.Get (Task3)'
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[102]
      Route matched with {action = "Get", controller = "Items"}. Executing controller action with signature Microsoft.AspNetCore.Mvc.IActionResult Get() on controller ItemsController (Task3).
info: Microsoft.AspNetCore.Mvc.Infrastructure.ObjectResultExecutor[1]
      Executing OkObjectResult, writing value of type '<>f__AnonymousType0`1[[System.String[], System.Private.CoreLib, Version=8.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e]]'.
info: Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker[105]
      Executed action ItemsController.Get (Task3) in 60.0325ms
info: Microsoft.AspNetCore.Routing.EndpointMiddleware[1]
      Executed endpoint 'ItemsController.Get (Task3)'
# Переходим по адресу: http://localhost:5000/items
info: Microsoft.AspNetCore.Hosting.Diagnostics[2]
      Request finished HTTP/1.1 GET http://localhost:5000/items - 200 - application/json;+charset=utf-8 196.6426ms
info: Microsoft.Hosting.Lifetime[0]
      Application is shutting down...
PS C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3>
```

### Пример тестирования:
```bash
PS C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3.Tests> dotnet test   
Восстановление завершено (0,5 с)
  Task3 net8.0 успешно выполнено (0,8 с) → C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3\bin\Debug\net8.0\Task3.dll
  Task3.Tests net8.0 успешно выполнено (0,4 с) → bin\Debug\net8.0\Task3.Tests.dll
[xUnit.net 00:00:00.00] xUnit.net VSTest Adapter v2.5.0.1+5ebf84cd75 (64-bit .NET 8.0.25)
[xUnit.net 00:00:00.07]   Discovering: Task3.Tests
[xUnit.net 00:00:00.10]   Discovered:  Task3.Tests
[xUnit.net 00:00:00.10]   Starting:    Task3.Tests
[xUnit.net 00:00:00.31]     ConfigTests.InvalidConfigThrows [FAIL]
[xUnit.net 00:00:00.31]       Assert.Throws() Failure: No exception was thrown
[xUnit.net 00:00:00.31]       Expected: typeof(System.Exception)
[xUnit.net 00:00:00.31]       Stack Trace:
[xUnit.net 00:00:00.31]         C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3.Tests\ConfigTests.cs(47,0): at ConfigTests.InvalidConfigThrows()
[xUnit.net 00:00:00.31]            at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
[xUnit.net 00:00:00.31]            at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
[xUnit.net 00:00:00.31]   Finished:    Task3.Tests
  Task3.Tests (тест) net8.0 сбой с ошибками (1) (1,2 с)
    C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3.Tests\ConfigTests.cs(47): error TESTERROR: 
      ConfigTests.InvalidConfigThrows (3ms): Сообщение об ошибке: Assert.Throws() Failure: No exception was thrown
      Expected: typeof(System.Exception)
      Трассировка стека:
         at ConfigTests.InvalidConfigThrows() in C:\Users\Kiril\Documents\GitHub\framework_task3_EreminKD\Task3.Tests\Conf
      igTests.cs:line 47
         at System.RuntimeMethodHandle.InvokeMethod(Object target, Void** arguments, Signature sig, Boolean isConstructor)
         at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)

Сводка теста: всего: 7; сбой: 1; успешно: 6; пропущено: 0; длительность: 1,1 с
Сборка сбой с ошибками (1) через 3,4 с
```