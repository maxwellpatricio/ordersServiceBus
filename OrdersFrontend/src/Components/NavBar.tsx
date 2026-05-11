import { NavLink } from 'react-router'

export function NavBar() {
  return (
    <nav className="border-b bg-background">
      <div className="max-w-6xl mx-auto px-6 py-3 flex gap-6 items-center">
      <span className="font-semibold text-foreground text-sm">TMB Orders</span>
      <div className="flex gap-1">
        <NavLink
          to="/"
          end
          className={({ isActive }) =>
            `text-sm px-3 py-1.5 rounded-md transition-colors ${
              isActive
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:text-foreground hover:bg-accent'
            }`
          }
        >
          Pedidos
        </NavLink>
        <NavLink
          to="/logs"
          className={({ isActive }) =>
            `text-sm px-3 py-1.5 rounded-md transition-colors ${
              isActive
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground hover:text-foreground hover:bg-accent'
            }`
          }
        >
          Logs
        </NavLink>
      </div>
      </div>
    </nav>
  )
}
