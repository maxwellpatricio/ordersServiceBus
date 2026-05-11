import { useEffect, useState } from 'react'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/Components/UI/Dialog'
import { Skeleton } from '@/Components/UI/Skeleton'
import { Separator } from '@/Components/UI/Separator'
import { StatusBadge } from '@/Components/StatusBadge'
import { ordersApi } from '@/Services/Api'
import type { Order } from '@/Types/Order'

interface OrderDetailProps {
  orderId: string | null
  open: boolean
  onOpenChange: (open: boolean) => void
}

function formatDate(iso: string) {
  return new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'medium' }).format(new Date(iso))
}

function formatBRL(value: number) {
  return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value)
}

export function OrderDetail({ orderId, open, onOpenChange }: OrderDetailProps) {
  const [order, setOrder] = useState<Order | null>(null)
  const [loading, setLoading] = useState(false)

  useEffect(() => {
    if (!orderId || !open) return
    setLoading(true)
    ordersApi.get(orderId)
      .then(setOrder)
      .finally(() => setLoading(false))
  }, [orderId, open])

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>Detalhes do Pedido</DialogTitle>
        </DialogHeader>

        {loading && (
          <div className="flex flex-col gap-3">
            <Skeleton className="h-5 w-3/4" />
            <Skeleton className="h-5 w-1/2" />
            <Skeleton className="h-5 w-2/3" />
          </div>
        )}

        {!loading && order && (
          <div className="flex flex-col gap-4">
            <div className="flex flex-col gap-2 text-sm">
              <div className="flex justify-between">
                <span className="text-muted-foreground">ID</span>
                <span className="font-mono text-xs">{order.id}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Cliente</span>
                <span>{order.cliente}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Produto</span>
                <span>{order.produto}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Valor</span>
                <span className="font-medium">{formatBRL(order.valor)}</span>
              </div>
              <div className="flex justify-between items-center">
                <span className="text-muted-foreground">Status</span>
                <StatusBadge status={order.status} />
              </div>
              <div className="flex justify-between">
                <span className="text-muted-foreground">Criado em</span>
                <span>{formatDate(order.dataCriacao)}</span>
              </div>
            </div>

            {order.statusHistory && order.statusHistory.length > 0 && (
              <>
                <Separator />
                <div className="flex flex-col gap-2">
                  <p className="text-sm font-medium">Histórico de Status</p>
                  <div className="flex flex-col gap-2">
                    {order.statusHistory.map((h, i) => (
                      <div key={i} className="flex items-center justify-between text-xs">
                        <div className="flex items-center gap-2">
                          {h.fromStatus && <StatusBadge status={h.fromStatus} className="text-xs" />}
                          {h.fromStatus && <span className="text-muted-foreground">→</span>}
                          <StatusBadge status={h.toStatus} className="text-xs" />
                        </div>
                        <span className="text-muted-foreground">{formatDate(h.changedAt)}</span>
                      </div>
                    ))}
                  </div>
                </div>
              </>
            )}
          </div>
        )}
      </DialogContent>
    </Dialog>
  )
}
