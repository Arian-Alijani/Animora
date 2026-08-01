---
id: 04-modules
title: Module Catalog
read_when: ["finding which module owns a feature", "adding a new module"]
topics: [modules, feature-map]
depends_on: [03-structure]
stability: stable
---

## Contract

Decides: the canonical module list, one owner per entity/feature, and the map from each of the 14
functional requirements in the product scope to its owning module(s) and endpoint group. Does not
decide internal entity design (see [05](05-domain-model.md)) or endpoint shapes (see [06](06-api-contract.md)).

## Module list

| Module | Responsibility | Owned entities | Public contract (in-process) | Consumes | Synced | Entitlement-gated |
|---|---|---|---|---|---|---|
| Identity | Users, roles, permission claims, auth sessions | User, Role, PermissionClaim, RefreshToken, Device | `IIdentityContract` (AuthenticateUser, GetPermissions, RegisterDevice) | — | No (server authoritative) | No |
| Clients | Owner (client) and patient (animal) records, medical file | Owner, Patient, MedicalFile | `IClientsContract` (GetPatient, GetOwner, GetMedicalFileSummary) | Identity (permission check) | Yes | No |
| Visits | Visit records, biometrics, lab/test results, visit outcomes, vaccination definitions and patient vaccination records | Visit, BiometricReading, LabResult, VisitOutcome, VaccineDefinition, VaccinationRecord | `IVisitsContract` (GetVisitHistory, GetBiometricSeries) | Clients, Files | Yes | No (core clinical) |
| Scheduling | Appointments, resources (doctors/groomers/rooms), calendar, due-reminder computation | Appointment, Resource, ServiceType, GroomingType, ReminderRule | `ISchedulingContract` (GetAvailability, GetUpcomingReminders) | Clients, Identity | Yes | Partial (resource count limits) |
| Finance | Ledger, invoices, point-of-sale, cheques, cash sessions, expenses/income, product catalog, inventory, suppliers and procurement | LedgerEntry, Invoice, InvoiceTemplate, Cheque, CashSession, Expense, Income, Product, InventoryItem, Supplier, PurchaseOrder, GoodsReceipt | `IFinanceContract` (GetInvoicePreview, PostLedgerEntry, GetCashSessionState) | Clients, Visits (for line items) | Yes (ledger append-only, see [15](15-finance-and-ledger.md)) | Partial (template count, advanced reports) |
| Reporting | Read models, KPI queries, visitor/traffic stats, exports | ReportView (virtual), KpiSnapshot (materialized) | `IReportingContract` (GetKpi, GetBestSelling, ExportReport) | Finance, Visits, Scheduling | No (derived) | Partial (advanced reports) |
| Notifications | Notification engine, channel delivery, preferences, delivery log | NotificationRule, NotificationPreference, DeliveryLog | `INotificationsContract` (Enqueue, GetPreferences) | Scheduling, Finance, Visits (event sources) | Partial (preferences sync, log server-only) | No |
| Licensing | Plans, entitlements, license tokens, payments, subscriptions | Plan, Entitlement, Subscription, LicenseToken, PaymentTransaction | `ILicensingContract` (GetEntitlements, IssueLicenseToken, VerifyPayment) | Identity (tenant), PlatformAdmin | No (server authoritative; token cached client-side) | n/a (this module defines gating) |
| Files | Attachment metadata, S3 orchestration, thumbnailing coordination | Attachment, AttachmentChunk | `IFilesContract` (InitiateUpload, GetDownloadUrl) | Clients, Visits | Yes (metadata only) | Partial (storage quota) |
| Sync | Sync protocol endpoints, per-device cursors, batch apply, conflict log | SyncCursor, ConflictLogEntry, DeadLetterBatch | `ISyncContract` (PullChanges, PushBatch) | All synced modules | n/a (infrastructure module) | No |
| PlatformAdmin | Tenant lifecycle, subscription oversight, backup oversight for platform admins | TenantRecord (admin view), BackupJob | `IPlatformAdminContract` (ListTenants, TriggerBackupCheck) | Licensing, Identity | No | No (platform-level, not tenant plan) |

## Feature (§3) -> Module -> Endpoint-group map

| # | Functional requirement (product scope §3) | Owning module(s) | Endpoint group (see [06](06-api-contract.md)) |
|---|---|---|---|
| 1 | Owner (client) management | Clients | `/api/v1/owners` |
| 2 | Patient (animal) management, medical file | Clients | `/api/v1/patients`, `/api/v1/patients/{id}/medical-file` |
| 3 | Visit management | Visits | `/api/v1/visits` |
| 4 | Biometric tracking + growth charts | Visits, Reporting | `/api/v1/patients/{id}/biometrics`, `/api/v1/patients/{id}/biometrics/chart` |
| 5 | Lab/test results and visit outcomes with attachments | Visits, Files | `/api/v1/visits/{id}/lab-results`, `/api/v1/attachments` |
| 6 | Cheque management | Finance | `/api/v1/cheques` |
| 7 | Professional invoicing, templates, live preview | Finance | `/api/v1/invoices`, `/api/v1/invoice-templates`, `/api/v1/invoices/preview` |
| 8 | Cash-register / till module | Finance | `/api/v1/cash-sessions` |
| 9 | Expense/income management + financial reporting/charts, best-sellers | Finance, Reporting | `/api/v1/expenses`, `/api/v1/incomes`, `/api/v1/reports/finance`, `/api/v1/reports/best-selling` |
| 10 | Visitor/traffic stats and clinic KPIs | Reporting | `/api/v1/reports/kpi`, `/api/v1/reports/traffic` |
| 11 | Subscription plans gating access | Licensing | `/api/v1/subscriptions`, `/api/v1/entitlements` |
| 12 | Multiple staff accounts, roles/permission sets | Identity | `/api/v1/staff`, `/api/v1/roles` |
| 13 | Multiple doctors, service/grooming appointment types with resources | Scheduling | `/api/v1/resources`, `/api/v1/service-types`, `/api/v1/grooming-types` |
| 14 | Appointment booking, vaccination reminders, rescheduling, calendar | Scheduling, Notifications | `/api/v1/appointments`, `/api/v1/calendar`, `/api/v1/reminders` |
| 15 | Vaccination definitions, patient plans, administration history | Visits, Notifications | `/api/v1/vaccines`, `/api/v1/patients/{id}/vaccinations`, `/api/v1/reminders` |
| 16 | Point-of-sale product/service invoicing and cash-register settlement | Finance | `/api/v1/invoices`, `/api/v1/cash-sessions` |
| 17 | Product catalog, inventory visibility and stock operations | Finance | `/api/v1/products`, `/api/v1/inventory` |
| 18 | Supplier, purchase, receiving and purchase-expense management | Finance | `/api/v1/suppliers`, `/api/v1/purchases`, `/api/v1/expenses` |

## Technical scope (§3) -> Module map

| Technical requirement | Owning module(s) / doc |
|---|---|
| Desktop + web + macOS seam | [12](12-desktop-architecture.md), [13](13-web-architecture.md) |
| Subscription enforcement / anti-tamper | Licensing, [11](11-licensing-and-entitlements.md) |
| Performance / fast startup | [01](01-context-and-drivers.md) targets, [12](12-desktop-architecture.md), [19](19-deployment-topology.md) |
| Cheap, low-risk feature addition | [03](03-solution-structure.md), [20](20-extensibility-playbook.md) |
| Loss-free sync after long offline | Sync module, [09](09-sync-architecture.md) |
| Config-only topology split | [19](19-deployment-topology.md) |
| Swappable infrastructure | [19](19-deployment-topology.md), TECH_STACK §6 HybridCache/S3 seams |
| Back-office tenant/subscription/backup admin | PlatformAdmin, [18](18-observability-and-operations.md) |
| Layered cross-platform architecture | [03](03-solution-structure.md) |
| Security at every tier | [10](10-security-and-access-control.md) |

## SMS notifications (cross-cutting, §3)

"Record created", vaccination alerts, and other message types are not a module of their own — they
are event sources feeding the Notifications module's alert-source catalog (see
[14-jobs-and-notifications.md#alert-source-catalog](14-jobs-and-notifications.md)). Source modules
(Clients, Scheduling, Finance) raise domain events; Notifications owns delivery.

## Encrypted backup (cross-cutting, §3)

Not an application module. Server-side backup is infrastructure (`pgBackRest`, `rclone`, see
[18](18-observability-and-operations.md)); desktop-side backup is local (SQLCipher snapshot, see
[08](08-desktop-local-data.md)). PlatformAdmin exposes backup status/oversight only.
