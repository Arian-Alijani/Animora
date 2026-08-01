# Design Reference — extracted visual language

The visual target for every Stage A screen (phases 01-13). It records **observed values only**: the
palette, type hierarchy, geometry, elevation, and component anatomy read off the product-owner's
reference screens. It defines no architecture rule and no business rule — token *architecture* is
DESK-ARCH-12/13's job, token *values* are this file's job, and the canonical machine-readable copy of
those values is the resource dictionary set under
`desktop/src/Animora.Desktop.UI/Theme/Tokens/` once phase 01 lands.

## Source and confidence

| Screen | File |
|---|---|
| Dashboard (`داشبورد`) | [`design-reference/01-dashboard.png`](design-reference/01-dashboard.png) |
| Appointments calendar (`تقویم نوبت‌ها`) | [`design-reference/02-appointments-calendar.png`](design-reference/02-appointments-calendar.png) |
| Sales report, lower fold (`آمار و گزارش فروش`) | [`design-reference/03-sales-report-lower.png`](design-reference/03-sales-report-lower.png) |
| Sales report, upper fold | [`design-reference/04-sales-report-upper.png`](design-reference/04-sales-report-upper.png) |

- **Colors are measured**, by pixel sampling and per-region hue clustering of the four frames. Hex
  values below are the sampled values, rounded to the nearest clean ramp step.
- **Lengths are proportional, not measured.** The frames are 1024 px wide re-encodings of a screen
  recording, so the source DPI scale is unknown. Every length below was derived from the observed
  ratios (sidebar ≈ 14.7 % of window width, card gap ≈ 0.67 × content margin, hero radius ≈ 1.4 ×
  card radius, and so on) and then snapped to a 4-DIP grid. Reproducing the *proportions* is the
  acceptance bar, not matching a pixel count.
- **Type sizes are proportional** for the same reason, with the extra caveat that anti-aliasing on
  small Persian glyphs makes ink-height sampling unreliable below ~16 DIP. The scale below preserves
  the observed hierarchy (five distinct steps between caption and hero title).
- The reference shows **light theme only**; a moon toggle is present in the top bar, so a dark
  variant is a product intent but its palette is not observable. §3 marks the dark values as derived.

## 1. Palette

### 1.1 Brand ramp (teal — the single hue the product is built on)

| Token | Hex | Where it was observed |
|---|---|---|
| `Brand50` | `#E6F6F3` | KPI icon-tile tint, chip tint, heatmap step 1 |
| `Brand100` | `#C7EAE4` | soft fills, disabled brand surface |
| `Brand200` | `#A3DDD5` | heatmap step 2 (`#B4DEDE`) |
| `Brand300` | `#66C0B4` | heatmap step 3 |
| `Brand400` | `#19A09B` | chart series 1, hero gradient light end (`#1DA99E`) |
| `Brand500` | `#109089` | **primary**: filled buttons (`#12908A`), active links, sparkline stroke |
| `Brand600` | `#0E7A75` | primary pressed, hero gradient dark end (`#0F7572`) |
| `Brand700` | `#0B5F5B` | primary focus ring, high-contrast text on `Brand50` |
| `Brand800` | `#113D3C` | sidebar active item (`#0F4140`), sidebar promo card |
| `Brand900` | `#0B2526` | sidebar / app rail background (measured on all four frames) |

### 1.2 Neutrals (light theme)

| Token | Hex | Observed as |
|---|---|---|
| `PageBackground` | `#F2F6F5` | content canvas and top-bar background (they are one surface) |
| `Surface` | `#FFFFFF` | every card, dialog, popup |
| `SurfaceMuted` | `#F9FBFA` | search field, hour-gutter column, inactive segmented segment |
| `SurfaceTrack` | `#F0F3F2` | ranked-bar track, progress track |
| `Border` | `#E6EBEA` | card outline (`#E8ECEB`) |
| `Divider` | `#EDF0F0` | chart gridlines, calendar hour lines, list separators |
| `TextPrimary` | `#14201F` | page title, card title, KPI number |
| `TextSecondary` | `#5B6664` | subtitles, card footer notes, axis labels |
| `TextMuted` | `#8A9694` | placeholder, breadcrumb tail, hour-gutter labels |
| `TextOnBrand` | `#FFFFFF` | label on filled brand surfaces |

### 1.3 On-dark (sidebar) neutrals

| Token | Hex | Observed as |
|---|---|---|
| `RailBackground` | `#0B2526` | sidebar fill |
| `RailItemActive` | `#113D3C` | selected nav pill |
| `RailItemHover` | `#0F332C` | hover pill (lighter than active, no border) |
| `RailTextActive` | `#FFFFFF` | selected nav label + icon |
| `RailTextInactive` | `#A6BCB8` | unselected nav label + icon |
| `RailSectionLabel` | `#6E8C8B` | group overlines (`مرکز فرمان`, `مدیریت کلینیک`, `عملیات مالی`) |

### 1.4 Accents (each has a tint fill, a base, and a strong text/icon step)

| Role | Tint | Base | Strong | Observed as |
|---|---|---|---|---|
| Info (blue) | `#E9F1FC` | `#4E84DE` | `#2F5FB0` | KPI "sales" tile, calendar exam-room-2 events, donut segment 2, bar series 2 (`#73A0E6`) |
| Warning (amber) | `#FFF3E0` | `#EBA53C` | `#9A6410` | vaccination events, insight callout, donut segment 3 |
| Violet | `#EFEAFC` | `#8C64D7` | `#5A3AA0` | KPI "patients" tile, exam-room-1 events, ranked-bar series 4 |
| Danger (red) | `#FDECE8` | `#E4574C` | `#A32A20` | low-stock alert card, negative KPI delta, ranked-bar series 5 |
| Success | `Brand50` | `Brand500` | `Brand700` | positive KPI delta, `پاسخگو` / `تأیید شده` chips — success reuses the brand hue, it is **not** a separate green |

### 1.5 Gradients and chart palette

- **Hero / CTA banner**: linear gradient along the flow axis, `#1FAB9F` at the leading (right, RTL)
  edge → `#0E726E` at the trailing edge, plus a large low-opacity white radial highlight behind the
  illustration. Overlay surfaces on the hero are white at low alpha: date pill ≈ 14 %, secondary
  button ≈ 22 %, solid-white primary button.
- **Chart series order**: 1 `#19A09B`, 2 `#73A0E6`, 3 `#EBA53C`, 4 `#8C64D7`, 5 `#E4574C`.
  Donut segments use the more saturated `#4E84DE` for series 2.
- **Area / sparkline fill**: `#19A09B` at 28 % alpha fading to fully transparent at the baseline;
  stroke `#109089`, ~2 DIP, no data-point markers.
- **Heatmap ramp (5 steps)**: `#E8F5F3`, `#B4DEDE`, `#66C0B4`, `#19A09B`, `#0E7A75`.
- **Chart chrome**: gridlines `#EDF0F0`, axis labels `TextSecondary`, no axis lines, no border.

## 2. Typography

- Family: **Vazirmatn** everywhere, including numerals; no secondary family, no monospace anywhere in
  the reference.
- Weights in use: 400 (body), 500 (labels, nav), 600 (card/section titles, KPI numbers), 700 (page
  and hero titles).
- Digits are **Persian-Indic** in every position — counts, money, times, dates, chart axis labels,
  percentages. Latin digits appear only in the OS chrome, never in the app.

| Token | Size / line-height | Weight | Observed on |
|---|---|---|---|
| `HeroTitle` | 32 / 44 | 700 | `امروز یک روز عالی برای مراقبت است.` |
| `PageTitle` | 20 / 30 | 700 | `صبح بخیر، دکتر آریا`, `تقویم نوبت‌ها` |
| `SectionTitle` | 16 / 24 | 600 | `نبض مالی کلینیک`, `نوبت‌های پیش رو` |
| `MetricValue` | 24 / 32 | 600 | `۱,۲۸۴`, `۸۶,۴۵۰,۰۰۰` |
| `Body` | 14 / 22 | 400 | list-row titles, hero subtitle, table cells |
| `BodyStrong` | 14 / 22 | 500 | nav labels, user chip name, button labels |
| `Caption` | 12 / 18 | 400 | list-row subtitles, KPI labels, footer notes, axis labels |
| `Overline` | 11 / 16 | 500 | sidebar group labels, card eyebrow labels (`روند ماهانه`, `جریان نقدی`) — slightly tracked out, `TextMuted` / `RailSectionLabel` |

## 3. Dark variant (derived, not observed)

Derived from the sidebar's own dark surfaces, which are the only dark values the reference actually
contains. Confirm with the product owner before treating these as final.

| Token | Hex |
|---|---|
| `PageBackground` | `#0B2526` |
| `Surface` | `#0F3130` |
| `SurfaceMuted` | `#113D3C` |
| `Border` | `#1C4B49` |
| `Divider` | `#173F3E` |
| `TextPrimary` | `#E7F2F0` |
| `TextSecondary` | `#A6BCB8` |
| `TextMuted` | `#6E8C8B` |
| Primary | `Brand400` (`#19A09B`) — one step lighter than light theme, for contrast on dark surfaces |
| Accent tints | accent base at 18 % alpha over `Surface`, instead of the light tint hexes |

### 3.1 Addendum — phase 01 dark-theme decision

No dark reference screen followed this table. Per the phase owner's direction (phase 01 TODO item 2),
the design system was completed rather than blocked: the table above is treated as final, and the
following additional tokens — not observable from any reference — were designed to round it out:

| Token | Hex | Rationale |
|---|---|---|
| `SurfaceTrack` | `#174443` | Interpolated between `SurfaceMuted` (`#113D3C`) and `Border` (`#1C4B49`); light theme's `SurfaceTrack` sits the same way between its `SurfaceMuted`/`Border`. |
| `RailBackground` | `#081C1D` | One notch darker than dark `PageBackground`/`Surface`, so the rail keeps acting as the darkest anchor surface the way it does in light theme (where it is far darker than the mint page). |
| `RailItemHover` | `#0F2F2D` | Between `RailBackground` and `RailItemActive` (`#113D3C`, unchanged), mirroring light theme's hover-lighter-than-background, darker-than-active relationship. |
| Accent `Strong` (info/warning/violet/danger) | `#9CC4F5` / `#F5C878` / `#C7ADF0` / `#F2A79E` | Lightened versions of each accent `Base`, since the light theme's near-black `Strong` text would fail contrast on a translucent dark tint. `Base` itself is reused unchanged (already legible on dark). |
| Accent `Tint` (info/warning/violet/danger) | `Base` colour at 18 % alpha (`#2E` + base hex) | Direct application of this table's own "accent base at 18 % alpha" instruction, made concrete as literal ARGB colors so `Color` resources stay solid values. |
| Success accent | `Brand400`/`Brand400`@18 %/`Brand300` | Success keeps reusing the brand ramp (per §1.4), shifted one step lighter to follow the dark-theme primary shift. |
| `StateHoverOverlay` / `StatePressedOverlay` | `#0AE7F2F0` / `#14E7F2F0` (white @ ~4 %/8 %) | Mirrors light theme's black-at-3 %/8 % overlay pattern, inverted for a dark base. |
| `StateFocusRing` | `Brand300` (`#66C0B4`) | `Brand700`, light theme's focus ring, is too close to the dark surface colors to read; `Brand300` is the ramp step that stays visible on both dark page and dark card surfaces. |

Canonical machine-readable form: `desktop/src/Animora.Desktop.UI/Theme/Tokens/Colors.Dark.axaml`
(semantic keys) and `Theme/Tokens/Palette.axaml` (raw values, `*Dark` suffix).

## 4. Geometry (4-DIP grid)

### 4.1 Layout

| Token | Value | Note |
|---|---|---|
| `RailWidth` | 240 | fixed, on the **right** edge |
| `TopBarHeight` | 88 | holds breadcrumb + page title stacked on the right, actions on the left |
| `ContentPaddingX` | 32 | left/right page gutter |
| `ContentPaddingY` | 24 | above the hero, below the last card |
| `CardGap` | 16 | both axes of the card grid |
| `HeroHeight` | 216 | dashboard CTA banner |
| `SectionGap` | 24 | between hero and the KPI row |

### 4.2 Radii

| Token | Value | Applies to |
|---|---|---|
| `RadiusHero` | 20 | hero banner, promo card |
| `RadiusCard` | 16 | every white card |
| `RadiusBlock` | 12 | calendar event block, icon tile, nav pill, segmented control |
| `RadiusControl` | 10 | buttons, text inputs, combo boxes |
| `RadiusBadge` | 8 | count badge, small tag |
| `RadiusPill` | 999 | status chips, date pill, avatar |

### 4.3 Sizes

| Token | Value |
|---|---|
| `ControlHeight` (default button, input, combo) | 36 |
| `ControlHeightSmall` (chip, segmented segment) | 28 |
| `ControlHeightLarge` (hero CTA) | 44 |
| `ButtonPaddingX` | 16 (20 for `ControlHeightLarge`) |
| `NavItemHeight` | 40 |
| `IconSizeSmall` / `IconSize` / `IconSizeLarge` | 16 / 20 / 24 |
| `IconTileSize` (KPI card tile) | 40, `RadiusBlock`, accent tint fill, accent-strong glyph |
| `AvatarSize` | 32 (top bar), 40 (list rows) |
| `AccentBarWidth` | 3 (list row), 4 (calendar event) — on the leading (right) edge |
| `CardPadding` | 20 (KPI card), 24 (section card) |
| `StrokeThickness` | 1 |

### 4.4 Spacing scale

`2, 4, 8, 12, 16, 20, 24, 32, 40, 48` — every gap observed in the reference lands on this scale;
inside cards, 8 (label ↔ value), 12 (icon ↔ text), 16 (row ↔ row) dominate.

## 5. Elevation

Shadows are ambient and very low contrast — the design separates surfaces with radius and the mint
canvas, not with depth.

| Token | Value |
|---|---|
| `ShadowCard` | `0 1 2 rgba(11,37,38,0.04)` + `0 8 24 rgba(11,37,38,0.05)` |
| `ShadowHero` | `0 12 32 rgba(14,114,110,0.22)` (brand-tinted, the only pronounced shadow) |
| `ShadowPopup` | `0 24 48 rgba(11,37,38,0.16)` (dialog, dropdown, toast) |
| none | chips, badges, icon tiles, calendar events, segmented controls — flat |

## 6. Component anatomy

- **App rail (sidebar, right)**: brand mark + product name + tagline block on top; nav groups, each
  introduced by an `Overline` label; nav item = leading icon (20) + label (`BodyStrong`) + optional
  count badge on the far side; active item is a `Brand800` pill spanning the rail's inner width with
  white icon+label; a collapse chevron sits at the rail's inner top corner. Bottom of the rail holds
  an upsell card (`Brand800` fill, `RadiusHero`, white title + `Caption` + full-width white outline
  button).
- **Top bar**: breadcrumb (`Caption`, `TextMuted`, ` / ` separators, coarsest crumb first from the
  right) above the page title (`PageTitle`), both right-aligned; on the left, a search field
  (`SurfaceMuted`, `RadiusPill`, leading magnifier glyph, trailing keyboard-shortcut hint), circular
  icon buttons (notifications with a dot badge, theme toggle), then a user chip: avatar (32,
  `RadiusPill`, brand fill, initial) + name (`BodyStrong`) + role (`Caption`, `TextMuted`) +
  chevron. A `StrokeThickness` `Divider` hairline closes the bar's bottom edge at `TopBarHeight`,
  spanning the content column edge to edge (no gutter inset) — the only separator between bar and
  content canvas, since both share the same fill. The same rule continues across the rail as a
  barely-lighter tint of `RailBackground`, under the brand block.
- **Hero / CTA banner**: full content width, `HeroHeight`, `RadiusHero`, brand gradient, `ShadowHero`.
  Contains a date pill (translucent white, leading calendar glyph) above a `HeroTitle`, a
  `Body` subtitle, then two buttons — solid white/`Brand600`-text primary and translucent-white
  secondary — and a decorative illustration on the trailing edge with a floating stat pill.
- **KPI stat card**: `Surface`, `RadiusCard`, `CardPadding` 20, `ShadowCard`. Icon tile in the
  leading top corner, delta badge (pill, accent tint fill, accent-strong text, ▲/▼ glyph) in the
  trailing top corner, `MetricValue` beneath, `Caption` label under it, and a `Caption`/`TextMuted`
  footer note separated by a `Divider`. Cards sit in an equal-width 4-up grid.
- **Section card**: header row = title (`SectionTitle`) on the leading side, an action text link
  (`Brand500`, `Caption`, trailing chevron) on the other side, optionally a segmented control
  (`SurfaceMuted` track, selected segment = `Surface` + `ShadowCard`, `RadiusBlock`). Body is the
  chart, list, or table.
- **List row (upcoming appointments)**: leading vertical accent bar (3), time (`Caption`,
  `TextMuted`), then a small avatar/initial tile, title (`Body`) and subtitle (`Caption`), with a
  status chip on the trailing edge. Rows separated by `Divider`, no zebra striping.
- **Status chip**: `RadiusPill`, accent tint fill, accent-strong text, `Caption`, height 28, no
  border, no shadow. Observed labels map to accents: `پاسخگو`/`تأیید شده` → success,
  `در انتظار` → warning, `لغو شده` → danger, `اصلاح` → info.
- **Count badge**: `RadiusBadge`, 18–20 tall, accent or `Brand800` fill, `Caption`, Persian digits;
  used on nav items.
- **Calendar (day view)**: resource columns with a header row of column titles; hour gutter on the
  **right** with `Caption`/`TextMuted` labels and full-width `Divider` hour lines; event block =
  accent tint fill, `RadiusBlock`, 4-DIP accent bar on its leading edge, time (`Overline`), title
  (`Body`), detail line (`Caption`), positioned and sized by start/duration; a date navigator
  (`امروز` + chevrons), a `روز/هفته/ماه` segmented control, and a resource filter sit above the grid.
- **Charts**: vertical grouped bars (rounded top corners, ~12 wide, 2 series), donut (thick ring,
  centred `MetricValue` + `Caption`, legend list beside it with a colour dot, name, value and
  share), ranked horizontal bars (label + value above a `SurfaceTrack` bar filled from the leading
  edge), and a heatmap grid of 5-step `RadiusBadge`-ish squares (~14, gap 4) with day/hour axes.
- **Insight callout**: `#FFF3E0` fill, `RadiusBlock`, leading lightbulb glyph in `#EBA53C`, single
  `Caption` line in `#9A6410`; sits inside a card's footer area.

## 7. RTL behaviour observed

- The rail is on the **right**; content flows from it leftwards. Breadcrumbs read right → left.
- All text is trailing-aligned to the right; icons sit to the **right** of their labels; chevrons on
  action links point **left** (toward the content they open).
- Accent bars, progress fills, and bar-chart growth all start at the **right** edge.
- Chart category axes run right → left (`شنبه` first from the right); the hour gutter is on the
  right of the calendar grid.
- Numbers keep their own LTR digit order inside the RTL line (`۸۶,۴۵۰,۰۰۰ تومان`), with the
  currency word trailing to the left of the amount.
