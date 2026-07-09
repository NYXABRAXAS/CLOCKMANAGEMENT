import type { AxiosInstance } from "axios";

/** Triggers a browser download for a blob response - shared by every module's export button so
 * each one doesn't reimplement the create-object-URL-then-click-a-link dance. */
export function downloadBlob(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = fileName;
  document.body.appendChild(link);
  link.click();
  link.remove();
  URL.revokeObjectURL(url);
}

/** Extracts the server-suggested filename from a Content-Disposition header, falling back to the
 * given default if the header is missing or unparseable. */
function fileNameFromContentDisposition(contentDisposition: string | undefined, fallback: string) {
  const match = contentDisposition?.match(/filename="?([^"]+)"?/);
  return match?.[1] ?? fallback;
}

/** GETs a file-download endpoint as a blob and triggers the browser download - the shared
 * plumbing behind every module's "Export" button. */
export async function fetchAndDownload(
  client: AxiosInstance,
  url: string,
  fallbackFileName: string,
  params?: Record<string, string | number | undefined>,
) {
  const response = await client.get(url, { params, responseType: "blob" });
  downloadBlob(response.data as Blob, fileNameFromContentDisposition(response.headers["content-disposition"], fallbackFileName));
}
