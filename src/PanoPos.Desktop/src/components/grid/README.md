# PanoDataGrid

Thin typed `AgGridReact` wrapper. Community modules are supplied per instance;
there is no enterprise package, licence, global grid defaults or API client.
The current Theming API uses PanoPOS CSS tokens, not legacy CSS themes.
Tauri replaces the index.html style nonce token on page load. The wrapper uses
that nonce for theme CSS without relaxing the production Content Security Policy.

Use the original AG Grid props: `columnDefs`, `rowData`, `gridOptions`,
`cellRenderer`, `cellEditor`, events, keyboard callbacks, selection, `initialState`,
`onStateUpdated`, `onGridReady` and `ref`. `ref.current.api` is the original API.
Top-level props take precedence over gridOptions. Default column/locale objects
merge with PanoPOS defaults; per-column settings still override defaultColDef.
Sorting, filtering, editing and selection are not forced on features.

Override theme with `panoGridTheme.withParams(...)`, height with `rowHeight` /
`headerHeight`, and container size with `containerStyle`. Scope any custom CSS
under the feature's `className`, never a global `.ag-*` selector. The default
container is 320px high. A percentage height requires a sized parent.

Numbers are opt-in: use `type: 'numericColumn'` for numeric alignment and the
display helpers as valueFormatters. `formatMoney` displays two decimals,
`formatQuantity` up to four, and `formatDecimal` exactly four. Null/undefined or
non-finite values display blank, not zero. They never change backend values.

Empty/loading text can be overridden through `localeText.noRowsToShow` /
`localeText.loadingOoo` or native `overlayComponent` / `overlayComponentSelector`
and their params. Grid errors and server state belong to the consuming feature.

Grid state remains accessible through AG Grid; no profile/persistence system
is implemented. Quick Sale quantity/unit/touch editors belong to that feature,
not this wrapper. The optional GridDemo in the placeholder is static test data,
not a sales screen, and may be removed in the next task.
