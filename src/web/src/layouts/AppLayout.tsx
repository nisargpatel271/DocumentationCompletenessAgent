import { Outlet, NavLink } from 'react-router-dom';
import { Box, Typography, Stack } from '@mui/material';
import { BookOpen, Database, LayoutDashboard } from 'lucide-react';

export default function AppLayout() {
    return (
        <Box sx={{ display: 'flex', height: '100vh', bgcolor: '#141414' }}>

            {/* Sidebar */}
            <Box sx={{
                width: 240,
                flexShrink: 0,
                bgcolor: '#0A0A0A',
                borderRight: '1px solid #2A2A2A',
                display: 'flex',
                flexDirection: 'column',
            }}>

                {/* Logo */}
                <Box sx={{
                    px: 3, py: 3,
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1.5,
                    borderBottom: '1px solid #2A2A2A',
                }}>
                    <Box sx={{
                        width: 32, height: 32,
                        bgcolor: '#E50914',
                        borderRadius: 1,
                        display: 'flex',
                        alignItems: 'center',
                        justifyContent: 'center',
                    }}>
                        <BookOpen size={18} color="white" />
                    </Box>
                    <Typography sx={{ fontWeight: 800, fontSize: 18, color: '#FFFFFF', letterSpacing: '-0.02em' }}>
                        DocAgent
                    </Typography>
                </Box>

                {/* Nav items */}
                <Box sx={{ p: 1.5, mt: 0.5 }}>
                    <Stack spacing={0.5}>
                        <NavItem to="/dashboard" icon={<LayoutDashboard size={17} />} label="Dashboard" />
                        <NavItem to="/repositories" icon={<Database size={17} />} label="Repositories" />
                    </Stack>
                </Box>

                {/* Bottom version tag */}
                <Box sx={{ mt: 'auto', p: 3, borderTop: '1px solid #2A2A2A' }}>
                    <Typography sx={{ fontSize: 11, color: '#4A4A4A', letterSpacing: '0.05em' }}>
                        DOCAGENT v1.0
                    </Typography>
                </Box>
            </Box>

            {/* Main content */}
            <Box sx={{ flex: 1, overflow: 'auto', p: 4, bgcolor: '#141414' }}>
                <Outlet />
            </Box>
        </Box>
    );
}

function NavItem({ to, icon, label }: { to: string, icon: React.ReactNode, label: string }) {
    return (
        <NavLink to={to} style={{ textDecoration: 'none' }}>
            {({ isActive }) => (
                <Box sx={{
                    display: 'flex',
                    alignItems: 'center',
                    gap: 1.5,
                    px: 2, py: 1.25,
                    borderRadius: 1.5,
                    cursor: 'pointer',
                    bgcolor: isActive ? 'rgba(229,9,20,0.1)' : 'transparent',
                    color: isActive ? '#E50914' : '#A3A3A3',
                    borderLeft: isActive ? '3px solid #E50914' : '3px solid transparent',
                    transition: 'all 0.15s ease',
                    '&:hover': {
                        bgcolor: isActive ? 'rgba(229,9,20,0.1)' : 'rgba(255,255,255,0.05)',
                        color: isActive ? '#E50914' : '#FFFFFF',
                    },
                }}>
                    {icon}
                    <Typography sx={{
                        fontSize: 14,
                        fontWeight: isActive ? 700 : 400,
                        letterSpacing: '0.01em',
                    }}>
                        {label}
                    </Typography>
                </Box>
            )}
        </NavLink>
    );
}
