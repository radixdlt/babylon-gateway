export const normalizeBasePath = (basePath?: string) => {
  if (!basePath) return ''
  return basePath.endsWith('/') ? basePath?.slice(0, -1) : basePath
}
