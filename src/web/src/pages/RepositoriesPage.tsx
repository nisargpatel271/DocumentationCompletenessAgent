import { useEffect, useState } from 'react';
import {
    Box,
    Typography,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Chip,
    Button,
    CircularProgress,
    Alert,
    IconButton,
    Tooltip
} from '@mui/material';
import { Plus, Github, GitBranch, Play, Trash2, Eye, BarChart3 } from 'lucide-react';
import type { Repository } from '../types/Repository';
import { RepositoryService } from '../services/RepositoryService';
import AddRepositoryModal from '../components/AddRepositoryModal';

import { useNavigate } from 'react-router-dom';
import { AnalysisService } from '../services/AnalysisService';

const RepositoriesPage = () => {
    const navigate = useNavigate();
    const [repositories, setRepositories] = useState<Repository[]>([]);
    const [loading, setLoading] = useState(true);
    const [runningAnalysis, setRunningAnalysis] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);

    const [isModalOpen, setIsModalOpen] = useState(false);

    useEffect(() => {
        loadRepositories();
    }, []);

    const loadRepositories = async () => {
        try {
            setLoading(true);
            const data = await RepositoryService.getAll();
            setRepositories(data);
        } catch (err) {
            setError('Failed to load repositories. Please try again later.');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const handleRunAnalysis = async (repoId: string) => {
        try {
            setRunningAnalysis(repoId);
            const job = await AnalysisService.runAnalysis(repoId);
            // Navigate to results page after job starts/completes
            navigate(`/analysis/${job.id}`);
        } catch (err) {
            setError('Failed to start analysis.');
            console.error(err);
        } finally {
            setRunningAnalysis(null);
        }
    };

    if (loading && repositories.length === 0) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <CircularProgress color="primary" />
            </Box>
        );
    }

    return (
        <Box>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h4" sx={{ fontWeight: 600, color: 'text.primary' }}>
                    Repositories
                </Typography>
                <Button
                    variant="contained"
                    startIcon={<Plus size={20} />}
                    onClick={() => setIsModalOpen(true)}
                    sx={{
                        borderRadius: 2,
                        textTransform: 'none',
                        px: 3
                    }}
                >
                    Add Repository
                </Button>
            </Box>

            <AddRepositoryModal
                open={isModalOpen}
                onClose={() => setIsModalOpen(false)}
                onRepositoryAdded={() => {
                    loadRepositories();
                    // Optional: Show success message
                }}
            />

            {error && (
                <Alert severity="error" sx={{ mb: 3 }}>
                    {error}
                </Alert>
            )}

            <TableContainer component={Paper} elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
                <Table>
                    <TableHead>
                        <TableRow sx={{ backgroundColor: 'action.hover' }}>
                            <TableCell>Name</TableCell>
                            <TableCell>Source</TableCell>
                            <TableCell>Branch</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Score</TableCell>
                            <TableCell>Last Scanned</TableCell>
                            <TableCell align="right">Actions</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {repositories.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                                    <Typography color="text.secondary">
                                        No repositories found. Add one to get started.
                                    </Typography>
                                </TableCell>
                            </TableRow>
                        ) : (
                            repositories.map((repo) => (
                                <TableRow key={repo.id} hover>
                                    <TableCell>
                                        <Box>
                                            <Typography
                                                variant="subtitle2"
                                                component="a"
                                                href={repo.repositoryUrl}
                                                target="_blank"
                                                rel="noopener noreferrer"
                                                sx={{
                                                    fontWeight: 600,
                                                    textDecoration: 'none',
                                                    color: 'text.primary',
                                                    cursor: 'pointer',
                                                    '&:hover': {
                                                        color: 'primary.main',
                                                        textDecoration: 'underline'
                                                    }
                                                }}
                                            >
                                                {repo.name}
                                            </Typography>
                                            <Typography variant="caption" color="text.secondary" display="block">
                                                {repo.repositoryUrl}
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            icon={<Github size={14} />}
                                            label={repo.source}
                                            size="small"
                                            variant="outlined"
                                            sx={{ textTransform: 'capitalize' }}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                            <GitBranch size={14} color="#6B7280" />
                                            <Typography variant="body2">{repo.defaultBranch}</Typography>
                                        </Box>
                                    </TableCell>
                                    <TableCell>
                                        <Chip
                                            label={repo.isActive ? 'Active' : 'Inactive'}
                                            color={repo.isActive ? 'success' : 'default'}
                                            size="small"
                                            sx={{
                                                backgroundColor: repo.isActive ? '#ECFDF5' : undefined,
                                                color: repo.isActive ? '#059669' : undefined,
                                                borderColor: 'transparent'
                                            }}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                            {/* Placeholder score logic */}
                                            <BarChart3 size={16} color="#9CA3AF" />
                                            <Typography variant="body2" color="text.secondary">
                                                N/A
                                            </Typography>
                                        </Box>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2" color="text.secondary">
                                            {repo.lastScannedAt ? new Date(repo.lastScannedAt).toLocaleDateString() : 'Never'}
                                        </Typography>
                                    </TableCell>
                                    <TableCell align="right">
                                        <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                                            <Tooltip title="Run Analysis">
                                                <IconButton
                                                    size="small"
                                                    color="primary"
                                                    onClick={() => handleRunAnalysis(repo.id)}
                                                    disabled={runningAnalysis === repo.id}
                                                >
                                                    {runningAnalysis === repo.id ? (
                                                        <CircularProgress size={18} />
                                                    ) : (
                                                        <Play size={18} />
                                                    )}
                                                </IconButton>
                                            </Tooltip>

                                            <Tooltip title="Delete">
                                                <IconButton size="small" color="error">
                                                    <Trash2 size={18} />
                                                </IconButton>
                                            </Tooltip>
                                        </Box>
                                    </TableCell>
                                </TableRow>
                            ))
                        )}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
};

export default RepositoriesPage;
