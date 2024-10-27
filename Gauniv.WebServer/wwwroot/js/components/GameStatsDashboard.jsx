import React, { useState, useEffect } from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend } from 'recharts';
import { Activity, Users, GameController, Trophy } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';

const StatCard = ({ title, value, icon: Icon, description }) => (
    <Card className="bg-card">
        <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">
                {title}
            </CardTitle>
            <Icon className="h-4 w-4 text-muted-foreground" />
        </CardHeader>
        <CardContent>
            <div className="text-2xl font-bold">{value}</div>
            <p className="text-xs text-muted-foreground">{description}</p>
        </CardContent>
    </Card>
);

const GameStatsDashboard = () => {
    const [stats, setStats] = useState(null);

    useEffect(() => {
        fetch('/Stats/Index')
            .then(res => res.json())
            .then(data => setStats(data));
    }, []);

    if (!stats) return <div className="p-4">Loading statistics...</div>;

    const categoryData = Object.entries(stats.gamesByCategory).map(([category, count]) => ({
        name: category,
        count: count
    }));

    return (
        <div className="p-4 space-y-4">
            <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
                <StatCard
                    title="Total Games"
                    value={stats.totalGames}
                    icon={GameController}
                    description="Games in platform"
                />
                <StatCard
                    title="Most Popular Game"
                    value={stats.topGame?.title || 'N/A'}
                    icon={Trophy}
                    description={`${stats.topGame?.playerCount || 0} players`}
                />
                <StatCard
                    title="Top Player"
                    value={stats.topPlayer?.userName || 'N/A'}
                    icon={Users}
                    description={`${stats.topPlayer?.gameCount || 0} games owned`}
                />
                <StatCard
                    title="Categories"
                    value={Object.keys(stats.gamesByCategory).length}
                    icon={Activity}
                    description="Available categories"
                />
            </div>

            <Card>
                <CardHeader>
                    <CardTitle>Games by Category</CardTitle>
                </CardHeader>
                <CardContent>
                    <div className="h-[300px] w-full">
                        <BarChart
                            width={800}
                            height={300}
                            data={categoryData}
                            margin={{ top: 5, right: 30, left: 20, bottom: 5 }}
                        >
                            <CartesianGrid strokeDasharray="3 3" />
                            <XAxis dataKey="name" />
                            <YAxis />
                            <Tooltip />
                            <Legend />
                            <Bar dataKey="count" fill="#1a9fff" name="Number of Games" />
                        </BarChart>
                    </div>
                </CardContent>
            </Card>
        </div>
    );
};

export default GameStatsDashboard;