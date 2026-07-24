# StepStyle — Интернет-магазин женской обуви

**StepStyle** — это веб-приложение на ASP.NET Core (Razor Pages) для каталога женской обуви с корзиной, избранным, тёмной темой и админ-панелью.

---

## Стек технологий

| Компонент          | Технология                                                       |
| ------------------ | ---------------------------------------------------------------- |
| **Backend**        | ASP.NET Core 9.0 (Razor Pages)                                   |
| **ORM**            | Entity Framework Core 9.0                                        |
| **База данных**    | MySQL / MariaDB (через Pomelo.EntityFrameworkCore.MySql)          |
| **Аутентификация** | ASP.NET Core Identity (регистрация, вход, роли Admin/User)       |
| **Фронтенд**       | Bootstrap 5, Bootstrap Icons, чистый JavaScript (vanilla JS)     |
| **Клиентское хранилище** | localStorage (корзина, избранное, тема)                     |

---

## Возможности

### Пользовательская часть

- **Каталог товаров** — карточки товаров с эмодзи-изображениями, ценой, скидками и бейджами «Новинка»
- **Страница товара** — подробная информация: описание, материал, цвет, выбор размера
- **Корзина** — добавление товаров с выбором размера, изменение количества, удаление, оформление заказа
- **Избранное** — сохранение товаров с разными размерами, группировка по названию, добавление в корзину со страницы избранного
- **Тёмная тема** — переключение светлой/тёмной темы (сохраняется в localStorage)
- **Аутентификация** — регистрация и вход (Identity)
- **Адаптивный дизайн** — Bootstrap 5, корректное отображение на мобильных устройствах

### Административная часть (`/Admin`)

- **Список товаров** — таблица с фильтрацией, сортировкой, управлением
- **Создание товара** — форма с предпросмотром в реальном времени, менеджер размеров
- **Редактирование товара** — изменение существующего товара (название, цена, размеры, описание)
- **Удаление товара** — с подтверждением
- **Ролевая модель** — доступ только для пользователей с ролью `Admin`

### Безопасность

- CSP-заголовки (Content-Security-Policy)
- Защита от XSS (экранирование вывода, JavaScriptEncoder)
- Защита от CSRF (Antiforgery)
- Secure cookies (HttpOnly, Secure, SameSite=Lax)
- Anti-fraud проверка цен при оформлении заказа
- Валидация размера и наличия на складе при оформлении
- Redirect только на локальные URL (LocalUrlHelper)

---

## Модели данных

### Product

| Поле         | Тип        | Описание                         |
| ------------- | ---------- | -------------------------------- |
| Id            | int        | Первичный ключ                   |
| Name          | string     | Название товара (уникальное)     |
| Description   | string     | Описание (до 500 символов)       |
| Price         | decimal    | Цена                             |
| OldPrice      | decimal?   | Старая цена (для скидки)         |
| Emoji         | string     | Эмодзи-изображение товара        |
| Category      | string     | Категория                        |
| Material      | string     | Материал                         |
| Color         | string     | Цвет                             |
| CreatedAt     | DateTime   | Дата создания                    |
| UpdatedAt     | DateTime   | Дата последнего изменения        |
| Sizes         | коллекция  | Список размеров с наличием       |

### ProductSize

| Поле   | Тип     | Описание                   |
| ------ | ------- | -------------------------- |
| Size   | decimal | Размер обуви (20–50)       |
| InStock| bool    | Наличие на складе          |

---

## API (страницы)

### Публичные страницы

| Маршрут                  | Описание                           |
| ------------------------ | ---------------------------------- |
| `/`                      | Каталог товаров                    |
| `/Product/{id}`          | Страница товара                    |
| `/Cart`                  | Корзина                            |
| `/Favorites`             | Избранное                          |
| `/Identity/Account/Login`  | Вход                             |
| `/Identity/Account/Register` | Регистрация                    |
| `/Identity/Account/Logout`  | Выход                          |

### Административные страницы (требуется роль Admin)

| Маршрут             | Описание                |
| ------------------- | ----------------------- |
| `/Admin`            | Список товаров          |
| `/Admin/Create`     | Создание товара         |
| `/Admin/Edit/{id}`  | Редактирование товара   |

### Server-side handler

| Маршрут            | Метод | Описание                        |
| ------------------ | ----- | ------------------------------- |
| `/Cart?handler=Checkout` | POST | Валидация и оформление заказа |

---

## Установка и запуск

### Требования

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- MySQL 8.0+ / MariaDB 10.5+
- Git

### 1. Клонирование

```bash
git clone https://github.com/your-username/StepStyle.git
cd StepStyle
```

### 2. Настройка базы данных

Отредактируйте `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=127.0.0.1;port=3306;database=shoesstore;user=root;password=your_password;"
  },
  "Seed": {
    "AdminEmail": "admin@stepstyle.ru",
    "AdminPassword": "Admin123!",
    "DefaultUserEmail": "user@stepstyle.ru",
    "DefaultUserPassword": "User123!"
  }
}
```

### 3. Запуск

```bash
dotnet run
```

При первом запуске будут автоматически применены миграции EF Core и созданы:
- Роли: `Admin`, `User`
- Пользователь администратора (admin@stepstyle.ru / Admin123!)
- Обычный пользователь (user@stepstyle.ru / User123!)

Приложение будет доступно по адресу: `http://localhost:5160`

### Миграции (если нужно вручную)

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## Структура проекта

```
ShoesStore/
├── Data/
│   ├── ApplicationDbContext.cs      # EF Core контекст
│   └── Migrations/                  # Миграции БД
├── Models/
│   ├── Product.cs                   # Модель товара
│   ├── ProductSize.cs               # Модель размера
│   ├── ProductInput.cs              # Input model для форм
│   └── ApplicationUser.cs           # Identity пользователь
├── Services/
│   ├── IProductService.cs           # Интерфейс сервиса товаров
│   ├── ProductService.cs            # Реализация (EF Core)
│   ├── DatabaseSeeder.cs            # Сидер БД (роли + пользователи)
│   └── LocalUrlHelper.cs            # Helper для безопасных редиректов
├── Pages/
│   ├── Index.cshtml(.cs)            # Каталог
│   ├── Product.cshtml(.cs)          # Страница товара
│   ├── Cart.cshtml(.cs)             # Корзина
│   ├── Favorites.cshtml(.cs)        # Избранное
│   ├── Admin/                       # Админ-панель
│   │   ├── Index.cshtml(.cs)        # Список товаров
│   │   ├── Create.cshtml(.cs)       # Создание товара
│   │   ├── Edit.cshtml(.cs)         # Редактирование товара
│   │   ├── _ProductFormFields.cshtml
│   │   └── _SizeManager.cshtml
│   ├── Identity/Account/            # Вход, регистрация, выход
│   └── Shared/                      # _Layout, _ValidationScriptsPartial
├── wwwroot/
│   ├── css/site.css                 # Основные стили (светлая/тёмная тема)
│   ├── js/
│   │   ├── theme-init.js            # Инициализация темы до загрузки страницы
│   │   ├── theme.js                 # Переключение темы
│   │   ├── shared.js                # Общие функции (localStorage, форматирование цен)
│   │   ├── cart.js                  # Логика корзины (добавление)
│   │   ├── cart-page.js             # Рендер страницы корзины
│   │   ├── favorites.js             # Логика избранного
│   │   ├── favorites-page.js        # Рендер страницы избранного
│   │   ├── product-page.js          # Логика страницы товара
│   │   └── admin-product-preview.js # Предпросмотр в админке
│   └── lib/                         # Bootstrap, jQuery, Bootstrap Icons
├── Program.cs                       # Точка входа, DI, middleware
├── appsettings.json                 # Конфигурация
└── ShoesStore.csproj                # Файл проекта
```

---

## Скрипты

### Сборка и запуск

```bash
dotnet build
dotnet run
```

### Миграции

```bash
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Опубликовать

```bash
dotnet publish -c Release -o ./publish
```

---

## Переменные окружения

| Переменная                                     | Описание                        | Значение по умолчанию |
| ---------------------------------------------- | ------------------------------- | --------------------- |
| `ConnectionStrings__DefaultConnection`          | Строка подключения к БД         | из appsettings.json   |
| `Seed__AdminEmail`                              | Email администратора            | admin@stepstyle.ru    |
| `Seed__AdminPassword`                           | Пароль администратора           | Admin123!             |
| `Seed__DefaultUserEmail`                        | Email обычного пользователя     | user@stepstyle.ru     |
| `Seed__DefaultUserPassword`                     | Пароль обычного пользователя    | User123!              |

---

## Клиентское хранилище (localStorage)

| Ключ       | Формат     | Описание                              |
| ---------- | ---------- | ------------------------------------- |
| `cart`     | JSON array | Товары в корзине (id, name, price, size, quantity, emoji) |
| `favorites`| JSON array | Товары в избранном (id, name, price, size, emoji, category) |
| `theme`    | `"dark"` / отсутствует | Тема оформления        |

---

## Лицензия

MIT