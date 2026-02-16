import { Badge } from "@/components/ui/Badge";
import { MoreHorizontal, UserCheck, UserX, Mail } from "lucide-react";

export interface User {
  id: string;
  name: string;
  email: string;
  role: string;
  status: "Active" | "Inactive";
  lastAccess: string;
}

interface UserRowProps {
  user: User;
}

export function UserRow({ user }: UserRowProps) {
  return (
    <tr className="hover:bg-white/5 transition-colors group">
      <td className="px-6 py-4">
        <div className="flex items-center gap-3">
          <div className="w-8 h-8 rounded-full bg-brand-primary/20 flex items-center justify-center text-brand-primary font-bold text-xs">
            {user.name.charAt(0)}
          </div>
          <div>
            <p className="font-medium text-white group-hover:text-brand-primary transition-colors">{user.name}</p>
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
  );
}
