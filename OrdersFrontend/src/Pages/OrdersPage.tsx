import { useEffect, useState, lazy, Suspense } from 'react'
import { toast } from 'sonner'
import { Button } from '@/Components/UI/Button'
import { Skeleton } from '@/Components/UI/Skeleton'
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/Components/UI/Table'
import { StatusBadge } from '@/Components/StatusBadge'
import { OrderForm } from '@/Components/OrderForm'
import { ordersApi } from '@/Services/Api'
import { startSignalR } from '@/Services/SignalR'
import type { Order } from '@/Types/Order'

const OrderDetail = lazy(() =>
  import('@/Components/OrderDetail').then(m => ({ default: m.OrderDetail }))
)

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' }).format(new Date(iso))
}

function formatBRL(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

export function OrdersPage() {
  const [orders, setOrders] = useState<Order[]>([])
  const [loading, setLoading] = useState(true)
  const [formOpen, setFormOpen] = useState(false)
  const [detailId, setDetailId] = useState<string | null>(null)

  useEffect(() => {
    ordersApi.list()
      .then(setOrders)
      .catch(() => toast.error('Erro ao carregar pedidos.'))
      .finally(() => setLoading(false))
  }, [])

  useEffect(() => {
    let cleanup: (() => void) | null = null

    startSignalR().then(conn => {
      const handler = (updated: Order) => {
        setOrders(prev => prev.map(o => o.id === updated.id ? { ...o, ...updated } : o))
        toast.info(`Pedido atualizado: ${updated.status}`, { duration: 3000 })
      }
      conn.on('OrderStatusUpdated', handler)
      cleanup = () => conn.off('OrderStatusUpdated', handler)
    }).catch(() => {})

    return () => { cleanup?.() }
  }, [])

  const handleCreated = (order: Order) => {
    setOrders(prev => [order, ...prev])
  }

  return (
    <div className="flex flex-col gap-6 p-6 max-w-6xl mx-auto">
      <div className="flex flex-col items-center gap-3 py-4">
        <h1 className="text-2xl font-semibold text-foreground">Pedidos</h1>
        <p className="text-sm text-muted-foreground">
          {orders.length} pedido{orders.length !== 1 ? 's' : ''} encontrado{orders.length !== 1 ? 's' : ''}
        </p>
        <Button onClick={() => setFormOpen(true)}>+ Novo Pedido</Button>
      </div>

      {loading ? (
        <div className="flex flex-col gap-3">
          {Array.from({ length: 5 }).map((_, i) => (
            <Skeleton key={i} className="h-12 w-full rounded" />
          ))}
        </div>
      ) : orders.length === 0 ? (
        <div className="flex flex-col items-center gap-2 py-16 text-muted-foreground">
          <p className="text-lg">Nenhum pedido encontrado</p>
          <p className="text-sm">Clique em &quot;Novo Pedido&quot; para começar.</p>
        </div>
      ) : (
        <div className="rounded-md border">
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead className="w-[120px]">ID</TableHead>
                <TableHead>Cliente</TableHead>
                <TableHead>Produto</TableHead>
                <TableHead className="text-right">Valor</TableHead>
                <TableHead>Status</TableHead>
                <TableHead>Criado em</TableHead>
                <TableHead className="w-[80px]" />
              </TableRow>
            </TableHeader>
            <TableBody>
              {orders.map(order => (
                <TableRow key={order.id}>
                  <TableCell className="font-mono text-xs text-muted-foreground">
                    {order.id.slice(0, 8)}…
                  </TableCell>
                  <TableCell className="font-medium">{order.cliente}</TableCell>
                  <TableCell>{order.produto}</TableCell>
                  <TableCell className="text-right">{formatBRL(order.valor)}</TableCell>
                  <TableCell>
                    <StatusBadge status={order.status} />
                  </TableCell>
                  <TableCell className="text-sm text-muted-foreground">
                    {formatDate(order.dataCriacao)}
                  </TableCell>
                  <TableCell>
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setDetailId(order.id)}
                    >
                      Ver
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      )}

      <OrderForm open={formOpen} onOpenChange={setFormOpen} onCreated={handleCreated} />

      <Suspense fallback={null}>
        <OrderDetail
          orderId={detailId}
          open={detailId !== null}
          onOpenChange={open => { if (!open) setDetailId(null) }}
        />
      </Suspense>
    </div>
  )
}
