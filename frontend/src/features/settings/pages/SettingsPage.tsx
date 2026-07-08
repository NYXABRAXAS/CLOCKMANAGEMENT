import * as React from "react";
import { Controller, useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import { useTheme } from "next-themes";
import { toast } from "sonner";
import { Loader2 } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { useAppDispatch, useAppSelector } from "@/app/hooks";
import { userUpdated } from "@/features/auth/authSlice";
import { settingsApi } from "../api/settingsApi";
import { religionsApi } from "@/features/religions/api/religionsApi";
import { toApiError } from "@/shared/lib/apiClient";
import { COUNTRIES, LANGUAGES, getTimezones } from "@/shared/lib/localeData";

const schema = z.object({
  theme: z.enum(["light", "dark", "system"]),
  timezoneId: z.string().min(1, "Required"),
  timeFormat: z.enum(["12h", "24h"]),
  language: z.string().min(1, "Required"),
  countryCode: z.string().nullable(),
  religionCode: z.string().nullable(),
});
type FormValues = z.infer<typeof schema>;

const NONE = "__none__";
const timezones = getTimezones();

export default function SettingsPage() {
  const dispatch = useAppDispatch();
  const user = useAppSelector((s) => s.auth.user);
  const { setTheme } = useTheme();
  const [submitting, setSubmitting] = React.useState(false);

  const { data: religions } = useQuery({ queryKey: ["religions"], queryFn: religionsApi.getReligions });

  const { control, handleSubmit, watch } = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      theme: user?.theme ?? "system",
      timezoneId: user?.timezoneId ?? "UTC",
      timeFormat: (user?.timeFormat as "12h" | "24h") ?? "24h",
      language: user?.language ?? "en",
      countryCode: user?.countryCode ?? null,
      religionCode: user?.religionCode ?? null,
    },
  });

  const watchedTheme = watch("theme");
  React.useEffect(() => {
    setTheme(watchedTheme);
  }, [watchedTheme, setTheme]);

  const onSubmit = async (values: FormValues) => {
    setSubmitting(true);
    try {
      const profile = await settingsApi.updateSettings({
        countryCode: values.countryCode,
        timezoneId: values.timezoneId,
        timeFormat: values.timeFormat,
        language: values.language,
        theme: values.theme,
        religionCode: values.religionCode,
        prayerLatitude: null,
        prayerLongitude: null,
      });
      dispatch(userUpdated(profile));
      toast.success("Settings saved.");
    } catch (err) {
      toast.error(toApiError(err).message);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="mx-auto flex max-w-3xl flex-col gap-6">
      <div>
        <h1 className="text-2xl font-semibold">Settings</h1>
        <p className="text-sm text-muted-foreground">Manage appearance, regional preferences, and your religion.</p>
      </div>

      <form onSubmit={handleSubmit(onSubmit)}>
        <Tabs defaultValue="appearance">
          <TabsList>
            <TabsTrigger value="appearance">Appearance</TabsTrigger>
            <TabsTrigger value="regional">Regional</TabsTrigger>
            <TabsTrigger value="religion">Religion</TabsTrigger>
          </TabsList>

          <TabsContent value="appearance" className="mt-4">
            <Card>
              <CardHeader>
                <CardTitle>Appearance</CardTitle>
                <CardDescription>Choose how STLMS looks on this device.</CardDescription>
              </CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <div className="flex flex-col gap-1.5">
                  <Label>Theme</Label>
                  <Controller
                    control={control}
                    name="theme"
                    render={({ field }) => (
                      <Select value={field.value} onValueChange={field.onChange}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="light">Light</SelectItem>
                          <SelectItem value="dark">Dark</SelectItem>
                          <SelectItem value="system">System</SelectItem>
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label>Time format</Label>
                  <Controller
                    control={control}
                    name="timeFormat"
                    render={({ field }) => (
                      <Select value={field.value} onValueChange={field.onChange}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="24h">24-hour</SelectItem>
                          <SelectItem value="12h">12-hour (AM/PM)</SelectItem>
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="regional" className="mt-4">
            <Card>
              <CardHeader>
                <CardTitle>Regional</CardTitle>
                <CardDescription>Timezone, language, and country used across the app.</CardDescription>
              </CardHeader>
              <CardContent className="grid gap-4 sm:grid-cols-2">
                <div className="flex flex-col gap-1.5">
                  <Label>Timezone</Label>
                  <Controller
                    control={control}
                    name="timezoneId"
                    render={({ field }) => (
                      <Select value={field.value} onValueChange={field.onChange}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {timezones.map((tz) => (
                            <SelectItem key={tz} value={tz}>
                              {tz}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <div className="flex flex-col gap-1.5">
                  <Label>Language</Label>
                  <Controller
                    control={control}
                    name="language"
                    render={({ field }) => (
                      <Select value={field.value} onValueChange={field.onChange}>
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {LANGUAGES.map((l) => (
                            <SelectItem key={l.code} value={l.code}>
                              {l.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
                <div className="flex flex-col gap-1.5 sm:col-span-2">
                  <Label>Country</Label>
                  <Controller
                    control={control}
                    name="countryCode"
                    render={({ field }) => (
                      <Select value={field.value ?? NONE} onValueChange={(v) => field.onChange(v === NONE ? null : v)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Not set" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value={NONE}>Not set</SelectItem>
                          {COUNTRIES.map((c) => (
                            <SelectItem key={c.code} value={c.code}>
                              {c.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
              </CardContent>
            </Card>
          </TabsContent>

          <TabsContent value="religion" className="mt-4">
            <Card>
              <CardHeader>
                <CardTitle>Religion</CardTitle>
                <CardDescription>
                  Used to personalize prayer times and festival reminders once that module is available.
                </CardDescription>
              </CardHeader>
              <CardContent>
                <div className="flex flex-col gap-1.5 sm:max-w-xs">
                  <Label>Religion</Label>
                  <Controller
                    control={control}
                    name="religionCode"
                    render={({ field }) => (
                      <Select value={field.value ?? NONE} onValueChange={(v) => field.onChange(v === NONE ? null : v)}>
                        <SelectTrigger>
                          <SelectValue placeholder="Not set" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value={NONE}>Not set</SelectItem>
                          {religions?.map((r) => (
                            <SelectItem key={r.code} value={r.code}>
                              {r.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </div>
              </CardContent>
            </Card>
          </TabsContent>
        </Tabs>

        <div className="mt-6 flex justify-end">
          <Button type="submit" disabled={submitting}>
            {submitting && <Loader2 className="animate-spin" />}
            Save changes
          </Button>
        </div>
      </form>
    </div>
  );
}
