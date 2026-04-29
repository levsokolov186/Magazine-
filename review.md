## 1. Dead Code
*   **1.1 `_Layout.cshtml.css`**: This scoped CSS file conflicts with `site.css` and contains unused classes. It silently overrides theming due to higher specificity. **Action:** Delete the entire file.
*   **1.2 `SizeEntry` Model**: A duplicate of `ProductSize`. Used only for form binding then immediately converted. **Action:** Remove `SizeEntry.cs` and use `ProductSize` everywhere.
*   **1.3 `ProductInput.CreatedAt`**: Set in Edit but never used in creation/update logic. Exists only for display. **Action:** Remove from DTO; add a separate read-only property to the PageModel.
*   **1.4 `Products` Setter**: The setter in `JsonDatabaseService` is never called; mutations happen via methods. **Action:** Make it `get`-only.
*   **1.5 `EnsureSeeded()` Null Check**: The null check for `_passwordHasher` never executes because it is always set earlier. **Action:** Remove the null check.
*   **1.6 Redundant DI Registration**: `IPasswordHasher` is explicitly registered in `Program.cs`, but `AddIdentity` already registers it. **Action:** Remove line 17.
*   **1.7 `Admin/_ViewStart.cshtml`**: Redundantly re-declares the layout already set by the root `_ViewStart`. **Action:** Delete the file.
*   **1.8 Unused JS Variable**: `productCategory` in `Product.cshtml` is declared but never used. **Action:** Remove.
*   **1.9 Unused Return Values**: `addToCart` and `addToFavorites` return arrays that no caller uses. **Action:** Remove return statements.
*   **1.10 Unused Query Params**: `navigateToProduct` sends `price`, `emoji`, and `badge` in the URL, but the server only reads `Name`. **Action:** Simplify to accept only `name`.
*   **1.11 Dead CSS Rules**: Animation delays for `.product-card:nth-child(2)` through `(8)` match nothing because each card is the only child of its wrapper. **Action:** Remove or fix selectors.

## 2. Redundancies & Duplication
*   **2.1 Create/Edit Forms**: ~90% identical HTML. **Action:** Extract shared partials (`_ProductForm.cshtml`, `_SizeManager.cshtml`).
*   **2.2 Size Mapping Logic**: Duplicated in `Product.FromInput()` and `Product.UpdateFrom()`. **Action:** Extract helper method.
*   **2.3 Badge Calculation**: Discount percentage calculation duplicated in View and ViewModel. **Action:** Add computed property to Model.
*   **2.4 Redundant `updateNavCounts()`**: Called in `theme.js` on load, and again redundantly in Index/Product pages. **Action:** Remove redundant calls.
*   **2.5 Identical Save Methods**: `SaveUsers`, `SaveRoles`, `SaveUserRoles` all just call `SaveData()`. **Action:** Consolidate into one public `Save()` method.

## 3. Simplifications & Optimizations
*   **3.1 Inefficient `SaveProduct`**: Removes and re-adds the same object reference. **Action:** Call `SaveData()` directly after mutation.
*   **3.2 Global jQuery Loading**: jQuery is loaded globally but only needed for validation on specific pages. **Action:** Move jQuery to `_ValidationScriptsPartial.cshtml`; convert other scripts to vanilla JS.
*   **3.3 Empty PageModels**: `Cart.cshtml.cs` and `Favorites.cshtml.cs` are empty. **Status:** Required by framework, but note for SPA migration.
*   **3.4 Misleading "Add to Cart" Button**: On Index page, the button navigates to details instead of adding to cart. **Action:** Change text to "Details" or implement actual add-to-cart logic.
*   **3.5 Favorites Remove Logic**: Removes only one size variant, not the whole product group. **Action:** Refactor to remove by product name.
*   **3.6 Undefined CSS Variable**: `--bg-light` is used but never defined, causing transparent backgrounds. **Action:** Define variable in theme blocks.
*   **3.7 XSS via `innerHTML`**: Product names inserted via string concatenation in Cart/Favorites. **Action:** Use `textContent` or escape HTML.
*   **3.8 Dead Breadcrumb Link**: Category link in breadcrumbs goes to `#`. **Action:** Remove link or implement filtering.
*   **3.9 Lost `ReturnUrl`**: Login form doesn't pass `returnUrl` back on failure. **Action:** Add hidden input.
*   **3.10 Language Inconsistency**: Error page is in English; rest of app is Russian. **Action:** Translate.

## 4. Critical Bugs
*   **4.1 Broken Admin Forms**: Product fields are in one `<form>` (no submit button), and the Save button is in another. Submitting saves only sizes, losing product metadata. **Severity: Critical.** **Action:** Merge into single form.
*   **4.2 Orphaned Schema Key**: `data.json` contains `ProductSizes: []` which is ignored. **Action:** Remove key.
*   **4.3 Null Role Names**: Seeded roles have `NormalizedName: null`, causing lookup failures. **Action:** Set `NormalizedName` during seeding.
*   **4.4 Thread Safety**: List mutations in `JsonDatabaseService` are not locked, risking race conditions and duplicate IDs. **Action:** Wrap mutations in `lock (_lock)`.
*   **4.5 Double Locking**: `AddProduct` locks, then calls `SaveData` which also locks. While reentrant, it’s misleading. **Action:** Choose one locking boundary.

## 5. Data Integrity & Logic
*   **5.1 Non-Unique Product Lookup**: Products looked up by Name. If two products share a name, the second is unreachable. **Action:** Use ID-based routing.
*   **5.2 Fragile URL Encoding**: Passing names in URLs can break with special characters. **Action:** Use ID routing.
*   **5.3 Reference Equality in Remove**: `RemoveProduct` uses object reference equality, failing after deserialization. **Action:** Use `RemoveAll(p => p.Id == id)`.
*   **5.4 Hardcoded Emoji in Cart**: Cart always shows 👠 regardless of product emoji. **Action:** Store and render dynamic emoji.
*   **5.5 Stale Cart Prices**: Cart stores price at add-time; admin price changes don’t update existing cart items. **Status:** Informational.

## 6. Security Vulnerabilities
*   **6.1 CSRF Tokens**: Size management forms lack explicit anti-forgery tokens (though Razor adds them automatically). **Status:** Verify after fixing form bug.
*   **6.2 Unnecessary Attribute**: `[IgnoreAntiforgeryToken]` on Error page is noise. **Action:** Remove.

## 7. Performance Issues
*   **7.1 Full File Rewrite**: Every save serializes/writes the entire DB. **Status:** Acceptable for small scale, bottleneck for growth.
*   **7.2 Stale Singleton Data**: Data loaded once at startup; external edits aren’t reflected. **Status:** Informational.
*   **7.3 Linear Search**: All lookups use `FirstOrDefault` on lists. **Status:** Acceptable for <100 users.

## 8. Accessibility & UX
*   **8.1 Keyboard Navigation**: Product cards use `onclick` on `<div>`, inaccessible via keyboard. **Action:** Use `<a>` tags or `tabindex`.
*   **8.2 Missing ARIA States**: Size buttons lack `aria-pressed`. **Action:** Toggle attribute.
*   **8.3 Missing ARIA Label**: Theme toggle lacks `aria-label`. **Action:** Add label.
*   **8.4 No SEO Meta Tags**: Missing description/OG tags. **Action:** Add meta tags.

## 9. Code Style & Consistency
*   **9.1 Inconsistent Naming**: Service named `_dbService`, `_db`, `Db` across files. **Action:** Standardize.
*   **9.2 Async/Sync Mismatch**: Handlers named `OnPostAsync` but contain no `await`. **Action:** Rename to `OnPost`.
*   **9.3 Empty ApplicationUser**: Class exists only for future extension. **Status:** Info.

## 10. CSS Issues
*   **10.1 Excessive `!important`**: 13 occurrences fighting Bootstrap specificity. **Action:** Use specific selectors or Sass variables.
*   **10.2 Redefined `.text-primary`**: Overrides Bootstrap default, causing unexpected colors. **Action:** Use custom class `.text-accent`.
*   **10.3 Fragile Footer Positioning**: Absolute footer with magic margin. **Action:** Use flexbox sticky footer.
*   **10.4 Duplicated Price Formatting**: Inline formatting repeated 7 times. **Action:** Create helper/tag helper.
*   **10.5 Inline Styles**: Extensive inline styles for layout. **Action:** Move to CSS classes.

## 11. JavaScript Issues
*   **11.1 Global Scope Pollution**: Functions exposed on `window`. **Action:** Namespace under `window.StepStyle`.
*   **11.2 Use of `var`**: Everywhere instead of `let`/`const`. **Action:** Replace.
*   **11.3 Duplicated LocalStorage Parsing**: Pages parse LS directly instead of using helpers. **Action:** Use `getCart()`/`getFavorites()`.
*   **11.4 Stale Module Variables**: `cart` variable cached locally, not synced with LS events. **Action:** Read from LS on render.
*   **11.5 HTML Injection in Favorites**: `onclick` attributes vulnerable to injection. **Action:** Use `data-*` attributes and event listeners.
*   **11.6 Price Formatting Mismatch**: JS regex vs Razor `N0` produce different results. **Action:** Unify formatting.
*   **11.7 FOUC on Dark Theme**: Theme applied after paint, causing flash. **Action:** Move script to `<head>` or set server-side.

## 12. Identity / Auth Integration
*   **12.1 Lifetime Mismatch**: Scoped stores depend on Singleton DB. **Status:** Anti-pattern, works but risky.
*   **12.2 Empty Dispose**: Implements IDisposable with empty body. **Status:** Info.
*   **12.3 Fragile Role Parsing**: Roles stored as pipe-delimited strings. **Action:** Use structured objects.
*   **12.4 Normalization Inconsistency**: Role comparisons use raw names, not normalized. **Action:** Normalize before comparison.

## 13. HTML / Razor Issues
*   **13.1 Missing Product ID in Form**: Hidden ID field outside submit form causes edit failure. **Action:** Include in submit form.
*   **13.2 Negative Discount Display**: Shows negative savings if OldPrice < Price. **Action:** Validate or use `Math.Abs`.
*   **13.3 Division by Zero**: Discount calc crashes if OldPrice is 0. **Action:** Add guard.
*   **13.4 XSS in `onclick`**: Single-quote escaping insufficient for double quotes/backslashes. **Action:** Use `Json.Serialize` or `data-*`.
*   **13.5 Blocking Dialogs**: `confirm()` dialogs block UI. **Status:** Info.
*   **13.6 Validation on Wrong Form**: Client-side validation attached to non-submitting form. **Action:** Fix form structure.

## 14. Vendor Bloat
*   **14.1 Unused Libraries**: 8 MB vendor libs, 95% unused. **Action:** Delete unused files, keep only minified essentials.
*   **14.2 Unused Bootstrap JS**: Loaded globally for 2 features. **Action:** Load conditionally or remove.
*   **14.3 CDN Icons**: External dependency for icons. **Action:** Self-host or use SVGs.

## 15. Responsive / Mobile Issues
*   **15.1 Media Query Overlap**: `min-width: 768px` and `max-width: 768px` overlap. **Action:** Use `767.98px`.
*   **15.2 Full-Width Cards on Mobile**: No column class for small screens. **Action:** Add `col-6`.
*   **15.3 Fixed Image Height**: 500px height pushes content below fold on phones. **Action:** Use responsive height.
*   **15.4 Missing Cursor Pointer**: Clickable cards lack visual cue. **Action:** Add `cursor: pointer`.

## 16. Performance & Loading
*   **16.1 No Reduced Motion Support**: Animations ignore user preference. **Action:** Add media query.
*   **16.2 Infinite Animation**: Hero wave animation runs constantly. **Action:** Pause when off-screen or remove.
*   **16.3 Heavy SVG Background**: Inline SVG for subtle effect. **Action:** Replace with gradient.
*   **16.4 No Theme Color Meta Tag**: Address bar color not set. **Action:** Add meta tag.
*   **16.5 Dev Config Risk**: `DetailedErrors: true` in dev config. **Status:** Low risk.

## 17. Missing Features
*   **17.1 No AccessDenied Page**: Returns 404. **Action:** Create page.
*   **17.2 No Custom 404 Page**: Default blank page. **Action:** Create custom page.
*   **17.3 Favorites State**: Button doesn’t reflect if item is favorited. **Action:** Check LS on load.
*   **17.4 No Search/Filter**: Feature gap.
*   **17.5 Stub Checkout**: Alert only, no order persistence. **Status:** Incomplete.

## 18. Error Handling
*   **18.1 No Try/Catch**: Backend crashes on corrupt JSON or disk errors. **Action:** Add error handling.
*   **18.2 TOCTOU Race**: `File.Exists` check not atomic with read. **Action:** Use try/catch.
*   **18.3 No JS Error Handling**: Silent failures on corrupt localStorage. **Action:** Wrap in try/catch.

## 19. Git Hygiene
*   **19.1 `data.json` Tracked**: Mutable DB in Git. **Action:** Ignore.
*   **19.2 `.csproj.user` Tracked**: User-specific settings in Git. **Action:** Untrack.
*   **19.3 Config Convention**: Base config tracked, dev ignored. **Status:** OK.
*   **19.4 No `.editorconfig`**: Inconsistent formatting. **Action:** Add file.

## 20. Identity Deep Dive
*   **20.1 No Security Stamp Store**: Password changes don’t invalidate sessions. **Severity: High.** **Action:** Implement interface.
*   **20.2 No Lockout Store**: No brute-force protection. **Action:** Implement interface.
*   **20.3 Seeded Roles Missing Fields**: `NormalizedName` and concurrency stamp missing. **Action:** Set manually.
*   **20.4 Invalid `aria-controls`**: References non-existent ID. **Action:** Fix ID or attribute.
*   **20.5 Redundant ARIA**: `role="main"` on `<main>`. **Action:** Remove.

## 21. .NET-Specific Issues
*   **21.1 Sync I/O in Async Methods**: Blocks thread pool. **Action:** Use async file I/O.
*   **21.2 Unused Using**: `Microsoft.AspNetCore.Authentication` in Login. **Action:** Remove.
*   **21.3 I/O in Constructor**: Delays startup. **Action:** Lazy init or hosted service.
*   **21.4 Sync Seeding**: Blocks first request. **Status:** Info.
*   **21.5 Null Suppression**: `_data = null!` risks NRE on load failure. **Action:** Initialize with new instance.

## 22. Security Headers
*   **22.1 Missing Headers**: No CSP, X-Frame-Options, etc. **Action:** Add middleware.
*   **22.2 Cookie Configuration**: Missing explicit HttpOnly/Secure/SameSite. **Action:** Configure explicitly.

## 23. Decimal/Size Edge Cases
*   **23.1 Precision Mismatch**: Decimal model vs step="0.5" input. **Action:** Add server-side validation.
*   **23.2 Float Equality**: Comparisons safe for decimal but fragile if changed. **Status:** Info.
*   **23.3 Price Display Rounding**: Input allows cents, display rounds to integer. **Action:** Align input/display precision.

## 24. Z-Index & Layering
*   **24.1 Sidebar Z-Index**: Inline style overrides Bootstrap, accidental correctness. **Action:** Use CSS class.
*   **24.2 Toast Z-Index**: 9999 above everything. **Action:** Define z-index scale.

## 25. CSS Architecture
*   **25.1 Mixed Units**: px/rem/em without consistency. **Action:** Standardize.
*   **25.2 No Spacing Variables**: Magic numbers throughout. **Action:** Define spacing scale.
*   **25.3 No Type Scale**: 20+ font sizes. **Action:** Define type scale.

## 26. HTML Semantics
*   **26.1 Heading Hierarchy**: Inconsistent across pages. **Status:** Minor.
*   **26.2 Forms Without IDs**: Accessibility risk. **Action:** Add IDs.
*   **26.3 Table Misuse**: Used for key-value pairs. **Action:** Use `<dl>`.
*   **26.4 Mixed JS Styles**: jQuery and Vanilla mixed inconsistently. **Action:** Pick one (Vanilla).

## 27. JSON Serialization
*   **27.1 Allocation Waste**: New `JsonSerializerOptions` per save. **Action:** Hoist to static readonly.
*   **27.2 Case Sensitivity**: Deserialization case-sensitive. **Action:** Enable case-insensitive.
*   **27.3 Option Mismatch**: Different options for save/load. **Action:** Use single shared instance.

## 28. XSS via Html.Raw
*   **28.1 Direct XSS**: `@Html.Raw(Model.Name)` in JS context. **Severity: High.** **Action:** Use `Json.Serialize`.
*   **28.2 Attribute XSS**: Emoji in `onclick` not JS-escaped. **Action:** Use `data-*`.

## 29. CSP Incompatibility
*   **29.1 Inline Handlers**: 17 inline handlers block strict CSP. **Action:** Move to external JS.
*   **29.2 Inline Scripts**: 6 inline script blocks. **Action:** Extract to files.

## 30. DateTime / Locale
*   **30.1 UTC Display**: Dates shown in UTC without conversion. **Action:** Convert to local time.
*   **30.2 Space Character Mismatch**: JS vs Razor use different space characters. **Status:** Cosmetic.
*   **30.3 MinValue Default**: `CreatedAt` defaults to year 0001 in DTO. **Action:** Remove from DTO.

## 31. Miscellaneous
*   **31.1 GET Logout**: Redirects without signing out. **Action:** Remove handler.
*   **31.2 No Delete Feedback**: Silent failure if product not found. **Action:** Add error message.
*   **31.3 Dead Function**: `clearAllFavorites` has no UI trigger. **Action:** Add button or remove function.
*   **31.4 Empty Cart Checkout**: No validation. **Status:** Low.
*   **31.5 Null Sizes**: `Sizes` can be null after deserialization. **Action:** Add null checks.

## 32. Model / Validation Gaps
*   **32.1 Dead Attributes**: Validation attributes on `Product` entity never triggered. **Action:** Move to DTO.
*   **32.2 Weak Error Messages**: DTO lacks custom messages. **Action:** Copy from Entity.
*   **32.3 Free Products**: Price can be 0. **Status:** Design decision.
*   **32.4 Optional Fields**: Category/Material/Color can be empty. **Action:** Add `[Required]` or handle gracefully.

## 33. Product Page Logic
*   **33.1 Gender Assumption**: Description assumes feminine gender. **Status:** Fragile.
*   **33.2 Unformatted Savings**: Raw decimal display. **Action:** Format number.
*   **33.3 Fragile Badge Detection**: Depends on dash character. **Action:** Use boolean flag.
*   **33.4 Culture Bug in JS**: Decimal separator comma breaks JS syntax. **Severity: Medium.** **Action:** Use invariant culture.

## 34. JsonUserStore Deep Issues
*   **34.1 Full Save on Update**: Saves all users/products on single user update. **Status:** Limitation.
*   **34.2 Reference Equality in Delete**: Fragile removal. **Action:** Use ID-based removal.
*   **34.3 Ambiguous Role Parsing**: `StartsWith` logic fragile. **Status:** Edge case.

## 35. Cart/Favorites Rendering Bugs
*   **35.1 DOM Rebuild Loop**: `innerHTML +=` rebuilds DOM every iteration. **Action:** Build string first.
*   **35.2 Prototype Iteration**: `for...in` iterates prototype. **Action:** Use `Object.keys`.
*   **35.3 Negative Quantity Race**: Rapid clicks cause negative qty. **Action:** Debounce.
*   **35.4 Lexicographic Sort**: Sizes sorted as strings ("10" before "9"). **Action:** Numeric sort.
*   **35.5 Integer-Only Formatter**: JS formatter fails on decimals. **Action:** Handle decimals.

## 36. Registration & Auth Flow
*   **36.1 Lost ReturnUrl**: Register form doesn’t pass returnUrl. **Action:** Add hidden input.
*   **36.2 Silent Role Failure**: Adding role fails silently if role missing. **Action:** Check result.
*   **36.3 Auto-Confirm Email**: No verification step. **Status:** Info.
*   **36.4 Password Hint Mismatch**: Hint may not match actual policy. **Action:** Align hint/config.

## 37. Layout & Navigation
*   **37.1 Double Container**: `container-fluid` wraps `container`. **Action:** Remove outer wrapper.
*   **37.2 Invalid Utility Class**: `align-items-center` on table. **Action:** Remove.
*   **37.3 Safe Emoji Render**: Edit sidebar renders safely. **Status:** Verified.
*   **37.4 Class-Based Collapse**: Navbar relies on class selector. **Action:** Use ID.
*   **37.5 Unused Scripts**: Cart/Favorites JS loaded globally. **Action:** Load conditionally.

## 38. Data Model & API
*   **38.1 Unsafe ID Generation**: Auto-increment not thread-safe. **Action:** Lock or use counter.
*   **38.2 Lossy Null Conversion**: `OldPrice` null becomes 0. **Status:** Minor.
*   **38.3 Opinionated Defaults**: Sizes 36-40 hardcoded. **Status:** UX friction.
*   **38.4 No Pagination**: Loads all products. **Status:** Scalability gap.

## 39. CSS Theming Gaps
*   **39.1 Static Hero Gradient**: Not overridden in dark theme. **Action:** Add dark variant.
*   **39.2 Static Badge Colors**: Not overridden in dark theme. **Action:** Review variables.
*   **39.3 Inconsistent Fallback**: Inline fallback color differs from variable. **Action:** Use semantic variable.
*   **39.4 Gradient Fallback**: Solid color fallback for gradient variable. **Action:** Standardize.
*   **39.5 Correct Toast Theming**: Uses variables correctly. **Status:** Positive finding.

## 40. CSS Hardcoded Colors
*   **40.1 Hero Button**: Hardcoded hex colors. **Action:** Use variables.
*   **40.2 Danger Button**: Hardcoded red. **Action:** Use variable.
*   **40.3 Cart Button**: Hardcoded green. **Action:** Use variable.
*   **40.4 Utility Override**: `.text-decoration-line-through` redefined. **Action:** Create custom class.

## 41. CancellationToken & Async
*   **41.1 Inconsistent Checks**: Some methods check cancellation, others don’t. **Action:** Standardize.
*   **41.2 O(n*m) Complexity**: Lazy LINQ in `GetUsersInRoleAsync`. **Action:** Materialize to HashSet.
*   **41.3 Sync Cancellation**: Token not passed to file I/O. **Status:** Low.

## 42. Accessibility Deep Dive
*   **42.1 No Noscript Fallback**: Site unusable without JS. **Action:** Add warning.
*   **42.2 Invisible Emojis**: Screen readers read Unicode name. **Action:** Add `aria-label`.
*   **42.3 Unlabeled Buttons**: +/- symbols lack context. **Action:** Add `aria-label`.
*   **42.4 Silent Validation Errors**: Not announced by screen readers. **Action:** Add `aria-live`.
*   **42.5 Inaccessible Confirm**: Blocking dialog. **Status:** Low.

## 43. Miscellaneous Remaining
*   **43.1 Cache Busting Gap**: Vendor scripts lack versioning. **Action:** Add `asp-append-version`.
*   **43.2 Title Inconsistency**: Varying title formats. **Action:** Standardize in layout.
*   **43.3 Missing User Role**: Seeded user lacks role assignment. **Action:** Add role.
*   **43.4 Smooth Scroll Disorientation**: Can affect motion-sensitive users. **Action:** Respect preference.
*   **43.5 Hover Noise**: Shadow animation on static cards. **Action:** Limit to interactive cards.
*   **43.6 Anchor Jump**: Functional but jarring without JS. **Status:** Correct.

## 44. CSS Transition Performance
*   **44.1 `transition: all`**: 19 occurrences animate unnecessary properties. **Action:** Specify properties.
*   **44.2 Repeated Easing**: Same bezier curve used 6 times. **Action:** Define variable.

## 45. Broken `.bg-light`
*   **45.1 Transparent Headers**: Admin headers invisible due to undefined variable. **Severity: Medium.** **Action:** Define variable.

## 46. Vendor Library Details
*   **46.1 Bootstrap**: 44 files, 3 used. **Action:** Delete unused.
*   **46.2 jQuery**: 3 files, 1 used. **Action:** Delete unused.
*   **46.3 Validation**: 4 files, 1 used. **Action:** Delete unused.

## 47. Project / Repo Misc
*   **47.1 Empty README**: No documentation. **Action:** Add docs.
*   **47.2 No NuGet Packages**: Bare framework. **Status:** Info.
*   **47.3 Naming Inconsistency**: Repo/Project/Brand names differ. **Action:** Align.
*   **47.4 Unused CSS Isolation**: Infrastructure active but unused. **Action:** Clean up after deleting dead file.
*   **47.5 Duplicate Gitignore Rules**: Redundant patterns. **Action:** Clean up.
*   **47.6 No Docker Files**: Deployment undefined. **Status:** Info.

## 48. ViewData/TempData
*   **48.1 Title Format**: Inconsistent branding. **Action:** Standardize.
*   **48.2 No Error Channel**: Only success messages supported. **Action:** Add error support.

## 49. Index Price Culture Bug
*   **49.1 Comma Operator Error**: Decimal prices in `onclick` break JS parsing. **Severity: Medium.** **Action:** Use invariant culture.

## 50. Null Safety
*   **50.1 Admin Index Crash**: `Sizes.Any()` throws if null. **Action:** Add null check.
*   **50.2 Material Null Reference**: `Contains` throws if material is null. **Action:** Add null check.

## 51. Model Architecture
*   **51.1 Drift Risk**: Parallel field lists in Product/Input. **Status:** Architectural observation.
*   **51.2 Coupled Data Class**: `ApplicationDbData` nested in service. **Action:** Move to Models.

## 52. Store Edge Cases
*   **52.1 Unchecked Remove**: `RemoveFromRole` saves even if nothing removed. **Action:** Check result.
*   **52.2 Reference Equality in Role Delete**: Fragile. **Action:** Use ID-based removal.
*   **52.3 No Duplicate Check**: Stores allow duplicates. **Action:** Add guards.
*   **52.4 Mutable Lists Exposed**: Callers can mutate state without saving. **Action:** Return `IReadOnlyList`.

## 53. Hidden Input Bloat
*   **53.1 Massive HTML Bloat**: Size inputs repeated 3x per form, 6x per page. **Action:** Merge forms.
*   **53.2 Boolean Casing**: Renders "True"/"False". **Status:** Cosmetic.

## 54. Size Decimal Locale
*   **54.1 Confirm Dialog Locale**: Accidentally correct for Russian, fragile. **Status:** Low.
*   **54.2 Admin Index Locale**: Consistent with Russian, inconsistent with input. **Status:** Low.

## 55. Bootstrap Audit
*   **55.1 Minimal Usage**: Only 30% of CSS used. **Action:** Purge unused.
*   **55.2 Minimal JS**: Only 2 interactions. **Status:** Info.

## 56. Final Micro-Level Findings
*   **56.1 Placeholder Inconsistency**: Create has placeholders, Edit doesn’t. **Action:** Add to Edit.
*   **56.2 Fragile Redirects**: Relative paths used. **Status:** Info.
*   **56.3 Missing UpdatedAt**: No record of last edit. **Action:** Add field.
*   **56.4 Accidental Replace**: `.Replace(",", " ")` does nothing in Russian locale. **Action:** Use invariant culture.
*   **56.5 Favorites Add Size**: Always adds first size. **Action:** Add selector.
*   **56.6 Hardcoded Condition**: Always "New". **Action:** Remove or add field.