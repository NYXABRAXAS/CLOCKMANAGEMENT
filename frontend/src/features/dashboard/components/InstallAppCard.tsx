import { Download, Share, SquarePlus } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useInstallPrompt } from "@/shared/lib/useInstallPrompt";

export function InstallAppCard() {
  const { installed, canPromptInstall, showIosInstructions, promptInstall } = useInstallPrompt();

  if (installed || (!canPromptInstall && !showIosInstructions)) return null;

  return (
    <Card>
      <CardHeader>
        <CardTitle>Get the app</CardTitle>
        <CardDescription>Install STLMS on your phone for a full-screen, app-like experience.</CardDescription>
      </CardHeader>
      <CardContent>
        {canPromptInstall && (
          <Button onClick={promptInstall}>
            <Download /> Install app
          </Button>
        )}
        {showIosInstructions && (
          <p className="flex flex-wrap items-center gap-1 text-sm text-muted-foreground">
            Tap <Share className="size-4" /> Share, then <SquarePlus className="size-4" /> "Add to Home Screen".
          </p>
        )}
      </CardContent>
    </Card>
  );
}
