import { useState, useRef, useEffect } from 'react'
import { Button } from '@/Components/UI/Button'
import { Input } from '@/Components/UI/Input'
import { Card, CardContent, CardHeader, CardTitle } from '@/Components/UI/Card'
import { Skeleton } from '@/Components/UI/Skeleton'
import { ordersApi } from '@/Services/Api'

interface Message {
  role: 'user' | 'assistant'
  text: string
}

const SUGGESTIONS = [
  'Quantos pedidos temos hoje?',
  'Quantos pedidos estão pendentes?',
  'Qual o valor total finalizado?',
]

export function AiChat() {
  const [open, setOpen] = useState(false)
  const [messages, setMessages] = useState<Message[]>([])
  const [input, setInput] = useState('')
  const [loading, setLoading] = useState(false)
  const bottomRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages, loading])

  const send = async (question: string) => {
    if (!question.trim() || loading) return
    setMessages(m => [...m, { role: 'user', text: question }])
    setInput('')
    setLoading(true)
    try {
      const { answer } = await ordersApi.ask(question)
      setMessages(m => [...m, { role: 'assistant', text: answer }])
    } catch {
      setMessages(m => [...m, { role: 'assistant', text: 'Erro ao processar sua pergunta. Tente novamente.' }])
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-3">
      {open && (
        <Card className="w-80 shadow-xl">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm flex items-center gap-2">
              <span>🤖</span> Pergunte sobre os Pedidos
            </CardTitle>
          </CardHeader>
          <CardContent className="flex flex-col gap-3">
            <div className="flex flex-col gap-2 max-h-60 overflow-y-auto pr-1">
              {messages.length === 0 && (
                <div className="flex flex-col gap-1">
                  <p className="text-xs text-muted-foreground">Sugestões:</p>
                  {SUGGESTIONS.map(s => (
                    <button
                      key={s}
                      onClick={() => send(s)}
                      className="text-xs text-left px-2 py-1 rounded bg-muted hover:bg-muted/80 transition-colors"
                    >
                      {s}
                    </button>
                  ))}
                </div>
              )}
              {messages.map((m, i) => (
                <div key={i} className={`flex ${m.role === 'user' ? 'justify-end' : 'justify-start'}`}>
                  <div className={`rounded-lg px-3 py-2 text-xs max-w-[90%] ${
                    m.role === 'user'
                      ? 'bg-primary text-primary-foreground'
                      : 'bg-muted text-foreground'
                  }`}>
                    {m.text}
                  </div>
                </div>
              ))}
              {loading && (
                <div className="flex justify-start">
                  <Skeleton className="h-8 w-40 rounded-lg" />
                </div>
              )}
              <div ref={bottomRef} />
            </div>
            <form
              onSubmit={e => { e.preventDefault(); send(input) }}
              className="flex gap-2"
            >
              <Input
                value={input}
                onChange={e => setInput(e.target.value)}
                placeholder="Faça uma pergunta..."
                className="text-xs h-8"
                disabled={loading}
              />
              <Button type="submit" size="sm" disabled={loading || !input.trim()}>
                →
              </Button>
            </form>
          </CardContent>
        </Card>
      )}
      <Button
        onClick={() => setOpen(o => !o)}
        size="lg"
        className="rounded-full size-14 text-xl shadow-lg"
        aria-label="Abrir chat de IA"
      >
        {open ? '✕' : '🤖'}
      </Button>
    </div>
  )
}
