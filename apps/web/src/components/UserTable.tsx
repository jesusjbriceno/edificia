import { UserRow } from './Admin/UserRow.js';

// ... interfaz User movida a UserRow ...

interface UserTableProps {
  users: any[];
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
            <UserRow key={user.id} user={user} />
          ))}
        </tbody>
      </table>
    </div>
  );
}
