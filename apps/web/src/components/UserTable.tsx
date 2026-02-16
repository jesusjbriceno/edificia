import { Badge } from "@/components/ui/Badge";
import { MoreHorizontal, UserCheck, UserX, Mail } from "lucide-react";

interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  status: "Active" | "Inactive";
  lastAccess: string;
}

interface UserTableProps {
  users: User[];
}

export default function UserTable({ users }: UserTableProps) {
  return (
    <div className="w-full overflow-x-auto rounded-xl border border-white/5 bg-dark-card/30 backdrop-blur-md">
      <table className="w-full text-left text-sm">
        <thead className="border-b border-white/5 text-gray-400 font-medium uppercase tracking-wider bg-white/5">
          <tr>
            <th className="px-6 py-4">Usuario</th>
            <th className="px-6 py-4">Rol</th>
            <th className="px-6 py-4">Estado</th>
            <th className="px-6 py-4">Último Acceso</th>
            <th className="px-6 py-4 text-right">Acciones</th>
          </tr>
        </thead>
        <tbody className="divide-y divide-white/5">
          {users.map((user) => (
            <tr key={user.id} className="hover:bg-white/5 transition-colors">
              <td className="px-6 py-4">
                <div className="flex items-center gap-3">
                  <div className="w-8 h-8 rounded-full bg-brand-primary/20 flex items-center justify-center text-brand-primary font-bold text-xs">
                    {user.name.charAt(0)}
                  </div>
                  <div>
                    <p className="font-medium text-white">{user.name}</p>
                    <p className="text-xs text-gray-500">{user.email}</p>
                  </div>
                </div>
              </td>
              <td className="px-6 py-4">
                <span className="text-gray-300">{user.role}</span>
              </td>
              <td className="px-6 py-4">
                <Badge variant={user.status === "Active" ? "success" : "error"}>
                  {user.status === "Active" ? "Activo" : "Desactivado"}
                </Badge>
              </td>
              <td className="px-6 py-4 text-gray-400 italic">
                {user.lastAccess}
              </td>
              <td className="px-6 py-4 text-right">
                <div className="flex justify-end gap-2">
                  <button title="Enviar email" className="p-1.5 text-gray-500 hover:text-white transition-colors">
                    <Mail size={16} />
                  </button>
                  <button title={user.status === "Active" ? "Desactivar" : "Activar"} className="p-1.5 text-gray-500 hover:text-brand-primary transition-colors">
                    {user.status === "Active" ? <UserX size={16} /> : <UserCheck size={16} />}
                  </button>
                  <button title="Más opciones" className="p-1.5 text-gray-500 hover:text-white transition-colors">
                    <MoreHorizontal size={16} />
                  </button>
                </div>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
