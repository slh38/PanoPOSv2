import { useMemo } from 'react';
import type { CSSProperties, Ref } from 'react';
import { AllCommunityModule } from 'ag-grid-community';
import { AgGridReact } from 'ag-grid-react';
import type { AgGridReactProps } from 'ag-grid-react';
import { panoGridTheme } from './panoGridTheme';
import { panoGridLocale } from './panoGridLocale';

const communityModules = [AllCommunityModule];
const columnDefaults = { resizable: true, sortable: false, filter: false, editable: false };
const containerDefaults: CSSProperties = { height: 'var(--grid-default-height)', width: '100%', minWidth: 0 };

export type PanoDataGridProps<TData> = AgGridReactProps<TData> & { ref?: Ref<AgGridReact<TData>> };

export function PanoDataGrid<TData>({ ref, gridOptions, defaultColDef, localeText, containerStyle,
  modules, theme, styleNonce, loadThemeGoogleFonts, ...props }: PanoDataGridProps<TData>) {
  // Stable column defaults avoid resetting column state on unrelated UI renders.
  const columns = useMemo(() => ({ ...columnDefaults, ...gridOptions?.defaultColDef, ...defaultColDef }),
    [gridOptions?.defaultColDef, defaultColDef]);
  const locale = useMemo(() => ({ ...panoGridLocale, ...gridOptions?.localeText, ...localeText }),
    [gridOptions?.localeText, localeText]);
  const nonce = document.querySelector<HTMLMetaElement>('meta[name="pano-style-nonce"]')?.content;
  return <AgGridReact<TData> {...props} ref={ref} gridOptions={gridOptions}
    modules={modules ?? communityModules}
    theme={theme ?? gridOptions?.theme ?? panoGridTheme}
    defaultColDef={columns} localeText={locale}
    containerStyle={{ ...containerDefaults, ...containerStyle }}
    loadThemeGoogleFonts={loadThemeGoogleFonts ?? gridOptions?.loadThemeGoogleFonts ?? false}
    styleNonce={styleNonce ?? gridOptions?.styleNonce ?? (nonce?.startsWith('__TAURI_') ? undefined : nonce)} />;
}
