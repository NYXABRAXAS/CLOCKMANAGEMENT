import * as React from "react";
import { useForm, Controller } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { Link, useNavigate } from "react-router";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { AuthLayout } from "../components/AuthLayout";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { login, verifyTwoFactorLogin } from "../authSlice";

const schema = z.object({
  email: z.string().min(1, "Required").email("Enter a valid email"),
  password: z.string().min(1, "Required"),
  rememberMe: z.boolean(),
});
type FormValues = z.infer<typeof schema>;

export default function LoginPage() {
  const dispatch = useAppDispatch();
  const navigate = useNavigate();
  const challengeToken = useAppSelector((s) => s.auth.twoFactorChallengeToken);
  const [submitting, setSubmitting] = React.useState(false);
  const [twoFactorCode, setTwoFactorCode] = React.useState("");

  const {
    register,
    handleSubmit,
    getValues,
    control,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema), defaultValues: { rememberMe: false } });

  const onSubmit = async (values: FormValues) => {
    setSubmitting(true);
    try {
      const result = await dispatch(login(values)).unwrap();
      if (!result.requiresTwoFactor) {
        toast.success(`Welcome back, ${result.profile?.firstName}!`);
        navigate("/dashboard");
      }
    } catch (err) {
      toast.error(typeof err === "string" ? err : "Unable to log in.");
    } finally {
      setSubmitting(false);
    }
  };

  const onSubmitTwoFactor = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!challengeToken) return;
    setSubmitting(true);
    try {
      const result = await dispatch(
        verifyTwoFactorLogin({ challengeToken, code: twoFactorCode, rememberMe: getValues("rememberMe") }),
      ).unwrap();
      toast.success(`Welcome back, ${result.profile.firstName}!`);
      navigate("/dashboard");
    } catch (err) {
      toast.error(typeof err === "string" ? err : "Invalid code.");
    } finally {
      setSubmitting(false);
    }
  };

  if (challengeToken) {
    return (
      <AuthLayout title="Two-factor verification" subtitle="Enter the 6-digit code from your authenticator app.">
        <form onSubmit={onSubmitTwoFactor} className="flex flex-col gap-4">
          <div className="flex flex-col gap-1.5">
            <Label htmlFor="code">Authentication code</Label>
            <Input
              id="code"
              inputMode="numeric"
              maxLength={6}
              value={twoFactorCode}
              onChange={(e) => setTwoFactorCode(e.target.value.replace(/\D/g, ""))}
              placeholder="123456"
              autoFocus
            />
          </div>
          <Button type="submit" disabled={submitting || twoFactorCode.length !== 6}>
            {submitting && <Loader2 className="animate-spin" />}
            Verify
          </Button>
        </form>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="Sign in" subtitle="Use your email and password to continue.">
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
        <div className="flex flex-col gap-1.5">
          <Label htmlFor="email">Email</Label>
          <Input id="email" type="email" autoComplete="email" {...register("email")} />
          {errors.email && <p className="text-xs text-destructive">{errors.email.message}</p>}
        </div>
        <div className="flex flex-col gap-1.5">
          <div className="flex items-center justify-between">
            <Label htmlFor="password">Password</Label>
            <Link to="/forgot-password" className="text-xs text-primary hover:underline">
              Forgot password?
            </Link>
          </div>
          <Input id="password" type="password" autoComplete="current-password" {...register("password")} />
          {errors.password && <p className="text-xs text-destructive">{errors.password.message}</p>}
        </div>
        <div className="flex items-center gap-2">
          <Controller
            control={control}
            name="rememberMe"
            render={({ field }) => <Checkbox id="rememberMe" checked={field.value} onCheckedChange={field.onChange} />}
          />
          <Label htmlFor="rememberMe" className="font-normal">
            Remember me for 30 days
          </Label>
        </div>
        <Button type="submit" disabled={submitting}>
          {submitting && <Loader2 className="animate-spin" />}
          Sign In
        </Button>
        <p className="text-center text-sm text-muted-foreground">
          Don&apos;t have an account?{" "}
          <Link to="/register" className="text-primary hover:underline">
            Create one
          </Link>
        </p>
      </form>
    </AuthLayout>
  );
}
