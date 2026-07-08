import * as React from "react";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { toast } from "sonner";
import { Loader2, Camera } from "lucide-react";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Separator } from "@/components/ui/separator";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { userUpdated } from "@/features/auth/authSlice";
import { profileApi } from "../api/profileApi";
import { toApiError } from "@/shared/lib/apiClient";
import { TwoFactorSection } from "../components/TwoFactorSection";
import { ChangePasswordDialog } from "../components/ChangePasswordDialog";

const schema = z.object({
  firstName: z.string().min(1, "Required").max(100),
  lastName: z.string().min(1, "Required").max(100),
});
type FormValues = z.infer<typeof schema>;

function initials(firstName: string, lastName: string) {
  return `${firstName.charAt(0)}${lastName.charAt(0)}`.toUpperCase();
}

export default function ProfilePage() {
  const dispatch = useAppDispatch();
  const user = useAppSelector((s) => s.auth.user);
  const [submitting, setSubmitting] = React.useState(false);
  const [uploadingPhoto, setUploadingPhoto] = React.useState(false);
  const fileInputRef = React.useRef<HTMLInputElement>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: { firstName: user?.firstName ?? "", lastName: user?.lastName ?? "" },
  });

  if (!user) return null;

  const onSubmit = async (values: FormValues) => {
    setSubmitting(true);
    try {
      const profile = await profileApi.updateProfile(values);
      dispatch(userUpdated(profile));
      toast.success("Profile updated.");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSubmitting(false);
    }
  };

  const onPhotoSelected = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = "";
    if (!file) return;

    setUploadingPhoto(true);
    try {
      const profile = await profileApi.uploadPhoto(file);
      dispatch(userUpdated(profile));
      toast.success("Profile photo updated.");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setUploadingPhoto(false);
    }
  };

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Profile</h1>
        <p className="text-sm text-muted-foreground">Manage your personal details and account security.</p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Photo</CardTitle>
        </CardHeader>
        <CardContent className="flex items-center gap-4">
          <Avatar className="size-20">
            <AvatarImage src={user.photoUrl ?? undefined} alt={`${user.firstName} ${user.lastName}`} />
            <AvatarFallback className="text-lg">{initials(user.firstName, user.lastName)}</AvatarFallback>
          </Avatar>
          <div>
            <input ref={fileInputRef} type="file" accept="image/png,image/jpeg,image/webp" className="hidden" onChange={onPhotoSelected} />
            <Button type="button" variant="outline" disabled={uploadingPhoto} onClick={() => fileInputRef.current?.click()}>
              {uploadingPhoto ? <Loader2 className="animate-spin" /> : <Camera />}
              Change photo
            </Button>
            <p className="mt-1 text-xs text-muted-foreground">JPG, PNG, or WEBP. Max 5MB.</p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Personal details</CardTitle>
        </CardHeader>
        <CardContent>
          <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
            <div className="grid gap-4 sm:grid-cols-2">
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="firstName">First name</Label>
                <Input id="firstName" {...register("firstName")} />
                {errors.firstName && <p className="text-xs text-destructive">{errors.firstName.message}</p>}
              </div>
              <div className="flex flex-col gap-1.5">
                <Label htmlFor="lastName">Last name</Label>
                <Input id="lastName" {...register("lastName")} />
                {errors.lastName && <p className="text-xs text-destructive">{errors.lastName.message}</p>}
              </div>
            </div>
            <div className="flex flex-col gap-1.5">
              <Label>Email</Label>
              <Input value={user.email} disabled />
            </div>
            <div>
              <Button type="submit" disabled={submitting}>
                {submitting && <Loader2 className="animate-spin" />}
                Save changes
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Account</CardTitle>
          <CardDescription>Roles, plan, and verification status.</CardDescription>
        </CardHeader>
        <CardContent className="grid gap-2 text-sm sm:grid-cols-2">
          <div>
            <span className="font-medium">Roles:</span> {user.roles.join(", ")}
          </div>
          <div>
            <span className="font-medium">Plan:</span> {user.subscriptionStatus}
          </div>
          <div>
            <span className="font-medium">Email verified:</span> {user.emailVerified ? "Yes" : "No"}
          </div>
          <div>
            <span className="font-medium">Permissions:</span> {user.permissions.length}
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Security</CardTitle>
        </CardHeader>
        <CardContent className="flex flex-col gap-4">
          <TwoFactorSection />
          <Separator />
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm font-medium">Password</p>
              <p className="text-xs text-muted-foreground">Change the password used to sign in.</p>
            </div>
            <ChangePasswordDialog />
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
