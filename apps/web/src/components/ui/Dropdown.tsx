import React, { useState, useRef, useEffect } from 'react';
import { createPortal } from 'react-dom';
import { cn } from '@/lib/utils';

interface DropdownProps {
  trigger: React.ReactNode;
  children: React.ReactNode;
  className?: string;
  align?: 'left' | 'right';
}

/**
 * Componente Dropdown que utiliza Portales de React para evitar problemas de clipping
 * causados por contenedores con 'overflow: hidden' o 'overflow: auto'.
 */
export function Dropdown({ trigger, children, className, align = 'right' }: DropdownProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [position, setPosition] = useState({ top: 0, left: 0 });
  const triggerRef = useRef<HTMLDivElement>(null);
  const dropdownRef = useRef<HTMLDivElement>(null);

  const updatePosition = () => {
    if (triggerRef.current) {
      const rect = triggerRef.current.getBoundingClientRect();
      setPosition({
        top: rect.bottom + window.scrollY,
        left: align === 'right' 
          ? rect.right + window.scrollX 
          : rect.left + window.scrollX
      });
    }
  };

  useEffect(() => {
    if (isOpen) {
      updatePosition();
      window.addEventListener('scroll', updatePosition, true);
      window.addEventListener('resize', updatePosition);
      
      const handleClickOutside = (event: MouseEvent) => {
        if (
          dropdownRef.current && !dropdownRef.current.contains(event.target as Node) &&
          triggerRef.current && !triggerRef.current.contains(event.target as Node)
        ) {
          setIsOpen(false);
        }
      };

      document.addEventListener('mousedown', handleClickOutside);
      return () => {
        window.removeEventListener('scroll', updatePosition, true);
        window.removeEventListener('resize', updatePosition);
        document.removeEventListener('mousedown', handleClickOutside);
      };
    }
  }, [isOpen, align]);

  return (
    <>
      <div ref={triggerRef} onClick={() => setIsOpen(!isOpen)} className="inline-block">
        {trigger}
      </div>
      {isOpen && createPortal(
        <div
          ref={dropdownRef}
          style={{
            position: 'absolute',
            top: `${position.top + 8}px`,
            left: align === 'right' ? 'auto' : `${position.left}px`,
            right: align === 'right' ? `${window.innerWidth - position.left}px` : 'auto',
          }}
          className={cn(
            "z-[9999] min-w-[176px] bg-dark-card border border-white/10 rounded-xl shadow-2xl overflow-hidden animate-in fade-in zoom-in-95 duration-150",
            className
          )}
        >
          {children}
        </div>,
        document.body
      )}
    </>
  );
}
