import type { ReactNode } from "react";
import { motion } from "framer-motion";
import { Clock } from "lucide-react";

export function AuthLayout({ children, title, subtitle }: { children: ReactNode; title: string; subtitle?: string }) {
  return (
    <div className="relative flex min-h-screen w-full items-center justify-center overflow-hidden bg-background p-6">
      <div className="pointer-events-none absolute inset-0 -z-10 gradient-brand opacity-10" />
      <div className="pointer-events-none absolute -top-32 -left-32 -z-10 size-96 rounded-full bg-primary/20 blur-3xl" />
      <div className="pointer-events-none absolute -bottom-32 -right-32 -z-10 size-96 rounded-full bg-accent/30 blur-3xl" />

      <motion.div
        initial={{ opacity: 0, y: 16 }}
        animate={{ opacity: 1, y: 0 }}
        transition={{ duration: 0.35, ease: "easeOut" }}
        className="w-full max-w-md"
      >
        <div className="mb-6 flex flex-col items-center gap-2 text-center">
          <span className="flex size-11 items-center justify-center rounded-2xl gradient-brand text-primary-foreground shadow-lg">
            <Clock className="size-6" />
          </span>
          <h1 className="text-xl font-semibold gradient-brand-text">Smart Time &amp; Lifestyle</h1>
        </div>

        <div className="glass-card rounded-2xl p-6">
          <div className="mb-5">
            <h2 className="text-lg font-semibold">{title}</h2>
            {subtitle && <p className="mt-1 text-sm text-muted-foreground">{subtitle}</p>}
          </div>
          {children}
        </div>
      </motion.div>
    </div>
  );
}
