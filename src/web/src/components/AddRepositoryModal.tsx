import React, { useState, useEffect } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    List,
    ListItem,
    ListItemButton,
    ListItemText,
    Checkbox,
    Tabs,
    Tab,
    Box,
    Paper,
    CircularProgress,
    Alert,
    TextField
} from '@mui/material';
import { Github, Cloud, Link as LinkIcon } from 'lucide-react';
import type { Repository } from '../types/Repository';
import { RepositoryService } from '../services/RepositoryService';

interface AddRepositoryModalProps {
    open: boolean;
    onClose: () => void;
    onRepositoryAdded: () => void;
}

const AddRepositoryModal: React.FC<AddRepositoryModalProps> = ({ open, onClose, onRepositoryAdded }) => {
    const [source, setSource] = useState(0); // 0: GitHub, 1: ADO, 2: Manual
    const [loading, setLoading] = useState(false);
    const [repos, setRepos] = useState<Repository[]>([]);
    const [selectedRepos, setSelectedRepos] = useState<string[]>([]);
    const [error, setError] = useState<string | null>(null);
    const [manualUrl, setManualUrl] = useState('');

    useEffect(() => {
        if (open) {
            setManualUrl('');
            if (source !== 2) {
                loadRepos();
            }
        } else {
            // Reset state when closed
            setRepos([]);
            setSelectedRepos([]);
            setError(null);
            setManualUrl('');
        }
    }, [open, source]);

    const loadRepos = async () => {
        setLoading(true);
        setError(null);
        try {
            let data: Repository[] = [];
            if (source === 0) {
                data = await RepositoryService.getGitHubRepositories();
            } else if (source === 1) {
                data = await RepositoryService.getAzureDevOpsRepositories();
            }
            setRepos(data);
        } catch (err) {
            console.error(err);
            setError('Failed to fetch repositories. Please check your integration settings.');
        } finally {
            setLoading(false);
        }
    };

    const handleToggle = (repoId: string) => {
        const currentIndex = selectedRepos.indexOf(repoId);
        const newChecked = [...selectedRepos];

        if (currentIndex === -1) {
            newChecked.push(repoId);
        } else {
            newChecked.splice(currentIndex, 1);
        }

        setSelectedRepos(newChecked);
    };

    const handleImport = async () => {
        setError(null);
        try {
            if (source === 2) {
                // Manual Import
                if (!manualUrl.trim()) {
                    setError('Please enter a valid repository URL');
                    return;
                }

                // Extract name from URL
                const nameMatch = manualUrl.match(/\/([^\/]+?)(\.git)?$/);
                const name = nameMatch ? nameMatch[1] : 'Unknown Repository';

                const newRepo = {
                    name: name,
                    source: "Manual",
                    repositoryUrl: manualUrl.trim(),
                    defaultBranch: "main",
                    isActive: true,
                    settings: JSON.stringify({ isManual: true }),
                    scanFrequency: 'daily'
                };
                await RepositoryService.create(newRepo);
            } else {
                // List Import
                const reposToImport = repos.filter(r => selectedRepos.includes(r.id));
                for (const repo of reposToImport) {
                    const newRepo = {
                        name: repo.name,
                        source: repo.source,
                        repositoryUrl: repo.repositoryUrl,
                        defaultBranch: repo.defaultBranch,
                        isActive: true,
                        settings: repo.settings,
                        scanFrequency: 'daily'
                    };
                    await RepositoryService.create(newRepo);
                }
            }
            onRepositoryAdded();
            onClose();
        } catch (err) {
            console.error(err);
            setError('Failed to import repositories. Please try again.');
        }
    };

    return (
        <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
            <DialogTitle>Add Repository</DialogTitle>
            <DialogContent>
                <Box sx={{ borderBottom: 1, borderColor: 'divider', mb: 2 }}>
                    <Tabs value={source} onChange={(_, newValue) => setSource(newValue)}>
                        <Tab icon={<Github size={20} />} label="GitHub" iconPosition="start" />
                        <Tab icon={<Cloud size={20} />} label="Azure DevOps" iconPosition="start" />
                        <Tab icon={<LinkIcon size={20} />} label="Manual" iconPosition="start" />
                    </Tabs>
                </Box>

                {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

                {source === 2 ? (
                    <Box sx={{ p: 2 }}>
                        <TextField
                            fullWidth
                            label="Repository URL"
                            placeholder="https://github.com/username/repo.git"
                            value={manualUrl}
                            onChange={(e) => setManualUrl(e.target.value)}
                            variant="outlined"
                            autoFocus
                        />
                    </Box>
                ) : (
                    loading ? (
                        <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                            <CircularProgress />
                        </Box>
                    ) : (
                        <Paper elevation={0} variant="outlined" sx={{ maxHeight: 400, overflow: 'auto' }}>
                            <List dense>
                                {repos.length === 0 ? (
                                    <ListItem>
                                        <ListItemText primary="No repositories found." secondary="Check your PAT configuration." />
                                    </ListItem>
                                ) : (
                                    repos.map((repo) => {
                                        const labelId = `checkbox-list-label-${repo.id}`;
                                        return (
                                            <ListItem
                                                key={repo.id}
                                                disablePadding
                                            >
                                                <ListItemButton onClick={() => handleToggle(repo.id)} dense>
                                                    <Checkbox
                                                        edge="start"
                                                        checked={selectedRepos.indexOf(repo.id) !== -1}
                                                        tabIndex={-1}
                                                        disableRipple
                                                        inputProps={{ 'aria-labelledby': labelId }}
                                                    />
                                                    <ListItemText
                                                        id={labelId}
                                                        primary={repo.name}
                                                        secondary={repo.repositoryUrl}
                                                    />
                                                </ListItemButton>
                                            </ListItem>
                                        );
                                    })
                                )}
                            </List>
                        </Paper>
                    )
                )}
            </DialogContent>
            <DialogActions>
                <Button onClick={onClose}>Cancel</Button>
                <Button
                    onClick={handleImport}
                    variant="contained"
                    disabled={source === 2 ? !manualUrl : selectedRepos.length === 0}
                >
                    {source === 2 ? 'Import Repository' : `Import Selected (${selectedRepos.length})`}
                </Button>
            </DialogActions>
        </Dialog>
    );
};

export default AddRepositoryModal;
