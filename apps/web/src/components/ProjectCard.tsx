import { Badge } from "@/components/ui/Badge";
import { Calendar, MoreVertical, FileText } from "lucide-react";

interface ProjectCardProps {
  title: string;
  status: "Draft" | "InProgress" | "Completed";
  lastModified: string;
  type: string;
}

export default function ProjectCard({ title, status, lastModified, type }: ProjectCardProps) {
  const statusConfig = {
    Draft: { label: "Borrador", variant: "default" as const },
    InProgress: { label: "En curso", variant: "info" as const },
    Completed: { label: "Completado", variant: "success" as const },
  };

  const { label, variant } = statusConfig[status];

  return (
    <div className="glass-card group p-6 rounded-xl transition-all duration-300 hover:border-brand-primary/30 hover:shadow-brand-primary/5 cursor-pointer flex flex-col justify-between h-48">
      <div className="flex justify-between items-start">
        <div className="p-2 bg-brand-primary/10 rounded-lg text-brand-primary">
          <FileText className="w-5 h-5" />
        </div>
        <button className="text-gray-500 hover:text-white transition-colors">
          <MoreVertical className="w-5 h-5" />
        </button>
      </div>

      <div className="space-y-1">
        <h3 className="font-semibold text-lg text-white group-hover:text-brand-primary transition-colors truncate">
          {title}
        </h3>
        <p className="text-xs text-gray-500 uppercase tracking-widest font-medium">
          {type}
        </p>
      </div>

      <div className="flex items-center justify-between pt-4 border-t border-white/5">
        <div className="flex items-center text-xs text-gray-500">
          <Calendar className="w-3.5 h-3.5 mr-1.5" />
          {lastModified}
        </div>
        <Badge variant={variant}>{label}</Badge>
      </div>
    </div>
  );
}
