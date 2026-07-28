import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { queryClient } from '../core/config/queryClient'
import { useAutoLogin } from '../features/auth/hooks/useAutoLogin'
import { router } from './router'

function AppContent() {
  useAutoLogin()
  return <RouterProvider router={router} />
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <AppContent />
    </QueryClientProvider>
  )
}
