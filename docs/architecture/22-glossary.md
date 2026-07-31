---
id: 22-glossary
title: Glossary
read_when: ["term ambiguity, EN/FA mapping"]
topics: [glossary]
depends_on: []
stability: stable
---

## Contract

Decides: the canonical English domain term, its Persian equivalent, and its precise definition, so
naming never drifts across code, UI copy, and docs. Does not decide entity schema (see
[05-domain-model.md](05-domain-model.md)) — this file fixes vocabulary only.

## Canonical terms

| English (canonical) | Persian | Definition |
|---|---|---|
| Owner / Client | صاحب حیوان / مشتری | The pet owner who is the clinic's customer; canonical code/API term is **Owner** — "Client" is reserved for generic software meaning (API client, desktop client) to avoid ambiguity. |
| Patient / Animal | بیمار / حیوان | The animal receiving care; canonical code/API term is **Patient**. "Animal" is acceptable in UI copy, never in code. |
| Visit / Encounter | ویزیت | A single clinical interaction between a patient and the clinic; canonical term is **Visit**. "Encounter" is not used anywhere in this system to avoid confusion with generic EHR terminology. |
| Service | خدمت | A billable clinical or non-clinical offering (exam, vaccination, boarding) scheduled and invoiced. |
| Grooming | نظافت / گرومینگ | A non-medical appointment type with its own resource pool, distinct from clinical **Service**. |
| Cheque | چک | A postdated or current bank cheque issued or received by the clinic; tracked through its own lifecycle (see [05](05-domain-model.md)). |
| Cash session | جلسه صندوق | A bounded period during which a physical till is open, from open to close/reconciliation (see [05](05-domain-model.md), [15](15-finance-and-ledger.md)). |
| Entitlement | حق دسترسی / امتیاز | A resolved, point-in-time grant (feature flag, limit) derived from a tenant's active `Subscription` and `Plan` (see [11](11-licensing-and-entitlements.md)). |
| Tenant | مستأجر / کلینیک (به عنوان واحد سازمانی) | One clinic's isolated data and configuration scope; not a user — a tenant has many `User` accounts. |
| Ledger entry | ردیف دفتر کل | A single append-only financial fact row; never edited or deleted after commit (INV-06). |
| Invoice | فاکتور | A billing document following the state machine in [05-domain-model.md](05-domain-model.md); immutable once `Issued`. |
| Appointment | نوبت | A scheduled booking against a `Resource` (doctor, groomer, room) for a `Service` or grooming type. |
| Resource | منبع | A schedulable entity (doctor, groomer, room, till) used by Scheduling/Finance. |
| Medical file | پرونده پزشکی | The aggregate clinical record header for a `Patient`, referencing its `Visit` history. |
| Biometric reading | اندازه‌گیری زیستی | A single immutable measurement (weight, height, vitals) recorded at a point in time (DOM-09). |
| Outbox | صندوق ارسال | The desktop's durable local queue of not-yet-confirmed writes awaiting sync push ([08](08-desktop-local-data.md)). |
| Tombstone | سنگ‌قبر / نشانه حذف | A soft-delete marker propagated through sync so deletes converge across devices (INV-04). |
| HLC (Hybrid Logical Clock) | ساعت منطقی-فیزیکی ترکیبی | The ordering mechanism for sync change capture (see [09](09-sync-architecture.md)); not a wall-clock timestamp. |
| Field group | گروه فیلد | A named subset of an entity's fields that shares one HLC version for conflict resolution (SYNC-R-03). |
| Plan | پلن اشتراک | A platform-defined bundle of entitlements a tenant can subscribe to. |
| Subscription | اشتراک | A tenant's active or historical relationship to a `Plan`, with its own state machine. |
| Platform admin | ادمین پلتفرم | A Genspark-side operator managing tenants/subscriptions/backups, distinct from a tenant's `owner-admin`. |
| Owner-admin | ادمین اصلی کلینیک | The primary staff account for a tenant, seeded at signup, cannot lose staff-management rights (SEC-11). |

## Usage rule

CONV-style: when in doubt, code and API names use the "canonical" column exactly; UI copy may use
either the Persian equivalent or a more casual English synonym in comments, but never a different
*code-level* identifier than the one listed here.
