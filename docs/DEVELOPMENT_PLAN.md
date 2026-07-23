# BuyFromArmenia (BFA) — План разработки

> Документ для отслеживания прогресса. Обновляйте статусы по мере выполнения задач.
>
> **Последнее обновление:** 2026-07-15  
> **Текущая фаза:** Phase 1–2 (MVP + Logistics)  
> **Текущий спринт:** S10–S12

---

## Как пользоваться этим документом

- `[ ]` — не начато
- `[~]` — в работе
- `[x]` — готово
- `[—]` — отложено / out of scope для текущей фазы

После завершения спринта обновите блок **«Текущий статус»** в начале файла.

---

## Текущий статус (snapshot)

| Метрика | Значение |
|---------|----------|
| Фаза | Phase 0 |
| Спринт | — |
| Public UI | Главная (EN/HY) |
| Supplier UI | Layout, Dashboard, Products, Orders, Finance |
| Admin UI | Login, Dashboard, Products (базово) |
| Backend modules | `BuildingBlocks` + `Identity`, `Suppliers`, `Catalog` (DDD) |
| Auth | Admin JWT + Public customer JWT + Supplier JWT |
| Background jobs | Hangfire (миграции, seed суперадмина) |

---

## 1. Архитектурные принципы

| Принцип | Описание |
|---------|----------|
| Три продукта | Public, Supplier, Admin — отдельные UI, API, общий домен |
| Modular Monolith | Модули с границами, PostgreSQL со схемами, без микросервисов на старте |
| CQRS | Commands/Queries в Application; API только маршрутизирует в MediatR |
| Разделение агрегатов | `CustomerOrder` ≠ `SupplierOrder` ≠ `Payment` ≠ `Settlement` |
| Catalog ≠ Inventory | `Product` не хранит остатки; остатки в `StockItem` |
| Money | Всегда `Money(Amount, Currency)`, не голый `decimal` |

### Ключевая доменная цепочка

```
CustomerOrder (1)
    ├── SupplierOrder A
    ├── SupplierOrder B
    └── SupplierOrder C
            ↓
    Warehouse / Consolidation
            ↓
    International Shipment
            ↓
    Supplier Settlement
```

### Три интерфейса

| Интерфейс | Аудитория | URL | Стиль |
|-----------|-----------|-----|-------|
| Public | Покупатели | `buyfromarmenia.com` | Эмоциональный, тёплый, premium |
| Supplier | Поставщики | `seller.buyfromarmenia.com` | Деловой, navy + terracotta CTA |
| Admin | Платформа | `admin.buyfromarmenia.com` | Функциональный, таблицы, фильтры |

---

## 2. Целевая структура solution

```
BFA.sln
src/
  BuildingBlocks/
    BFA.BuildingBlocks.Domain/
    BFA.BuildingBlocks.Application/
    BFA.BuildingBlocks.Infrastructure/

  Modules/
    Identity/
    Suppliers/
    Catalog/
    Inventory/
    Shopping/
    Ordering/
    Fulfillment/
    Warehouse/          # Phase 2
    Shipping/           # Phase 2
    Payments/
    Settlements/        # Phase 3
    Returns/            # Phase 3
    Compliance/         # Phase 3

  Api/
    BFA.Public.Api/
    BFA.Supplier.Api/
    BFA.Admin.Api/

  Persistence/
    BFA.Persistence/

  Infrastructure/
    BFA.Infrastructure/

  UI/
    BFA.Public.UI/
    BFA.Supplier.UI/
    BFA.Admin.UI/
```

### PostgreSQL schemas

```
identity.*
suppliers.*
catalog.*
inventory.*
ordering.*
fulfillment.*
warehouse.*      # Phase 2
shipping.*         # Phase 2
payments.*
settlements.*      # Phase 3
```

---

## 3. Bounded Contexts

| Context | Aggregate Root | Phase |
|---------|----------------|-------|
| Identity & Access | User, CustomerProfile, SupplierMember, AdminEmployee | 0–1 |
| Supplier Management | Supplier | 1 |
| Catalog | Product, Category | 1 |
| Inventory | StockItem, StockReservation | 1 |
| Shopping | ShoppingCart | 1 |
| Ordering | CustomerOrder | 1 |
| Supplier Fulfillment | SupplierOrder | 1 |
| Warehouse | InboundShipment, Consolidation | 2 |
| Shipping | Shipment | 2 |
| Payments | Payment | 1 stub → 3 full |
| Settlements | SupplierSettlement, Payout | 3 |
| Returns | ReturnRequest | 3 |
| Compliance | ProductComplianceProfile, TradeRestriction | 3 |
| Notifications | Notification | 2 |
| Customer Support | SupportCase | 3 |

---

## 4. Фазы (roadmap)

| Фаза | Название | Срок | Статус |
|------|----------|------|--------|
| **0** | Foundation | 2–3 нед. | `[~]` в работе |
| **1** | Marketplace MVP | 8–10 нед. | `[ ]` |
| **2** | Operations & Logistics | 6–8 нед. | `[ ]` |
| **3** | Finance & Scale | 6–8 нед. | `[ ]` |

---

## 5. Phase 0 — Foundation (недели 1–3)

### 5.1 Backend

| # | Задача | Статус |
|---|--------|--------|
| 0.1 | `BFA.BuildingBlocks.Domain` — AggregateRoot, Entity, ValueObject, DomainEvent | `[x]` |
| 0.2 | `Money`, `Address`, `LanguageCode` value objects | `[x]` |
| 0.3 | `BFA.BuildingBlocks.Application` — Result, pipeline behaviors | `[~]` Result готов |
| 0.4 | Реструктуризация solution (`Modules/`, `BuildingBlocks/`) | `[x]` |
| 0.5 | PostgreSQL schemas (identity, suppliers, catalog, …) | `[x]` identity, suppliers, catalog |
| 0.6 | Outbox table + интерфейс (worker — Phase 2) | `[x]` таблица + `IOutboxStore` |
| 0.7 | Audit log (`AuditEntry`) | `[x]` таблица + `IAuditLogger` |
| 0.8 | Role-based permissions (Admin + Supplier API) | `[~]` JWT roles, без policy matrix |
| 0.9 | Миграция существующих `Product`, `AdminUser` в модули | `[x]` |

### 5.2 Frontend

| # | Задача | Статус |
|---|--------|--------|
| 0.10 | Public UI — design tokens, shared components | `[~]` частично (главная) |
| 0.11 | Supplier UI — layout (navy sidebar), design system | `[x]` |
| 0.12 | Admin UI — компактные таблицы, фильтры, design system | `[~]` частично |
| 0.13 | Routing по URL-структуре из спецификации | `[ ]` |
| 0.14 | Shared API client (auth, errors) во всех UI | `[~]` Admin + Public + Supplier |

### DoD Phase 0

- [x] Solution собирается
- [~] Миграции применяются (`InitialDddSchema`, автоматически через Hangfire)
- [~] Три UI с разными layout/theme (Public, Supplier, Admin)
- [~] Роли и permissions в Admin API
- [ ] Naming: `*Command.cs`, `*Query.cs`

---

## 6. Phase 1 — Marketplace MVP (недели 4–13)

### Порядок модулей

```
Identity → Suppliers → Catalog → Inventory → Shopping → Ordering → Fulfillment
```

---

### 6.1 Identity & Access

| Задача | Статус |
|--------|--------|
| User aggregate (email, password, status) | `[x]` |
| CustomerProfile | `[x]` |
| SupplierMember + роли (Owner, ProductManager, …) | `[x]` домен + login |
| AdminEmployee + роли (SuperAdmin, Moderator, …) | `[~]` AdminUser есть |
| RegisterCustomer, Login | `[x]` API + JWT |
| Supplier login | `[x]` API + UI `/login` |
| Public: регистрация / логин покупателя | `[x]` UI `/account/login`, `/account/register` |

---

### 6.2 Supplier Management

| Задача | Статус |
|--------|--------|
| Supplier aggregate + статусы (Application → Active → Suspended) | `[x]` домен |
| RegisterSupplier, SubmitApplication | `[x]` API |
| ApproveSupplier, RejectSupplier, SuspendSupplier | `[~]` approve/reject |
| SupplierMember, BankAccount, Documents | `[x]` домен |
| **Supplier UI:** onboarding | `[x]` wizard |
| **Supplier UI:** settings (название, лого, реквизиты) | `[~]` профиль + адреса (без лого) |
| **Admin UI:** `/vendors` — список, approve/reject | `[x]` |

---

### 6.3 Catalog

| Задача | Статус |
|--------|--------|
| Product aggregate (полная модель) | `[x]` домен |
| ProductTranslation (en, hy) | `[x]` полные поля |
| ProductVariant (SKU, size, color, weight) | `[x]` домен |
| ProductMedia, ProductDocument | `[x]` домен |
| ShippingProfile (вес, габариты, fragile, …) | `[x]` домен |
| Category (иерархия) | `[x]` домен |
| Статусы: Draft → PendingReview → Approved → Published | `[x]` домен |
| CreateProduct, UpdateProduct, SubmitForReview | `[x]` Supplier API + UI |
| ApproveProduct, RejectProduct, RequestChanges + шаблоны | `[x]` Admin API + UI |
| **Public UI:** каталог, категории, поиск | `[~]` каталог + category filter |
| **Public UI:** страница товара | `[x]` |
| **Supplier UI:** products CRUD | `[x]` list/create/edit |
| **Admin UI:** модерация товаров | `[x]` |

**Шаблоны отклонения (Admin):**
- [x] Poor image quality
- [x] Incorrect category
- [x] Missing certificate
- [x] Incomplete description
- [x] Restricted for international shipment
- [x] Incorrect product weight

---

### 6.4 Inventory

| Задача | Статус |
|--------|--------|
| StockItem aggregate (OnHand, Reserved, Available) | `[x]` |
| StockReservation + optimistic concurrency (RowVersion) | `[x]` PostgreSQL xmin |
| ReserveStock, ReleaseReservation, ConfirmReservation | `[x]` application commands |
| LowStockDetected event | `[x]` |
| **Supplier UI:** остатки в товаре, alert на dashboard | `[x]` `/inventory`, low-stock на dashboard |

---

### 6.5 Shopping

| Задача | Статус |
|--------|--------|
| ShoppingCart aggregate | `[x]` |
| AddItem, RemoveItem, ChangeQuantity | `[x]` |
| Wishlist / избранное | `[x]` anonymous cart |
| **Public UI:** корзина | `[x]` |
| **Public UI:** избранное | `[x]` |

---

### 6.6 Ordering

| Задача | Статус |
|--------|--------|
| CustomerOrder aggregate | `[x]` |
| CustomerOrderItem + snapshots (name, price, SKU) | `[x]` |
| OrderStatus, PaymentStatus, FulfillmentStatus (раздельно) | `[x]` |
| PlaceOrder process (sync MVP) | `[x]` stub payment |
| **Public UI:** checkout | `[x]` |
| **Public UI:** личный кабинет, история заказов | `[~]` guest `/orders` by cartId |
| **Public UI:** отслеживание заказа | `[x]` `/orders/{id}` |

---

### 6.7 Supplier Fulfillment

| Задача | Статус |
|--------|--------|
| SupplierOrder aggregate | `[x]` |
| Статусы: New → Confirmed → Preparing → ReadyForPickup → TransferredToWarehouse | `[x]` |
| Создание SupplierOrders из CustomerOrder при оплате | `[x]` |
| **Supplier UI:** Dashboard (приоритет) | `[x]` |
| **Supplier UI:** `/orders` | `[x]` |
| **Admin UI:** `/orders` (CustomerOrder + все SupplierOrders) | `[x]` |

**Supplier Dashboard metrics:**
- [x] Продажи за сегодня / месяц
- [x] Количество заказов
- [x] Товары с низким остатком
- [x] Товары на модерации
- [x] Ожидающие отправки
- [x] Возвраты (stub)
- [x] Баланс / ближайшая выплата (stub)

---

### 6.8 Payments (MVP stub)

| Задача | Статус |
|--------|--------|
| Payment aggregate (базовый) | `[x]` |
| Manual / stub capture в Phase 1 | `[x]` |
| Stripe Connect — Phase 3 | `[~]` webhook skeleton |

---

### DoD Phase 1

- [x] Покупатель: browse → cart → checkout → order
- [x] Поставщик: product → moderation → fulfill order
- [x] Админ: suppliers, moderation, orders overview
- [x] CustomerOrder → N × SupplierOrder при оплате
- [~] E2E тест критического пути (`tests/BFA.IntegrationTests` — PlaceOrder + fulfillment inbound)

---

## 7. Phase 2 — Operations & Logistics (недели 14–21)

### 7.1 Warehouse

| Задача | Статус |
|--------|--------|
| InboundShipment aggregate | `[x]` |
| WarehouseReceipt (scan, photo, weight, inspection) | `[x]` |
| Consolidation aggregate | `[x]` |
| Package | `[x]` |
| **Admin UI:** `/warehouse` | `[x]` |

### 7.2 Shipping

| Задача | Статус |
|--------|--------|
| Shipment aggregate | `[x]` stub |
| Carrier integration (DHL, FedEx, …) | `[~]` stub carriers |
| CustomsDeclaration | `[x]` basic |
| Tracking updates | `[x]` advance status |
| **Admin UI:** `/logistics` | `[x]` |
| Shipping rate brackets (country + weight) | `[x]` Admin `/settings/shipping-rates` |
| Error margin % on quote | `[x]` global setting |
| Checkout shipping quote + order fees | `[x]` estimate at place-order |
| Post-warehouse shipping fee adjustment | `[x]` Admin order adjust |

### 7.3 Event infrastructure

| Задача | Статус |
|--------|--------|
| Outbox Pattern + worker | `[~]` Hangfire job `ProcessOutboxMessages` |
| Integration events между модулями | `[~]` outbox: OrderPlaced, ProductApproved, SupplierRegistered |
| OrderFulfillmentProcess saga | `[~]` auto-advance SupplierOrders → inbound + settlement on OrderPlaced |

### DoD Phase 2

- [~] Supplier → warehouse → consolidation → international ship (auto inbound on OrderPlaced; consolidation/ship still admin)
- [~] Tracking для покупателя
- [ ] Warehouse Operator role изолирован

---

## 8. Phase 3 — Finance & Scale (недели 22–29)

| Модуль | Задачи | Статус |
|--------|--------|--------|
| Payments | Stripe Connect, webhooks, idempotency | `[~]` webhook skeleton `/api/webhooks/stripe` |
| Settlements | SupplierSettlement, Payout, eligibility policy | `[~]` stub |
| Returns | ReturnRequest, disputes, refunds | `[~]` domain + request/approve/refund stub |
| Compliance | TradeRestriction, export validation | `[~]` country block at checkout + admin UI |
| Analytics | Supplier + Admin dashboards (read models) | `[ ]` |
| Notifications | Email/SMS/InApp по событиям | `[~]` logging stub on OrderPlaced |
| Customer Support | SupportCase | `[ ]` |

### Admin roles (полный набор)

| Роль | Статус |
|------|--------|
| Super Admin | `[~]` |
| Operations Manager | `[ ]` |
| Product Moderator | `[ ]` |
| Warehouse Operator | `[ ]` |
| Logistics Manager | `[ ]` |
| Finance Manager | `[ ]` |
| Customer Support | `[ ]` |
| Supplier Support | `[ ]` |
| Content Manager | `[ ]` |

### DoD Phase 3

- [ ] Автоматические выплаты поставщикам
- [ ] Возвраты end-to-end
- [x] Compliance при checkout (TradeRestriction country/category block)
- [ ] Finance role изолирован от catalog

---

## 9. Спринты (2 недели каждый)

| Sprint | Фокус | Backend | Frontend | Статус |
|--------|-------|---------|----------|--------|
| **S1** | Foundation | BuildingBlocks, schemas, Identity base | Design systems × 3 | `[ ]` |
| **S2** | Suppliers | Supplier aggregate, onboarding API | Supplier onboarding, Admin vendors | `[~]` |
| **S3** | Catalog | Product, Category, translations, variants | Supplier products, Public catalog | `[~]` |
| **S4** | Moderation | Reject templates, RequestChanges | Admin moderation, Public product page | `[~]` |
| **S5** | Inventory + Cart | StockItem, ShoppingCart | Cart, favorites, supplier stock | `[~]` |
| **S6** | Ordering | CustomerOrder, PlaceOrder | Checkout, account, Supplier Dashboard | `[~]` account позже |
| **S7** | Fulfillment | SupplierOrder | Supplier orders, Admin orders view | `[~]` |
| **S8** | Payments stub | Payment aggregate | Order payment flow, dashboards | `[~]` |
| **S9** | Warehouse | InboundShipment, Receipt | Admin warehouse UI | `[x]` |
| **S10** | Consolidation | Consolidation, Package | Admin consolidation UI | `[x]` |
| **S11** | Shipping | Shipment, carriers | Logistics UI, buyer tracking | `[~]` |
| **S12** | Settlements | SupplierSettlement, Payout | Supplier finance, Admin finance | `[~]` stub |

---

## 10. Public UI — checklist

| Страница | Route | Статус |
|----------|-------|--------|
| Главная | `/` | `[x]` |
| Каталог | `/products` | `[x]` |
| Категории | `/categories` | `[x]` |
| Поиск и фильтры | `/products?…` | `[x]` category + search |
| Страница товара | `/products/{slug}` | `[x]` по id (`/products/{id}`) |
| Корзина | `/cart` | `[x]` |
| Checkout | `/checkout` | `[x]` |
| Личный кабинет | `/account` | `[x]` guest hub |
| Заказы | `/account/orders` | `[x]` guest `/orders` |
| Отслеживание | `/account/orders/{id}` | `[x]` `/orders/{id}` |
| Избранное | `/account/wishlist` | `[x]` anonymous `/wishlist` |

---

## 11. Supplier UI — checklist

| Раздел | Route | Статус |
|--------|-------|--------|
| Dashboard | `/` | `[x]` базовый |
| Товары — список | `/products` | `[x]` |
| Товары — создание | `/products/new` | `[x]` |
| Товары — редактирование | `/products/{id}` | `[x]` |
| Остатки | `/inventory` | `[x]` |
| Заказы | `/orders` | `[x]` |
| Финансы | `/finance` | `[x]` базовый |
| Аналитика | `/analytics` | `[—]` Phase 3 |
| Настройки магазина | `/settings` | `[~]` заглушка |
| Onboarding | `/onboarding` | `[~]` базовый |

---

## 12. Admin UI — checklist

| Раздел | Route | Статус |
|--------|-------|--------|
| Login | `/login` | `[x]` |
| Dashboard | `/dashboard` | `[x]` базовый |
| Поставщики | `/vendors` | `[x]` |
| Модерация товаров | `/products` | `[x]` |
| Заказы | `/orders` | `[x]` |
| Склад | `/warehouse` | `[x]` базовый |
| Логистика | `/logistics` | `[x]` базовый |
| Финансы | `/finance` | `[x]` базовый |
| Пользователи / роли | `/settings/users` | `[x]` read-only list |
| Audit log | `/settings/audit` | `[x]` |

---

## 13. Технические стандарты

| Область | Стандарт |
|---------|----------|
| Commands | `{Action}{Entity}Command.cs` + Handler в том же файле или рядом |
| Queries | `Get{Entity}Query.cs` + Handler |
| API | Только `IMediator.Send()`, без бизнес-логики |
| DB | Schema per module; command side не делает cross-schema JOIN |
| Events | Domain events внутри модуля; Integration events между модулями |
| Idempotency | Webhooks, PlaceOrder, warehouse scan, payouts |
| i18n Public | en + hy |
| i18n Supplier/Admin | en (hy — позже) |

---

## 14. Риски

| Риск | Митигация | Статус |
|------|-----------|--------|
| Смешивание агрегатов | Code review checklist, отдельные модули | ongoing |
| Scope creep в Phase 1 | Compliance/Returns — stub only | — |
| UI отстаёт от API | API-first, OpenAPI, mocks | — |
| Double-sale inventory | Reservation + RowVersion с S5 | — |
| Логистика до заказов | Строгий порядок модулей (§6) | — |

---

## 15. Журнал изменений плана

| Дата | Изменение |
|------|-----------|
| 2026-07-11 | Создан документ. Baseline: Public home, Admin auth, упрощённый Product |
| 2026-07-15 | S10–S12: Consolidation, Shipping, Settlements stub; account, categories, search |

---

## 16. Ссылки

- Solution: `BFA.sln`
- Public API: `src/BFA.Public.Api` (port 5100)
- Supplier API: `src/BFA.Supplier.Api` (port 5102)
- Admin API: `src/BFA.Admin.Api` (port 5101)
- Hangfire: `src/BFA.Hangfire` (port 5103, dashboard `/hangfire`)
- Public UI: `src/BFA.Public.UI` (port 3200)
- Supplier UI: `src/BFA.Supplier.UI` (port 3202)
- Admin UI: `src/BFA.Admin.UI` (port 3201)

**Dev credentials (Admin):** `admin@buyfromarmenia.com` / `Admin123!`
