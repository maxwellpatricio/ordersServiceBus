import axios from 'axios'
import type { Order, CreateOrderPayload, AiAnswer } from '@/Types/Order'
import type { AppLog, StatusHistoryLog, OutboxLog } from '@/Types/Logs'

const api = axios.create({ baseURL: '/api' })

export const ordersApi = {
  list: () => api.get<Order[]>('/orders').then(r => r.data),
  get: (id: string) => api.get<Order>(`/orders/${id}`).then(r => r.data),
  create: (payload: CreateOrderPayload) => api.post<Order>('/orders', payload).then(r => r.data),
  ask: (question: string) => api.post<AiAnswer>('/ai/ask', { question }).then(r => r.data),
}

export const logsApi = {
  getAppLogs: () => api.get<AppLog[]>('/logs/app').then(r => r.data),
  getStatusHistory: () => api.get<StatusHistoryLog[]>('/logs/status-history').then(r => r.data),
  getOutbox: () => api.get<OutboxLog[]>('/logs/outbox').then(r => r.data),
}
