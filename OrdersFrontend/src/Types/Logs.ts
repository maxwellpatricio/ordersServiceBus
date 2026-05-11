export interface AppLog {
  id: string
  level: 'Info' | 'Warning' | 'Error'
  source: 'API' | 'Worker'
  message: string
  details?: string
  orderId?: string
  createdAt: string
}

export interface StatusHistoryLog {
  id: string
  orderId: string
  cliente: string
  fromStatus: string | null
  toStatus: string
  changedAt: string
}

export interface OutboxLog {
  id: string
  orderId: string
  cliente: string
  eventType: string
  createdAt: string
  processedAt: string | null
  processed: boolean
}
