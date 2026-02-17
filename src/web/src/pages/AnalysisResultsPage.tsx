import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box,
    Typography,
    Paper,
    Grid,
    CircularProgress,
    Alert,
    Button,
    Chip,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Card,
    CardContent,
    Tooltip // Added
} from '@mui/material';
import { ArrowLeft, AlertTriangle, CheckCircle, FileText, AlertOctagon, Sparkles } from 'lucide-react'; // Added Sparkles
import { AnalysisService } from '../services/AnalysisService';
import type { AnalysisResult } from '../services/AnalysisService';

const AnalysisResultsPage = () => {
    const { jobId } = useParams<{ jobId: string }>();
    const navigate = useNavigate();
    const [result, setResult] = useState<AnalysisResult | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (jobId) {
            loadResults(jobId);
        }
    }, [jobId]);

    const loadResults = async (id: string) => {
        try {
            setLoading(true);
            const data = await AnalysisService.getJobResults(id);
            setResult(data);
        } catch (err) {
            if (err instanceof Error) {
                setError(err.message);
            } else {
                setError('Failed to load analysis results.');
            }
            console.error(err);
        } finally {
            setLoading(false);
        }
    };

    const getSeverityColor = (severity: string) => {
        switch (severity.toLowerCase()) {
            case 'critical': return 'error';
            case 'high': return 'warning';
            case 'medium': return 'info';
            case 'low': return 'success';
            default: return 'default';
        }
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
                <CircularProgress />
            </Box>
        );
    }

    if (error || !result) {
        return (
            <Box sx={{ mt: 4 }}>
                <Alert severity="error">{error || 'Result not found'}</Alert>
                <Button startIcon={<ArrowLeft />} onClick={() => navigate('/repositories')} sx={{ mt: 2 }}>
                    Back to Repositories
                </Button>
            </Box>
        );
    }

    return (
        <Box>
            <Button startIcon={<ArrowLeft />} onClick={() => navigate('/repositories')} sx={{ mb: 3 }}>
                Back to Repositories
            </Button>

            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 4 }}>
                <Typography variant="h4" sx={{ fontWeight: 600 }}>
                    Analysis Results
                </Typography>
                <Chip
                    label={`Score: ${result.overallCoverage}%`}
                    color={result.overallCoverage >= 80 ? 'success' : result.overallCoverage >= 50 ? 'warning' : 'error'}
                    sx={{ fontSize: '1.2rem', py: 2, px: 2, fontWeight: 'bold' }}
                />
            </Box>

            <Grid container spacing={3} sx={{ mb: 4 }}>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
                        <CardContent>
                            <Typography color="text.secondary" gutterBottom>Total Gaps</Typography>
                            <Typography variant="h4">{result.totalGaps}</Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card elevation={0} sx={{ border: '1px solid', borderColor: 'error.main', bgcolor: '#FEF2F2' }}>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <AlertOctagon size={20} color="#EF4444" />
                                <Typography color="error" gutterBottom sx={{ mb: 0 }}>Critical</Typography>
                            </Box>
                            <Typography variant="h4" color="error" sx={{ mt: 1 }}>{result.criticalGaps}</Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card elevation={0} sx={{ border: '1px solid', borderColor: 'warning.main', bgcolor: '#FFFBEB' }}>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <AlertTriangle size={20} color="#F59E0B" />
                                <Typography color="warning.main" gutterBottom sx={{ mb: 0 }}>High Priority</Typography>
                            </Box>
                            <Typography variant="h4" color="warning.main" sx={{ mt: 1 }}>{result.highPriorityGaps}</Typography>
                        </CardContent>
                    </Card>
                </Grid>
                <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                    <Card elevation={0} sx={{ border: '1px solid', borderColor: 'info.main', bgcolor: '#EFF6FF' }}>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <FileText size={20} color="#3B82F6" />
                                <Typography color="info.main" gutterBottom sx={{ mb: 0 }}>Analyzed Files</Typography>
                            </Box>
                            <Typography variant="h4" color="info.main" sx={{ mt: 1 }}>{result.analyzedFiles}</Typography>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            <Typography variant="h5" sx={{ mb: 2, fontWeight: 600 }}>
                Detailed Gaps
            </Typography>

            {/* Debug Info */}
            <Paper sx={{ p: 2, mb: 2, bgcolor: '#f5f5f5', border: '1px dashed #ccc' }}>
                <Typography variant="caption" fontFamily="monospace">
                    Debug Info: JobID={jobId} | TotalGaps_DB={result.totalGaps} | GapsArray_Len={result.gaps ? result.gaps.length : 'undefined'}
                </Typography>
            </Paper>

            <TableContainer component={Paper} elevation={0} sx={{ border: '1px solid', borderColor: 'divider' }}>
                <Table>
                    <TableHead>
                        <TableRow sx={{ backgroundColor: 'action.hover' }}>
                            <TableCell>Severity</TableCell>
                            <TableCell>Element</TableCell>
                            <TableCell>Issue</TableCell>
                            <TableCell>File Location</TableCell>
                            <TableCell>Actions</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {result.gaps.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={5} align="center" sx={{ py: 4 }}>
                                    <Box sx={{ display: 'flex', flexDirection: 'column', alignItems: 'center', gap: 2 }}>
                                        <CheckCircle size={48} color="#10B981" />
                                        <Typography variant="h6" color="text.secondary">
                                            No documentation gaps found! Great job!
                                        </Typography>
                                    </Box>
                                </TableCell>
                            </TableRow>
                        ) : (
                            result.gaps.map((gap) => (
                                <TableRow key={gap.id} hover>
                                    <TableCell>
                                        <Chip
                                            label={gap.severity}
                                            color={getSeverityColor(gap.severity) as any}
                                            size="small"
                                            variant="outlined"
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="subtitle2">
                                            {gap.elementName}
                                        </Typography>
                                        <Typography variant="caption" color="text.secondary">
                                            {gap.elementType}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2">
                                            {gap.message}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography
                                            variant="body2"
                                            sx={{
                                                fontFamily: 'monospace',
                                                fontSize: '0.85rem',
                                                textDecoration: 'underline',
                                                cursor: 'pointer',
                                                color: 'primary.main',
                                                '&:hover': { color: 'primary.dark' }
                                            }}
                                            onClick={() => navigate(`/file-view?path=${encodeURIComponent(gap.filePath)}&line=${gap.lineNumber}`)}
                                        >
                                            {gap.filePath.split('/').pop()}
                                        </Typography>
                                        <Typography variant="caption" color="text.secondary">
                                            Line: {gap.lineNumber}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Tooltip title="Get AI Suggestion (Phase 4)">
                                            <Button
                                                variant="outlined"
                                                size="small"
                                                startIcon={<Sparkles size={16} />}
                                                onClick={() => alert("AI Suggestion coming in Phase 4!")}
                                                sx={{ textTransform: 'none', fontSize: '0.75rem' }}
                                            >
                                                Suggest Fix
                                            </Button>
                                        </Tooltip>
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

export default AnalysisResultsPage;
