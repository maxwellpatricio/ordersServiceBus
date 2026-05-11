export type OrderStatus = 'Pendente' | 'Processando' | 'Finalizado'

export interface StatusHistoryEntry {
  fromStatus: OrderStatus | null
  toStatus: OrderStatus
  changedAt: string
}

export interface Order {
  id: string
  cliente: string
  produto: string
  valor: number
  status: OrderStatus
  dataCriacao: string
  statusHistory?: StatusHistoryEntry[]
}

export interface CreateOrderPayload {
  cliente: string
  produto: string
  valor: number
}

export interface AiAnswer {
  answer: string
}
