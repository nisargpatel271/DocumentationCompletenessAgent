import { useState, useEffect } from 'react';
import {
    Box, Typography, Paper, Stack, CircularProgress,
    Table, TableHead, TableRow, TableCell, TableBody, Chip, Button
} from '@mui/material';
import {
    BarChart, Bar, XAxis, YAxis, Tooltip as ReTooltip,
    ResponsiveContainer, Cell, LineChart, Line, Legend
} from 'recharts';
import { Shield, AlertTriangle, Database, Activity, Download } from 'lucide-react';
import { DashboardService, type DashboardSummary, type DashboardTrends } from '../services/DashboardService';
import { jsPDF } from 'jspdf';

export default function DashboardPage() {
    const [summary, setSummary] = useState<DashboardSummary | null>(null);
    const [trends, setTrends] = useState<DashboardTrends | null>(null);
    const [trendView, setTrendView] = useState<'overall' | 'byRepo'>('overall');
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const load = async () => {
            try {
                const [summaryData, trendsData] = await Promise.all([
                    DashboardService.getSummary(),
                    DashboardService.getTrends(30)
                ]);
                setSummary(summaryData);
                setTrends(trendsData);
            } catch (e) {
                console.error('Failed to load dashboard', e);
            } finally {
                setLoading(false);
            }
        };
        load();
    }, []);

    const handleExportPDF = () => {
        if (!summary) return;

        const doc = new jsPDF();
        const now = new Date().toLocaleDateString();

        // Title
        doc.setFontSize(22);
        doc.setTextColor(20, 20, 20);
        doc.text('DocAgent — Documentation Health Report', 20, 25);

        // Date
        doc.setFontSize(11);
        doc.setTextColor(100, 100, 100);
        doc.text(`Generated: ${now}`, 20, 35);

        // Summary stats
        doc.setFontSize(16);
        doc.setTextColor(40, 40, 40);
        doc.text('Summary Metrics', 20, 50);

        doc.setFontSize(11);
        doc.setTextColor(60, 60, 60);
        doc.text(`Overall Coverage: ${summary.overallCoverage}%`, 25, 60);
        doc.text(`Total Deficit Gaps: ${summary.totalGaps}`, 25, 68);
        doc.text(`Critical Vulnerabilities: ${summary.criticalGaps}`, 25, 76);
        doc.text(`Active Repositories: ${summary.totalRepositories}`, 25, 84);

        // Repository breakdown
        doc.setFontSize(16);
        doc.setTextColor(40, 40, 40);
        doc.text('Lowest Coverage Repository Breakdown', 20, 105);

        let y = 115;
        summary.topGapsRepos.forEach((repo) => {
            if (y > 270) {
                doc.addPage();
                y = 20;
            }
            doc.setFontSize(11);
            doc.setTextColor(50, 50, 50);
            doc.text(`${repo.name}: ${repo.coverage}%`, 25, y);
            doc.setTextColor(120, 120, 120);
            doc.text(`(${repo.totalGaps} identified gaps)`, 110, y);
            y += 10;
        });

        // Save
        const fileName = `docagent-report-${now.replace(/\//g, '-')}.pdf`;
        doc.save(fileName);
    };

    if (loading) return (
        <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', height: '60vh' }}>
            <CircularProgress color="primary" />
        </Box>
    );

    if (!summary) return null;

    return (
        <Box>
            <Stack direction="row" justifyContent="space-between" alignItems="center" mb={4}>
                <Typography variant="h5" fontWeight={700}>Documentation Health Dashboard</Typography>
                <Button
                    variant="outlined"
                    size="small"
                    onClick={handleExportPDF}
                    startIcon={<Download size={16} />}
                    sx={{
                        textTransform: 'none',
                        borderColor: 'rgba(255,255,255,0.15)',
                        color: '#A3A3A3',
                        '&:hover': { borderColor: '#fff', color: '#fff', bgcolor: 'rgba(255,255,255,0.05)' }
                    }}
                >
                    Export PDF
                </Button>
            </Stack>

            {/* Top Stats - 4 column grid */}
            <Box sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr 1fr' },
                gap: 3, mb: 4
            }}>
                <StatCard
                    title="Overall Coverage"
                    value={`${summary.overallCoverage}%`}
                    icon={<Shield size={24} color="#46D369" />}
                    accent="#46D369"
                />
                <StatCard
                    title="Critical Gaps"
                    value={summary.criticalGaps}
                    icon={<AlertTriangle size={24} color="#E50914" />}
                    accent="#E50914"
                />
                <StatCard
                    title="Total Gaps"
                    value={summary.totalGaps}
                    icon={<Database size={24} color="#0080FF" />}
                    accent="#0080FF"
                />
                <StatCard
                    title="Repositories"
                    value={summary.totalRepositories}
                    icon={<Activity size={24} color="#F5A623" />}
                    accent="#F5A623"
                />
            </Box>

            {/* Charts Row - 7:5 ratio grid */}
            <Box sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', md: '1.4fr 1fr' },
                gap: 3, mb: 4
            }}>
                {/* Coverage Chart */}
                <Paper sx={{ p: 3, height: '400px', display: 'flex', flexDirection: 'column' }}>
                    <Typography variant="h6" mb={3} sx={{ fontSize: 16, fontWeight: 600 }}>Repository Coverage</Typography>
                    <ResponsiveContainer width="100%" height="100%">
                        <BarChart data={summary.topGapsRepos}>
                            <XAxis dataKey="name" fontSize={11} tick={{ fill: '#A3A3A3' }} axisLine={false} tickLine={false} />
                            <YAxis fontSize={11} tick={{ fill: '#A3A3A3' }} axisLine={false} tickLine={false} unit="%" />
                            <ReTooltip
                                contentStyle={{ backgroundColor: '#1F1F1F', border: '1px solid #2A2A2A', borderRadius: 8 }}
                                itemStyle={{ color: '#E50914' }}
                            />
                            <Bar dataKey="coverage" radius={[4, 4, 0, 0]} barSize={40}>
                                {summary.topGapsRepos.map((entry, index) => (
                                    <Cell key={`cell-${index}`} fill={entry.coverage > 70 ? '#46D369' : entry.coverage > 40 ? '#F5A623' : '#E50914'} />
                                ))}
                            </Bar>
                        </BarChart>
                    </ResponsiveContainer>
                </Paper>

                {/* Top Critical Repos */}
                <Paper sx={{ p: 3, height: '400px', overflow: 'auto' }}>
                    <Typography variant="h6" mb={2} sx={{ fontSize: 16, fontWeight: 600 }}>Lowest Coverage Metrics</Typography>
                    <Stack spacing={2}>
                        {summary.topGapsRepos.map((repo) => (
                            <Box key={repo.id} sx={{ p: 2, bgcolor: '#141414', borderRadius: 2, border: '1px solid #2A2A2A' }}>
                                <Stack direction="row" justifyContent="space-between" alignItems="center">
                                    <Box>
                                        <Typography sx={{ fontWeight: 600, fontSize: 14 }}>{repo.name}</Typography>
                                        <Typography sx={{ fontSize: 12, color: 'text.secondary' }}>{repo.totalGaps} total gaps identified</Typography>
                                    </Box>
                                    <Chip
                                        label={`${repo.coverage}%`}
                                        size="small"
                                        sx={{
                                            bgcolor: repo.coverage > 70 ? 'rgba(70,211,105,0.1)' : 'rgba(229,9,20,0.1)',
                                            color: repo.coverage > 70 ? '#46D369' : '#E50914',
                                            fontWeight: 700
                                        }}
                                    />
                                </Stack>
                            </Box>
                        ))}
                    </Stack>
                </Paper>
            </Box>

            {/* Trends Section */}
            <Paper elevation={0} sx={{
                p: 3,
                bgcolor: '#1F1F1F',
                border: '1px solid #2A2A2A',
                borderRadius: 2,
                mb: 4
            }}>
                <Stack direction="row" justifyContent="space-between" alignItems="center" mb={4}>
                    <Typography variant="h6" sx={{ fontSize: 16, fontWeight: 600 }}>
                        Coverage Trends — Last 30 Days
                    </Typography>
                    <Box sx={{
                        display: 'flex',
                        bgcolor: '#141414',
                        borderRadius: 2,
                        p: 0.5,
                        border: '1px solid #2A2A2A'
                    }}>
                        <Button
                            size="small"
                            onClick={() => setTrendView('overall')}
                            sx={{
                                textTransform: 'none', px: 2, fontSize: 12, borderRadius: 1.5,
                                bgcolor: trendView === 'overall' ? '#2A2A2A' : 'transparent',
                                color: trendView === 'overall' ? '#fff' : '#A3A3A3',
                                '&:hover': { bgcolor: trendView === 'overall' ? '#2A2A2A' : '#1F1F1F' }
                            }}
                        >
                            Overall
                        </Button>
                        <Button
                            size="small"
                            onClick={() => setTrendView('byRepo')}
                            sx={{
                                textTransform: 'none', px: 2, fontSize: 12, borderRadius: 1.5,
                                bgcolor: trendView === 'byRepo' ? '#2A2A2A' : 'transparent',
                                color: trendView === 'byRepo' ? '#fff' : '#A3A3A3',
                                '&:hover': { bgcolor: trendView === 'byRepo' ? '#2A2A2A' : '#1F1F1F' }
                            }}
                        >
                            Per Repository
                        </Button>
                    </Box>
                </Stack>

                <Box sx={{ height: 350, width: '100%' }}>
                    {trends && (trendView === 'overall' ? trends.overall.length > 0 : trends.byRepository.length > 0) ? (
                        <ResponsiveContainer width="100%" height="100%">
                            {trendView === 'overall' ? (
                                <LineChart data={trends.overall}>
                                    <XAxis dataKey="date" fontSize={11} tick={{ fill: '#A3A3A3' }} axisLine={false} tickLine={false} />
                                    <YAxis domain={[0, 100]} fontSize={11} tick={{ fill: '#A3A3A3' }} axisLine={false} tickLine={false} unit="%" />
                                    <ReTooltip
                                        contentStyle={{ backgroundColor: '#1F1F1F', border: '1px solid #2A2A2A', borderRadius: 8 }}
                                        labelStyle={{ color: '#fff', marginBottom: 4 }}
                                    />
                                    <Line
                                        type="monotone"
                                        dataKey="coverage"
                                        stroke="#46D369"
                                        strokeWidth={3}
                                        dot={{ fill: '#46D369', strokeWidth: 0, r: 4 }}
                                        activeDot={{ r: 6, stroke: 'rgba(70,211,105,0.4)', strokeWidth: 8 }}
                                    />
                                </LineChart>
                            ) : (
                                <LineChart>
                                    <XAxis
                                        allowDuplicatedCategory={false}
                                        dataKey="date"
                                        fontSize={11} tick={{ fill: '#A3A3A3' }} axisLine={false} tickLine={false}
                                    />
                                    <YAxis domain={[0, 100]} fontSize={11} tick={{ fill: '#A3A3A3' }} axisLine={false} tickLine={false} unit="%" />
                                    <ReTooltip
                                        contentStyle={{ backgroundColor: '#1F1F1F', border: '1px solid #2A2A2A', borderRadius: 8 }}
                                        labelStyle={{ color: '#fff', marginBottom: 4 }}
                                    />
                                    <Legend iconType="circle" wrapperStyle={{ paddingTop: 20 }} />
                                    {trends.byRepository.map((repo, idx) => (
                                        <Line
                                            key={repo.repositoryId}
                                            data={repo.data}
                                            name={repo.repositoryName}
                                            type="monotone"
                                            dataKey="coverage"
                                            stroke={['#E50914', '#46D369', '#0080FF', '#F5A623', '#A855F7', '#EC4899'][idx % 6]}
                                            strokeWidth={2}
                                            dot={false}
                                        />
                                    ))}
                                </LineChart>
                            )}
                        </ResponsiveContainer>
                    ) : (
                        <Box sx={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '100%', color: '#666' }}>
                            <Typography fontSize={14}>Run more analyses to see trends</Typography>
                        </Box>
                    )}
                </Box>
            </Paper>

            {/* Recent Analysis Activity Table */}
            <Paper sx={{ p: 3 }}>
                <Typography variant="h6" mb={3} sx={{ fontSize: 16, fontWeight: 600 }}>Recent Analysis Activity</Typography>
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>Repository</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell>Started At</TableCell>
                            <TableCell>Completed At</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {summary.recentJobs.map((job) => (
                            <TableRow key={job.id}>
                                <TableCell sx={{ fontWeight: 600 }}>{job.repoName}</TableCell>
                                <TableCell>
                                    <Chip
                                        label={job.status}
                                        size="small"
                                        sx={{
                                            bgcolor: job.status === 'Completed' ? 'rgba(70,211,105,0.1)' : 'rgba(245,166,35,0.1)',
                                            color: job.status === 'Completed' ? '#46D369' : '#F5A623'
                                        }}
                                    />
                                </TableCell>
                                <TableCell sx={{ fontSize: 13, color: 'text.secondary' }}>
                                    {new Date(job.createdAt).toLocaleString()}
                                </TableCell>
                                <TableCell sx={{ fontSize: 13, color: 'text.secondary' }}>
                                    {job.completedAt ? new Date(job.completedAt).toLocaleString() : '-'}
                                </TableCell>
                            </TableRow>
                        ))}
                    </TableBody>
                </Table>
            </Paper>
        </Box>
    );
}

function StatCard({ title, value, icon, accent }: { title: string, value: string | number, icon: React.ReactNode, accent: string }) {
    return (
        <Paper elevation={0} sx={{
            p: 3, position: 'relative', overflow: 'hidden',
            '&::before': { content: '""', position: 'absolute', top: 0, left: 0, width: '4px', height: '100%', bgcolor: accent }
        }}>
            <Stack direction="row" justifyContent="space-between" alignItems="flex-start">
                <Box>
                    <Typography sx={{ color: 'text.secondary', fontSize: 13, fontWeight: 500, mb: 1 }}>{title}</Typography>
                    <Typography variant="h4" sx={{ fontWeight: 800 }}>{value}</Typography>
                </Box>
                <Box sx={{ mt: 0.5 }}>{icon}</Box>
            </Stack>
        </Paper>
    );
}
