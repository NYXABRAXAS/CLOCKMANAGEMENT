import * as React from "react";
import { Link, useParams } from "react-router";
import { Loader2, CheckCircle2, XCircle } from "lucide-react";
import { AuthLayout } from "../components/AuthLayout";
import { Button } from "@/components/ui/button";
import { authApi } from "../api/authApi";
import { toApiError } from "@/shared/lib/apiClient";

export default function VerifyEmailPage() {
  const { token } = useParams<{ token: string }>();
  const [status, setStatus] = React.useState<"loading" | "success" | "error">("loading");
  const [message, setMessage] = React.useState("");

  React.useEffect(() => {
    if (!token) {
      setStatus("error");
      setMessage("Missing verification token.");
      return;
    }
    authApi
      .verifyEmail(token)
      .then(() => setStatus("success"))
      .catch((err) => {
        setStatus("error");
        setMessage(toApiError(err).message);
      });
  }, [token]);

  return (
    <AuthLayout title="Email verification">
      <div className="flex flex-col items-center gap-4 py-4 text-center">
        {status === "loading" && <Loader2 className="size-10 animate-spin text-primary" />}
        {status === "success" && (
          <>
            <CheckCircle2 className="size-10 text-green-500" />
            <p className="text-sm text-muted-foreground">Your email has been verified.</p>
          </>
        )}
        {status === "error" && (
          <>
            <XCircle className="size-10 text-destructive" />
            <p className="text-sm text-muted-foreground">{message || "This verification link is invalid or has expired."}</p>
          </>
        )}
        <Link to="/login" className="w-full">
          <Button className="w-full">Back to sign in</Button>
        </Link>
      </div>
    </AuthLayout>
  );
}
