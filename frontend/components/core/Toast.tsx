"use client";

import { CheckIcon, XMarkIcon } from "@heroicons/react/24/outline";

import { cn } from "@/lib/utils";

interface ToastProps {
  /** Notification message text. */
  message: string;
  /** Called when the user dismisses the toast. */
  onDismiss?: () => void;
  className?: string;
}

/**
 * Bottom-centred dismissible notification overlay.
 * Rendered with a Nottingham Blue background and Paper text.
 * @param message - Text content to display
 * @param onDismiss - Callback invoked when the dismiss button is clicked
 */
export function Toast({ message, onDismiss, className }: ToastProps) {
  return (
    <div
      className={cn(
        "fixed bottom-6 left-1/2 -translate-x-1/2",
        "flex items-center gap-3 px-4 py-[10px]",
        "bg-ink text-paper rounded-md text-[14px]",
        "shadow-modal z-[100]",
        className,
      )}
    >
      <CheckIcon className="w-4 h-4 shrink-0" />
      <span>{message}</span>
      {onDismiss && (
        <button
          aria-label="Dismiss notification"
          onClick={onDismiss}
          className="ml-2 opacity-70 hover:opacity-100 cursor-pointer transition-opacity bg-transparent border-none text-paper p-0"
        >
          <XMarkIcon className="w-[14px] h-[14px]" />
        </button>
      )}
    </div>
  );
}
