import { afterEach, beforeEach } from 'vitest';
import { cleanup } from '@testing-library/react';

afterEach(cleanup);
beforeEach(() => { window.history.replaceState(null, '', '/'); });
