import { useAuthStore } from '@/store/useAuthStore';
import { RoleLabels, type Role } from '@/lib/types';

export default function HeaderUser() {
  const user = useAuthStore((s) => s.user);

  if (!user) {
    return (
      <div className="flex items-center gap-3 pl-4 border-l border-white/10 animate-pulse">
        <div className="text-right space-y-1">
          <div className="h-4 w-24 bg-white/10 rounded" />
          <div className="h-3 w-16 bg-white/5 rounded" />
        </div>
        <div className="w-10 h-10 rounded-full bg-white/10" />
      </div>
    );
  }

  const roleLabel = RoleLabels[user.roles?.[0] as Role] ?? user.roles?.[0] ?? '';
  const initials = user.fullName
    .split(' ')
    .map((w) => w[0])
    .join('')
    .slice(0, 2)
    .toUpperCase();

  return (
    <div className="flex items-center gap-3 pl-4 border-l border-white/10">
      <div className="text-right">
        <p className="text-sm font-medium text-white">{user.fullName}</p>
        <p className="text-xs text-gray-500">{roleLabel}</p>
      </div>
      <div className="w-10 h-10 rounded-full bg-linear-to-br from-brand-primary to-blue-600 flex items-center justify-center font-bold text-white text-sm">
        {initials}
      </div>
    </div>
  );
}
