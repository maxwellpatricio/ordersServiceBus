import { useState } from 'react'
import { toast } from 'sonner'
import { Button } from '@/Components/UI/Button'
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/Components/UI/Dialog'
import { Input } from '@/Components/UI/Input'
import { Label } from '@/Components/UI/Label'
import { ordersApi } from '@/Services/Api'
import type { Order } from '@/Types/Order'

interface OrderFormProps {
  open: boolean
  onOpenChange: (open: boolean) => void
  onCreated: (order: Order) => void
}

export function OrderForm({ open, onOpenChange, onCreated }: OrderFormProps) {
  const [loading, setLoading] = useState(false)
  const [form, setForm] = useState({ cliente: '', produto: '', valor: '' })

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    const valor = parseFloat(form.valor)
    if (!form.cliente.trim() || !form.produto.trim() || isNaN(valor) || valor <= 0) {
      toast.error('Preencha todos os campos corretamente.')
      return
    }

    setLoading(true)
    try {
      const order = await ordersApi.create({ cliente: form.cliente, produto: form.produto, valor })
      toast.success('Pedido criado com sucesso!')
      onCreated(order)
      onOpenChange(false)
      setForm({ cliente: '', produto: '', valor: '' })
    } catch {
      toast.error('Erro ao criar pedido. Tente novamente.')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-sm">
        <DialogHeader>
          <DialogTitle>Novo Pedido</DialogTitle>
        </DialogHeader>
        <form onSubmit={handleSubmit} className="flex flex-col gap-4">
          <div className="flex flex-col gap-2">
            <Label htmlFor="cliente">Cliente</Label>
            <Input
              id="cliente"
              placeholder="Nome do cliente"
              value={form.cliente}
              onChange={e => setForm(f => ({ ...f, cliente: e.target.value }))}
              disabled={loading}
            />
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="produto">Produto</Label>
            <Input
              id="produto"
              placeholder="Nome do produto"
              value={form.produto}
              onChange={e => setForm(f => ({ ...f, produto: e.target.value }))}
              disabled={loading}
            />
          </div>
          <div className="flex flex-col gap-2">
            <Label htmlFor="valor">Valor (R$)</Label>
            <Input
              id="valor"
              type="number"
              placeholder="0,00"
              min="0.01"
              step="0.01"
              value={form.valor}
              onChange={e => setForm(f => ({ ...f, valor: e.target.value }))}
              disabled={loading}
            />
          </div>
          <Button type="submit" disabled={loading}>
            {loading ? 'Criando...' : 'Criar Pedido'}
          </Button>
        </form>
      </DialogContent>
    </Dialog>
  )
}
