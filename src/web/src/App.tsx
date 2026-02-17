import { CssBaseline, ThemeProvider } from '@mui/material';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { theme } from './theme/theme';
import Layout from './components/layout/Layout';
import RepositoriesPage from './pages/RepositoriesPage';

import AnalysisResultsPage from './pages/AnalysisResultsPage';
import FileViewerPage from './pages/FileViewerPage';

// Placeholder Pages
const Dashboard = () => <div>Dashboard Content</div>;
const Settings = () => <div>Settings Content</div>;

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/repositories" element={<RepositoriesPage />} />
            <Route path="/analysis/:jobId" element={<AnalysisResultsPage />} />
            <Route path="/file-view" element={<FileViewerPage />} />
            <Route path="/settings" element={<Settings />} />
          </Routes>
        </Layout>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
