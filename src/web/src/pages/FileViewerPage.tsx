import { useEffect, useState } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Box, Typography, Paper, CircularProgress, Alert, Button } from '@mui/material';
import { ArrowLeft, FileCode, CheckCircle, AlertTriangle } from 'lucide-react';
import axios from 'axios';

const FileViewerPage = () => {
    const [searchParams] = useSearchParams();
    const navigate = useNavigate();
    const path = searchParams.get('path');
    const line = parseInt(searchParams.get('line') || '0', 10);

    const [content, setContent] = useState<string | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (path) {
            loadFile(path);
        }
    }, [path]);

    const loadFile = async (filePath: string) => {
        try {
            setLoading(true);
            const apiUrl = `http://localhost:5022/api/Files/content`;
            console.log('Fetching file from:', apiUrl);
            console.log('File path:', filePath);

            const response = await axios.get(apiUrl, {
                params: { path: filePath }
            });
            console.log('Response received:', response.data);
            setContent(response.data.content);
        } catch (err: any) {
            const errorMsg = err.response?.data || err.message || 'Failed to load file content.';
            setError(`Failed to load file: ${errorMsg}`);
            console.error('File load error:', err);
            console.error('Attempted path:', filePath);
        } finally {
            setLoading(false);
        }
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
                <CircularProgress />
            </Box>
        );
    }

    if (error || !content) {
        return (
            <Box sx={{ p: 4 }}>
                <Alert severity="error">{error || 'File content not found'}</Alert>
                <Button startIcon={<ArrowLeft />} onClick={() => navigate(-1)} sx={{ mt: 2 }}>
                    Go Back
                </Button>
            </Box>
        );
    }

    const lines = content.split('\n');

    return (
        <Box sx={{ height: 'calc(100vh - 100px)', display: 'flex', flexDirection: 'column' }}>
            <Box sx={{ mb: 2, display: 'flex', alignItems: 'center', gap: 2 }}>
                <Button startIcon={<ArrowLeft />} onClick={() => navigate(-1)}>
                    Back
                </Button>
                <Typography variant="h6" sx={{ fontFamily: 'monospace', flexGrow: 1 }}>
                    <FileCode size={20} style={{ marginRight: 8, verticalAlign: 'middle' }} />
                    {path?.split('/').pop()}
                </Typography>
                <Button
                    variant="contained"
                    color="primary"
                    startIcon={<CheckCircle size={18} />}
                    onClick={() => alert("AI Suggestion Feature Coming in Phase 4!")}
                >
                    Generate AI Fix
                </Button>
            </Box>

            <Paper
                elevation={0}
                sx={{
                    flexGrow: 1,
                    overflow: 'auto',
                    p: 2,
                    bgcolor: '#1e1e1e',
                    color: '#d4d4d4',
                    fontFamily: 'monospace',
                    borderRadius: 2,
                    border: '1px solid #333'
                }}
            >
                <table style={{ borderCollapse: 'collapse', width: '100%' }}>
                    <tbody>
                        {lines.map((text, index) => {
                            const lineNumber = index + 1;
                            const isTarget = lineNumber === line;
                            return (
                                <tr
                                    key={index}
                                    id={`line-${lineNumber}`}
                                    style={{
                                        backgroundColor: isTarget ? '#37373d' : 'transparent',
                                        borderLeft: isTarget ? '4px solid #f59e0b' : '4px solid transparent'
                                    }}
                                >
                                    <td style={{
                                        userSelect: 'none',
                                        width: '40px',
                                        textAlign: 'right',
                                        paddingRight: '16px',
                                        color: '#858585',
                                        verticalAlign: 'top'
                                    }}>
                                        {lineNumber}
                                    </td>
                                    <td style={{ whiteSpace: 'pre-wrap', wordBreak: 'break-word' }}>
                                        {text}
                                        {isTarget && (
                                            <Box sx={{
                                                display: 'flex',
                                                alignItems: 'center',
                                                gap: 1,
                                                mt: 0.5,
                                                p: 1,
                                                bgcolor: 'rgba(245, 158, 11, 0.1)',
                                                border: '1px solid #f59e0b',
                                                borderRadius: 1,
                                                color: '#f59e0b'
                                            }}>
                                                <AlertTriangle size={16} />
                                                <Typography variant="caption" sx={{ fontWeight: 'bold' }}>
                                                    Documentation Issue Detected Here
                                                </Typography>
                                            </Box>
                                        )}
                                    </td>
                                </tr>
                            );
                        })}
                    </tbody>
                </table>
            </Paper>
        </Box>
    );
};

export default FileViewerPage;
