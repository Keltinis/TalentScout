# TalentScout — архітектура рішення

## Призначення

`TalentScout` — внутрішня CRM кастинг- або модельного агентства. Застосунок дає менеджеру змогу вести детальні анкети талантів, знаходити їх за параметрами, добирати кандидатів до кастинг-проєктів, відстежувати їхній статус і в наступних ітераціях експортувати презентацію для режисера.

## Технологічні рішення

| Частина | Рішення |
| --- | --- |
| UI | WPF (.NET 9), XAML |
| Патерн | MVVM, `CommunityToolkit.Mvvm` |
| Дані | Entity Framework Core + `Microsoft.EntityFrameworkCore.Sqlite` |
| Локальна БД | SQLite, файл `Data/TalentScout.db` у корені рішення під час розробки та поруч із застосунком після публікації |
| Створення БД | під час старту застосунку через `Database.EnsureCreated()`; сервер SQLite або окреме ПЗ не потрібні |
| Залежності | DI-контейнер `Microsoft.Extensions.DependencyInjection` у `App.xaml.cs` |

> Під час запуску з Visual Studio файл бази опиниться в `Data\\TalentScout.db` поруч із `.sln`; у опублікованій версії — у `Data` в папці з `.exe`. Це гарантує, що програма переноситься разом із даними та не залежить від встановленого SQLite-сервера.

## Структура одного WPF-проєкту

```text
TalentScout.sln
├── Documentation/                    # ця схема та технічні нотатки
└── TalentScout/
    ├── Assets/Photos/                 # імпортовані фото талантів (створюється за потреби)
    ├── Common/                        # ObservableObject, RelayCommand, навігація
    ├── Converters/                    # XAML-конвертери
    ├── Data/                          # TalentScoutDbContext, конфігурації EF, DbInitializer
    ├── Models/
    │   ├── Entities/                  # сутності EF Core
    │   ├── Enums/                     # значення-статуси й довідникові типи
    │   └── Dtos/                      # SearchCriteria, результати пошуку/експорту
    ├── Repositories/
    │   ├── Interfaces/                # контракти доступу до даних
    │   └── Implementations/           # EF-реалізації контрактів
    ├── Resources/Styles/              # палітра, типографіка, стилі контролів
    ├── Services/
    │   ├── Interfaces/                # бізнес-функції та діалоги
    │   └── Implementations/           # пошук, фото, експорт, навігація
    ├── ViewModels/                    # логіка екранів, без залежності від View
    ├── Views/                         # XAML-вікна, сторінки та UserControl-и
    ├── App.xaml                       # загальні ресурси UI
    ├── App.xaml.cs                    # composition root: DI, міграція БД, головне вікно
    └── MainWindow.xaml                # оболонка застосунку і навігація
```

Поки що папки є каркасом. Реальні C#-класи, залежності NuGet і база даних додаються у пункті 2, а XAML-екрани — у пункті 3.

## Модель предметної області

```text
Talent 1 ─── * TalentPhoto
HairColorOption 1 ─── * Talent / ProjectRequirement
Talent * ─── * Skill              через TalentSkill
CastingProject 1 ─── 1 ProjectRequirement
ProjectRequirement * ─── * Skill через RequirementSkill
CastingProject * ─── * Talent    через CastingApplication
```

### Сутності EF Core

| Клас | Призначення й основні поля |
| --- | --- |
| `Talent` | `Id`, ім'я, прізвище, дата народження, стать, телефон, email, місто, зріст, вага, колір очей, колір волосся, розмір одягу/взуття, примітки. Вік обчислюється з `BirthDate`, не зберігається окремо. Для створення анкети обов'язкове головне фото. |
| `TalentPhoto` | `Id`, `TalentId`, відносний шлях до файлу, підпис, порядок, ознака головного фото. Один талант має багато фото портфоліо. |
| `HairColorOption` | Користувацький колір волосся, який менеджер додає до довідника та використовує і в анкеті, і в критеріях проєкту. |
| `Skill` | `Id`, назва, категорія: акторська майстерність, вокал, спорт, танець, мови тощо. Це довідник. |
| `TalentSkill` | Зв'язок таланту з навичкою: `TalentId`, `SkillId`, рівень (`SkillLevel`) та примітка. |
| `CastingProject` | `Id`, назва, клієнт, опис, місто, дата зйомки, дата створення, стан проєкту. |
| `ProjectRequirement` | `Id`, `CastingProjectId`, обмеження для пошуку: стать, мін./макс. вік, мін./макс. зріст, допустимі колір очей/волосся, примітка. |
| `RequirementSkill` | Навички, обрані у вимогах: `ProjectRequirementId`, `SkillId`. Поле `MatchMode` визначає: потрібні всі (`All`) або будь-яка (`Any`) з навичок. |
| `CastingApplication` | Участь таланту у проєкті: `Id`, `CastingProjectId`, `TalentId`, статус, дата додавання, коментар. Пара `CastingProjectId + TalentId` унікальна. |

### Переліки (Enums)

`Gender` (`Female`, `Male`, `Other`), `EyeColor`, `HairColor`, `SkillCategory`, `SkillLevel`, `ProjectStatus` (`Draft`, `Active`, `Closed`) та `CastingStatus` (`Invited`, `Audition`, `Approved`, `Reserve`, `Rejected`).

Статуси кастингу точно відображають ТЗ: **запрошений → проби → затверджений → резерв**. `Rejected` додано для реальної роботи менеджера й не порушує цей основний маршрут.

## Контракти та шари

```text
View (XAML)
      ↕ binding / commands
ViewModel
      ↕ interfaces
Services (пошук, фото, експорт, навігація)
      ↕ interfaces
Repositories
      ↕
EF Core DbContext → SQLite-файл
```

Заплановані контракти:

- `ITalentRepository` — читання, додавання, редагування, видалення та завантаження повного профілю з фото й навичками;
- `IProjectRepository` — проєкти, вимоги та кандидати;
- `ISkillRepository` — довідник навичок;
- `ITalentSearchService` — параметричний пошук за `TalentSearchCriteria`; реалізує умови AND/OR/IN LINQ-to-Entities;
- `IPhotoStorageService` — копіювання, безпечне перейменування й видалення файлів фото у `Assets/Photos`;
- `IExportService` — підготовка презентації режисеру (PDF або Excel на окремому етапі);
- `INavigationService` і `IDialogService` — UI-навігація та повідомлення, щоб ViewModel не залежав від `Window` або `MessageBox`.

## Екрани та їхні ViewModel

| Екран | View / ViewModel | Призначення |
| --- | --- | --- |
| Головна | `MainWindow` / `MainViewModel` | Ліва навігація, заголовок, область активної сторінки. |
| Панель | `DashboardView` / `DashboardViewModel` | Лічильники талантів, активних кастингів і кандидатів. |
| Таланти | `TalentListView` / `TalentListViewModel` | Таблиця/картки, швидкий пошук, перехід до профілю. |
| Профіль | `TalentProfileView` / `TalentProfileViewModel` | Детальна анкета, навички й галерея фото. |
| Редагування | `TalentEditorView` / `TalentEditorViewModel` | Валідація полів і збереження анкети. |
| Пошук | `TalentSearchView` / `TalentSearchViewModel` | Параметри та список збігів. |
| Проєкти | `ProjectListView`, `ProjectEditorView` / відповідні VM | Створення проєкту й його критерії. |
| Кастинг | `CastingBoardView` / `CastingBoardViewModel` | Кандидати проєкту й зміна статусу. |

## Візуальна система

Палітра з наданого зразка буде оголошена один раз у `Resources/Styles/Colors.xaml`:

| Роль | Колір |
| --- | --- |
| Primary / заголовки | `#800021` |
| Secondary | `#881144` |
| Accent | `#243A66` |
| Surface accent | `#C24366` |
| Background accent | `#FF69B4` |

Основою інтерфейсу будуть світлий нейтральний фон, бордовий сайдбар, рожеві акценти, картки з заокругленими кутами та читабельна темна типографіка. Це збереже професійний вигляд і не зробить великі форми втомливими.

## Межі наступних етапів

1. **Пункт 2:** додати NuGet-пакети, EF-моделі, контекст, Fluent-конфігурації, репозиторії, сервіси та автостворення/початкове наповнення БД.
2. **Пункт 3:** реалізувати XAML-стилі та екрани за цією схемою.
3. **Пункт 4:** підключити ViewModel до сервісів через DI, перевірити CRUD і пошук.
4. **Пункт 5:** окремий агент-тестувальник перевірить сценарії та збірку.
5. **Пункт 6:** звірити реалізацію з ТЗ і підготувати аналіз коду.
