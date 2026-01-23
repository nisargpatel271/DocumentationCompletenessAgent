import React, { useEffect, useState } from 'react';
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
    Alert
} from '@mui/material';
import { Plus, Github, GitBranch } from 'lucide-react';
import { Repository } from '../types/Repository';
import { RepositoryService } from '../services/RepositoryService';

const RepositoriesPage = () => {
    const [repositories, setRepositories] = useState<Repository[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        loadRepositories();
    }, []);

    const loadRepositories = async () => {
        try {
            const data = await RepositoryService.getAll();
            setRepositories(data);
        } catch (err) {
            setError('Failed to load repositories. Please try again later.');
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
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
                    sx={{
                        borderRadius: 2,
                        textTransform: 'none',
                        px: 3
                    }}
                >
                    Add Repository
                </Button>
            </Box>

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
                            <TableCell>Last Scanned</TableCell>
                            <TableCell align="right">Actions</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {repositories.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={6} align="center" sx={{ py: 4 }}>
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
                                            <Typography variant="subtitle2" sx={{ fontWeight: 600 }}>
                                                {repo.name}
                                            </Typography>
                                            <Typography variant="caption" color="text.secondary">
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
                                        <Typography variant="body2" color="text.secondary">
                                            {repo.lastScannedAt ? new Date(repo.lastScannedAt).toLocaleDateString() : 'Never'}
                                        </Typography>
                                    </TableCell>
                                    <TableCell align="right">
                                        <Button size="small" variant="text">
                                            Manage
                                        </Button>
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
