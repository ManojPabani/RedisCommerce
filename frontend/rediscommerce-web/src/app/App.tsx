import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router-dom'
import { Toaster } from 'sonner'
import { queryClient } from '../core/config/queryClient'
import { useAutoLogin } from '../features/auth/hooks/useAutoLogin'
import { ThemeProvider } from '../shared/theme/ThemeProvider'
import { router } from './router'

function AppContent() {
  useAutoLogin()
  return (
    <>
      <RouterProvider router={router} />
      <Toaster
        position="bottom-right"
        theme="system"
        toastOptions={{
          className: 'font-sans!',
          style: {
            fontFamily: 'DM Sans, ui-sans-serif, system-ui, sans-serif',
          },
        }}
      />
    </>
  )
}

export function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ThemeProvider>
        <AppContent />
      </ThemeProvider>
    </QueryClientProvider>
  )
}
