import { useEffect, useState } from 'react'
import { toast } from 'sonner'
import { Button } from '@/Components/UI/Button'
import { Badge } from '@/Components/UI/Badge'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/Components/UI/Table'
import { Skeleton } from '@/Components/UI/Skeleton'
import { logsApi } from '@/Services/Api'
import type { AppLog, StatusHistoryLog, OutboxLog } from '@/Types/Logs'
import { StatusBadge } from '@/Components/StatusBadge'
import type { OrderStatus } from '@/Types/Order'

type Tab = 'app' | 'status' | 'outbox'

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'medium' }).format(new Date(iso))
}

function LevelBadge({ level }: { level: AppLog['level'] }) {
  const variants: Record<AppLog['level'], string> = {
    Info: 'bg-blue-100 text-blue-800',
    Warning: 'bg-yellow-100 text-yellow-800',
    Error: 'bg-red-100 text-red-800',
  }
  return (
    <span className={`inline-flex items-center px-2 py-0.5 rounded text-xs font-medium ${variants[level]}`}>
      {level}
    </span>
  )
}

function AppLogsTab() {
  const [logs, setLogs] = useState<AppLog[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    logsApi.getAppLogs()
      .then(setLogs)
      .catch(() => toast.error('Erro ao carregar logs da aplicação.'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <LoadingSkeleton />

  return (
    <div className="rounded-md border w-full">
      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[8%]">Nível</TableHead>
            <TableHead className="w-[8%]">Fonte</TableHead>
            <TableHead className="w-[50%]">Mensagem</TableHead>
            <TableHead className="w-[12%]">Pedido</TableHead>
            <TableHead className="w-[22%]">Data</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {logs.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="text-center text-muted-foreground py-8">
                Nenhum log registrado.
              </TableCell>
            </TableRow>
          ) : logs.map(log => (
            <TableRow key={log.id}>
              <TableCell><LevelBadge level={log.level} /></TableCell>
              <TableCell>
                <Badge variant="outline" className="text-xs">{log.source}</Badge>
              </TableCell>
              <TableCell className="max-w-md">
                <p className="truncate">{log.message}</p>
                {log.details && <p className="text-xs text-muted-foreground truncate">{log.details}</p>}
              </TableCell>
              <TableCell className="font-mono text-xs text-muted-foreground">
                {log.orderId ? log.orderId.slice(0, 8) + '…' : '—'}
              </TableCell>
              <TableCell className="text-sm text-muted-foreground whitespace-nowrap">
                {formatDate(log.createdAt)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

function StatusHistoryTab() {
  const [history, setHistory] = useState<StatusHistoryLog[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    logsApi.getStatusHistory()
      .then(setHistory)
      .catch(() => toast.error('Erro ao carregar histórico de status.'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <LoadingSkeleton />

  return (
    <div className="rounded-md border w-full">
      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[20%]">Cliente</TableHead>
            <TableHead className="w-[18%]">De</TableHead>
            <TableHead className="w-[18%]">Para</TableHead>
            <TableHead className="w-[20%]">Pedido</TableHead>
            <TableHead className="w-[24%]">Data</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {history.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} className="text-center text-muted-foreground py-8">
                Nenhum histórico encontrado.
              </TableCell>
            </TableRow>
          ) : history.map(h => (
            <TableRow key={h.id}>
              <TableCell className="font-medium">{h.cliente}</TableCell>
              <TableCell>
                {h.fromStatus
                  ? <StatusBadge status={h.fromStatus as OrderStatus} />
                  : <span className="text-muted-foreground text-xs">—</span>}
              </TableCell>
              <TableCell><StatusBadge status={h.toStatus as OrderStatus} /></TableCell>
              <TableCell className="font-mono text-xs text-muted-foreground">
                {h.orderId.slice(0, 8)}…
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {formatDate(h.changedAt)}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

function OutboxTab() {
  const [messages, setMessages] = useState<OutboxLog[]>([])
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    logsApi.getOutbox()
      .then(setMessages)
      .catch(() => toast.error('Erro ao carregar mensagens outbox.'))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <LoadingSkeleton />

  return (
    <div className="rounded-md border w-full">
      <Table className="table-fixed">
        <TableHeader>
          <TableRow>
            <TableHead className="w-[14%]">Evento</TableHead>
            <TableHead className="w-[16%]">Cliente</TableHead>
            <TableHead className="w-[12%]">Status</TableHead>
            <TableHead className="w-[16%]">Pedido</TableHead>
            <TableHead className="w-[21%]">Criado em</TableHead>
            <TableHead className="w-[21%]">Processado em</TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {messages.length === 0 ? (
            <TableRow>
              <TableCell colSpan={6} className="text-center text-muted-foreground py-8">
                Nenhuma mensagem encontrada.
              </TableCell>
            </TableRow>
          ) : messages.map(m => (
            <TableRow key={m.id}>
              <TableCell>
                <Badge variant="outline" className="text-xs">{m.eventType}</Badge>
              </TableCell>
              <TableCell className="font-medium">{m.cliente}</TableCell>
              <TableCell>
                {m.processed
                  ? <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-green-100 text-green-800">Processado</span>
                  : <span className="inline-flex items-center px-2 py-0.5 rounded text-xs font-medium bg-yellow-100 text-yellow-800">Pendente</span>}
              </TableCell>
              <TableCell className="font-mono text-xs text-muted-foreground">
                {m.orderId.slice(0, 8)}…
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {formatDate(m.createdAt)}
              </TableCell>
              <TableCell className="text-sm text-muted-foreground">
                {m.processedAt ? formatDate(m.processedAt) : '—'}
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  )
}

function LoadingSkeleton() {
  return (
    <div className="flex flex-col gap-3">
      {Array.from({ length: 5 }).map((_, i) => (
        <Skeleton key={i} className="h-12 w-full rounded" />
      ))}
    </div>
  )
}

export function LogsPage() {
  const [activeTab, setActiveTab] = useState<Tab>('app')

  const tabs: { id: Tab; label: string }[] = [
    { id: 'app', label: 'Eventos da Aplicação' },
    { id: 'status', label: 'Histórico de Status' },
    { id: 'outbox', label: 'Outbox Messages' },
  ]

  return (
    <div className="flex flex-col gap-6 p-6 max-w-6xl mx-auto">
      <div>
        <h1 className="text-2xl font-semibold text-foreground">Logs</h1>
        <p className="text-sm text-muted-foreground">Eventos registrados pela aplicação</p>
      </div>

      <div className="flex gap-1 border-b">
        {tabs.map(tab => (
          <Button
            key={tab.id}
            variant="ghost"
            onClick={() => setActiveTab(tab.id)}
            className={`rounded-none border-b-2 pb-2 px-4 text-sm transition-colors ${
              activeTab === tab.id
                ? 'border-foreground text-foreground'
                : 'border-transparent text-muted-foreground hover:text-foreground'
            }`}
          >
            {tab.label}
          </Button>
        ))}
      </div>

      <div className="min-h-[420px] w-full">
        {activeTab === 'app' && <AppLogsTab />}
        {activeTab === 'status' && <StatusHistoryTab />}
        {activeTab === 'outbox' && <OutboxTab />}
      </div>
    </div>
  )
}
