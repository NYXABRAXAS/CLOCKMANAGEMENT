import * as React from "react";
import { toast } from "sonner";
import { Loader2, ShieldCheck, ShieldOff } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Dialog, DialogContent, DialogDescription, DialogFooter, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { userUpdated } from "@/features/auth/authSlice";
import { authApi } from "@/features/auth/api/authApi";
import { toApiError } from "@/shared/lib/apiClient";

export function TwoFactorSection() {
  const dispatch = useAppDispatch();
  const user = useAppSelector((s) => s.auth.user);
  const [enableOpen, setEnableOpen] = React.useState(false);
  const [disableOpen, setDisableOpen] = React.useState(false);
  const [qrCode, setQrCode] = React.useState<string | null>(null);
  const [code, setCode] = React.useState("");
  const [busy, setBusy] = React.useState(false);

  if (!user) return null;

  const openEnable = async () => {
    setEnableOpen(true);
    setCode("");
    try {
      const result = await authApi.setupTwoFactor();
      setQrCode(result.qrCodePngBase64);
    } catch (err) {
      toast.error(toApiError(err).message);
      setEnableOpen(false);
    }
  };

  const confirmEnable = async () => {
    setBusy(true);
    try {
      await authApi.verifyTwoFactorSetup(code);
      dispatch(userUpdated({ ...user, twoFactorEnabled: true }));
      toast.success("Two-factor authentication enabled.");
      setEnableOpen(false);
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setBusy(false);
    }
  };

  const confirmDisable = async () => {
    setBusy(true);
    try {
      await authApi.disableTwoFactor(code);
      dispatch(userUpdated({ ...user, twoFactorEnabled: false }));
      toast.success("Two-factor authentication disabled.");
      setDisableOpen(false);
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setBusy(false);
    }
  };

  return (
    <div className="flex items-center justify-between rounded-lg border p-4">
      <div className="flex items-center gap-3">
        {user.twoFactorEnabled ? <ShieldCheck className="size-5 text-primary" /> : <ShieldOff className="size-5 text-muted-foreground" />}
        <div>
          <p className="text-sm font-medium">Two-factor authentication</p>
          <p className="text-xs text-muted-foreground">{user.twoFactorEnabled ? "Enabled" : "Not enabled"}</p>
        </div>
      </div>

      {user.twoFactorEnabled ? (
        <Button variant="outline" onClick={() => { setCode(""); setDisableOpen(true); }}>
          Disable
        </Button>
      ) : (
        <Button variant="outline" onClick={openEnable}>
          Enable
        </Button>
      )}

      <Dialog open={enableOpen} onOpenChange={setEnableOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Enable two-factor authentication</DialogTitle>
            <DialogDescription>Scan this QR code with an authenticator app, then enter the 6-digit code it shows.</DialogDescription>
          </DialogHeader>
          <div className="flex flex-col items-center gap-4">
            {qrCode ? (
              <img src={`data:image/png;base64,${qrCode}`} alt="2FA QR code" className="size-48 rounded-md border" />
            ) : (
              <Loader2 className="size-8 animate-spin text-muted-foreground" />
            )}
            <div className="flex w-full flex-col gap-1.5">
              <Label htmlFor="2fa-code">Verification code</Label>
              <Input id="2fa-code" inputMode="numeric" maxLength={6} value={code} onChange={(e) => setCode(e.target.value)} />
            </div>
          </div>
          <DialogFooter>
            <Button onClick={confirmEnable} disabled={busy || code.length !== 6}>
              {busy && <Loader2 className="animate-spin" />}
              Verify &amp; Enable
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      <Dialog open={disableOpen} onOpenChange={setDisableOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Disable two-factor authentication</DialogTitle>
            <DialogDescription>Enter a current 6-digit code from your authenticator app to confirm.</DialogDescription>
          </DialogHeader>
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="2fa-disable-code">Verification code</Label>
            <Input id="2fa-disable-code" inputMode="numeric" maxLength={6} value={code} onChange={(e) => setCode(e.target.value)} />
          </div>
          <DialogFooter>
            <Button variant="destructive" onClick={confirmDisable} disabled={busy || code.length !== 6}>
              {busy && <Loader2 className="animate-spin" />}
              Disable
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
