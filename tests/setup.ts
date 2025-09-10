// Test setup file
import 'jest';

// Mock Electron APIs
const mockElectron = {
  app: {
    getPath: jest.fn((name: string) => {
      switch (name) {
        case 'userData': return '/mock/userData';
        case 'logs': return '/mock/logs';
        default: return '/mock';
      }
    }),
    quit: jest.fn(),
    whenReady: jest.fn(() => Promise.resolve()),
    on: jest.fn(),
    requestSingleInstanceLock: jest.fn(() => true)
  },
  BrowserWindow: jest.fn(() => ({
    loadURL: jest.fn(),
    loadFile: jest.fn(),
    show: jest.fn(),
    hide: jest.fn(),
    close: jest.fn(),
    on: jest.fn(),
    once: jest.fn(),
    webContents: {
      openDevTools: jest.fn()
    }
  })),
  ipcMain: {
    handle: jest.fn(),
    on: jest.fn()
  },
  Notification: jest.fn(() => ({
    show: jest.fn(),
    on: jest.fn()
  })),
  Tray: jest.fn(() => ({
    setToolTip: jest.fn(),
    setContextMenu: jest.fn(),
    on: jest.fn()
  })),
  Menu: {
    buildFromTemplate: jest.fn()
  },
  nativeImage: {
    createFromPath: jest.fn()
  },
  contextBridge: {
    exposeInMainWorld: jest.fn()
  },
  shell: {
    beep: jest.fn()
  }
};

// Mock native modules
jest.mock('electron', () => mockElectron);

jest.mock('better-sqlite3', () => {
  return jest.fn(() => ({
    prepare: jest.fn(() => ({
      run: jest.fn(),
      get: jest.fn(),
      all: jest.fn()
    })),
    exec: jest.fn(),
    pragma: jest.fn(),
    close: jest.fn()
  }));
});

jest.mock('ffi-napi', () => ({
  Library: jest.fn(() => ({})),
  Callback: jest.fn()
}));

jest.mock('ref-napi', () => ({
  types: {
    uint32: 'uint32',
    long: 'long',
    uint64: 'uint64',
    int64: 'int64',
    void: 'void',
    bool: 'bool',
    int: 'int',
    uint: 'uint',
    short: 'short'
  },
  get: jest.fn(),
  alloc: jest.fn()
}));

// Global test utilities
global.mockElectron = mockElectron;

// Suppress console logs during tests unless explicitly needed
global.console = {
  ...console,
  log: jest.fn(),
  debug: jest.fn(),
  info: jest.fn(),
  warn: jest.fn(),
  error: jest.fn()
};
