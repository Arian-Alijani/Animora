# Open Questions

Unresolved decisions, assumptions made during architecture authoring, and any proposed stack
addition awaiting approval. All items here are non-blocking assumptions unless flagged otherwise.

## Assumptions made (no stack conflict, decided in absence of explicit product spec)

1. **One user account per tenant (DOM-04)**: a person working at two clinics needs two logins.
   Assumed for simplicity; revisit if multi-tenant staff sharing becomes a real requirement — would
   need a cross-tenant identity model, which is a non-trivial RBAC/RLS change.
2. **Tombstone retention window default: 90 days** (SYNC-R-22): a reasonable default balancing
   storage growth against re-seed frequency; not specified in TECH_STACK or product scope. Should
   be confirmed against real clinic offline-absence patterns once field data exists.
3. **Offline grace period length tied to the ≥14-day offline quality target** (LIC-11): product
   scope says "days," TECH_STACK says "offline grace period" without a number. 14 days was chosen
   as a concrete, testable value consistent with "days" language; needs business confirmation.
4. **Bucket-per-environment (not per-tenant) for MinIO** (FILE-03): chosen for single-VPS
   operational simplicity; flagged as the weakest tenant-isolation layer in
   [23-architecture-risks.md](../23-architecture-risks.md). Acceptable given TECH_STACK's explicit
   MinIO/local-disk scope, but a bucket-per-tenant policy is a valid future hardening if the risk
   materializes.
5. **Fixed chart of accounts (FIN-04)** rather than a fully configurable general-ledger accounts
   system: TECH_STACK does not specify accounting depth; a small fixed set matching the product's
   actual revenue areas was chosen to avoid building a general-purpose accounting product (out of
   scope per product framing, though not explicitly listed in TECH_STACK §19).
6. **Malware scanning is allowlist-only, no scanning engine** (FILE-14): TECH_STACK §7 says
   "virus-free policy by extension allowlist" with no scanning tool listed; treated as the complete,
   intended control rather than a gap to fill.

## Proposed stack additions awaiting approval

None. Every capability in the product scope (§3) was achievable from TECH_STACK.md's listed stack
without requiring an addition. If a genuine gap is found during implementation, follow
TECH_STACK.md's rule 2 (check §17 Feature -> Stack Map, then §18 Do NOT Use) before proposing
anything here.

## Items to revisit if the product scope narrows or widens

- If macOS support (`[later]`) becomes `[core]`, `[08-desktop-local-data.md]` key-storage design
  (DPAPI) needs its Keychain counterpart fully specified beyond the current seam-only placeholder.
- If a public third-party API program is ever re-scoped in (currently explicitly out of scope per
  TECH_STACK §19), [06-api-contract.md](../06-api-contract.md) needs an API-key/developer-portal
  addendum — not designed here since it is currently prohibited.
