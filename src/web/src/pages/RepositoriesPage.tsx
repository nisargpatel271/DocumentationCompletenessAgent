import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Typography, Button, Stack, Alert, Paper, Table, TableHead,
  TableRow, TableCell, TableBody, CircularProgress, Chip, IconButton,
  Tooltip, Dialog, DialogTitle, DialogContent, DialogActions, TextField,
  FormControl, InputLabel, Select, MenuItem, Box, Divider
} from '@mui/material';
import { Plus, Play, Trash2, Github, Cloud, Search, ArrowRight } from 'lucide-react';
import { RepositoryService } from '../services/RepositoryService';
import { AnalysisService } from '../services/AnalysisService';
import { IntegrationService } from '../services/IntegrationService';
import type { Repository } from '../types';

export default function RepositoriesPage() {
  const navigate = useNavigate();
  const [repos, setRepos] = useState<Repository[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [runningId, setRunningId] = useState<string | null>(null);

  // Management States
  const [modalOpen, setModalOpen] = useState(false);
  const [importModalOpen, setImportModalOpen] = useState(false);
  const [importType, setImportType] = useState<'github' | 'ado'>('github');
  const [externalRepos, setExternalRepos] = useState<Repository[]>([]);
  const [fetchingExternal, setFetchingExternal] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const [modalError, setModalError] = useState<string | null>(null);
  const [submitting, setSubmitting] = useState(false);
  const [form, setForm] = useState({
    name: '',
    repositoryUrl: '',
    source: 'github',
    defaultBranch: 'main',
    personalAccessToken: ''
  });

  const loadRepos = async () => {
    try {
      setLoading(true);
      const data = await RepositoryService.getAll();
      setRepos(data);
    } catch (e: any) {
      setError('Failed to load repositories');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadRepos();
  }, []);

  const handleRunAnalysis = async (id: string) => {
    try {
      setRunningId(id);
      const job = await AnalysisService.runAnalysis(id);
      navigate(`/analysis/${job.id}`);
    } catch (e: any) {
      setError('Failed to start analysis');
    } finally {
      setRunningId(null);
    }
  };

  const handleDelete = async (id: string) => {
    if (!window.confirm('Are you sure you want to delete this repository?')) return;
    try {
      await RepositoryService.delete(id);
      loadRepos();
    } catch (e: any) {
      setError('Failed to delete repository');
    }
  };

  const handleAddRepo = async () => {
    if (!form.name || !form.repositoryUrl) {
      setModalError('Name and URL are required');
      return;
    }
    try {
      setSubmitting(true);
      setModalError(null);
      await RepositoryService.create(form);
      setModalOpen(false);
      resetForm();
      loadRepos();
    } catch (e: any) {
      setModalError(e.response?.data?.message || 'Failed to add repository');
    } finally {
      setSubmitting(false);
    }
  };

  const resetForm = () => {
    setForm({
      name: '',
      repositoryUrl: '',
      source: 'github',
      defaultBranch: 'main',
      personalAccessToken: ''
    });
  };

  const openImport = async (type: 'github' | 'ado') => {
    setImportType(type);
    setImportModalOpen(true);
    setFetchingExternal(true);
    setExternalRepos([]);
    try {
      const data = type === 'github'
        ? await IntegrationService.getGitHubRepos()
        : await IntegrationService.getAdoRepos();
      setExternalRepos(data);
    } catch (e: any) {
      setModalError('Failed to fetch external repositories');
    } finally {
      setFetchingExternal(false);
    }
  };

  const handleImportRepo = async (repo: Repository) => {
    try {
      setSubmitting(true);
      await RepositoryService.create({
        name: repo.name,
        repositoryUrl: repo.repositoryUrl,
        source: repo.source,
        defaultBranch: repo.defaultBranch,
        personalAccessToken: '' // Assumes token is already in backend settings
      });
      setImportModalOpen(false);
      loadRepos();
    } catch (e: any) {
      alert('Failed to import repository');
    } finally {
      setSubmitting(false);
    }
  };

  const filteredExternal = externalRepos.filter(r =>
    r.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
    r.repositoryUrl.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <>
      <Stack direction="row" justifyContent="space-between"
        alignItems="center" mb={3}>
        <Typography variant="h5" fontWeight={700}>Repositories</Typography>
        <Stack direction="row" gap={2}>
          <Button variant="outlined" startIcon={<Github size={16} />}
            onClick={() => openImport('github')}>
            Import GitHub
          </Button>
          <Button variant="outlined" startIcon={<Cloud size={16} />}
            onClick={() => openImport('ado')}>
            Import ADO
          </Button>
          <Button variant="contained" startIcon={<Plus size={16} />}
            onClick={() => setModalOpen(true)}>
            Add Manual
          </Button>
        </Stack>
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <Paper elevation={0}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Name</TableCell>
              <TableCell>Source</TableCell>
              <TableCell>Branch</TableCell>
              <TableCell>Status</TableCell>
              <TableCell>Last Scanned</TableCell>
              <TableCell align="right">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 6 }}>
                  <CircularProgress size={32} color="primary" />
                </TableCell>
              </TableRow>
            ) : repos.length === 0 ? (
              <TableRow>
                <TableCell colSpan={6} align="center" sx={{ py: 6 }}>
                  <Typography color="text.secondary">
                    No repositories yet. Add one to get started.
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              repos.map(repo => (
                <TableRow key={repo.id}>
                  <TableCell>
                    <Typography fontWeight={600} fontSize={14}>{repo.name}</Typography>
                    <Typography fontSize={12} color="text.secondary">
                      {repo.repositoryUrl}
                    </Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={repo.source?.toLowerCase() === 'github' ? 'GitHub' : repo.source}
                      size="small" variant="outlined" />
                  </TableCell>
                  <TableCell>
                    <Typography fontSize={14}>{repo.defaultBranch}</Typography>
                  </TableCell>
                  <TableCell>
                    <Chip label={repo.isActive ? 'Active' : 'Inactive'}
                      size="small"
                      sx={{
                        bgcolor: repo.isActive ? 'rgba(70,211,105,0.1)' : 'rgba(163,163,163,0.1)',
                        color: repo.isActive ? '#46D369' : '#A3A3A3',
                        border: repo.isActive ? '1px solid rgba(70,211,105,0.2)' : '1px solid rgba(163,163,163,0.2)'
                      }} />
                  </TableCell>
                  <TableCell>
                    <Typography fontSize={13} color="text.secondary">
                      {repo.lastScannedAt
                        ? new Date(repo.lastScannedAt).toLocaleDateString()
                        : 'Never'}
                    </Typography>
                  </TableCell>
                  <TableCell align="right">
                    <Stack direction="row" justifyContent="flex-end" gap={1}>
                      <Tooltip title="Run Analysis">
                        <IconButton size="small" color="primary"
                          disabled={runningId === repo.id}
                          onClick={() => handleRunAnalysis(repo.id)}>
                          {runningId === repo.id
                            ? <CircularProgress size={18} color="inherit" />
                            : <Play size={16} />}
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton size="small" color="error"
                          onClick={() => handleDelete(repo.id)}>
                          <Trash2 size={16} />
                        </IconButton>
                      </Tooltip>
                    </Stack>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </Paper>

      {/* Manual Add Dialog */}
      <Dialog open={modalOpen} onClose={() => !submitting && setModalOpen(false)}
        maxWidth="sm" fullWidth>
        <DialogTitle sx={{ fontWeight: 700 }}>Add Repository Manually</DialogTitle>
        <DialogContent>
          <Stack spacing={2.5} sx={{ mt: 1 }}>
            {modalError && <Alert severity="error">{modalError}</Alert>}
            <TextField label="Repository Name" required fullWidth
              value={form.name}
              onChange={e => setForm(f => ({ ...f, name: e.target.value }))} />
            <TextField label="Repository URL" required fullWidth
              value={form.repositoryUrl}
              onChange={e => setForm(f => ({ ...f, repositoryUrl: e.target.value }))} />
            <FormControl fullWidth required>
              <InputLabel>Source</InputLabel>
              <Select label="Source" value={form.source}
                onChange={e => setForm(f => ({ ...f, source: e.target.value as string }))}>
                <MenuItem value="github">GitHub</MenuItem>
                <MenuItem value="ado">Azure DevOps</MenuItem>
              </Select>
            </FormControl>
            <TextField label="Default Branch" fullWidth
              value={form.defaultBranch}
              onChange={e => setForm(f => ({ ...f, defaultBranch: e.target.value }))} />
            <TextField label="Personal Access Token" fullWidth type="password"
              placeholder="Optional if token is in appsettings"
              value={form.personalAccessToken}
              onChange={e => setForm(f => ({ ...f, personalAccessToken: e.target.value }))} />
          </Stack>
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 3 }}>
          <Button onClick={() => setModalOpen(false)} disabled={submitting}>Cancel</Button>
          <Button variant="contained" onClick={handleAddRepo} disabled={submitting} sx={{ minWidth: 140 }}>
            {submitting ? <CircularProgress size={20} color="inherit" /> : 'Add Repository'}
          </Button>
        </DialogActions>
      </Dialog>

      {/* Import Dialog */}
      <Dialog open={importModalOpen} onClose={() => setImportModalOpen(false)} maxWidth="md" fullWidth>
        <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1.5, fontWeight: 700 }}>
          {importType === 'github' ? <Github size={24} color="#E50914" /> : <Cloud size={24} color="#E50914" />}
          Import from {importType === 'github' ? 'GitHub' : 'Azure DevOps'}
        </DialogTitle>
        <DialogContent sx={{ minHeight: '400px', display: 'flex', flexDirection: 'column' }}>
          <TextField
            fullWidth
            placeholder="Search repositories..."
            variant="outlined"
            sx={{ my: 2 }}
            InputProps={{ startAdornment: <Search size={18} style={{ marginRight: 8, opacity: 0.5 }} /> }}
            value={searchQuery}
            onChange={e => setSearchQuery(e.target.value)}
          />

          <Divider sx={{ mb: 2 }} />

          {fetchingExternal ? (
            <Box sx={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', justifyContent: 'center', gap: 2 }}>
              <CircularProgress color="primary" />
              <Typography color="text.secondary">Fetching your repositories...</Typography>
            </Box>
          ) : filteredExternal.length === 0 ? (
            <Box sx={{ flex: 1, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
              <Typography color="text.secondary">No repositories found.</Typography>
            </Box>
          ) : (
            <Box sx={{ flex: 1, overflow: 'auto' }}>
              {filteredExternal.map((repo, idx) => (
                <Paper key={idx} elevation={0}
                  sx={{
                    p: 2, mb: 1.5, border: '1px solid #2A2A2A', bgcolor: '#1A1A1A',
                    display: 'flex', alignItems: 'center', justifyContent: 'space-between',
                    '&:hover': { border: '1px solid #E50914', cursor: 'pointer' },
                    transition: 'all 0.2s'
                  }}
                  onClick={() => handleImportRepo(repo)}>
                  <Box>
                    <Typography fontWeight={600} sx={{ color: 'white' }}>{repo.name}</Typography>
                    <Typography fontSize={12} color="text.secondary">{repo.repositoryUrl}</Typography>
                  </Box>
                  <IconButton color="primary" sx={{ bgcolor: 'rgba(229,9,20,0.1)' }}>
                    <ArrowRight size={20} />
                  </IconButton>
                </Paper>
              ))}
            </Box>
          )}
        </DialogContent>
        <DialogActions sx={{ px: 3, pb: 2 }}>
          <Button onClick={() => setImportModalOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
