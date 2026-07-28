import {
  Bar,
  BarChart,
  CartesianGrid,
  ResponsiveContainer,
  Tooltip,
  XAxis,
  YAxis,
} from 'recharts'

interface ChartDatum {
  name: string
  value: number
}

interface SimpleBarChartProps {
  data: ChartDatum[]
  valueLabel?: string
  height?: number
  color?: string
}

function readCssColor(variable: string, fallback: string) {
  if (typeof window === 'undefined') return fallback
  const value = getComputedStyle(document.documentElement).getPropertyValue(variable).trim()
  return value || fallback
}

export function SimpleBarChart({
  data,
  valueLabel = 'Value',
  height = 240,
  color,
}: SimpleBarChartProps) {
  const barColor = color ?? readCssColor('--color-accent', '#dc382c')
  const gridColor = readCssColor('--color-border', '#e2e8f0')
  const tickColor = readCssColor('--color-ink-subtle', '#94a3b8')
  const surface = readCssColor('--color-surface', '#ffffff')
  const ink = readCssColor('--color-ink', '#0f172a')

  if (data.length === 0) {
    return <p className="py-10 text-center text-sm text-ink-subtle">No chart data yet.</p>
  }

  return (
    <div style={{ width: '100%', height }}>
      <ResponsiveContainer>
        <BarChart data={data} margin={{ top: 8, right: 8, left: -8, bottom: 0 }}>
          <CartesianGrid strokeDasharray="3 3" stroke={gridColor} vertical={false} />
          <XAxis
            dataKey="name"
            tick={{ fill: tickColor, fontSize: 12 }}
            tickLine={false}
            axisLine={{ stroke: gridColor }}
            interval={0}
            angle={data.length > 4 ? -20 : 0}
            textAnchor={data.length > 4 ? 'end' : 'middle'}
            height={data.length > 4 ? 50 : 30}
          />
          <YAxis
            allowDecimals={false}
            tick={{ fill: tickColor, fontSize: 12 }}
            tickLine={false}
            axisLine={false}
            width={36}
          />
          <Tooltip
            cursor={{ fill: 'rgb(148 163 184 / 0.12)' }}
            contentStyle={{
              borderRadius: 8,
              border: `1px solid ${gridColor}`,
              background: surface,
              color: ink,
              boxShadow: '0 4px 16px rgb(15 23 42 / 0.08)',
              fontSize: 13,
            }}
            formatter={(value) => [value as number, valueLabel]}
          />
          <Bar dataKey="value" fill={barColor} radius={[6, 6, 0, 0]} maxBarSize={48} />
        </BarChart>
      </ResponsiveContainer>
    </div>
  )
}
