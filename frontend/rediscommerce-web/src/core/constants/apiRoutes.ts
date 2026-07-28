export const API_ROUTES = {
  products: '/products',
  product: (id: number) => `/products/${id}`,
  productSearch: '/products/search',
  popularProducts: '/products/popular',
  cart: (userId: number) => `/cart/${userId}`,
  cartItems: (userId: number) => `/cart/${userId}/items`,
  cartItem: (userId: number, productId: number) => `/cart/${userId}/items/${productId}`,
  favorites: (userId: number) => `/users/${userId}/favorites`,
  favorite: (userId: number, productId: number) => `/users/${userId}/favorites/${productId}`,
  checkout: '/orders/checkout',

  login: '/auth/login',
  logout: '/auth/logout',
  session: '/auth/session',

  adminSessions: '/admin/sessions',
  adminActivitySummary: '/admin/activity/summary',
  adminMostActiveDay: '/admin/activity/most-active-day',
  adminActivityForUser: (userId: number) => `/admin/activity/${userId}`,
  adminVisitors: '/admin/visitors',
}
