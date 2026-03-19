import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import {
  Box, Typography, Button, Stack, Alert, Paper, Table,
  TableHead, TableRow, TableCell, TableBody, CircularProgress,
  Chip
} from '@mui/material';
import { ArrowLeft, Sparkles, ExternalLink } from 'lucide-react';
import { AnalysisService } from '../services/AnalysisService';
import { SuggestionService } from '../services/SuggestionService';
import type { AnalysisResult, AISuggestion, DocumentationGap } from '../types';

export default function AnalysisResultsPage() {
  const { jobId } = useParams<{ jobId: string }>();
  const navigate = useNavigate();
  const [result, setResult] = useState<AnalysisResult | null>(null);
  const [polling, setPolling] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loadingSuggestions, setLoadingSuggestions] = useState<Record<string, boolean>>({});
  const [suggestions, setSuggestions] = useState<Record<string, AISuggestion>>({});
  const [suggestionStatus, setSuggestionStatus] = useState<Record<string, string>>({});

  useEffect(() => {
    let interval: ReturnType<typeof setInterval>;
    let isMounted = true;

    const checkJob = async () => {
      try {
        const job = await AnalysisService.getJobStatus(jobId!);
        if (!isMounted) return;

        if (job.status === 'Completed') {
          setPolling(false);
          const data = await AnalysisService.getJobResults(jobId!);
          if (isMounted) setResult(data);
        } else if (job.status === 'Failed') {
          setPolling(false);
          setError(job.errorMessage || 'Analysis failed');
        } else if (!interval) {
          // If still running/queued and we haven't started polling, start it
          setPolling(true);
          interval = setInterval(async () => {
            try {
              const updated = await AnalysisService.getJobStatus(jobId!);
              if (!isMounted) {
                clearInterval(interval);
                return;
              }

              if (updated.status === 'Completed') {
                clearInterval(interval);
                setPolling(false);
                const data = await AnalysisService.getJobResults(jobId!);
                if (isMounted) setResult(data);
              } else if (updated.status === 'Failed') {
                clearInterval(interval);
                setPolling(false);
                setError(updated.errorMessage || 'Analysis failed');
              }
            } catch (e) {
              if (isMounted) {
                clearInterval(interval);
                setPolling(false);
                setError('Lost connection to server');
              }
            }
          }, 3000);
        }
      } catch (e: any) {
        if (isMounted) {
          setPolling(false);
          setError('Failed to load results');
        }
      }
    };

    checkJob();

    return () => {
      isMounted = false;
      if (interval) clearInterval(interval);
    };
  }, [jobId]);

  const handleSuggest = async (gapId: string) => {
    try {
      setLoadingSuggestions(prev => ({ ...prev, [gapId]: true }));
      const s = await SuggestionService.generate(gapId);
      setSuggestions(prev => ({ ...prev, [gapId]: s }));
    } catch (e: any) {
      console.error('Failed to generate suggestion', e);
    } finally {
      setLoadingSuggestions(prev => ({ ...prev, [gapId]: false }));
    }
  };

  const handleAccept = async (suggestionId: string, gapId: string) => {
    try {
      await SuggestionService.accept(suggestionId);
      setSuggestionStatus(prev => ({ ...prev, [gapId]: 'Accepted' }));
    } catch (e: any) {
      console.error('Failed to accept suggestion', e);
    }
  };

  const handleReject = async (suggestionId: string, gapId: string) => {
    try {
      await SuggestionService.reject(suggestionId);
      setSuggestionStatus(prev => ({ ...prev, [gapId]: 'Rejected' }));
    } catch (e: any) {
      console.error('Failed to reject suggestion', e);
    }
  };

  const handleDownload = (
    gap: DocumentationGap,
    suggestion: AISuggestion,
    format: 'txt' | 'md'
  ) => {
    const cleanContent = suggestion.generatedDocumentation
      .replace(/#{1,6}\s+/g, '')
      .replace(/\*\*(.+?)\*\*/g, '$1')
      .replace(/\*(.+?)\*/g, '$1')
      .replace(/`{3}[\s\S]*?`{3}/g, (m) => m.replace(/`{3}\w*\n?/, '').replace(/`{3}/, ''))
      .replace(/`(.+?)`/g, '$1')
      .trim();

    const content = format === 'md'
      ? `# Documentation Fix — ${gap.elementName}\n\n${suggestion.generatedDocumentation}`
      : `DOCUMENTATION FIX — ${gap.elementName}\n${'='.repeat(50)}\n\n${cleanContent}\n`;

    const blob = new Blob([content], { type: 'text/plain' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `${gap.elementName}-fix.${format}`;
    a.click();
    URL.revokeObjectURL(url);
  };

  const formatSuggestionDisplay = (text: string): React.ReactNode => {
    const lines = text.split('\n');

    return (
      <>
        {lines.map((line, i) => {

          // XML doc comments (///) or block comments (*) → green
          if (/^\s*(\/\/\/|\*)/.test(line)) {
            return (
              <Typography key={i} sx={{
                fontSize: '0.75rem',
                color: '#6A9955',
                fontFamily: '"Fira Code","JetBrains Mono",monospace',
                lineHeight: 1.7,
                whiteSpace: 'pre',
              }}>
                {line}
              </Typography>
            );
          }

          // Python docstrings (""" or ''') → green
          if (/^\s*("""|''')/.test(line) || /^("""|''')/.test(line.trim())) {
            return (
              <Typography key={i} sx={{
                fontSize: '0.75rem',
                color: '#6A9955',
                fontFamily: '"Fira Code","JetBrains Mono",monospace',
                lineHeight: 1.7,
                whiteSpace: 'pre',
              }}>
                {line}
              </Typography>
            );
          }

          // Docstring content lines (indented lines between """) → green
          if (/^\s{4,}(Args|Returns|Raises|Example|Note|Summary)/.test(line)
            || /^\s{8,}\w/.test(line)) {
            return (
              <Typography key={i} sx={{
                fontSize: '0.75rem',
                color: '#6A9955',
                fontFamily: '"Fira Code","JetBrains Mono",monospace',
                lineHeight: 1.7,
                whiteSpace: 'pre',
              }}>
                {line}
              </Typography>
            );
          }

          // Keywords → blue
          if (/^\s*(public|private|protected|async|def |class |function |const |let |var |return|import|export|from)/.test(line)) {
            return (
              <Typography key={i} sx={{
                fontSize: '0.75rem',
                color: '#569CD6',
                fontFamily: '"Fira Code","JetBrains Mono",monospace',
                lineHeight: 1.7,
                whiteSpace: 'pre',
              }}>
                {line}
              </Typography>
            );
          }

          // Control flow → purple  
          if (/^\s*(if |else|for |while |try|catch|raise|throw|switch)/.test(line)) {
            return (
              <Typography key={i} sx={{
                fontSize: '0.75rem',
                color: '#C586C0',
                fontFamily: '"Fira Code","JetBrains Mono",monospace',
                lineHeight: 1.7,
                whiteSpace: 'pre',
              }}>
                {line}
              </Typography>
            );
          }

          // Empty lines
          if (line.trim() === '') {
            return <Box key={i} sx={{ height: 4 }} />;
          }

          // Everything else → white
          return (
            <Typography key={i} sx={{
              fontSize: '0.75rem',
              color: '#D4D4D4',
              fontFamily: '"Fira Code","JetBrains Mono",monospace',
              lineHeight: 1.7,
              whiteSpace: 'pre',
            }}>
              {line}
            </Typography>
          );
        })}
      </>
    );
  };

  if (polling) return (
    <Box sx={{
      display: 'flex', flexDirection: 'column',
      alignItems: 'center', justifyContent: 'center',
      minHeight: '60vh', gap: 3,
    }}>
      <Box sx={{
        width: 80, height: 80,
        borderRadius: '50%',
        border: '3px solid #2A2A2A',
        borderTop: '3px solid #E50914',
        animation: 'spin 1s linear infinite',
        '@keyframes spin': { '0%': { transform: 'rotate(0deg)' }, '100%': { transform: 'rotate(360deg)' } },
      }} />
      <Typography variant="h6" color="white">Analyzing repository...</Typography>
      <Typography color="#A3A3A3" fontSize={14}>
        Scanning files and detecting documentation gaps
      </Typography>
    </Box>
  );

  if (error) return (
    <Box sx={{ p: 4 }}>
      <Button startIcon={<ArrowLeft size={16} />} onClick={() => navigate('/repositories')} sx={{ mb: 3 }}>
        Back to Repositories
      </Button>
      <Alert severity="error">{error}</Alert>
    </Box>
  );

  if (!result) return null;

  return (
    <Box>
      <Button startIcon={<ArrowLeft size={16} />} onClick={() => navigate('/repositories')}
        sx={{ mb: 3, textTransform: 'none' }}>
        Back to Repositories
      </Button>

      <Typography variant="h5" fontWeight={700} mb={3}>Analysis Results</Typography>

      <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr 1fr' }, gap: 3, mb: 4 }}>
        <StatCard title="Documentation Coverage" value={`${result.overallCoverage}%`} accent="#46D369" />
        <StatCard title="Total Gaps" value={result.totalGaps} accent="#0080FF" />
        <StatCard title="Critical Gaps" value={result.criticalGaps} accent="#E50914" />
        <StatCard title="Files Analyzed" value={result.analyzedFiles} accent="#F5A623" />
      </Box>

      <Paper elevation={0}>
        <Table>
          <TableHead>
            <TableRow>
              <TableCell>Severity</TableCell>
              <TableCell>Element</TableCell>
              <TableCell>Issue</TableCell>
              <TableCell>File</TableCell>
              <TableCell align="right">Documentation Fix</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {result.gaps.map(gap => {
              const suggestion = suggestions[gap.id];
              return (
                <TableRow key={gap.id}>
                  <TableCell>
                    <SeverityChip severity={gap.severity} />
                  </TableCell>
                  <TableCell>
                    <Typography fontWeight={600} fontSize={14}>{gap.elementName}</Typography>
                    <Typography fontSize={12} color="text.secondary">{gap.elementType}</Typography>
                  </TableCell>
                  <TableCell>
                    <Typography fontSize={14}>{gap.message}</Typography>
                  </TableCell>
                  <TableCell>
                    <Link to={`/file-viewer?path=${encodeURIComponent(gap.filePath)}&line=${gap.lineNumber}&gapId=${gap.id}`}
                      style={{ textDecoration: 'none' }}>
                      <Stack direction="row" alignItems="center" gap={0.5}>
                        <Typography fontSize={13} color="primary" sx={{ '&:hover': { textDecoration: 'underline' } }}>
                          {gap.filePath.split('/').pop()}:{gap.lineNumber}
                        </Typography>
                        <ExternalLink size={12} color="#E50914" />
                      </Stack>
                    </Link>
                  </TableCell>
                  <TableCell align="right">
                    {loadingSuggestions[gap.id] ? (
                      <Button variant="outlined" size="small" disabled>
                        <CircularProgress size={16} color="inherit" />
                      </Button>
                    ) : suggestion ? (
                      <Box sx={{
                        mt: 1,
                        borderRadius: 2,
                        overflow: 'hidden',
                        border: '1px solid rgba(255,255,255,0.08)',
                        bgcolor: '#0D0D0D',
                      }}>

                        {/* Header bar */}
                        <Box sx={{
                          px: 2, py: 1,
                          display: 'flex',
                          alignItems: 'center',
                          justifyContent: 'space-between',
                          bgcolor: '#1A1A1A',
                          borderBottom: '1px solid rgba(255,255,255,0.06)',
                        }}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                            <Box sx={{
                              width: 8, height: 8,
                              borderRadius: '50%',
                              bgcolor: '#60A5FA'
                            }} />
                            <Typography sx={{
                              fontSize: 11, fontWeight: 600, color: '#60A5FA',
                              letterSpacing: '0.08em', textTransform: 'uppercase'
                            }}>
                              AI Suggested Fix
                            </Typography>
                          </Box>
                          <Box sx={{ display: 'flex', gap: 1 }}>
                            {suggestion.needsHumanReview && (
                              <Chip label="Needs Review" size="small" sx={{
                                height: 20, fontSize: 10, fontWeight: 600,
                                bgcolor: 'rgba(245,166,35,0.15)', color: '#F5A623',
                                border: '1px solid rgba(245,166,35,0.3)',
                              }} />
                            )}
                            <Chip
                              label={`${Math.round(suggestion.confidenceScore * 100)}% confidence`}
                              size="small" sx={{
                                height: 20, fontSize: 10, fontWeight: 600,
                                bgcolor: suggestion.confidenceScore >= 0.7
                                  ? 'rgba(70,211,105,0.1)' : 'rgba(245,166,35,0.1)',
                                color: suggestion.confidenceScore >= 0.7
                                  ? '#46D369' : '#F5A623',
                                border: `1px solid ${suggestion.confidenceScore >= 0.7
                                  ? 'rgba(70,211,105,0.3)' : 'rgba(245,166,35,0.3)'}`,
                              }}
                            />
                          </Box>
                        </Box>

                        {/* Code content — clean, no markdown */}
                        <Box sx={{
                          px: 2.5, py: 2,
                          fontSize: '0.75rem',
                          lineHeight: 1.8,
                          maxHeight: 300,
                          overflowY: 'auto',
                          '&::-webkit-scrollbar': { width: 4 },
                          '&::-webkit-scrollbar-thumb': {
                            bgcolor: 'rgba(255,255,255,0.1)', borderRadius: 2
                          },
                        }}>
                          {formatSuggestionDisplay(suggestion.generatedDocumentation)}
                        </Box>

                        {/* Actions + Download */}
                        {suggestionStatus[gap.id] ? (
                          <Box sx={{
                            px: 2, py: 1.5,
                            borderTop: '1px solid rgba(255,255,255,0.06)',
                            display: 'flex', alignItems: 'center', gap: 1,
                          }}>
                            <Box sx={{
                              width: 8, height: 8, borderRadius: '50%',
                              bgcolor: suggestionStatus[gap.id] === 'Accepted'
                                ? '#46D369' : '#E50914'
                            }} />
                            <Typography sx={{
                              fontSize: 12, fontWeight: 600,
                              color: suggestionStatus[gap.id] === 'Accepted'
                                ? '#46D369' : '#E50914',
                            }}>
                              {suggestionStatus[gap.id]}
                            </Typography>
                          </Box>
                        ) : (
                          <Box sx={{
                            px: 2, py: 1.5,
                            borderTop: '1px solid rgba(255,255,255,0.06)',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'space-between',
                          }}>
                            {/* Accept / Reject */}
                            <Box sx={{ display: 'flex', gap: 1 }}>
                              <Button size="small"
                                onClick={() => {
                                  navigator.clipboard.writeText(
                                    suggestion.generatedDocumentation
                                  );
                                }}
                                sx={{
                                  textTransform: 'none',
                                  fontSize: 12,
                                  fontWeight: 700,
                                  px: 2,
                                  py: 0.5,
                                  borderRadius: 1.5,
                                  bgcolor: 'rgba(37,99,235,0.15)',
                                  color: '#60A5FA',
                                  border: '1px solid rgba(37,99,235,0.4)',
                                  '&:hover': {
                                    bgcolor: 'rgba(37,99,235,0.25)',
                                    color: '#93C5FD'
                                  }
                                }}>
                                ⎘ Copy Fix
                              </Button>
                              <Button size="small"
                                onClick={() => handleAccept(suggestion.id, gap.id)}
                                sx={{
                                  textTransform: 'none', fontSize: 12, fontWeight: 600,
                                  px: 2, py: 0.5, borderRadius: 1.5,
                                  bgcolor: 'rgba(70,211,105,0.1)', color: '#46D369',
                                  border: '1px solid rgba(70,211,105,0.3)',
                                  '&:hover': { bgcolor: 'rgba(70,211,105,0.2)' }
                                }}>
                                ✓ Accept
                              </Button>
                              <Button size="small"
                                onClick={() => handleReject(suggestion.id, gap.id)}
                                sx={{
                                  textTransform: 'none', fontSize: 12, fontWeight: 600,
                                  px: 2, py: 0.5, borderRadius: 1.5,
                                  bgcolor: 'rgba(229,9,20,0.1)', color: '#E50914',
                                  border: '1px solid rgba(229,9,20,0.3)',
                                  '&:hover': { bgcolor: 'rgba(229,9,20,0.2)' }
                                }}>
                                ✕ Reject
                              </Button>
                            </Box>

                            {/* Download */}
                            <Box sx={{ display: 'flex', gap: 1 }}>
                              <Button size="small"
                                onClick={() => handleDownload(gap, suggestion, 'txt')}
                                sx={{
                                  textTransform: 'none', fontSize: 11, fontWeight: 600,
                                  px: 1.5, py: 0.5, borderRadius: 1.5,
                                  bgcolor: 'rgba(255,255,255,0.05)', color: '#A3A3A3',
                                  border: '1px solid rgba(255,255,255,0.1)',
                                  '&:hover': { bgcolor: 'rgba(255,255,255,0.1)', color: '#fff' }
                                }}>
                                ↓ TXT
                              </Button>
                              <Button size="small"
                                onClick={() => handleDownload(gap, suggestion, 'md')}
                                sx={{
                                  textTransform: 'none', fontSize: 11, fontWeight: 600,
                                  px: 1.5, py: 0.5, borderRadius: 1.5,
                                  bgcolor: 'rgba(255,255,255,0.05)', color: '#A3A3A3',
                                  border: '1px solid rgba(255,255,255,0.1)',
                                  '&:hover': { bgcolor: 'rgba(255,255,255,0.1)', color: '#fff' }
                                }}>
                                ↓ MD
                              </Button>
                            </Box>
                          </Box>
                        )}
                      </Box>
                    ) : (
                      <Button variant="outlined" size="small" startIcon={<Sparkles size={14} />}
                        onClick={() => handleSuggest(gap.id)}>
                        Suggest Fix
                      </Button>
                    )}
                  </TableCell>
                </TableRow>
              );
            })}
          </TableBody>
        </Table>
      </Paper>
    </Box>
  );
}

function StatCard({ title, value, accent }: { title: string, value: string | number, accent: string }) {
  return (
    <Paper elevation={0} sx={{
      p: 3,
      bgcolor: '#1F1F1F',
      border: '1px solid #2A2A2A',
      borderRadius: 2,
      position: 'relative',
      overflow: 'hidden',
      '&::before': {
        content: '""',
        position: 'absolute',
        top: 0, left: 0, right: 0,
        height: '3px',
        bgcolor: accent,
      }
    }}>
      <Typography color="text.secondary" fontSize={13} fontWeight={500} mb={0.5}>{title}</Typography>
      <Typography variant="h5" fontWeight={700}>{value}</Typography>
    </Paper>
  );
}

function SeverityChip({ severity }: { severity: string }) {
  const getSeverityStyle = (severity: string) => {
    switch (severity.toLowerCase()) {
      case 'critical': return { bgcolor: 'rgba(229,9,20,0.15)', color: '#FF4444', border: '1px solid rgba(229,9,20,0.3)' };
      case 'high': return { bgcolor: 'rgba(245,166,35,0.15)', color: '#F5A623', border: '1px solid rgba(245,166,35,0.3)' };
      case 'medium': return { bgcolor: 'rgba(0,128,255,0.15)', color: '#4DA6FF', border: '1px solid rgba(0,128,255,0.3)' };
      case 'low': return { bgcolor: 'rgba(70,211,105,0.15)', color: '#46D369', border: '1px solid rgba(70,211,105,0.3)' };
      default: return { bgcolor: '#2A2A2A', color: '#A3A3A3', border: '1px solid #3A3A3A' };
    }
  };

  return (
    <Chip label={severity} size="small" sx={getSeverityStyle(severity)} />
  );
}
