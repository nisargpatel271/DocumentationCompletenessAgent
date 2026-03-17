import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { ThemeProvider, createTheme, CssBaseline } from '@mui/material';
import AppLayout from './layouts/AppLayout';
import RepositoriesPage from './pages/RepositoriesPage';
import AnalysisResultsPage from './pages/AnalysisResultsPage';
import FileViewerPage from './pages/FileViewerPage';
import DashboardPage from './pages/DashboardPage';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: { main: '#E50914' },
    background: {
      default: '#141414',
      paper: '#1F1F1F',
    },
    text: {
      primary: '#FFFFFF',
      secondary: '#A3A3A3',
    },
    divider: '#2A2A2A',
    error: { main: '#E50914' },
    success: { main: '#46D369' },
    warning: { main: '#F5A623' },
    info: { main: '#0080FF' },
  },
  typography: {
    fontFamily: '"Inter", "Helvetica Neue", Helvetica, Arial, sans-serif',
    h4: { fontWeight: 700 },
    h5: { fontWeight: 700 },
    h6: { fontWeight: 600 },
  },
  shape: { borderRadius: 6 },
  components: {
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          backgroundColor: '#1F1F1F',
          border: '1px solid #2A2A2A',
        },
      },
    },
    MuiTableHead: {
      styleOverrides: {
        root: {
          '& .MuiTableCell-root': {
            backgroundColor: '#181818',
            color: '#A3A3A3',
            fontWeight: 600,
            fontSize: '0.75rem',
            textTransform: 'uppercase',
            letterSpacing: '0.08em',
            borderBottom: '1px solid #2A2A2A',
          },
        },
      },
    },
    MuiTableBody: {
      styleOverrides: {
        root: {
          '& .MuiTableRow-root': {
            borderBottom: '1px solid #2A2A2A',
            '&:hover': {
              backgroundColor: '#252525',
            },
            '&:last-child td': {
              borderBottom: 'none',
            },
          },
          '& .MuiTableCell-root': {
            borderBottom: 'none',
            color: '#FFFFFF',
          },
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        contained: {
          backgroundColor: '#E50914',
          color: '#FFFFFF',
          fontWeight: 700,
          textTransform: 'none',
          boxShadow: 'none',
          '&:hover': {
            backgroundColor: '#F40612',
            boxShadow: '0 0 20px rgba(229, 9, 20, 0.4)',
          },
        },
        outlined: {
          borderColor: '#A3A3A3',
          color: '#FFFFFF',
          textTransform: 'none',
          '&:hover': {
            borderColor: '#FFFFFF',
            backgroundColor: 'rgba(255,255,255,0.05)',
          },
        },
        text: {
          color: '#A3A3A3',
          textTransform: 'none',
          '&:hover': {
            color: '#FFFFFF',
            backgroundColor: 'rgba(255,255,255,0.05)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          fontWeight: 600,
          fontSize: '0.7rem',
          border: 'none',
        },
      },
    },
    MuiDialog: {
      styleOverrides: {
        paper: {
          backgroundColor: '#1F1F1F',
          border: '1px solid #2A2A2A',
        },
      },
    },
    MuiTextField: {
      defaultProps: { variant: 'outlined' },
      styleOverrides: {
        root: {
          '& .MuiOutlinedInput-root': {
            backgroundColor: '#141414',
            '& fieldset': { borderColor: '#2A2A2A' },
            '&:hover fieldset': { borderColor: '#A3A3A3' },
            '&.Mui-focused fieldset': { borderColor: '#E50914' },
          },
          '& .MuiInputLabel-root.Mui-focused': { color: '#E50914' },
        },
      },
    },
    MuiSelect: {
      styleOverrides: {
        root: {
          backgroundColor: '#141414',
        },
      },
    },
    MuiAlert: {
      styleOverrides: {
        root: { borderRadius: 6 },
      },
    },
  },
});

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<AppLayout />}>
            <Route index element={<Navigate to="/dashboard" replace />} />
            <Route path="dashboard" element={<DashboardPage />} />
            <Route path="repositories" element={<RepositoriesPage />} />
            <Route path="analysis/:jobId" element={<AnalysisResultsPage />} />
            <Route path="file-viewer" element={<FileViewerPage />} />
          </Route>
        </Routes>
      </BrowserRouter>
    </ThemeProvider>
  );
}
