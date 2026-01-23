import { createTheme, type ThemeOptions } from '@mui/material/styles';

const themeOptions: ThemeOptions = {
  palette: {
    mode: 'light',
    primary: {
      main: '#2F5E2E', // Brand Green
      light: '#5C8F4E', // Accent Green
      dark: '#1A3B1A',
      contrastText: '#ffffff',
    },
    secondary: {
      main: '#5C8F4E',
      light: '#E6EFE2', // Soft Accent
      dark: '#2F5E2E',
      contrastText: '#ffffff',
    },
    background: {
      default: '#FAFCF7', // Soft off-white
      paper: '#FFFFFF',
    },
    text: {
      primary: '#1F2937', // Main Text
      secondary: '#6B7280', // Muted Text
    },
    divider: '#E2E8D9', // Soft Green Border
  },
  typography: {
    fontFamily: '"Poppins", "Inter", "Helvetica", "Arial", sans-serif',
    h1: { fontWeight: 600, letterSpacing: '0.01em' },
    h2: { fontWeight: 600, letterSpacing: '0.01em' },
    h3: { fontWeight: 600, letterSpacing: '0.01em' },
    h4: { fontWeight: 600, letterSpacing: '0.01em' },
    h5: { fontWeight: 600, letterSpacing: '0.01em' },
    h6: { fontWeight: 600, letterSpacing: '0.01em' },
    body1: { fontFamily: '"Inter", sans-serif' },
    body2: { fontFamily: '"Inter", sans-serif' },
    button: { textTransform: 'none', fontWeight: 600, fontFamily: '"Poppins", sans-serif' },
  },
  shape: {
    borderRadius: 16, // Friendly, modern radius (14-18px range)
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          backgroundColor: '#FAFCF7',
          color: '#1F2937',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 16,
          padding: '8px 24px',
          boxShadow: 'none',
          transition: 'all 0.2s ease',
          '&:hover': {
            boxShadow: '0 4px 12px rgba(47, 94, 46, 0.15)',
            transform: 'translateY(-1px)',
          },
        },
        containedPrimary: {
          backgroundColor: '#2F5E2E',
          '&:hover': {
            backgroundColor: '#1F3F1F', // Darker green on hover
          },
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          boxShadow: '0 1px 3px rgba(0,0,0,0.05)',
          border: '1px solid #E2E8D9',
        },
        elevation1: {
          boxShadow: '0 4px 6px -1px rgba(0, 0, 0, 0.05)',
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          backgroundColor: 'rgba(250, 252, 247, 0.8)', // Transparent blur effect
          backdropFilter: 'blur(8px)',
          boxShadow: 'none',
          borderBottom: '1px solid #E2E8D9',
          color: '#1F2937',
          height: 170,
        },
      },
    },
    MuiDrawer: {
      styleOverrides: {
        paper: {
          backgroundColor: '#FAFCF7',
          borderRight: '1px solid #E2E8D9',
        },
      },
    },
    MuiListItemButton: {
      styleOverrides: {
        root: {
          borderRadius: 12,
          margin: '4px 8px',
          '&.Mui-selected': {
            backgroundColor: '#E6EFE2', // Soft Accent Green
            color: '#2F5E2E',
            '&:hover': {
              backgroundColor: '#D9E8D0',
            },
            '& .MuiListItemIcon-root': {
              color: '#2F5E2E',
            },
          },
        },
      },
    },
  },
};

export const theme = createTheme(themeOptions);
