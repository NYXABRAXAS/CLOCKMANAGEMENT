import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";

const API_BASE = import.meta.env.VITE_API_URL ?? "http://localhost:5080/api/v1";

// The csrf_token cookie lives on the API's own origin - if the frontend and API are ever deployed
// to different sites (e.g. separate subdomains), JS on the frontend's origin can never read a
// cookie set for the API's origin. So the API also returns the token in the JSON body of
// login/refresh/me/external-login; it's cached here and echoed back as the x-csrf-token header on
// state-changing requests. (Lesson learned the hard way on a previous project this session -
// applying it from the start here instead of hitting the same bug again.)
let csrfToken: string | undefined;

export function setCsrfToken(token: string | undefined) {
  csrfToken = token;
}

export const apiClient = axios.create({
  baseURL: API_BASE,
  withCredentials: true,
});

apiClient.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  const method = (config.method ?? "get").toUpperCase();
  if (!["GET", "HEAD", "OPTIONS"].includes(method) && csrfToken) {
    config.headers.set("x-csrf-token", csrfToken);
  }
  return config;
});

let refreshPromise: Promise<boolean> | null = null;

async function tryRefresh(): Promise<boolean> {
  refreshPromise ??= apiClient
    .post("/auth/refresh", undefined, { skipAuthRefresh: true } as never)
    .then((res) => {
      const token = (res.data as { csrfToken?: string })?.csrfToken;
      if (token) setCsrfToken(token);
      return true;
    })
    .catch(() => false)
    .finally(() => {
      refreshPromise = null;
    });
  return refreshPromise;
}

apiClient.interceptors.response.use(
  (response) => {
    const token = (response.data as { csrfToken?: string } | undefined)?.csrfToken;
    if (token) setCsrfToken(token);
    return response;
  },
  async (error: AxiosError) => {
    const config = error.config as (InternalAxiosRequestConfig & { skipAuthRefresh?: boolean; _retried?: boolean }) | undefined;
    const isAuthEndpoint = config?.url?.includes("/auth/login") || config?.url?.includes("/auth/refresh");

    if (error.response?.status === 401 && config && !config.skipAuthRefresh && !config._retried && !isAuthEndpoint) {
      config._retried = true;
      const refreshed = await tryRefresh();
      if (refreshed) return apiClient(config);
    }

    return Promise.reject(error);
  },
);

export class ApiError extends Error {
  status: number;
  body: unknown;
  constructor(status: number, message: string, body?: unknown) {
    super(message);
    this.status = status;
    this.body = body;
  }
}

export function toApiError(error: unknown): ApiError {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as { title?: string; message?: string } | undefined;
    const message = data?.title ?? data?.message ?? error.message ?? "Request failed";
    return new ApiError(error.response?.status ?? 0, message, data);
  }
  return new ApiError(0, error instanceof Error ? error.message : "Unknown error");
}
