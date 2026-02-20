import * as React from "react";
import { X } from "lucide-react";
import { cn } from "@/lib/utils";

interface ModalProps {
  isOpen: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  className?: string;
}

export default function Modal({ isOpen, onClose, title, children, className }: ModalProps) {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-300">
      <div 
        className={cn(
          "relative w-full max-w-2xl bg-dark-card border border-white/10 rounded-2xl shadow-2xl overflow-hidden animate-in zoom-in-95 duration-300",
          className
        )}
      >
        <div className="flex items-center justify-between px-6 py-4 border-b border-white/5">
          <h3 className="text-xl font-semibold text-white">{title}</h3>
          <button 
            onClick={onClose}
            className="p-1 text-gray-500 hover:text-white transition-colors"
          >
            <X size={20} />
          </button>
        </div>
        
        <div className="p-6 overflow-y-auto max-h-[calc(90vh-70px)]">
          {children}
        </div>
      </div>
    </div>
  );
}
