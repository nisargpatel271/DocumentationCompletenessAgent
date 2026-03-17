import React, { useState, useEffect, useRef } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import {
  Box, Typography, Button, IconButton, CircularProgress,
  Alert, Stack, Chip
} from '@mui/material';
import { ArrowLeft, Sparkles } from 'lucide-react';
import { api } from '../services/api';
import { SuggestionService } from '../services/SuggestionService';
import type { AISuggestion, DocumentationGap } from '../types';

export default function FileViewerPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const filePath = searchParams.get('path');
  const line = parseInt(searchParams.get('line') || '0');
  const gapId = searchParams.get('gapId');

  const [content, setContent] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [gap, setGap] = useState<DocumentationGap | null>(null);
  const [suggestion, setSuggestion] = useState<AISuggestion | null>(null);
  const [loadingSuggestion, setLoadingSuggestion] = useState(false);
  const [suggestionStatus, setSuggestionStatus] = useState<string | null>(null);

  const targetLineRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const loadContent = async () => {
      if (!filePath) return;
      try {
        setLoading(true);
        const res = await api.get(`/files/content?path=${encodeURIComponent(filePath)}`);
        setContent(res.data.content);
      } catch (e: any) {
        setError('Failed to load file content');
      } finally {
        setLoading(false);
      }
    };
    loadContent();
  }, [filePath]);

  useEffect(() => {
    const loadGap = async () => {
      if (!gapId) return;
      try {
        const res = await api.get(`/documentation/gaps/${gapId}`);
        setGap(res.data);
      } catch (e) {
        console.error('Failed to load gap details', e);
      }
    };
    loadGap();
  }, [gapId]);

  useEffect(() => {
    if (content && targetLineRef.current) {
      targetLineRef.current.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }
  }, [content]);

  const handleSuggest = async () => {
    if (!gapId) return;
    try {
      setLoadingSuggestion(true);
      const s = await SuggestionService.generate(gapId);
      setSuggestion(s);
    } catch (e: any) {
      console.error('Failed to generate suggestion', e);
    } finally {
      setLoadingSuggestion(false);
    }
  };

  const handleAccept = async () => {
    if (!suggestion) return;
    try {
      await SuggestionService.accept(suggestion.id);
      setSuggestionStatus('Accepted');
    } catch (e: any) {
      console.error('Failed to accept', e);
    }
  };

  const handleReject = async () => {
    if (!suggestion) return;
    try {
      await SuggestionService.reject(suggestion.id);
      setSuggestionStatus('Rejected');
    } catch (e: any) {
      console.error('Failed to reject', e);
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

  if (loading) return (
    <Box sx={{ display: 'flex', justifyContent: 'center', py: 10 }}>
      <CircularProgress color="primary" />
    </Box>
  );

  return (
    <Box sx={{ height: '100%', display: 'flex', flexDirection: 'column' }}>
      <Stack direction="row" alignItems="center" gap={2} mb={3}>
        <IconButton onClick={() => navigate(-1)}><ArrowLeft size={20} /></IconButton>
        <Typography variant="h6" sx={{ fontFamily: 'monospace', fontSize: 16, color: 'white' }}>
          {filePath?.split('/').pop()}
        </Typography>
        <Box sx={{ flex: 1 }} />
        {gapId && !suggestion && !suggestionStatus && (
          <Button variant="contained" startIcon={loadingSuggestion ? null : <Sparkles size={16} />}
            disabled={loadingSuggestion}
            onClick={handleSuggest}>
            {loadingSuggestion ? <CircularProgress size={16} color="inherit" /> : 'Generate AI Fix'}
          </Button>
        )}
      </Stack>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {suggestion && gap && (
        <Box sx={{
          mb: 4,
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
          {suggestionStatus ? (
            <Box sx={{
              px: 2, py: 1.5,
              borderTop: '1px solid rgba(255,255,255,0.06)',
              display: 'flex', alignItems: 'center', gap: 1,
            }}>
              <Box sx={{
                width: 8, height: 8, borderRadius: '50%',
                bgcolor: suggestionStatus === 'Accepted'
                  ? '#46D369' : '#E50914'
              }} />
              <Typography sx={{
                fontSize: 12, fontWeight: 600,
                color: suggestionStatus === 'Accepted'
                  ? '#46D369' : '#E50914',
              }}>
                {suggestionStatus}
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
                  onClick={handleAccept}
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
                  onClick={handleReject}
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
      )}

      <Box sx={{
        flex: 1, overflow: 'auto', bgcolor: '#0A0A0A', color: '#d4d4d4',
        borderRadius: 2, fontFamily: 'monospace', fontSize: 13, p: 2,
        lineHeight: 1.6, border: '1px solid #2A2A2A'
      }}>
        {content?.split('\n').map((lineText, idx) => {
          const lineNum = idx + 1;
          const isTarget = lineNum === line;
          return (
            <Box key={idx} ref={isTarget ? targetLineRef : null}
              sx={{
                display: 'flex',
                bgcolor: isTarget ? 'rgba(229,9,20,0.08)' : 'transparent',
                borderLeft: isTarget ? '3px solid #E50914' : '3px solid transparent'
              }}>
              <Typography sx={{ width: 40, color: '#4A4A4A', userSelect: 'none', textAlign: 'right', pr: 2 }}>
                {lineNum}
              </Typography>
              <Typography sx={{ whiteSpace: 'pre' }}>
                {lineText || ' '}
              </Typography>
            </Box>
          );
        })}
      </Box>
    </Box>
  );
}
