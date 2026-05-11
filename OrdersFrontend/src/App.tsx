import { BrowserRouter, Routes, Route } from 'react-router'
import { Toaster } from '@/Components/UI/Sonner'
import { NavBar } from '@/Components/NavBar'
import { OrdersPage } from '@/Pages/OrdersPage'
import { LogsPage } from '@/Pages/LogsPage'
import { AiChat } from '@/Components/AiChat'

function App() {
  return (
    <BrowserRouter>
      <NavBar />
      <Routes>
        <Route path="/" element={<OrdersPage />} />
        <Route path="/logs" element={<LogsPage />} />
      </Routes>
      <AiChat />
      <Toaster position="top-right" richColors />
    </BrowserRouter>
  )
}

export default App
