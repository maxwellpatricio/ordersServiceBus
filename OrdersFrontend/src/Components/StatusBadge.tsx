import { Badge } from '@/Components/UI/Badge'
import { cn } from '@/Lib/Utils'
import type { OrderStatus } from '@/Types/Order'

interface StatusBadgeProps {
  status: OrderStatus
  className?: string
}

const config: Record<OrderStatus, { label: string; className: string }> = {
  Pendente: {
    label: 'Pendente',
    className: 'bg-yellow-100 text-yellow-800 hover:bg-yellow-100',
  },
  Processando: {
    label: 'Processando',
    className: 'bg-orange-100 text-orange-800 hover:bg-orange-100 animate-pulse',
  },
  Finalizado: {
    label: 'Finalizado',
    className: 'bg-green-100 text-green-800 hover:bg-green-100',
  },
}

export function StatusBadge({ status, className }: StatusBadgeProps) {
  const { label, className: statusClass } = config[status]
  return (
    <Badge variant="secondary" className={cn(statusClass, className)}>
      {label}
    </Badge>
  )
}
