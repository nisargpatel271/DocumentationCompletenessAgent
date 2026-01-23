import { Box, CssBaseline, ThemeProvider } from '@mui/material';
import { BrowserRouter, Route, Routes } from 'react-router-dom';
import { theme } from './theme/theme';
import Layout from './components/layout/Layout';

// Placeholder Pages
const Dashboard = () => <div>Dashboard Content</div>;
const Repositories = () => <div>Repositories Content</div>; // Reverted to placeholder
const Settings = () => <div>Settings Content</div>;

function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <BrowserRouter>
        <Layout>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/repositories" element={<Repositories />} />
            <Route path="/settings" element={<Settings />} />
          </Routes>
        </Layout>
      </BrowserRouter>
    </ThemeProvider>
  );
}

export default App;
